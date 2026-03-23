---
name: robot-control-add
description: "멀티로봇 RobotControl 풀 스택 추가 — Template, Mock Client, Presets, TemplateDefinition, Factory 등록, Catalog 등록, Preview 포즈, EditMode 테스트"
---

## Trigger
새 로봇의 RobotControl 풀 스택 (Demo + Sandbox + RobotControl)을 추가할 때.
DH 파라미터와 메시가 확보된 상태에서 코드 측 전체 등록이 필요한 경우.

## Input Context
- 로봇 ID (예: "UR5e", "DOOSAN_M1013", "MECA500")
- 벤더명 (예: "UniversalRobots", "Doosan", "Mecademic")
- DH 파라미터 (Standard DH: theta, d, a, alpha per joint)
- 관절 제한 (라디안)
- 기본 포즈 (Home, Ready, Folded — 도 단위)
- Mock 초기 포즈 (도 단위)
- SDK 연결 정보 (IP, Port)

## Read First
1. `Assets/Scripts/Templates/TemplateFAIRINO_FR5.cs` — 최초 참조 템플릿
2. `Assets/Scripts/Templates/TemplateUR5e.cs` — 최신 템플릿 패턴
3. `Assets/Scripts/Templates/RobotCatalog.cs` — 카탈로그 등록 패턴
4. `Assets/Scripts/App/UniversalRobots/UR5eRobotControlTemplateDefinition.cs` — 최신 TemplateDefinition
5. `Assets/Scripts/App/UniversalRobots/MockUR5eClient.cs` — 최신 Mock 패턴
6. `Assets/Scripts/App/UniversalRobots/UR5ePosePresets.cs` — 프리셋 패턴
7. `Assets/Scripts/App/RobotControlFactory.cs` — Factory case 추가 위치
8. `Assets/Scripts/Visualization/RobotPreviewFactory.cs` — showroom 프리뷰 포즈
9. `Assets/Tests/EditMode/TemplateUR5eTests.cs` — 테스트 패턴
10. `docs/ref/code-patterns.md` — C# 코딩 패턴 (§8-9)

## Do
1. **Template 생성**: `Assets/Scripts/Templates/Template{RobotId}.cs`
   - `KineTutor3D.Templates` 네임스페이스
   - `public static class Template{RobotId}` + `public const string Name`
   - DHLink 배열 + JointLimit 배열 + `Create()` 팩토리
   - `using UnityEngine` 금지, `double` 전용
2. **카탈로그 등록**: `RobotCatalog.cs`에 `Register()` 호출 추가
   - RobotId, DisplayName, Dof, DifficultyRating
   - LibraryInteractionMode: SandboxSupported + RobotControlSupported
   - TemplateFactory 연결
   - RobotLibraryOrder 배열에 추가
3. **Mock Client**: `Assets/Scripts/App/{Vendor}/Mock{Vendor}Client.cs`
   - `IFairinoRobotClient` 인터페이스 구현
   - 로봇별 초기 포즈 설정
   - `MockFairinoClient`/`MockUR5eClient` 패턴 동일하게 따름
4. **Pose Presets**: `Assets/Scripts/App/{Vendor}/{RobotId}PosePresets.cs`
   - Home, Ready, Folded, Current (Mutable)
   - `FR5PosePresets`/`UR5ePosePresets` 패턴
5. **TemplateDefinition**: `Assets/Scripts/App/{Vendor}/{RobotId}RobotControlTemplateDefinition.cs`
   - RobotId, DisplayName, prefab 경로, JointCount, ConfigResourceName
   - RuntimeRootName, ControlRobotInstanceName
   - KinematicsFactory: `new RobotKinematicsFacade(Template{RobotId}.Create())`
   - ConnectionServiceFactory: `new FairinoConnectionService(new Mock{Vendor}Client(), translator)`
   - FallbackConfigFactory: IP, Port, 관절 제한, 속도 프리셋
6. **Factory 등록**: `RobotControlFactory.cs`에 `case "{RobotId}"` 추가
7. **Preview 포즈**: `RobotPreviewFactory.cs`에 showroom 포즈 추가
8. **EditMode 테스트**: `Assets/Tests/EditMode/Template{RobotId}Tests.cs`
   - DH 파라미터 검증 (각 링크별)
   - 관절 타입 검증 (전부 Revolute)
   - 관절 제한 검증
   - 카탈로그 등록 검증
   - Template 생성 검증

## Do Not
- 기존 로봇(FR5, UR5e 등) 파일 수정 (Factory, Catalog, PreviewFactory 제외)
- `_` 접두사 private 필드 사용
- Templates/ 모듈에서 `using UnityEngine`
- 테스트에서 delta 없는 float 비교
- 에모지 사용

## Validation
- `uc.sh check` 컴파일 0 에러
- EditMode 테스트 전부 통과 (기존 실패 수 증가 없음)
- `RobotCatalog.TryGet("{RobotId}")` 성공 (테스트로 검증)
- `RobotControlFactory.Create("{RobotId}")` 정상 반환

## Output Template
```
✅ {RobotId} RobotControl 풀 스택 추가 완료
- Template: Assets/Scripts/Templates/Template{RobotId}.cs (DOF={N})
- Mock: Assets/Scripts/App/{Vendor}/Mock{Vendor}Client.cs
- Presets: Assets/Scripts/App/{Vendor}/{RobotId}PosePresets.cs
- Definition: Assets/Scripts/App/{Vendor}/{RobotId}RobotControlTemplateDefinition.cs
- Factory: case "{RobotId}" 추가
- Catalog: {DisplayName} 등록 (Sandbox+RobotControl)
- Tests: {N}개 EditMode 테스트
```
