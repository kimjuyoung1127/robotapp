import asyncio
import glob
import json
import math
import os
import subprocess
import sys
from pathlib import Path


ROBOTAPP_ROOT = Path(r"C:\Users\ezen601\Desktop\Jason\robotapp2")
UNITYCTL_ROOT = Path(r"C:\Users\ezen601\Desktop\Jason\unityctl")
PROJECT_PATH = "C:/Users/ezen601/Desktop/Jason/robotapp2"
UNITYCTL_EXE = UNITYCTL_ROOT / "publish" / "win-x64" / "unityctl.exe"
UNITYCTL_MCP_EXE = UNITYCTL_ROOT / "publish" / "mcp-win-x64" / "unityctl-mcp.exe"


def ensure_fastmcp_importable() -> None:
    try:
        import fastmcp  # noqa: F401
        return
    except ImportError:
        pass

    local_app_data = os.environ.get("LOCALAPPDATA")
    if not local_app_data:
        raise RuntimeError("LOCALAPPDATA is not set; cannot locate fastmcp site-packages.")

    pattern = os.path.join(
        local_app_data,
        "uv",
        "cache",
        "archive-v0",
        "*",
        "Lib",
        "site-packages",
    )
    candidates = []
    for site_packages in glob.glob(pattern):
        if os.path.isdir(os.path.join(site_packages, "fastmcp")):
            candidates.append(site_packages)

    if not candidates:
        raise RuntimeError("Could not locate fastmcp in uv cache.")

    candidates.sort(reverse=True)
    sys.path.insert(0, candidates[0])


ensure_fastmcp_importable()

from fastmcp import Client  # noqa: E402
from fastmcp.client import UvxStdioTransport  # noqa: E402
from fastmcp.client.transports import StdioTransport  # noqa: E402


def approx_tokens(text: str) -> int:
    # Simple UTF-8/JSON heuristic when tokenizer is not locally available.
    return math.ceil(len(text.encode("utf-8")) / 4)


def run_command(executable: str, arguments: list[str], cwd: Path) -> str:
    result = subprocess.run(
        [executable, *arguments],
        cwd=str(cwd),
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    return result.stdout.strip()


def to_payload_metrics(text: str) -> dict:
    return {
        "bytes": len(text.encode("utf-8")),
        "approx_tokens": approx_tokens(text),
        "text": text,
    }


def measure_cli_schema() -> dict:
    schema_json = run_command(str(UNITYCTL_EXE), ["schema", "--format", "json"], UNITYCTL_ROOT)
    payload = to_payload_metrics(schema_json)
    payload["command"] = f"{UNITYCTL_EXE} schema --format json"
    return payload


def measure_cli_requests() -> dict:
    status_command = f'{UNITYCTL_EXE} status --project {PROJECT_PATH} --json'
    status_response = run_command(
        str(UNITYCTL_EXE),
        ["status", "--project", PROJECT_PATH, "--json"],
        UNITYCTL_ROOT,
    )

    build_command = f'{UNITYCTL_EXE} build --project {PROJECT_PATH} --target StandaloneWindows64 --dry-run --json'
    build_response = run_command(
        str(UNITYCTL_EXE),
        ["build", "--project", PROJECT_PATH, "--target", "StandaloneWindows64", "--dry-run", "--json"],
        UNITYCTL_ROOT,
    )

    return {
        "status": {
            "request": to_payload_metrics(status_command),
            "response": to_payload_metrics(status_response),
        },
        "build_dry_run": {
            "request": to_payload_metrics(build_command),
            "response": to_payload_metrics(build_response),
        },
    }


def serialize_tools(tools) -> str:
    return json.dumps(
        [tool.model_dump(by_alias=True, exclude_none=True) for tool in tools],
        ensure_ascii=False,
        separators=(",", ":"),
    )


async def collect_mcp_data() -> dict:
    unityctl_transport = StdioTransport(
        command=str(UNITYCTL_MCP_EXE),
        args=[],
        env={
            **os.environ,
            "Logging__LogLevel__Default": "None",
            "Logging__LogLevel__Microsoft": "None",
            "Logging__LogLevel__System": "None",
            "Logging__Console__LogToStandardErrorThreshold": "Trace",
        },
        keep_alive=False,
        log_file=sys.stderr,
    )
    unityctl_client = Client(unityctl_transport)

    coplaydev_transport = UvxStdioTransport(
        "mcp-for-unity",
        from_package="mcpforunityserver==9.4.7",
        tool_args=["--transport", "stdio"],
        env_vars={
            "UNITY_MCP_TELEMETRY_ENABLED": "0",
            "UNITY_MCP_SKIP_STARTUP_CONNECT": "1",
        },
        keep_alive=False,
    )
    coplaydev_client = Client(coplaydev_transport)

    async with unityctl_client, coplaydev_client:
        unityctl_tools = await unityctl_client.list_tools()
        coplaydev_tools = await coplaydev_client.list_tools()

        unityctl_tools_json = serialize_tools(unityctl_tools)
        coplaydev_tools_json = serialize_tools(coplaydev_tools)

        unityctl_status_request = json.dumps(
            {"name": "unityctl_status", "arguments": {"project": PROJECT_PATH, "wait": False}},
            ensure_ascii=False,
            separators=(",", ":"),
        )
        unityctl_status_result = await unityctl_client.call_tool(
            "unityctl_status",
            {"project": PROJECT_PATH, "wait": False},
            raise_on_error=False,
        )
        unityctl_status_response = unityctl_status_result.content[0].text if unityctl_status_result.content else ""

        unityctl_build_request = json.dumps(
            {
                "name": "unityctl_build",
                "arguments": {
                    "project": PROJECT_PATH,
                    "target": "StandaloneWindows64",
                    "dryRun": True,
                },
            },
            ensure_ascii=False,
            separators=(",", ":"),
        )
        unityctl_build_result = await unityctl_client.call_tool(
            "unityctl_build",
            {"project": PROJECT_PATH, "target": "StandaloneWindows64", "dryRun": True},
            raise_on_error=False,
        )
        unityctl_build_response = unityctl_build_result.content[0].text if unityctl_build_result.content else ""

        coplaydev_tool_names = [tool.name for tool in coplaydev_tools]
        build_like_tools = [name for name in coplaydev_tool_names if "build" in name.lower()]

        return {
            "unityctl_mcp": {
                "tools_list": {
                    **to_payload_metrics(unityctl_tools_json),
                    "tool_count": len(unityctl_tools),
                    "tool_names": [tool.name for tool in unityctl_tools],
                },
                "status": {
                    "request": to_payload_metrics(unityctl_status_request),
                    "response": to_payload_metrics(unityctl_status_response),
                },
                "build_dry_run": {
                    "request": to_payload_metrics(unityctl_build_request),
                    "response": to_payload_metrics(unityctl_build_response),
                },
            },
            "coplaydev_mcp": {
                "tools_list": {
                    **to_payload_metrics(coplaydev_tools_json),
                    "tool_count": len(coplaydev_tools),
                    "tool_names": coplaydev_tool_names,
                },
                "build_capability": {
                    "has_direct_build_tool": len(build_like_tools) > 0,
                    "matching_tools": build_like_tools,
                },
            },
        }


def summarize_cumulative(schema_bytes: int, request_bytes: int, response_bytes: int, runs: int) -> dict:
    total_bytes = schema_bytes + runs * (request_bytes + response_bytes)
    return {
        "bytes": total_bytes,
        "approx_tokens": approx_tokens("x" * total_bytes),
    }


async def main() -> None:
    cli_schema = measure_cli_schema()
    cli_requests = measure_cli_requests()
    mcp_data = await collect_mcp_data()

    output = {
        "benchmark_name": "token-efficiency-unityctl-vs-coplaydev",
        "collected_at_utc": subprocess.run(
            ["powershell", "-NoProfile", "-Command", "[DateTimeOffset]::UtcNow.ToString('o')"],
            check=True,
            capture_output=True,
            text=True,
            encoding="utf-8",
        ).stdout.strip(),
        "environment": {
            "robotapp_root": str(ROBOTAPP_ROOT),
            "unityctl_root": str(UNITYCTL_ROOT),
            "project_path": PROJECT_PATH,
        },
        "notes": [
            "Token estimate uses a simple UTF-8 bytes/4 heuristic because a tokenizer library was not installed locally.",
            "CoplayDev tools/list is measured from the packaged stdio MCP server (mcpforunityserver==9.4.7).",
            "CoplayDev exposes no direct build tool in tools/list, so build roundtrip cost is N/A on that stack.",
        ],
        "data": {
            "unityctl_cli": {
                "schema": cli_schema,
                "requests": cli_requests,
                "ten_run_totals": {
                    "status": summarize_cumulative(
                        cli_schema["bytes"],
                        cli_requests["status"]["request"]["bytes"],
                        cli_requests["status"]["response"]["bytes"],
                        10,
                    ),
                    "build_dry_run": summarize_cumulative(
                        cli_schema["bytes"],
                        cli_requests["build_dry_run"]["request"]["bytes"],
                        cli_requests["build_dry_run"]["response"]["bytes"],
                        10,
                    ),
                },
            },
            "unityctl_mcp": {
                **mcp_data["unityctl_mcp"],
                "ten_run_totals": {
                    "status": summarize_cumulative(
                        mcp_data["unityctl_mcp"]["tools_list"]["bytes"],
                        mcp_data["unityctl_mcp"]["status"]["request"]["bytes"],
                        mcp_data["unityctl_mcp"]["status"]["response"]["bytes"],
                        10,
                    ),
                    "build_dry_run": summarize_cumulative(
                        mcp_data["unityctl_mcp"]["tools_list"]["bytes"],
                        mcp_data["unityctl_mcp"]["build_dry_run"]["request"]["bytes"],
                        mcp_data["unityctl_mcp"]["build_dry_run"]["response"]["bytes"],
                        10,
                    ),
                },
            },
            "coplaydev_mcp": mcp_data["coplaydev_mcp"],
        },
    }

    output_dir = ROBOTAPP_ROOT / "docs" / "benchmark"
    output_dir.mkdir(parents=True, exist_ok=True)
    json_path = output_dir / "2026-03-18-token-efficiency.json"
    md_path = output_dir / "2026-03-18-token-efficiency.md"

    json_text = json.dumps(output, ensure_ascii=False, indent=2)
    json_path.write_text(json_text, encoding="utf-8")

    cli_schema_bytes = output["data"]["unityctl_cli"]["schema"]["bytes"]
    unityctl_mcp_schema_bytes = output["data"]["unityctl_mcp"]["tools_list"]["bytes"]
    coplaydev_schema_bytes = output["data"]["coplaydev_mcp"]["tools_list"]["bytes"]

    lines = [
        "# Token Efficiency Benchmark",
        "",
        f"- Collected at (UTC): {output['collected_at_utc']}",
        f"- Project: `{PROJECT_PATH}`",
        "",
        "## Schema Size",
        "",
        "| Stack | Payload | Bytes | Approx tokens |",
        "| --- | --- | ---: | ---: |",
        f"| unityctl CLI | `schema --format json` | {cli_schema_bytes} | {output['data']['unityctl_cli']['schema']['approx_tokens']} |",
        f"| Unityctl.Mcp | `tools/list` | {unityctl_mcp_schema_bytes} | {output['data']['unityctl_mcp']['tools_list']['approx_tokens']} |",
        f"| CoplayDev MCP | `tools/list` | {coplaydev_schema_bytes} | {output['data']['coplaydev_mcp']['tools_list']['approx_tokens']} |",
        "",
        "## Single Roundtrip",
        "",
        "| Stack | Intent | Request bytes | Response bytes | Total bytes | Approx tokens |",
        "| --- | --- | ---: | ---: | ---: | ---: |",
    ]

    for stack_key, label in (
        ("unityctl_cli", "unityctl CLI"),
        ("unityctl_mcp", "Unityctl.Mcp"),
    ):
        for intent_key, intent_label in (("status", "status"), ("build_dry_run", "build dry-run")):
            if stack_key == "unityctl_cli":
                request_bytes = output["data"][stack_key]["requests"][intent_key]["request"]["bytes"]
                response_bytes = output["data"][stack_key]["requests"][intent_key]["response"]["bytes"]
            else:
                request_bytes = output["data"][stack_key][intent_key]["request"]["bytes"]
                response_bytes = output["data"][stack_key][intent_key]["response"]["bytes"]
            total_bytes = request_bytes + response_bytes
            lines.append(
                f"| {label} | {intent_label} | {request_bytes} | {response_bytes} | {total_bytes} | {approx_tokens('x' * total_bytes)} |"
            )

    lines += [
        "",
        "## Ten Repeated Operations",
        "",
        "| Stack | Intent | Schema once + 10x request/response bytes | Approx tokens |",
        "| --- | --- | ---: | ---: |",
        f"| unityctl CLI | status | {output['data']['unityctl_cli']['ten_run_totals']['status']['bytes']} | {output['data']['unityctl_cli']['ten_run_totals']['status']['approx_tokens']} |",
        f"| unityctl CLI | build dry-run | {output['data']['unityctl_cli']['ten_run_totals']['build_dry_run']['bytes']} | {output['data']['unityctl_cli']['ten_run_totals']['build_dry_run']['approx_tokens']} |",
        f"| Unityctl.Mcp | status | {output['data']['unityctl_mcp']['ten_run_totals']['status']['bytes']} | {output['data']['unityctl_mcp']['ten_run_totals']['status']['approx_tokens']} |",
        f"| Unityctl.Mcp | build dry-run | {output['data']['unityctl_mcp']['ten_run_totals']['build_dry_run']['bytes']} | {output['data']['unityctl_mcp']['ten_run_totals']['build_dry_run']['approx_tokens']} |",
        "",
        "## CoplayDev Build Capability",
        "",
        f"- Direct build tool present in `tools/list`: `{output['data']['coplaydev_mcp']['build_capability']['has_direct_build_tool']}`",
        f"- Matching tool names: `{', '.join(output['data']['coplaydev_mcp']['build_capability']['matching_tools']) if output['data']['coplaydev_mcp']['build_capability']['matching_tools'] else 'none'}`",
        "",
        "## Files",
        "",
        f"- Raw JSON: `{json_path.name}`",
    ]

    md_path.write_text("\n".join(lines), encoding="utf-8")
    print(str(json_path))
    print(str(md_path))


if __name__ == "__main__":
    asyncio.run(main())
