# Headless Batch Validation

- Collected at (UTC): 2026-03-18T06:56:00Z
- Tool under test: `C:\Users\ezen601\Desktop\Jason\unityctl\publish\win-x64\unityctl.exe`
- Test project: `C:/Users/ezen601/Desktop/Jason/20260309`
- Unity Editor for this project: not running
- Note: `com.unityctl.bridge` was added to `20260309/Packages/manifest.json` via `unityctl init` immediately before testing.

## Results

| Scenario | Command | Result | Evidence |
| --- | --- | --- | --- |
| Headless compile check | `unityctl check --project C:/Users/ezen601/Desktop/Jason/20260309 --json` | PASS | `statusCode=0`, `assemblies=40`, flight log `durationMs=18582`, measured wall time `26847ms` |
| Headless EditMode test | `unityctl test --project C:/Users/ezen601/Desktop/Jason/20260309 --mode edit --json` | PASS | `statusCode=0`, `18 passed`, response `durationMs=2710`, flight log `durationMs=25801`, measured wall time `15409ms` |
| Headless build preflight | `unityctl build --project C:/Users/ezen601/Desktop/Jason/20260309 --target StandaloneWindows64 --dry-run --json` | FAIL | `statusCode=500`, `Unity exited with code 1073741845 but no response file was written.`, flight log `durationMs=9388` |
| Headless status | `unityctl status --project C:/Users/ezen601/Desktop/Jason/20260309 --json` | FAIL | `statusCode=500`, same crash signature, flight log `durationMs=9312` |

## Interpretation

- `unityctl` headless batch path is verified to work for at least `check` and `test --mode edit` on a closed-editor Unity project.
- `build --dry-run` and `status` do not currently succeed on this specific project in headless mode. The failure is a Unity process exit before a response file is written, so this looks project- or batch-startup-specific rather than a clean `unityctl` protocol failure.
- This run is strong evidence for the README claim that `unityctl` can execute meaningful CI-safe operations without an open Editor.
- This run is not sufficient to generalize that every headless command succeeds on every project.

## CoplayDev Status

| Scenario | CoplayDev MCP |
| --- | --- |
| Editor 없이 동작 | N/A in this session |
| Headless batch mode | N/A in this session |

- Reason: other Unity Editors were open globally, and the CoplayDev bridge in this workstation routes to live Editor instances rather than a closed-project batch path.
- Existing benchmark evidence still shows no direct build tool in CoplayDev `tools/list`, which weakens CI-style parity claims even before headless execution is considered.
