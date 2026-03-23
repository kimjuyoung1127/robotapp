import argparse
import asyncio
import glob
import json
import math
import os
import statistics
import sys
import time
from pathlib import Path


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
        fastmcp_dir = os.path.join(site_packages, "fastmcp")
        if os.path.isdir(fastmcp_dir):
            candidates.append(site_packages)

    if not candidates:
        raise RuntimeError("Could not locate fastmcp in uv cache.")

    candidates.sort(reverse=True)
    sys.path.insert(0, candidates[0])


ensure_fastmcp_importable()

from fastmcp import Client  # noqa: E402
from fastmcp.client.transports import StdioTransport  # noqa: E402


def build_stats(values: list[float]) -> dict[str, float | int | None]:
    if not values:
        return {
            "runs": 0,
            "min_ms": None,
            "max_ms": None,
            "mean_ms": None,
            "median_ms": None,
            "stdev_ms": None,
        }

    return {
        "runs": len(values),
        "min_ms": round(min(values), 2),
        "max_ms": round(max(values), 2),
        "mean_ms": round(statistics.fmean(values), 2),
        "median_ms": round(statistics.median(values), 2),
        "stdev_ms": round(statistics.pstdev(values) if len(values) > 1 else 0.0, 2),
    }


async def measure_operation(client: Client, tool_name: str, arguments: dict, warmups: int, runs: int) -> dict:
    warmup_runs = []
    last_payload = None
    for index in range(1, warmups + 1):
        start = time.perf_counter()
        result = await client.call_tool(tool_name, arguments, raise_on_error=False)
        elapsed_ms = (time.perf_counter() - start) * 1000.0
        warmup_runs.append({"run": index, "ms": round(elapsed_ms, 2)})
        content_text = result.content[0].text if result.content else ""
        last_payload = {
            "text": content_text,
            "json": json.loads(content_text) if content_text else None,
            "is_error": result.is_error,
        }

    measured_runs = []
    values = []
    for index in range(1, runs + 1):
        start = time.perf_counter()
        result = await client.call_tool(tool_name, arguments, raise_on_error=False)
        elapsed_ms = (time.perf_counter() - start) * 1000.0
        elapsed_ms = round(elapsed_ms, 2)
        measured_runs.append({"run": index, "ms": elapsed_ms})
        values.append(elapsed_ms)
        content_text = result.content[0].text if result.content else ""
        last_payload = {
            "text": content_text,
            "json": json.loads(content_text) if content_text else None,
            "is_error": result.is_error,
        }

    return {
        "tool_name": tool_name,
        "warmup_runs": warmup_runs,
        "measured_runs": measured_runs,
        "stats": build_stats(values),
        "sample": last_payload,
    }


async def main() -> None:
    parser = argparse.ArgumentParser(description="Benchmark Unityctl.Mcp persistent server mode.")
    parser.add_argument("--server-exe", required=True)
    parser.add_argument("--project", required=True)
    parser.add_argument("--warmups", type=int, default=2)
    parser.add_argument("--runs", type=int, default=8)
    args = parser.parse_args()

    server_exe = Path(args.server_exe)
    if not server_exe.exists():
        raise FileNotFoundError(f"Unityctl.Mcp executable not found: {server_exe}")

    transport = StdioTransport(
        command=str(server_exe),
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
    client = Client(transport)

    operations = [
        ("ping", "unityctl_ping", {"project": args.project}),
        ("editor_state", "unityctl_status", {"project": args.project, "wait": False}),
        (
            "active_scene",
            "unityctl_exec",
            {
                "project": args.project,
                "code": "UnityEngine.SceneManagement.SceneManager.GetActiveScene().path",
            },
        ),
        ("diagnostic", "unityctl_check", {"project": args.project, "type": "compile"}),
    ]

    results = []
    async with client:
        await client.list_tools()
        for name, tool_name, tool_args in operations:
            measurement = await measure_operation(
                client,
                tool_name,
                tool_args,
                warmups=args.warmups,
                runs=args.runs,
            )
            results.append(
                {
                    "name": name,
                    "tool_name": tool_name,
                    **measurement,
                }
            )

    print(
        json.dumps(
            {
                "server_exe": str(server_exe),
                "project": args.project,
                "warmups": args.warmups,
                "runs": args.runs,
                "results": results,
            },
            ensure_ascii=False,
        )
    )


if __name__ == "__main__":
    asyncio.run(main())
