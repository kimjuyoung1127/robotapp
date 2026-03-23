"""
Claude Code Token Efficiency Benchmark
— unity-cli (Bash) vs Unityctl.Mcp vs CoplayDev MCP

Measures schema size, request/response bytes, and cumulative token cost
for 5 read-only KineTutor3D QA operations.

Usage:
    python measure_claude_code_tokens.py [--skip-coplaydev] [--skip-mcp]
"""

import asyncio
import glob
import json
import math
import os
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path


ROBOTAPP_ROOT = Path(r"C:\Users\ezen601\Desktop\Jason\robotapp2")
UNITYCTL_ROOT = Path(r"C:\Users\ezen601\Desktop\Jason\unityctl")
PROJECT_PATH = "C:/Users/ezen601/Desktop/Jason/robotapp2"
UNITYCTL_EXE = UNITYCTL_ROOT / "publish" / "win-x64" / "unityctl.exe"
UNITYCTL_MCP_EXE = UNITYCTL_ROOT / "publish" / "mcp-win-x64" / "unityctl-mcp.exe"
OUTPUT_DIR = ROBOTAPP_ROOT / "docs" / "benchmark"

# Read-only operations to benchmark
OPERATIONS = [
    {
        "id": "compile_check",
        "label": "Compile Check",
        "cli_args": ["check", "--project", PROJECT_PATH, "--type", "compile", "--json"],
        "mcp_tool": "unityctl_check",
        "mcp_args": {"project": PROJECT_PATH, "type": "compile"},
        "coplaydev_capable": False,
    },
    {
        "id": "scene_hierarchy",
        "label": "Scene Hierarchy (depth=2)",
        "cli_args": ["exec", "--project", PROJECT_PATH, "--code",
                     "string.Join(\"\\n\", UnityEngine.SceneManagement.SceneManager.GetActiveScene()"
                     ".GetRootGameObjects().Select(go => go.name))", "--json"],
        "mcp_tool": "unityctl_exec",
        "mcp_args": {"project": PROJECT_PATH,
                     "code": "string.Join(\"\\n\", UnityEngine.SceneManagement.SceneManager"
                             ".GetActiveScene().GetRootGameObjects().Select(go => go.name))"},
        "coplaydev_capable": True,
        "coplaydev_type": "manage_scene",
        "coplaydev_params": {"action": "get_hierarchy"},
    },
    {
        "id": "robot_catalog",
        "label": "Robot Catalog",
        "cli_args": ["exec", "--project", PROJECT_PATH, "--code",
                     "JsonUtility.ToJson(Resources.FindObjectsOfTypeAll"
                     "<KineTutor3D.Templates.RobotCatalog>()[0])", "--json"],
        "mcp_tool": "unityctl_exec",
        "mcp_args": {"project": PROJECT_PATH,
                     "code": "JsonUtility.ToJson(Resources.FindObjectsOfTypeAll"
                             "<KineTutor3D.Templates.RobotCatalog>()[0])"},
        "coplaydev_capable": False,
    },
    {
        "id": "dh_table",
        "label": "DH Table (2DOF_RR)",
        "cli_args": ["exec", "--project", PROJECT_PATH, "--code",
                     "KineTutor3D.Kinematics.DHParameterFactory.GetParameters(\"2DOF_RR\").Count", "--json"],
        "mcp_tool": "unityctl_exec",
        "mcp_args": {"project": PROJECT_PATH,
                     "code": "KineTutor3D.Kinematics.DHParameterFactory.GetParameters(\"2DOF_RR\").Count"},
        "coplaydev_capable": False,
    },
    {
        "id": "build_settings",
        "label": "Build Settings",
        "cli_args": ["build", "--project", PROJECT_PATH, "--target", "StandaloneWindows64",
                     "--dry-run", "--json"],
        "mcp_tool": "unityctl_build",
        "mcp_args": {"project": PROJECT_PATH, "target": "StandaloneWindows64", "dryRun": True},
        "coplaydev_capable": False,
    },
]


def approx_tokens(text_or_bytes) -> int:
    if isinstance(text_or_bytes, str):
        byte_len = len(text_or_bytes.encode("utf-8"))
    elif isinstance(text_or_bytes, (bytes, bytearray)):
        byte_len = len(text_or_bytes)
    else:
        byte_len = int(text_or_bytes)
    return math.ceil(byte_len / 4)


def to_metrics(text: str) -> dict:
    b = len(text.encode("utf-8"))
    return {"bytes": b, "approx_tokens": approx_tokens(b), "text": text}


def run_cli(args: list[str]) -> str:
    result = subprocess.run(
        [str(UNITYCTL_EXE), *args],
        cwd=str(UNITYCTL_ROOT),
        capture_output=True, text=True, encoding="utf-8",
        timeout=30,
    )
    return result.stdout.strip()


def build_bash_request(args: list[str]) -> str:
    """Simulate the Bash tool JSON that Claude Code sends for a CLI call."""
    cmd = f"{UNITYCTL_EXE} {' '.join(args)}"
    return json.dumps({
        "command": cmd,
        "description": "Run unity-cli command",
    }, ensure_ascii=False, separators=(",", ":"))


def build_mcp_request(tool_name: str, arguments: dict) -> str:
    """Simulate the MCP tool call JSON that Claude Code sends."""
    return json.dumps({
        "name": tool_name,
        "arguments": arguments,
    }, ensure_ascii=False, separators=(",", ":"))


# ── Schema measurement ──────────────────────────────────────────

def measure_cli_schema() -> dict:
    schema_text = run_cli(["schema", "--format", "json"])
    return to_metrics(schema_text)


def measure_bash_tool_schema() -> dict:
    """The Bash tool schema is already in Claude Code's system prompt.
    We estimate its fixed size here for reference."""
    # Approximation of the Bash tool JSONSchema as it appears in system prompt
    bash_schema = json.dumps({
        "name": "Bash",
        "description": "Executes a given bash command and returns its output.",
        "parameters": {
            "type": "object",
            "properties": {
                "command": {"type": "string", "description": "The command to execute"},
                "description": {"type": "string", "description": "Description of what this command does"},
                "timeout": {"type": "number", "description": "Optional timeout in milliseconds"},
            },
            "required": ["command"],
        },
    }, ensure_ascii=False, separators=(",", ":"))
    return to_metrics(bash_schema)


# ── fastmcp import ──────────────────────────────────────────────

def ensure_fastmcp():
    try:
        import fastmcp  # noqa: F401
        return
    except ImportError:
        pass

    local_app_data = os.environ.get("LOCALAPPDATA")
    if not local_app_data:
        raise RuntimeError("LOCALAPPDATA not set")

    pattern = os.path.join(
        local_app_data, "uv", "cache", "archive-v0", "*", "Lib", "site-packages",
    )
    for sp in sorted(glob.glob(pattern), reverse=True):
        if os.path.isdir(os.path.join(sp, "fastmcp")):
            sys.path.insert(0, sp)
            return
    raise RuntimeError("fastmcp not found in uv cache")


# ── MCP measurement ─────────────────────────────────────────────

async def measure_mcp(skip_coplaydev: bool) -> dict:
    ensure_fastmcp()
    from fastmcp import Client  # noqa: E402
    from fastmcp.client.transports import StdioTransport  # noqa: E402

    data = {"unityctl_mcp": {}, "coplaydev_mcp": {}}

    # ── Unityctl.Mcp ──
    if UNITYCTL_MCP_EXE.exists():
        transport = StdioTransport(
            command=str(UNITYCTL_MCP_EXE), args=[],
            env={
                **os.environ,
                "Logging__LogLevel__Default": "None",
                "Logging__LogLevel__Microsoft": "None",
                "Logging__LogLevel__System": "None",
                "Logging__Console__LogToStandardErrorThreshold": "Trace",
            },
            keep_alive=False, log_file=sys.stderr,
        )
        client = Client(transport)

        async with client:
            tools = await client.list_tools()
            tools_json = json.dumps(
                [t.model_dump(by_alias=True, exclude_none=True) for t in tools],
                ensure_ascii=False, separators=(",", ":"),
            )
            data["unityctl_mcp"]["schema"] = to_metrics(tools_json)
            data["unityctl_mcp"]["tool_count"] = len(tools)
            data["unityctl_mcp"]["tool_names"] = [t.name for t in tools]

            # Measure each operation
            data["unityctl_mcp"]["operations"] = {}
            for op in OPERATIONS:
                req_text = build_mcp_request(op["mcp_tool"], op["mcp_args"])
                try:
                    result = await client.call_tool(
                        op["mcp_tool"], op["mcp_args"], raise_on_error=False,
                    )
                    resp_text = result.content[0].text if result.content else ""
                except Exception as exc:
                    resp_text = f"ERROR: {exc}"
                data["unityctl_mcp"]["operations"][op["id"]] = {
                    "request": to_metrics(req_text),
                    "response": to_metrics(resp_text),
                }
    else:
        data["unityctl_mcp"]["error"] = f"Executable not found: {UNITYCTL_MCP_EXE}"

    # ── CoplayDev MCP ──
    if not skip_coplaydev:
        try:
            from fastmcp.client import UvxStdioTransport  # noqa: E402
            coplay_transport = UvxStdioTransport(
                "mcp-for-unity",
                from_package="mcpforunityserver==9.4.7",
                tool_args=["--transport", "stdio"],
                env_vars={
                    "UNITY_MCP_TELEMETRY_ENABLED": "0",
                    "UNITY_MCP_SKIP_STARTUP_CONNECT": "1",
                },
                keep_alive=False,
            )
            coplay_client = Client(coplay_transport)

            async with coplay_client:
                tools = await coplay_client.list_tools()
                tools_json = json.dumps(
                    [t.model_dump(by_alias=True, exclude_none=True) for t in tools],
                    ensure_ascii=False, separators=(",", ":"),
                )
                data["coplaydev_mcp"]["schema"] = to_metrics(tools_json)
                data["coplaydev_mcp"]["tool_count"] = len(tools)
                data["coplaydev_mcp"]["tool_names"] = [t.name for t in tools]

                # Measure operations that CoplayDev can handle
                data["coplaydev_mcp"]["operations"] = {}
                for op in OPERATIONS:
                    if op.get("coplaydev_capable"):
                        req_text = build_mcp_request(
                            op["coplaydev_type"], op["coplaydev_params"],
                        )
                        try:
                            result = await coplay_client.call_tool(
                                op["coplaydev_type"], op["coplaydev_params"],
                                raise_on_error=False,
                            )
                            resp_text = result.content[0].text if result.content else ""
                        except Exception as exc:
                            resp_text = f"ERROR: {exc}"
                        data["coplaydev_mcp"]["operations"][op["id"]] = {
                            "request": to_metrics(req_text),
                            "response": to_metrics(resp_text),
                        }
                    else:
                        data["coplaydev_mcp"]["operations"][op["id"]] = {
                            "status": "N/A — no matching tool",
                        }
        except Exception as exc:
            data["coplaydev_mcp"]["error"] = str(exc)
    else:
        data["coplaydev_mcp"]["skipped"] = True

    return data


# ── CLI measurement ──────────────────────────────────────────────

def measure_cli_operations() -> dict:
    results = {}
    for op in OPERATIONS:
        req_text = build_bash_request(op["cli_args"])
        try:
            resp_text = run_cli(op["cli_args"])
        except Exception as exc:
            resp_text = f"ERROR: {exc}"
        results[op["id"]] = {
            "request": to_metrics(req_text),
            "response": to_metrics(resp_text),
        }
    return results


# ── Cumulative cost model ────────────────────────────────────────

def cumulative_cost(schema_bytes: int, operations: dict, runs: int) -> dict:
    """Calculate total token cost for N runs of all operations."""
    per_run_bytes = sum(
        op.get("request", {}).get("bytes", 0) + op.get("response", {}).get("bytes", 0)
        for op in operations.values()
        if isinstance(op, dict) and "request" in op
    )
    total_bytes = schema_bytes + runs * per_run_bytes
    return {
        "schema_bytes": schema_bytes,
        "per_run_bytes": per_run_bytes,
        "runs": runs,
        "total_bytes": total_bytes,
        "total_approx_tokens": approx_tokens(total_bytes),
    }


# ── Report generation ────────────────────────────────────────────

def generate_report(data: dict) -> str:
    lines = [
        "# Claude Code Token Efficiency Benchmark",
        "",
        f"- Collected at (UTC): {data['collected_at_utc']}",
        f"- Project: `{PROJECT_PATH}`",
        "",
        "## 1. Schema Fixed Cost (per session)",
        "",
        "| Stack | Context | Bytes | Approx Tokens | Claude Code 추가비용 |",
        "| --- | --- | ---: | ---: | --- |",
    ]

    cli_schema = data["data"]["cli"]["schema"]
    bash_schema = data["data"]["cli"]["bash_tool_schema"]
    lines.append(
        f"| unity-cli (Bash) | CLI schema (참고용) | {cli_schema['bytes']} | {cli_schema['approx_tokens']}"
        f" | **0** (Bash 도구 이미 로드됨) |"
    )
    lines.append(
        f"| Bash tool itself | 시스템 프롬프트 포함 | {bash_schema['bytes']} | {bash_schema['approx_tokens']}"
        f" | 공유 (다른 도구와 공용) |"
    )

    mcp_data = data["data"].get("unityctl_mcp", {})
    if "schema" in mcp_data:
        s = mcp_data["schema"]
        lines.append(
            f"| Unityctl.Mcp | tools/list | {s['bytes']} | {s['approx_tokens']}"
            f" | +{s['approx_tokens']} tok (deferred fetch) |"
        )

    coplay_data = data["data"].get("coplaydev_mcp", {})
    if "schema" in coplay_data:
        s = coplay_data["schema"]
        lines.append(
            f"| CoplayDev MCP | tools/list | {s['bytes']} | {s['approx_tokens']}"
            f" | +{s['approx_tokens']} tok (deferred fetch) |"
        )

    # Per-operation comparison
    lines += [
        "",
        "## 2. Per-Operation Cost",
        "",
        "| Operation | Stack | Req Bytes | Resp Bytes | Total Bytes | Approx Tokens |",
        "| --- | --- | ---: | ---: | ---: | ---: |",
    ]

    cli_ops = data["data"]["cli"]["operations"]
    mcp_ops = mcp_data.get("operations", {})
    coplay_ops = coplay_data.get("operations", {})

    for op in OPERATIONS:
        oid = op["id"]

        # CLI via Bash
        if oid in cli_ops:
            c = cli_ops[oid]
            req_b = c["request"]["bytes"]
            resp_b = c["response"]["bytes"]
            total = req_b + resp_b
            lines.append(f"| {op['label']} | unity-cli (Bash) | {req_b} | {resp_b} | {total} | {approx_tokens(total)} |")

        # Unityctl.Mcp
        if oid in mcp_ops and "request" in mcp_ops[oid]:
            m = mcp_ops[oid]
            req_b = m["request"]["bytes"]
            resp_b = m["response"]["bytes"]
            total = req_b + resp_b
            lines.append(f"| {op['label']} | Unityctl.Mcp | {req_b} | {resp_b} | {total} | {approx_tokens(total)} |")

        # CoplayDev
        if oid in coplay_ops:
            if "request" in coplay_ops[oid]:
                cd = coplay_ops[oid]
                req_b = cd["request"]["bytes"]
                resp_b = cd["response"]["bytes"]
                total = req_b + resp_b
                lines.append(f"| {op['label']} | CoplayDev MCP | {req_b} | {resp_b} | {total} | {approx_tokens(total)} |")
            else:
                lines.append(f"| {op['label']} | CoplayDev MCP | — | — | — | N/A |")

    # Cumulative cost
    lines += [
        "",
        "## 3. Cumulative Cost (5 ops × N sessions)",
        "",
        "| Stack | Schema (1회) | 5 ops × 1 | 5 ops × 5 | 5 ops × 10 |",
        "| --- | ---: | ---: | ---: | ---: |",
    ]

    for stack_key, stack_label, schema_bytes, ops in [
        ("cli", "unity-cli (Bash)", 0, cli_ops),
        ("unityctl_mcp", "Unityctl.Mcp",
         mcp_data.get("schema", {}).get("bytes", 0), mcp_ops),
    ]:
        c1 = cumulative_cost(schema_bytes, ops, 1)
        c5 = cumulative_cost(schema_bytes, ops, 5)
        c10 = cumulative_cost(schema_bytes, ops, 10)
        lines.append(
            f"| {stack_label} | {schema_bytes} B | "
            f"{c1['total_approx_tokens']} tok | "
            f"{c5['total_approx_tokens']} tok | "
            f"{c10['total_approx_tokens']} tok |"
        )

    if "schema" in coplay_data:
        schema_b = coplay_data["schema"]["bytes"]
        c1 = cumulative_cost(schema_b, coplay_ops, 1)
        c5 = cumulative_cost(schema_b, coplay_ops, 5)
        c10 = cumulative_cost(schema_b, coplay_ops, 10)
        lines.append(
            f"| CoplayDev MCP | {schema_b} B | "
            f"{c1['total_approx_tokens']} tok | "
            f"{c5['total_approx_tokens']} tok | "
            f"{c10['total_approx_tokens']} tok |"
        )

    lines += [
        "",
        "## 4. Key Findings",
        "",
        "_(자동 생성 — 수치 기반 분석은 리포트 생성 후 수동 추가)_",
        "",
        "## Files",
        "",
        "- Methodology: `benchmark-methodology.md`",
        "- Raw JSON: `2026-03-20-claude-code-token-benchmark.json`",
        "- Runner: `measure_claude_code_tokens.py`",
    ]

    return "\n".join(lines)


# ── Main ─────────────────────────────────────────────────────────

async def main() -> None:
    skip_coplaydev = "--skip-coplaydev" in sys.argv
    skip_mcp = "--skip-mcp" in sys.argv

    print("[1/4] Measuring CLI schema...", file=sys.stderr)
    cli_schema = measure_cli_schema()
    bash_schema = measure_bash_tool_schema()

    print("[2/4] Measuring CLI operations (read-only)...", file=sys.stderr)
    cli_ops = measure_cli_operations()

    mcp_result = {}
    if not skip_mcp:
        print("[3/4] Measuring MCP stacks...", file=sys.stderr)
        mcp_result = await measure_mcp(skip_coplaydev)
    else:
        print("[3/4] Skipping MCP measurement (--skip-mcp)", file=sys.stderr)

    print("[4/4] Generating report...", file=sys.stderr)

    output = {
        "benchmark_name": "claude-code-token-efficiency",
        "collected_at_utc": datetime.now(timezone.utc).isoformat(),
        "project": PROJECT_PATH,
        "operations": [{"id": op["id"], "label": op["label"]} for op in OPERATIONS],
        "notes": [
            "Token estimate uses UTF-8 bytes/4 heuristic.",
            "CLI schema cost in Claude Code context is 0 (Bash tool shared).",
            "All operations are read-only and do not modify project state.",
        ],
        "data": {
            "cli": {
                "schema": cli_schema,
                "bash_tool_schema": bash_schema,
                "operations": cli_ops,
            },
            **mcp_result,
        },
    }

    # Strip response text from JSON output to keep file small
    output_slim = json.loads(json.dumps(output, ensure_ascii=False))
    for stack_key in ("cli", "unityctl_mcp", "coplaydev_mcp"):
        stack = output_slim.get("data", {}).get(stack_key, {})
        if "schema" in stack:
            stack["schema"].pop("text", None)
        for op_data in stack.get("operations", {}).values():
            if isinstance(op_data, dict):
                for field in ("request", "response"):
                    if field in op_data:
                        op_data[field].pop("text", None)

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    json_path = OUTPUT_DIR / "2026-03-20-claude-code-token-benchmark.json"
    json_path.write_text(
        json.dumps(output_slim, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    print(f"  JSON: {json_path}", file=sys.stderr)

    md_path = OUTPUT_DIR / "2026-03-20-claude-code-token-benchmark.md"
    report = generate_report(output)
    md_path.write_text(report, encoding="utf-8")
    print(f"  Report: {md_path}", file=sys.stderr)

    print("Done.", file=sys.stderr)


if __name__ == "__main__":
    asyncio.run(main())
