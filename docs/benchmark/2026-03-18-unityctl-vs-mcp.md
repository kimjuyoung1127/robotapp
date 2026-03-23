# unityctl vs CoplayDev Unity MCP benchmark

- Collected at (UTC): 2026-03-18T05:59:33.4843652+00:00
- Unity project target: `C:/Users/ezen601/Desktop/Jason/robotapp2`
- CoplayDev endpoint: `127.0.0.1:6400`
- Warmup runs per operation: `2`
- Measured runs per operation: `8`

## Important caveats

- `docs/tasks/benchmark-vs-mcp.md` was not present, so this benchmark spec was inferred.
- CoplayDev was measured by direct framed TCP calls to the exposed Unity bridge on port `6400`.
- Two Unity Editor processes were open. Scene-oriented reads on both stacks sometimes resolved to `C:/Users/ezen601/Desktop/Jason/My project` instead of `robotapp2`.

## Median latency

| Operation | unityctl median ms | MCP bridge median ms | MCP/unityctl ratio |
| --- | ---: | ---: | ---: |
| ping | 2102.15 | 1.14 | 0.0005 |
| editor_state | 2104.32 | 99.18 | 0.0471 |
| active_scene | 2108.27 | 98.64 | 0.0468 |
| diagnostic | 2104.86 | 99.88 | 0.0475 |

## Notes

### ping

- Comparability: high
- Description: Basic reachability check.
- unityctl sample: message=pong; unityVersion=6000.0.64f1
- MCP sample: message=pong

### editor_state

- Comparability: high
- Description: Editor state lookup.
- unityctl sample: projectPath=C:/Users/ezen601/Desktop/Jason/robotapp2/Assets; isPlaying=False; isCompiling=False
- MCP sample: activeScene=C:/Users/ezen601/Desktop/Jason/My project/Assets/Scenes/SampleScene.unity; isPlaying=False; isCompiling=False

### active_scene

- Comparability: medium
- Description: Active scene lookup. unityctl uses exec; MCP uses manage_scene/get_active.
- unityctl sample: activeScene=C:/Users/ezen601/Desktop/Jason/My project/Assets/Scenes/SampleScene.unity
- MCP sample: activeScene=C:/Users/ezen601/Desktop/Jason/My project/Assets/Scenes/SampleScene.unity

### diagnostic

- Comparability: low
- Description: Diagnostic read. Not apples-to-apples: unityctl=check, MCP=read_console.
- unityctl sample: assemblies=44; scriptCompilationFailed=False
- MCP sample: count=5; firstLine=Invalid AssetDatabase path: C:/Users/ezen601/Desktop/Jason/My project/Assets/Scenes/SampleScene.unity. Use path relative to the project folder.

## Files

- Raw JSON: `2026-03-18-unityctl-vs-mcp.json`
- Runner: `run-unityctl-vs-mcp.ps1`
