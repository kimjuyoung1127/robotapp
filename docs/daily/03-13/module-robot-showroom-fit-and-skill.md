# Robot Showroom Fit And Skill

## Summary
- `RobotLibrary` showroom이 Game 뷰에서 과도하게 작아 보이던 문제를 viewport rect 기준 RenderTexture/카메라 프레이밍으로 보정했다.
- showroom page/hero 선택 규칙을 정리해 첫 페이지 hero가 `SCARA`로 유지되고, 이전 페이지 복귀 시 `2DOF_RR`로 잘못 바뀌지 않게 수정했다.
- 반복 디버깅 포인트를 `.claude/skills/kinetutor-guide/ui/robot-showroom-debug/` 스킬로 정리했다.

## Root Cause
- RenderTexture 크기를 `Screen.width/height` 중심으로 잡아 실제 `showroomOutput` rect 비율과 어긋났다.
- `ExecuteAlways` 경로에서 `RobotShowroomRuntime`가 중복 생성될 수 있었다.
- 페이지 이동 후 hero가 페이지의 중심 로봇이 아니라 시작 로봇으로 복귀했다.

## Changes
- `RobotLibraryManager`: actual `showroomOutput` rect 기반 RT 크기/카메라 aspect/framing 적용
- `RobotShowroomManager`: page hero 규칙 및 previous/next page 복귀 로직 보정
- `RobotPreviewPod`: side pod가 Game 뷰에서 너무 작지 않도록 preview scale 상향
- `RobotLibraryManager`: `CompareStrip` 제거, 카드 클릭 시 기본 실습 경로로 즉시 이동
- `RobotLibraryManager` + `RobotPreviewPod`: showroom 3D 로봇 직접 클릭 → 해당 로봇 실습 진입 연결
- docs/skill index 업데이트

## Verification
- `dotnet build KineTutor3D.Runtime.csproj` 성공
- 잘못 쌓여 있던 `RobotShowroomRuntime` root 정리 완료
