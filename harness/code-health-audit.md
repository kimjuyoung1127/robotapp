# Code Health Audit Harness

## 목적
- 에이전트의 컨텍스트 로트(Context Rot) 방지.
- 코드 품질 및 아키텍처 경계 준수 여부 정기 점검.
- 테스트 커버리지 및 컴파일 상태 유지.

## 진단 단계 (Audit Steps)

### STEP 1: 프로젝트 위생 상태 확인
```bash
# 컴파일 오류 여부 확인
unityctl check --type compile
# 최신 에러 로그 확인
unityctl console get-entries --limit 20
```

### STEP 2: 아키텍처 경계 위반 확인
- `Math`, `Kinematics`, `Types` 모듈에서 `UnityEngine` 임포트 여부 스캔.
- `double` 정밀도가 아닌 `float`이 도메인 로직에 사용되었는지 확인.

### STEP 3: 문서-코드 동기화 확인
- `AGENTS.md`의 현재 구조 설명과 실제 폴더 구조/코드 흐름이 일치하는지 대조.
- 최신 `.cs` 파일들에 `<ai_context>` 헤더가 포함되어 있는지 확인.

### STEP 4: 테스트 실행
- `EditMode` 테스트 전체 실행 (`unityctl test --mode edit`).
- 실패 시 즉시 원인 분석 및 수정 루프 진입.

## 회복 프로토콜 (Recovery Protocol)
만약 에이전트가 같은 실수를 반복하거나 품질이 떨어진다고 판단되면:
1. `/compact` 명령을 사용해 대화 내역을 압축.
2. `harness/REGISTRY.md`와 `CLAUDE.md`를 다시 읽어 지침 재숙지.
3. 가장 최근에 성공한 `Quality Gate` 통과 지점으로 롤백 고려.
