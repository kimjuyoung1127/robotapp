# RobotControl FR5 V2 Doc Lane

## Purpose

이 폴더는 `FR5 실기 연동 + RobotControl V2` 기준선을 기존 문서와 섞이지 않게 잠그기 위한 전용 문서 lane이다.

- 기존 root canonical 문서(`PRD`, `WIREFRAME`, `PRODUCT-ROADMAP`)를 직접 오염시키지 않는다.
- 기존 `docs/ref/product/ux/robotcontrol-*` 문서는 reference로만 읽고, 새 결정은 여기서 먼저 잠근다.
- 공식 자료 검증이 끝난 항목만 나중에 기존 leaf 문서로 승격한다.

## Isolation Rules

1. 이 폴더의 문서는 `quarantine lane`이다.
2. 여기서 잠근 내용은 곧바로 기존 `robotcontrol-*` 문서에 합치지 않는다.
3. 기존 문서의 표현을 그대로 복사하지 않고, `carry-over note`가 있는 항목만 가져온다.
4. 모든 결정에는 아래 `Evidence Class` 중 하나 이상을 붙인다.
5. `official-doc` 또는 `official-sdk` 근거가 없는 실기 기능은 `locked`로 올리지 않는다.
6. `field-verified`가 없는 실기 동작은 `live-ready`로 표기하지 않는다.

## Evidence Class

- `official-doc`
  - FAIRINO 공식 매뉴얼 / 공식 다운로드 문서
- `official-sdk`
  - FAIRINO 공식 SDK 저장소 / 공식 샘플 / 공개 릴리즈
- `official-ui-ref`
  - 제조사 teaching pad 공식 문서
- `competitive-official-ref`
  - UR / Doosan 등 타사 공식 teaching pad 문서
- `field-verified`
  - 실제 FR5 장비에서 검증 완료
- `product-decision`
  - 우리 제품 UX/안전 정책으로 별도 잠금

## Status Key

- `candidate`
  - 아이디어는 있으나 아직 공식 근거가 부족하다.
- `researching`
  - 공식 자료 또는 SDK 확인 중이다.
- `locked-doc`
  - 공식 문서/SDK 기준으로 잠겼다.
- `locked-product`
  - 공식 근거 위에 우리 제품 정책까지 잠겼다.
- `field-needed`
  - 문서상 정의는 가능하지만 실기 검증이 남아 있다.
- `field-verified`
  - 실기 검증까지 끝났다.

## Read Order

1. [fr5-official-source-register.md](./fr5-official-source-register.md)
2. [fr5-gripper-tcp-calibration-spec.md](./fr5-gripper-tcp-calibration-spec.md)
3. [fr5-command-ssot.md](./fr5-command-ssot.md)
4. [fr5-ui-capability-map.md](./fr5-ui-capability-map.md)
5. [fr5-live-capture-checklist.md](./fr5-live-capture-checklist.md)
6. [fr5-live-validation-plan.md](./fr5-live-validation-plan.md)
7. [open-questions.md](./open-questions.md)

## Promotion Rule

아래 조건을 모두 만족할 때만 기존 leaf 문서로 승격한다.

1. `official-doc` 또는 `official-sdk` 링크가 적혀 있다.
2. `command / state / ui / validation` 중 어느 층의 결정인지 분류되어 있다.
3. 기존 문서와 충돌할 때 어느 쪽을 대체하는지 명시되어 있다.
4. 실기 기능이면 `field-needed` 또는 `field-verified` 상태가 적혀 있다.

## Current Position

- 기존 SimMachine 스크린샷 기반 V2 시안은 `UI reference`로만 취급한다.
- 실기 연동 기능의 진실값은 FAIRINO 공식 문서와 SDK에서 다시 잠근다.
- UR / Doosan 등의 teaching pad는 `UI 보강 reference`로만 사용하고, FAIRINO 기능 계약을 대체하지 않는다.

## Reference Only

- [robotcontrol-soft-teaching-pad.md](../ux/robotcontrol-soft-teaching-pad.md)
- [robotcontrol-implementation-bridge.md](../ux/robotcontrol-implementation-bridge.md)
- [robotcontrol-v2-naming-ssot.md](../ux/robotcontrol-v2-naming-ssot.md)
- [fairino-fr5-integration-reference.md](../robots/fairino-fr5-integration-reference.md)
- [fairino-teaching-pad-feature-matrix.md](../robots/fairino-teaching-pad-feature-matrix.md)
