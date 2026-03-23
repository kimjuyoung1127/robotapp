# unityctl vs CoplayDev Unity MCP benchmark (extended)

- Collected at (UTC): 2026-03-18T07:06:09.4192660+00:00
- Unity project target: `C:/Users/ezen601/Desktop/Jason/robotapp2`
- CoplayDev endpoint: `127.0.0.1:6400`
- Warmup runs per operation: `2`
- Measured runs per operation: `8`

## Important caveats

- `docs/tasks/benchmark-vs-mcp.md` was not present, so this benchmark spec was inferred.
- This rerun used a single `robotapp2` Unity session. Cross-project leakage was not observed.
- `active_scene` responses were consistent across stacks, but the active scene path was blank during measurement.
- `dotnet run` and `published exe` are per-invocation timings. `Unityctl.Mcp` is a resident-session timing.

## Median latency

| Operation | dotnet run ms | published exe ms | Unityctl.Mcp ms | CoplayDev bridge ms |
| --- | ---: | ---: | ---: | ---: |
| ping | 2015.37 | 304.42 | 99.81 | 1.08 |
| editor_state | 2095.47 | 303.24 | 100.07 | 100.14 |
| active_scene | 2094.17 | 304.26 | 99.38 | 100.16 |
| diagnostic | 2053 | 400.8 | 100.91 | 100.01 |

## Notes

### ping

- Comparability: high
- Description: Basic reachability check.
- dotnet run sample: message=pong; unityVersion=6000.0.64f1
- published exe sample: message=pong; unityVersion=6000.0.64f1
- Unityctl.Mcp sample: message=pong; unityVersion=6000.0.64f1
- CoplayDev bridge sample: message=pong

### editor_state

- Comparability: high
- Description: Editor state lookup.
- dotnet run sample: projectPath=C:/Users/ezen601/Desktop/Jason/robotapp2/Assets; isPlaying=False; isCompiling=False
- published exe sample: projectPath=C:/Users/ezen601/Desktop/Jason/robotapp2/Assets; isPlaying=False; isCompiling=False
- Unityctl.Mcp sample: projectPath=C:/Users/ezen601/Desktop/Jason/robotapp2/Assets; isPlaying=False; isCompiling=False
- CoplayDev bridge sample: activeScene=; isPlaying=False; isCompiling=False

### active_scene

- Comparability: medium
- Description: Active scene lookup. unityctl uses exec; CoplayDev uses manage_scene/get_active.
- dotnet run sample: activeScene=
- published exe sample: activeScene=
- Unityctl.Mcp sample: activeScene=
- CoplayDev bridge sample: activeScene=

### diagnostic

- Comparability: low
- Description: Diagnostic read. Not apples-to-apples: unityctl=check, CoplayDev=read_console.
- dotnet run sample: assemblies=44; scriptCompilationFailed=False
- published exe sample: assemblies=44; scriptCompilationFailed=False
- Unityctl.Mcp sample: assemblies=44; scriptCompilationFailed=False
- CoplayDev bridge sample: count=5; firstLine=[unityctl] IPC connection error: Pipe closed before full message was read.

## Files

- Raw JSON: `2026-03-18-unityctl-vs-mcp-extended.json`
- Runner: `run-unityctl-vs-mcp-extended.ps1`
- MCP helper: `bench_unityctl_mcp.py`
