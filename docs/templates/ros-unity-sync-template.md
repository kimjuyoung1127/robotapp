# ROS-Unity Sync Template

## Purpose

ROS와 Unity 연동 작업을 협업자끼리 빠르게 동기화하기 위한 아주 짧은 템플릿입니다.

- 길게 쓰지 않아도 됩니다.
- 모르면 `모름`이라고 적으면 됩니다.
- 한 번에 완벽하게 쓰는 것보다, 매일 짧게 남기는 것이 더 중요합니다.

## Rule

1. 한 섹션당 1~3줄이면 충분합니다.
2. 파일 경로와 명령어는 가능한 한 그대로 적습니다.
3. 직접 확인한 것과 추측한 것을 섞지 않습니다.
4. 막힌 점이 있으면 원인 추측보다 `재현 방법`을 먼저 적습니다.

---

## Copy-Paste Version

```md
# ROS-Unity Sync

## Date
- YYYY-MM-DD HH:mm

## Owner
- 이름

## Goal Today
- 오늘 하려던 일 1줄

## Current Status
- 지금 되는 것:
- 지금 안 되는 것:

## Changed Files
- [absolute path]
- [absolute path]

## Commands Run
- `command 1`
- `command 2`

## Result
- 확인된 사실 1
- 확인된 사실 2

## Blocker
- 지금 막힌 문제 1줄

## Repro
1. 무엇을 켠다
2. 무엇을 누른다
3. 어떤 에러/증상이 나온다

## Next Person Do This
1. 바로 다음으로 해볼 것
2. 확인할 로그/파일/토픽
3. 성공 기준

## Open Questions
- 아직 모르는 점

## Evidence
- 스크린샷:
- 로그:
- json:
```

---

## Short Example

```md
# ROS-Unity Sync

## Date
- 2026-04-02 14:30

## Owner
- Jason

## Goal Today
- Unity에서 ROS topic 구독 후 robot joint 값 반영 확인

## Current Status
- 지금 되는 것: ROS bridge 연결, topic publish
- 지금 안 되는 것: Unity joint transform이 실제로 안 움직임

## Changed Files
- `C:\repo\Assets\Scripts\App\Ros\RosJointSubscriber.cs`
- `C:\repo\Assets\Scenes\RobotControlV2.unity`

## Commands Run
- `ros2 topic list`
- `ros2 topic echo /joint_states`

## Result
- `/joint_states` 데이터는 들어온다
- Unity console에는 callback 로그가 찍힌다
- scene joint transform 반영은 안 된다

## Blocker
- callback 이후 transform 적용 경로가 끊긴 것 같음

## Repro
1. ROS bridge 실행
2. Unity play
3. `/joint_states` publish
4. console log는 보이는데 robot mesh는 그대로

## Next Person Do This
1. callback 이후 joint index mapping 확인
2. transform target reference null 여부 확인
3. play mode에서 scene object binding 확인

## Open Questions
- ROS joint order와 Unity joint order가 같은가

## Evidence
- 스크린샷: `C:\...\robot-not-moving.png`
- 로그: `C:\...\unity-console.txt`
- json: `C:\...\ros-joint-sample.json`
```

---

## What Good Looks Like

- 오늘 목표가 1줄로 보인다
- 지금 되는 것 / 안 되는 것이 분리돼 있다
- 다음 사람이 바로 재현할 수 있다
- 다음 행동이 3개 이하로 정리돼 있다
