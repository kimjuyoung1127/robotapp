# Asset Hierarchy Normalization

## Summary
- `Assets/realvirtual`은 유지하고, 나머지 외부 vendor/source 폴더를 `Assets/Vendors/Archive/` 아래로 재배치했다.
- curated runtime 자산은 `Assets/Runtime/Art`, `Assets/Runtime/Prefabs`, `Assets/Runtime/Resources`로 통합했다.
- 코드/문서/테스트의 절대 경로 참조를 새 구조에 맞춰 정리했다.

## Moved
- `Assets/_Heathen Engineering` -> `Assets/Vendors/Archive/HeathenEngineering`
- `Assets/Glowing Rifts` -> `Assets/Vendors/Archive/GlowingRifts`
- `Assets/HQP Studios` -> `Assets/Vendors/Archive/HQPStudios`
- `Assets/DemoRealvirtual` -> `Assets/Vendors/Archive/DemoRealvirtual`
- `Assets/Art` -> `Assets/Runtime/Art`
- `Assets/Prefabs` -> `Assets/Runtime/Prefabs`
- `Assets/Resources` -> `Assets/Runtime/Resources`

## Follow-up Code Updates
- `Assets/Editor/KineTutor3D/UxDataSeeder.cs`
- `Assets/Scripts/Visualization/TargetMarkerVisual.cs`
- `Assets/Tests/EditMode/TargetMarkerAssetResolutionTests.cs`
- 관련 asset path 문서들

## Verification
- `dotnet build KineTutor3D.Runtime.csproj` 성공
- Unity refresh/compile 요청 성공
- 새 경로 존재 확인, 기존 루트 경로 제거 확인
- `pre-commit-check.sh --all` 실행 결과:
  - 실패 (`BLOCKED: 303 에러`)
  - 원인: 저장소 전반의 기존 BOM/헤더/UTF-8 규칙 위반
  - 판단: 이번 폴더 이동 자체보다는 기존 코드베이스 품질 규칙 이슈
