# RobotControl 페이지 기준으로 본 FAIRINO FR5 공식문서 -> Unity 반영 흐름

## 요약

`RobotControl` 페이지에 한해서 보면, FAIRINO FR5를 Unity로 가져온 방식은 "공식문서를 그대로 넣은 것"이 아니라, 공식문서를 기준으로 아래 3가지를 `RobotControl`에 맞게 재구성한 흐름이다.

1. FR5 제어용 3D 계층
2. FR5 기구학(FK) 계산 구조
3. FR5 실기 연결용 SDK 구조

즉, 공식문서는 기준이었고, `RobotControl` 페이지는 그 기준을 바탕으로 실제 조작 가능한 화면과 제어 계층을 만든 결과물이다.

## 1. 공식문서에서 먼저 확인한 것

`RobotControl` 페이지를 만들기 위해 먼저 공식문서에서 아래 내용을 확인했다.

- FR5 기본 사양
- 설치 조건과 load curve
- DH 파라미터
- C# SDK 연결 방식
- 상태 조회 API
- MoveJ, MoveL, ServoJ 같은 모션 API
- 상태 피드백/프로토콜 문서

이 단계의 목적은 "RobotControl 페이지가 어떤 로봇을 어떤 방식으로 제어해야 하는가"를 공식 기준으로 고정하는 것이었다.

## 2. RobotControl용 3D는 URDF에서 시작했다

`RobotControl` 기준 실제 3D 자산의 출발점은 `URDF`였다.

프로젝트 안의 기준 파일은 아래 경로다.

- `Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf`

에디터 메뉴 `ImportFairinoFr5Urdf()`가 이 파일을 읽어 Unity Robotics URDF Importer로 hierarchy를 만든다.

이 과정에서 같이 하는 일은 아래와 같다.

- STL mesh 전처리
- URDF import
- visual mesh 재바인딩
- control prefab 저장

## 3. RobotControl은 전용 control prefab을 쓴다

`RobotControl` 페이지는 아래 prefab을 사용한다.

- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`

이 prefab은 단순히 "FR5가 화면에 보이기 위한 모델"이 아니라, `RobotControl`의 조작 로직과 맞는 joint hierarchy를 유지하기 위한 제어용 prefab이다.

즉 `RobotControl`에서는 아래가 중요했다.

- joint 순서가 유지될 것
- joint axis 계약이 맞을 것
- preset, slider, TCP control 검증이 가능할 것

그래서 `RobotControl`은 시각적으로 예쁜 preview보다 "제어 가능한 원본 계층"을 더 중요하게 봤다.

## 4. 왜 showroom prefab을 그대로 쓰지 않았는가

중간에 `RobotControl`에서 FR5 mesh가 안 보였을 때 showroom용 prefab을 대신 써보기도 했다.

하지만 이 방식은 `RobotControl` 기준으로 문제가 있었다.

- control hierarchy가 다름
- joint axis 계약이 다름
- `Ready`, `Folded`, slider preset 검증에 부적합

즉 showroom용 prefab은 "보이게 하는 데"는 도움이 될 수 있었지만, `RobotControl` 페이지의 목적은 "실제로 제어 흐름을 검증하는 것"이기 때문에 최종 해법이 될 수 없었다.

그래서 결론은 다음처럼 정리됐다.

- showroom fallback은 임시 시각 확인용까지만 허용
- `RobotControl` 검증은 반드시 `FAIRINO_FR5_Control.prefab` 기준으로 진행

## 5. RobotControl의 FK 계산은 공식 DH 자료를 바탕으로 만들었다

`RobotControl` 페이지는 슬라이더나 프리셋을 움직일 때 관절값만 바꾸는 것이 아니라, FR5의 현재 pose를 해석해야 한다.

그래서 공식 DH 자료를 기준으로 아래 구조를 만들었다.

- `TemplateFAIRINO_FR5`
- `FR5KinematicsFacade`

이 구조를 통해 `RobotControl`은 다음 같은 기능을 수행할 수 있게 됐다.

- 관절값 기반 pose 계산
- preset 적용 시 end-effector 변화 계산
- 상태 패널 갱신
- Why-It-Moved 설명

즉, 공식 DH 표는 `RobotControl` 안에서 실제 계산 로직으로 변환됐다.

## 6. RobotControl의 실기 연결 구조는 공식 C# SDK 문서를 기준으로 만들었다

`RobotControl`은 나중에 실제 FR5와도 연결할 수 있도록 구조를 분리해서 만들었다.

핵심 구조는 아래와 같다.

- `IFairinoRobotClient`
- `MockFairinoClient`
- `LiveFairinoClient`
- `FairinoConnectionService`

이 구조의 의미는 다음과 같다.

### `MockFairinoClient`

- 실제 로봇 없이도 `RobotControl` UI와 흐름을 검증할 수 있게 함

### `LiveFairinoClient`

- 실제 FAIRINO SDK를 감싸는 구현
- 공식 C# SDK 문서의 메서드 개념을 Unity 안에 연결

`RobotControl`에서 중요하게 본 SDK API는 아래 쪽이다.

- `RPC`
- `MoveJ`
- `MoveL`
- `ServoJ`
- `GetActualJointPosDegree`
- `GetActualTCPPose`

즉, 공식 SDK 문서를 읽고 `RobotControl` 화면이 직접 SDK를 호출하는 게 아니라, 중간 어댑터를 두는 방식으로 안전하게 분리했다.

## 7. RobotControl 기준으로 실제로 반영된 흐름

`RobotControl`에 한정해서 보면, 공식문서에서 Unity로 온 흐름은 아래 순서로 이해하면 된다.

1. 공식문서에서 FR5 사양, DH, SDK, 상태 피드백 기준을 확인했다.
2. FR5 URDF/STL 자산을 Unity로 가져와 control hierarchy를 만들었다.
3. 이 hierarchy를 `FAIRINO_FR5_Control.prefab`으로 정리했다.
4. 공식 DH 자료를 `TemplateFAIRINO_FR5`, `FR5KinematicsFacade`에 반영했다.
5. 공식 C# SDK 문서를 기준으로 `IFairinoRobotClient -> LiveFairinoClient` 구조를 만들었다.
6. `RobotControlSceneCoordinator`가 위 자산, UI, FK, 연결 서비스를 한 화면에서 묶도록 구성했다.

## 8. 한 문장으로 요약

`RobotControl` 페이지에서 FR5를 Unity로 가져온 방식은, 공식문서를 그대로 임포트한 것이 아니라 공식문서를 기준으로 `제어용 URDF prefab`, `DH 기반 FK`, `SDK 어댑터 구조`를 따로 구현해서 하나의 제어 화면으로 묶은 것이다.

## 9. 최종 정리

`RobotControl` 기준에서 공식문서는 "설계 기준"이었다.

그리고 Unity 안에 실제로 만들어진 결과물은 아래였다.

- `FAIRINO_FR5_Control.prefab`
- `TemplateFAIRINO_FR5`
- `FR5KinematicsFacade`
- `IFairinoRobotClient / LiveFairinoClient`
- `FairinoConnectionService`
- `RobotControlSceneCoordinator`

즉, `RobotControl`은 공식문서를 읽어 만든 제어 기준을 Unity의 3D, 기구학, 통신 계층으로 나눠 구현한 결과물이다.
