#!/bin/bash
# session-context.sh
# Claude Code SessionStart 훅 — 세션 시작/압축 시 핵심 코딩 규칙을 컨텍스트에 주입.
# stdout으로 출력된 내용이 자동으로 Claude 컨텍스트에 추가됩니다.

cat << 'RULES'
## KineTutor3D 코딩 규칙 리마인더 (code-patterns.md 요약)

### 파일 인프라 (§8.1, §9)
- 인코딩: UTF-8 with BOM (`EF BB BF`)
- 1행 헤더: `// Folder: {Module} - {설명}`
- XML doc: 한국어 `<summary>`

### 네이밍 (§8.2)
- private 필드: `camelCase` (접두사 없음)
- 금지: `_` 접두사, `m_` 헝가리안, `s_` 정적 접두사

### MonoBehaviour 수명주기 (§8.4-8.6)
- 초기화: `Awake` + `OnEnable` → `EnsurePresentation()` + `BindListeners()`
- 정리: `OnDisable` → `UnbindListeners()`
- 중복 방지: `listenersBound` 플래그 필수
- 금지: `Init()`, `Setup()`, `BindButtons()`, `buttonsBound`

### 순수 C# 모듈 (§1-7)
- Math/Types/Kinematics: `using UnityEngine` 금지, `double` 전용
- 생성자: NaN/Infinity 가드 필수
- 테스트: `Assert.AreEqual(expected, actual, delta)` — delta 필수

### 검증
- 커밋 전: `scripts/pre-commit-check.sh` 실행
- 전체 파일 스캔: `scripts/pre-commit-check.sh --all`
RULES

exit 0
