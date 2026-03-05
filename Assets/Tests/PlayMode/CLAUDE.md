# PlayMode/

Play 모드에서 씬 컨텍스트와 함께 실행하는 통합 테스트.

## 컨벤션
1. 용도: 검증 스모크 테스트, UI 상호작용 테스트
2. `[UnityTest]` 어트리뷰트 + `yield return` 사용
3. 필요 시 Main.unity 씬 로드
4. Assembly Definition: `KineTutor3D.Tests.PlayMode.asmdef`
