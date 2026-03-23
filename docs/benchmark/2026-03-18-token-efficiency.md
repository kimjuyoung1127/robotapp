# Token Efficiency Benchmark

- Collected at (UTC): 2026-03-18T06:42:09.2468760+00:00
- Project: `C:/Users/ezen601/Desktop/Jason/robotapp2`

## Schema Size

| Stack | Payload | Bytes | Approx tokens |
| --- | --- | ---: | ---: |
| unityctl CLI | `schema --format json` | 11927 | 2982 |
| Unityctl.Mcp | `tools/list` | 5024 | 1256 |
| CoplayDev MCP | `tools/list` | 45705 | 11427 |

## Single Roundtrip

| Stack | Intent | Request bytes | Response bytes | Total bytes | Approx tokens |
| --- | --- | ---: | ---: | ---: | ---: |
| unityctl CLI | status | 133 | 348 | 481 | 121 |
| unityctl CLI | build dry-run | 171 | 4429 | 4600 | 1150 |
| Unityctl.Mcp | status | 106 | 361 | 467 | 117 |
| Unityctl.Mcp | build dry-run | 137 | 4571 | 4708 | 1177 |

## Ten Repeated Operations

| Stack | Intent | Schema once + 10x request/response bytes | Approx tokens |
| --- | --- | ---: | ---: |
| unityctl CLI | status | 16737 | 4185 |
| unityctl CLI | build dry-run | 57927 | 14482 |
| Unityctl.Mcp | status | 9694 | 2424 |
| Unityctl.Mcp | build dry-run | 52104 | 13026 |

## CoplayDev Build Capability

- Direct build tool present in `tools/list`: `False`
- Matching tool names: `none`

## Files

- Raw JSON: `2026-03-18-token-efficiency.json`