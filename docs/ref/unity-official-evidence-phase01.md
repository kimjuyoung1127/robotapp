# Unity 공식문서 근거 (Phase 0+1)

최종 업데이트: 2026-03-05 (KST)  
적용 대상: KineTutor3D Phase 0 + Phase 1

## 1) asmdef
- 결론:
  `Types`, `Math`, `Tests.EditMode`를 Assembly Definition으로 분리하고, 테스트 어셈블리는 대상 런타임 어셈블리를 명시 참조한다.
- 공식 문서 근거(링크):
  [Assembly Definitions](https://docs.unity3d.com/6000.0/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
  [Assembly references](https://docs.unity3d.com/kr/6000.0/Manual/assembly-definitions-referencing.html)
- 프로젝트 적용 규칙:
  `KineTutor3D.Math`(noEngineReferences=true) -> `KineTutor3D.Types`(Math 참조) -> `KineTutor3D.Tests.EditMode`(Math/Types 참조) DAG를 유지한다.
- 버전 차이 메모(필요 시):
  Unity 6000.0.64f1 기준 문서 우선, 2022.3 LTS에서도 asmdef 기본 개념(분리/참조)은 동일하다.

## 2) Unity Test Runner (EditMode/PlayMode)
- 결론:
  수학/타입 로직은 EditMode, 씬/UX 흐름은 PlayMode로 분리한다.
- 공식 문서 근거(링크):
  [Unity Test Framework manual](https://docs.unity3d.com/Packages/com.unity.test-framework@1.5/manual/index.html)
  [Testing in Unity](https://docs.unity3d.com/cn/2022.3/Manual/testing-editortestsrunner.html)
- 프로젝트 적용 규칙:
  Phase 1 완료 조건은 EditMode Green이며, 기존 PlayMode 스모크(5건)는 회귀 검증으로 유지한다.
- 버전 차이 메모(필요 시):
  패키지 버전/메뉴 경로 표기는 버전별로 달라질 수 있으나 EditMode/PlayMode 분리 원칙은 동일하다.

## 3) Serialization
- 결론:
  Phase 1의 `Types/Math`는 직렬화 의존성을 두지 않고 순수 C# 불변 타입으로 유지한다.
- 공식 문서 근거(링크):
  [Serialization rules](https://docs.unity3d.com/6000.0/Documentation/Manual/script-serialization.html)
- 프로젝트 적용 규칙:
  `Types/Math`에서 `UnityEngine` 참조를 금지하고, 직렬화가 필요한 데이터는 UI/ScriptableObject 계층으로 분리한다.
- 버전 차이 메모(필요 시):
  Unity 직렬화 지원 타입 목록은 버전별로 세부 차이가 있어도, `Phase 1 순수 수학 계층 분리` 결정에는 영향이 없다.

## 4) Script Compilation
- 결론:
  스크립트 컴파일 경계를 asmdef로 고정해 불필요한 재컴파일을 줄이고 모듈 의존성을 명확히 한다.
- 공식 문서 근거(링크):
  [Script compilation](https://docs.unity3d.com/6000.0/Documentation/Manual/script-compilation.html)
- 프로젝트 적용 규칙:
  새 모듈 추가 시 asmdef 의존성 그래프와 테스트 어셈블리 참조를 함께 갱신한다.
- 버전 차이 메모(필요 시):
  Unity 버전에 따라 컴파일 파이프라인 내부 동작은 다를 수 있으나 asmdef 기반 분리 전략은 공통 적용 가능하다.

## 5) API Compatibility
- 결론:
  API Compatibility Level은 프로젝트 단위 정책으로 고정하고, Phase 1 코드는 특정 UnityEngine API에 의존하지 않게 유지한다.
- 공식 문서 근거(링크):
  [API Compatibility Level](https://docs.unity3d.com/6000.0/Documentation/Manual/dotnet-profile-support.html)
- 프로젝트 적용 규칙:
  `Types/Math`는 BCL 수준(`System`)만 사용하고, 플랫폼/API 차이는 App/UI 계층에서만 다룬다.
- 버전 차이 메모(필요 시):
  Unity 6/2022.3 간 API 프로필 선택 UI가 달라도, 수학 계층의 순수 C# 유지 원칙은 동일하다.
