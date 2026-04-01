# FAIRINO SimMachine 정보구조 맵

## Purpose
- SimMachine의 `Initial / Program / Status / Application / System` 구조를 하나의 문서로 통합 정리한다.
- 공식 문서 근거, 실제 캡처 근거, Unity 반영 위치, `필수 / 선택 / 제외` 판단을 한 번에 볼 수 있게 한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)
- [robotcontrol-soft-teaching-pad.md](./robotcontrol-soft-teaching-pad.md)
- [robotcontrol-scene-hierarchy.md](./robotcontrol-scene-hierarchy.md)
- [fairino-simmachine-screen-structure-draft.md](../robots/fairino-simmachine-screen-structure-draft.md)

## When To Read
- SimMachine 전체 정보구조를 한 문서로 파악할 때
- Unity `RobotControl`에 무엇을 넣고 무엇을 뺄지 정리할 때
- 기능 캡처가 추가될 때 누적 기준 문서로 쓸 때

## Last Updated
- 2026-03-31 (KST)

## 사용 규칙
- `공식 근거`는 제조사 문서에서 기능군 존재를 뒷받침하는 수준을 뜻한다.
- `캡처 근거`는 실제 SimMachine UI 배치와 세부 메뉴 이름을 뜻한다.
- `Unity 반영 위치`는 지금 제품 방향에서 어디로 재구성할지 뜻한다.
- `판단`은 `필수 / 선택 / 제외` 3단계로 나눈다.

## 전체 구조 요약

| 카테고리 | 역할 | Unity 해석 |
|---|---|---|
| `Initial` | 설치/기본/안전/주변장치 초기 설정 | 메인 조작 화면이 아니라 설정 허브 |
| `Program` | 코딩/그래픽/노드/포인트 | V1은 `Points + Motion Sequence + Preview`만 일부 채택 |
| `Status` | 로그/조회/파형/궤적 데이터 | V1은 상태 요약 + 최근 이벤트 + 세션 리포트로 축소 |
| `Application` | 산업용 확장 앱/공정 패키지 | V1 전체 제외 |
| `System` | 계정/정보/시간/네트워크/티칭팬던트/IPC 등 시스템 운영 | 운영자용 설정 허브, 메인 조작기와 분리 |

## 1. Initial

### 기능 트리
- `Base`
  - `Mounting`
  - `World TCP`
  - `TCP`
  - `Ext. TCP`
  - `Workpiece TCP`
  - `Payload`
  - `Joint`
  - `I/O setup`
  - `Home point`
  - `Configuration file`
- `Safety`
  - `Safe Stop`
  - `Safe speed`
  - `I/O safety`
  - `Emergency stop`
  - `Protective stop`
  - `Safety plane`
  - `Interference area`
  - `Reduction Mode`
  - `Daemon`
  - `Direction limit`
  - `Robot limit`
  - `Motion Configuration`
  - `Rewind`
- `Peripheral`
  - `Gripper`
  - `Force sensor`
  - `Welding Handle`
  - `Spray gun`
  - `Welder`
  - `Ext. axis`
  - `Line Laser Sensor`
  - `Polishing`
  - `CNC`
  - `Auxiliary Sensor`
  - `Combination Device`
  - `Board communication`
  - `Array suction cups`

### 근거
- 공식 근거:
  - [Teaching pendant software](https://fairino-doc-en.readthedocs.io/latest/CobotsManual/teaching_pendant_software.html)
  - 설치/좌표계/tool/wobj/payload/safety/peripheral 관련 기능군
- 캡처 근거:
  - Initial > Base 캡처
  - Initial > Safety 캡처
  - Initial > Peripheral 캡처

### Unity 반영 위치
- `Robot Setup` 또는 `Initial Setup` 별도 허브
- `RobotControl` 메인 조작 화면에는 직접 넣지 않음

### 판단
- `필수`
  - 없음
- `선택`
  - `Home point`의 일부 개념
  - `Gripper` 상태 보기
- `제외`
  - 나머지 대부분

## 2. Program

### 기능 트리
- `Coding`
- `Graphical`
- `Node Graph`
- `Points`

### 근거
- 공식 근거:
  - Program, graphical programming, node graph, teaching point 개념
- 캡처 근거:
  - Coding 캡처
  - Graphical 캡처
  - Node Graph 캡처
  - Points / Teaching Management 캡처

### Unity 반영 위치
- `Program`은 별도 탭 또는 후속 모드
- V1은 `Points + Motion Sequence + Preview`

### 판단
- `필수`
  - 없음
- `선택`
  - `Points`
  - `Motion Sequence`
  - `Preview`
- `제외`
  - `Coding`
  - `Graphical`
  - `Node Graph`

## 3. Status

### 기능 트리
- `Log`
- `Query`

### 근거
- 공식 근거:
  - status bar, robot/program/io/exaxis/gripper/ft/conveyor 상태 관련 기능군
- 캡처 근거:
  - System Log 캡처
  - Status Query 캡처

### Unity 반영 위치
- `Status` 탭
- 우측 상태 패널 + 상태 전용 탭

### 판단
- `필수`
  - 상태 요약
  - 최근 이벤트
- `선택`
  - 세션 리포트
  - 간단 차트
- `제외`
  - 전문 로그 테이블 전체
  - 파형/궤적 조회기 전체

## 4. Application

### 기능 트리
- `Tool App`
  - `Robot packing`
  - `Data backup`
  - `Data recording`
  - `End-LED`
  - `Drag locking`
  - `Intersection Generation`
  - `Peripheral protocol`
  - `G-code Conversion`
- `Process Package`
  - `Welding expert`
  - `Torque`
  - `Stiffness level`
  - `Palletization`
  - `Conveyor Tracking`

### 근거
- 공식 근거:
  - application/process package 성격 기능군
- 캡처 근거:
  - Tool App 캡처
  - Process Package 캡처

### Unity 반영 위치
- 메인 소프트 티칭패드에는 반영하지 않음
- 후속 산업 데모 모드나 별도 앱 영역 후보

### 판단
- `필수`
  - 없음
- `선택`
  - `Data recording`
  - `Drag locking`
  - `Intersection Generation`
  - `Palletization`
  - `Conveyor Tracking`
- `제외`
  - V1 기준 전체

## 5. System

### 기능 트리
- `General`
  - `General settings`
    - `Time setting`
    - `Synchronize`
    - `Network settings`
    - `Teach pendant`
    - `Peripheral IPC configuration`
- `Account`
- `About`
- `Custom Info.`
- `Maintenance`

### 근거
- 공식 근거:
  - system settings, network settings, time sync, users/password/system information
- 캡처 근거:
  - System > General 캡처

### Unity 반영 위치
- `System / Operator Settings` 별도 허브
- 메인 `RobotControl` 조작 화면과 분리

### 판단
- `필수`
  - 없음
- `선택`
  - 네트워크 상태 read-only 정보
  - About 일부 정보
- `제외`
  - 설정 화면 대부분

## Unity 최종 컷

### 필수
- `RobotControl`
  - 연결/상태
  - Base/Tool/Wobj
  - 단일축 조그
  - 다축 연계
  - 포인트 이동
  - preview
  - 위험 경고
  - 한국어 안내 팝업

### 선택
- `Program`
  - Points
  - Motion Sequence
  - Preview
- `Status`
  - 최근 이벤트
  - 세션 리포트
- `I/O 최소 버전`
- `그리퍼 상태 보기`

### 제외
- Initial 설치형 상세
- Safety 상세 설정
- Peripheral 상세 설정
- Program IDE 계열
- Application 전체
- System 설정 대부분

## 씬 구조 연결
- 메인 조작은 [robotcontrol-scene-hierarchy.md](./robotcontrol-scene-hierarchy.md)의 `RobotControlShell` 아래로 들어간다.
- `Initial`, `Application`, `System`은 메인 `RobotControl`이 아니라 별도 설정 허브 또는 후속 앱 영역으로 분리한다.
- `Program`과 `Status`는 `RobotControl` 안에 축소된 서브탭으로만 일부 흡수한다.

## 결론
- SimMachine은 `조작기`를 넘어 `설정 + 개발 + 운영 + 산업 패키지`까지 포함한 대형 WebApp이다.
- Unity는 이 전체를 복제하는 대신, `초보자용 조작 핵심`만 가져오고 나머지는 후속/별도 허브로 분리하는 것이 맞다.
