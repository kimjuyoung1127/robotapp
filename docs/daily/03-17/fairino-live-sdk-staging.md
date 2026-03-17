# 2026-03-17 — FAIRINO Live SDK Staging

## Summary
- 공식 `fairino-csharp-sdk` ZIP을 다시 확인해 실제 배포 DLL 이름을 검증했다.
- 로컬 프로젝트 `Assets/Plugins/Fairino/`에 실기 연동용 DLL을 staging 했다.
- Unity compile을 막던 기존 blocker 4건을 함께 정리했다.
- 아직 실기 컨트롤러 IP를 모르는 상태라, Live 연결 검증은 `Connect` 이전 단계에서 보류했다.

## Official SDK Check
- 공식 ZIP 안에서 확인한 실제 DLL 이름:
  - `libfairino.dll`
  - `CookComputing.XmlRpcV2.dll`
- 기존 README에 적혀 있던 `fairino.dll`, `xmlrpcnet.dll` 표기는 실제 배포 파일명과 달라 수정했다.

## Local Changes
- `Assets/Plugins/Fairino/libfairino.dll` 추가
- `Assets/Plugins/Fairino/CookComputing.XmlRpcV2.dll` 추가
- `Assets/Plugins/Fairino/README.md` 업데이트
  - 실제 배포 DLL 이름 기준으로 수정
- Unity compile blocker 수정
  - `Assets/Scripts/App/SceneCameraDirector.cs`
  - `Assets/Editor/KineTutor3D/CliTools/ConsoleCheckTool.cs`
  - `Assets/Editor/KineTutor3D/CliTools/AssetSizeTool.cs`
  - `Assets/Editor/KineTutor3D/CliTools/SceneHierarchyTool.cs`

## Verification
- 공식 SDK ZIP 내부에서 `libfairino.dll`, `CookComputing.XmlRpcV2.dll` 존재 확인
- 로컬 `Assets/Plugins/Fairino/`에 두 DLL 복사 완료
- Unity refresh 후 compile blocker 제거 확인

## Remaining Blocker
- 실기 컨트롤러 IP 미확정

## Next Live Test Order
1. Live 모드에서 `Connect`만 검증
2. `GetVersion`, `ReadState` 같은 읽기 API 검증
3. `Enable`
4. 아주 작은 범위의 `MoveJ`
