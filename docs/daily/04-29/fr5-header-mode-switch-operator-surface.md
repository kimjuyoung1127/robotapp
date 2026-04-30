# FR5 Header Mode Switch Operator Surface

Date: 2026-04-29 (KST)

## What Changed

- `Pendant V3` 상단 헤더에 `자동` / `수동` 버튼을 추가했다.
- 이 버튼들은 기존 `ConnectionHome` 안쪽 mode 버튼과 별도 로직을 만들지 않고, 같은 runtime verified mode-transition path를 호출한다.
- current verified path는:
  - `SetMode`
  - `SyncCurrentState` retry
  - `requested/actual mode match` 확인

## Why

- 2026-04-29 field 확인 기준으로 외부 teach pendant `manual <-> auto` 토글은 이제 `latest-state.json` `mode` truth를 정상적으로 바꾼다.
- 따라서 다음 단계는 `truth readback` 자체가 아니라, 그 성공패턴을 `Pendant V3` 운영자 surface에 직접 올려서 기존 teach pendant 없이 normal case mode 전환을 닫는 것이다.

## UI Surface

- added header buttons:
  - `BtnHeaderModeAuto`
  - `BtnHeaderModeManual`
- current intent:
  - operator가 어떤 탭에 있든 mode 전환 액션을 top header에서 바로 호출
  - 기존 teach pendant 없이 app-owned auto/manual path를 normal-case bring-up flow로 승격

## Verification

- code verification:
  - `dotnet build /Users/family/jason/FR5UNITY/robotapp/robotapp.slnx -nologo`
  - result: `warnings only`, `errors 0`
- field verification already known in this chain:
  - external teach pendant `manual <-> auto` mode truth follow confirmed
- not yet field-verified in this entry:
  - header `자동` button live smoke
  - header `수동` button live smoke
  - operator-facing recovery copy sufficiency

## Next Verify

1. `Pendant V3` 상단 헤더 `자동` 버튼 클릭
2. `latest-state.json` / V3 mode surface / last transition summary 확인
3. `수동` 버튼 복귀 확인
4. failure reason과 operator hint가 충분한지 점검
