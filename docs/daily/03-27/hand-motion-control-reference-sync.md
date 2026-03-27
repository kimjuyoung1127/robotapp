# 2026-03-27 — Hand Motion Control Reference Sync

## What Changed

- 손 움직임 기반 로봇 제어 구조를 정리한 참조 문서를 추가했다.
- 파츠 분리 / mesh 누락 이슈를 다시 볼 때 바로 열어야 할 문서와 코드 경로도 함께 묶었다.
- 무선 연결을 1차 권장 경로로 정리하고, 초기 파일럿에서 잠가야 할 안전 항목을 문서에 추가했다.
- 최종 목적지를 `Hand Teaching Mode`로 구체화하고, 폰은 거치대에 고정하는 물리 setup 결정을 문서에 반영했다.
- teaching 중심 사용자 흐름과 MVP 범위를 문서에 추가했다.
- 기존 `RobotControl`과 공존하기 위한 입력 중재층 중심 확장 방향을 문서에 추가했다.
- Step 1 성공 기준을 `Hand Input: Fresh` 신호 확인으로 명시했다.

## Added Doc

- `docs/ref/product/robots/hand-motion-control-integration-reference.md`

## Why

- 현재 `robotapp2`에는 손 추적 입력 구현은 없지만, `RobotControl`의 preview/live 훅은 이미 존재한다.
- 그래서 "무엇을 새로 만들고", "어떤 경로는 이미 있고", "TCP hand-follow는 왜 다음 단계인지"를 한 번에 보는 문서가 필요했다.
- 파츠 분리 이슈는 매번 showroom fallback과 control prefab 문제를 섞기 쉬워서, 재탐색 순서를 같이 남겼다.

## Notes

- 현재 권장 시작점은 `joint-space hand teleop`이다.
- 1차 연결은 `폰 hand tracking -> 로컬 Wi-Fi -> robotapp2 수신` 경로를 기본으로 본다.
- 최종 목표는 "손으로 계속 직접 조종"보다 "손으로 포즈를 만들고 저장"하는 teaching mode다.
- 폰은 손에 들거나 로봇에 매다는 대신 고정 거치대를 기본으로 본다.
- 기존 기능과 공존하려면 hand input은 별도 입력 소스로 추가하고, 최종적으로는 중앙 input arbitration 구조로 묶는 것이 맞다.
- `TCP hand follow`는 IK 또는 `ServoCart` 계층이 준비된 뒤 다음 단계로 본다.
- 파츠 분리 복구의 정식 기준은 `showroom preview`가 아니라 `FAIRINO_FR5_Control.prefab` 재생성/검증이다.
