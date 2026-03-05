# PlayMode Debug Checklist

## 1) 런너 상태
1. `tests_running` 잠금 여부 확인
2. 잠금 시 Unity 재시작 후 재실행

## 2) 입력 시스템
1. `EventSystem` 입력 모듈 확인
2. Input System 프로젝트에서는 `InputSystemUIInputModule` 사용
3. `StandaloneInputModule` 사용 시 Input 예외 로그 발생 가능

## 3) UI/씬 배선
1. 테스트가 참조하는 UI 오브젝트 존재 여부 확인
2. `RectTransform`/`Button`/`Image` 등 필수 컴포넌트 존재 확인
3. `TooltipRoot` 같은 UI 루트는 UI 타입(`RectTransform`) 유지

## 4) 테스트 자산
1. PlayMode asmdef 분리 여부 확인
2. asmdef가 NUnit/Unity Test Runner 참조를 올바르게 포함하는지 확인
3. 실패 케이스는 재현 가능한 단일 테스트로 축소 후 해결

## 5) 완료 게이트
1. PlayMode 결과 수집 (passed/failed/skipped)
2. Console 에러 0 확인 (MCP 시스템 로그 제외)
3. 상태 문서 동기화
