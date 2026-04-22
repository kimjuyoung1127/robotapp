# App/Fairino/Teaching

RobotControl V3의 포인트 저장, 시퀀스 실행, manual readback teaching state를 다룹니다.

## 포함 대상
- waypoint
- sequence runner
- point/program minimum state
- Unity/Mock manual readback probe
- `PendantV3Points` store adapter
- teaching sequence state DTO

## 금지
- 복잡한 Program IDE 구현
- UI 표/카드 레이아웃 처리
- 제조사 Lua/program load/run
- Mesh/capsule collision engine

## 현재 파일
- `ManualReadbackTeachingProbe.cs` — Mock에서 실기기 수동 이동 readback을 시뮬레이션하고 `FairinoConnectionService.OnStateUpdated` 경로로 흘린다.
- `TeachingPointStoreAdapter.cs` — `WaypointStore`의 `PendantV3Points` 로드/저장/요약 경계.
- `TeachingSequenceState.cs` — V3 teaching sequence 상태 요약 DTO.
