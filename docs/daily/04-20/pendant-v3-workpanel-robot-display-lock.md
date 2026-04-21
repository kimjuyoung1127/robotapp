# Pendant V3 WorkPanel Robot Display Lock

## Summary

- 현재 하이라이트된 `WorkPanel`을 V3 메인 로봇 표시 패널로 확정했다.
- `ViewportHost`는 메인 디지털 트윈 패널이 아니라 보조/유틸 영역으로 후퇴시켰다.
- 로봇은 `WorkPanel` 안에서 `상단 RobotStage + 하단 ControlDock` 구조로 표시하기로 잠갔다.

## Locked Decisions

1. `WorkPanel`이 로봇 표시 핵심 패널이다.
2. `RobotStage`는 `WorkPanel` 상단 62%를 차지한다.
3. `ControlDock`는 `WorkPanel` 하단 38%를 차지한다.
4. `관절 / 쉬운조작 / TCP / 포인트 이동`은 같은 `RobotStage`를 공유하고, 하단 조작 UI만 갈아낀다.
5. `Base축 / Tool축 / 궤적 / 고스트 / 카메라 리셋 / TCP 3D 화살표`는 메인 `RobotStage`를 침범하지 않고 별도 `ViewportHost` 보조 패널로 둔다.
6. `ViewportHost`는 1차 구현에서 제거하지 않더라도 메인 로봇 표시 책임을 맡지 않는다.

## Why This Layout

- 사용자가 이미 "큰 작업 패널이 우리가 작업할 패널"이라고 확정했다.
- 탭을 바꿔도 로봇이 같은 위치에 남아 있어야 조작 문맥이 끊기지 않는다.
- 별도 `ViewportHost`에 로봇을 두면 다시 "로봇 패널 / 조작 패널" 경계 혼선이 생긴다.
- 그래서 로봇은 `WorkPanel` 안에만 두고, 보조 토글과 축 패널만 `ViewportHost`로 분리하는 방향으로 정리했다.

## Next Execution Unit

1. `WorkPanelBody`를 `RobotStageHost`와 `ControlDockHost`로 재구성
2. 기존 `Visualization/` 카메라 출력 기준 rect를 `RobotStageHost`로 옮김
3. 탭별 조작 패널은 `ControlDockHost`에만 렌더
4. `ViewportHost`는 남겨두더라도 메인 메시 표시를 끄고 보조 패널 역할만 유지

## Verification Intent

- `WorkPanel` 안에서 로봇이 항상 같은 위치에 보이는지
- 탭 전환 시 로봇은 유지되고 하단 조작만 바뀌는지
- `ViewportHost`가 비어 있어도 사용자가 메인 로봇 패널을 헷갈리지 않는지
