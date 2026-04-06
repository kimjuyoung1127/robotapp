# FR5 V2 Open Questions

## Purpose

공식 근거 또는 실기 검증이 아직 부족한 항목만 따로 적는다.

- 기존 문서에 섞어 적지 않는다.
- 해결 전까지 SSOT를 강하게 잠그지 않는다.

## Current Questions

1. FAIRINO 공식 문서 기준으로 `현재 joint/TCP 읽기`의 권장 API와 polling 전략은 무엇인가.
2. `Sync current pose`를 별도 명령으로 둘지, `Read actual pose -> state overwrite` 조합으로 볼지 공식 기준이 있는가.
3. manual / auto / drag teach 전환 시 `MoveJ`, `MoveL`, jog, point record의 선행조건은 정확히 무엇인가.
4. FAIRINO 공식 teaching point 개념이 우리 `remember current pose / save point / run loop` 요구와 얼마나 직접 매핑되는가.
5. C# SDK 공개 릴리즈 버전과 manual 버전 차이에서 실제 메서드명 드리프트가 있는가.
6. `Joint arrow jog`를 공식 API로 어떻게 구현하는 것이 안전한가.
7. `TCP increment jog`를 공식 기준으로 어떤 좌표계 옵션까지 제공해야 하는가.
8. `Stop` 이후 복구 시 권장 순서가 `Enable -> Sync -> Retry`인지, 다른 공식 절차가 있는가.
9. `ghost preview / floor grid / path estimate` 중 어떤 항목을 V1 실기 bring-up 전에도 필수로 볼지 제품 기준을 어디까지 잠글 것인가.
10. 시중 teaching pad reference 중에서 UI만 참고하고 기능은 가져오지 않을 항목은 무엇인가.

## Resolved This Turn

- `gripper TCP`의 작업점은 `그리퍼 사이 정중앙 공중점`으로 잠겼다.
- 이 항목은 더 이상 open question이 아니다.

## Resolution Rule

- 공식 링크가 확보되면 `fr5-official-source-register.md`에 먼저 등록한다.
- 명령 규격이 정리되면 `fr5-command-ssot.md`로 옮긴다.
- UI 범위가 정리되면 `fr5-ui-capability-map.md`로 옮긴다.
- 실기 확인이 끝나면 `fr5-live-validation-plan.md`에 결과를 남긴다.
