# Claude Code Token Efficiency Benchmark

- Collected at (UTC): 2026-03-20T10:35:10.443293+00:00
- Project: `C:/Users/ezen601/Desktop/Jason/robotapp2`

## 1. Schema Fixed Cost (per session)

| Stack | Context | Bytes | Approx Tokens | Claude Code 추가비용 |
| --- | --- | ---: | ---: | --- |
| unity-cli (Bash) | CLI schema (참고용) | 11927 | 2982 | **0** (Bash 도구 이미 로드됨) |
| Bash tool itself | 시스템 프롬프트 포함 | 384 | 96 | 공유 (다른 도구와 공용) |
| Unityctl.Mcp | tools/list | 5024 | 1256 | +1256 tok (deferred fetch) |
| CoplayDev MCP | tools/list | 45705 | 11427 | +11427 tok (deferred fetch) |

## 2. Per-Operation Cost

| Operation | Stack | Req Bytes | Resp Bytes | Total Bytes | Approx Tokens |
| --- | --- | ---: | ---: | ---: | ---: |
| Compile Check | unity-cli (Bash) | 207 | 514 | 721 | 181 |
| Compile Check | Unityctl.Mcp | 109 | 525 | 634 | 159 |
| Compile Check | CoplayDev MCP | — | — | — | N/A |
| Scene Hierarchy (depth=2) | unity-cli (Bash) | 321 | 339 | 660 | 165 |
| Scene Hierarchy (depth=2) | Unityctl.Mcp | 223 | 344 | 567 | 142 |
| Scene Hierarchy (depth=2) | CoplayDev MCP | 62 | 2864 | 2926 | 732 |
| Robot Catalog | unity-cli (Bash) | 290 | 243 | 533 | 134 |
| Robot Catalog | Unityctl.Mcp | 192 | 248 | 440 | 110 |
| Robot Catalog | CoplayDev MCP | — | — | — | N/A |
| DH Table (2DOF_RR) | unity-cli (Bash) | 273 | 273 | 546 | 137 |
| DH Table (2DOF_RR) | Unityctl.Mcp | 175 | 278 | 453 | 114 |
| DH Table (2DOF_RR) | CoplayDev MCP | — | — | — | N/A |
| Build Settings | unity-cli (Bash) | 231 | 4429 | 4660 | 1165 |
| Build Settings | Unityctl.Mcp | 137 | 4571 | 4708 | 1177 |
| Build Settings | CoplayDev MCP | — | — | — | N/A |

## 3. Cumulative Cost (5 ops × N sessions)

| Stack | Schema (1회) | 5 ops × 1 | 5 ops × 5 | 5 ops × 10 |
| --- | ---: | ---: | ---: | ---: |
| unity-cli (Bash) | 0 B | 1780 tok | 8900 tok | 17800 tok |
| Unityctl.Mcp | 5024 B | 2957 tok | 9759 tok | 18261 tok |
| CoplayDev MCP | 45705 B | 12158 tok | 15084 tok | 18742 tok |

## 4. Key Findings

### 스키마 고정비용이 결정적 차이

| Stack | 스키마 토큰 | 비율 |
| --- | ---: | ---: |
| unity-cli (Bash) | **0** | 1x (기준) |
| Unityctl.Mcp | 1,256 | ∞ (0 대비) |
| CoplayDev MCP | 11,427 | ∞ (0 대비) |

- unity-cli는 Bash 도구를 공유하므로 **도구 스키마 추가비용이 0**
- CoplayDev MCP는 30개 도구 스키마를 모두 로드하며, 그 중 KineTutor3D 작업에 사용 가능한 것은 **1개** (scene hierarchy만 가능)
- CoplayDev 스키마(45,705 B)는 Unityctl.Mcp(5,024 B) 대비 **9.1배**

### 요청당 토큰: CLI가 약간 크지만 무시할 수준

- CLI Bash 요청은 MCP 대비 평균 ~90 bytes 더 큼 (커맨드 문자열이 길기 때문)
- 응답 크기는 거의 동일 (같은 데이터를 반환하므로)
- 작업당 차이: 약 20~25 토큰 → 10회 반복해도 200~250 토큰

### 누적 비용: 세션 길이에 따라 역전

- **단발 세션(5 ops × 1)**: unity-cli가 최소 (1,780 tok), CoplayDev가 최대 (12,158 tok) → **6.8배 차이**
- **반복 세션(5 ops × 10)**: 차이 수렴 (17,800 vs 18,742 tok) → 1.05배
- **역전 지점**: ~15회 반복 이후 CLI와 MCP의 누적비용 동등
- 이유: CLI는 스키마 0 + 매회 큰 요청, MCP는 큰 스키마 + 매회 작은 요청

### CoplayDev MCP의 한계

- 5개 작업 중 **4개** (compile check, robot catalog, DH table, build settings)에 대응 도구 없음
- 유일한 대응 작업(scene hierarchy)도 응답이 2,864 B로 CLI(339 B) 대비 **8.4배** 더 큼
- 프로젝트 특화 QA 작업에는 사실상 사용 불가

### 결론

| 시나리오 | 추천 |
| --- | --- |
| Claude Code + KineTutor3D QA | **unity-cli (Bash)** — 스키마 비용 0, 전체 26개 도구 지원 |
| 짧은 세션 (1~5 작업) | unity-cli 압도적 우위 (6.8x 토큰 절약) |
| 장시간 세션 (10+ 작업) | unity-cli 여전히 유리하나 차이 수렴 |
| 범용 Unity 조작 (씬 편집 등) | CoplayDev MCP 도구 범위 활용 가능 (단, 토큰 비용 높음) |

## Files

- Methodology: `benchmark-methodology.md`
- Raw JSON: `2026-03-20-claude-code-token-benchmark.json`
- Runner: `measure_claude_code_tokens.py`