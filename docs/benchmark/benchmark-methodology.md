# Token Efficiency Benchmark — Methodology

## 목적

Claude Code 세션에서 동일한 KineTutor3D QA 작업을 수행할 때,
**unity-cli (Bash)** vs **Unityctl.Mcp** vs **CoplayDev MCP** 3개 스택의
누적 토큰 소모량 차이를 정량화한다.

## 측정 축 3개

```
총 토큰 = (A) 스키마 고정비용 + (B) 호출 요청 × N + (C) 응답 결과 × N
```

### (A) 스키마 고정비용 — 세션당 1회

| 스택 | 측정 방법 | Claude Code에서의 실제 비용 |
|------|-----------|---------------------------|
| unity-cli (Bash) | `unityctl schema --format json` bytes | **0 추가** — Bash 도구는 시스템 프롬프트에 이미 포함 |
| Unityctl.Mcp | `tools/list` JSON bytes | deferred tool fetch 시 주입 |
| CoplayDev MCP | `tools/list` JSON bytes | deferred tool fetch 시 주입 |

### (B) 호출 요청 — 작업당

| 스택 | 형식 |
|------|------|
| unity-cli (Bash) | `{"command": "unityctl ...", "description": "..."}` |
| Unityctl.Mcp | `{"name": "tool_name", "arguments": {...}}` |
| CoplayDev MCP | `{"name": "tool_name", "arguments": {...}}` |

### (C) 응답 결과 — 작업당

동일 작업의 stdout/JSON 응답 크기를 비교.

## 토큰 환산 공식

```
approx_tokens = ceil(utf8_bytes / 4)
```

Claude의 실제 토크나이저(cl100k_base 계열)는 로컬에서 사용 불가하므로,
UTF-8 바이트 / 4 휴리스틱을 사용한다. 기존 벤치마크와 동일한 방식.

## 읽기 전용 작업 5개

모든 작업은 **읽기 전용**이며 프로젝트 상태를 변경하지 않는다.

| # | 작업 | unity-cli 도구명 | 설명 |
|---|------|-----------------|------|
| A | 컴파일 체크 | `compile_check_tool` | 컴파일 에러/경고 카운트 |
| B | 씬 계층 조회 | `scene_hierarchy_tool` | 씬 GameObject 트리 (depth=2) |
| C | 로봇 카탈로그 | `robot_catalog_tool` | 등록된 로봇 목록 |
| D | DH 테이블 | `dh_table_tool` | 2DOF_RR DH 파라미터 |
| E | 빌드 설정 | `build_settings_tool` | Build Settings 검증 |

## 안전성

| 항목 | 보장 |
|------|------|
| Assets/ 변경 | 없음 |
| 씬 변경 | 없음 |
| PlayerPrefs 변경 | 없음 |
| Build 실행 | 없음 (dry-run/읽기만) |
| Git 변경 | 없음 (결과 파일은 untracked) |

## 파일 구조

```
docs/benchmark/
  benchmark-methodology.md                      ← 이 문서
  measure_claude_code_tokens.py                 ← 토큰 측정 Python 스크립트
  run-claude-code-token-benchmark.ps1           ← PowerShell 실행기
  2026-03-20-claude-code-token-benchmark.json   ← 생 데이터 (자동 생성)
  2026-03-20-claude-code-token-benchmark.md     ← 요약 리포트 (자동 생성)
```

## 실행 전제조건

1. Unity Editor가 robotapp2 프로젝트를 열고 있어야 함
2. `unityctl.exe`와 `unityctl-mcp.exe`가 publish 경로에 존재
3. Python 3.11+ (fastmcp 의존)
4. CoplayDev MCP 서버가 포트 6400에서 실행 중 (선택사항)
