# RobotControl 소프트 티칭패드 계획

## Purpose
- `RobotControl`을 FAIRINO FR5용 한국어 소프트 티칭패드로 승격시키기 위한 UX/구현 기준을 정리한다.
- 목표는 제조사 티칭패드를 그대로 흉내 내는 것이 아니라, `초보자도 쉽게 쓸 수 있는 Unity 기반 교육형/운영형 콘솔`을 만드는 것이다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)
- [tablet-first-policy.md](./tablet-first-policy.md)
- [fairino-teaching-pad-feature-matrix.md](../robots/fairino-teaching-pad-feature-matrix.md)

## When To Read
- `RobotControl` UX를 제품 수준으로 재설계할 때
- 초보자 친화형 한국어 조작 흐름을 만들 때
- authored-first / 디자인 토큰 / 태블릿 대응 / 컴포넌트 분리를 함께 결정할 때

## Last Updated
- 2026-03-31 (KST)

## Product Goal
- 노트북과 태블릿에서 FR5를 조작할 수 있는 `나만의 티칭패드`를 만든다.
- 초보자는 위험한 기능을 바로 보지 않게 하고, 단계적으로 이해와 자신감을 얻도록 만든다.
- Unity의 강점을 살려 제조사 WebApp보다 더 나은 `미리보기`, `설명`, `복기`, `학습` 경험을 제공한다.

## Locked Decisions
- 한국어를 1차 기본 언어로 사용한다.
- 모든 실기 명령은 `preview -> 안내 -> 확인 -> 실행` 흐름을 기본으로 둔다.
- `authored-first`를 유지하고, 런타임은 authored layout 바인딩을 우선한다.
- 하드코딩 색상/간격/폰트 크기를 늘리지 않고 `UIDesignTokens`, `UIComponentFactory`, `UILayoutProfile`를 우선 사용한다.
- 기능별 UI는 flat giant panel이 아니라 `Connection / Motion / Teaching / Diagnostics / Safety / Help` 단위 컴포넌트로 분리한다.
- Desktop + Tablet을 정식 지원 대상으로 본다.
- 제조사 기능 복제보다 `초보자 이해도 + 안전성 + 예측 가능성`을 더 중요한 KPI로 둔다.
- LLM은 후속 단계에서만 `설명층`으로 연결하고, 실기 runtime 결정권은 deterministic 계층에 둔다.

## 공식 문서 기준선
- 티칭패드/WebApp의 기본 기능 범위는 [Teaching pendant software](https://fairino-doc-en.readthedocs.io/latest/CobotsManual/teaching_pendant_software.html)를 기준으로 본다.
- 실제 연결/모드/Enable/Drag는 [C# Robot Basics](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotBase.html)를 기준으로 본다.
- 실제 모션 명령은 [C# Robot Motion](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotMovement.html)를 기준으로 본다.
- I/O는 [C# Robot IO](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotIO.html), 상태 조회는 [C# Robot Status Check](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotStatusInquiry.html), 그리퍼/주변장치는 [C# Robot Peripherals](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotPeripherals.html), 로그/샘플링 주기/증거 수집은 [C# Other interfaces](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotOther.html)를 기준으로 본다.
- 실기 이전의 WebApp 구조 확인과 절차 리허설은 [FAIRINO SimMachine](https://fairino-doc-en.readthedocs.io/latest/VMMachine/controller_virtual_machine.html)를 활용한다.

## 공식 제약을 반영한 제품 해석
- 제조사 C# 예제는 `powered on + enabled + workspace interference 없음`을 기본 전제로 둔다. 따라서 Unity UX는 사용자가 이 전제를 모르고 건너뛰지 않게 해야 한다.
- 제조사 문서에는 manual/auto, drag, tool/wobj, payload, fault, TPD 절차가 분산되어 있다. Unity는 이것을 초보자 관점에서 하나의 흐름으로 재조합해야 한다.
- 문서상 WebApp은 브라우저 접근 구조를 갖고 있지만, Unity 버전은 단순 대체 브라우저가 아니라 `더 잘 보이고 더 잘 이해되는 운영/교육 도구`를 목표로 한다.
- 공식 문서에 있는 모든 기능을 한 번에 옮기지 않는다. `실기 제어 핵심 -> 교육형 차별화 -> 운영 확장` 순서로 단계화한다.

## UX 원칙

### Beginner First
- 한 번에 모든 기능을 보여주지 않는다.
- 초보자 모드에서는 필요한 버튼만 보여주고, 고급 기능은 접거나 잠근다.
- 오류 메시지는 코드를 보여주기보다 `무슨 상태인지`, `왜 그런지`, `지금 무엇을 하면 되는지` 순서로 설명한다.

### Preview First
- 실제 로봇을 움직이기 전에 Unity 안에서 목표 자세와 경로를 먼저 보여준다.
- 사용자는 실행 전 `도달 가능`, `충돌 위험`, `특이점 가능성`, `현재 포즈와 차이`를 본다.

### Touch Friendly
- 태블릿에서는 3D 뷰포트를 우선하고, 긴 rail/표는 접기/탭/하단 시트로 이동한다.
- 주 조작 버튼은 44px 이상을 기본으로 한다.

### Safe By Default
- 첫 연결 후 기본은 `DryRun On`으로 시작한다.
- Live 모드에서 위험 명령은 확인 팝업을 반드시 거친다.
- 연결 끊김, fault, safety stop 상태에서는 조작보다 복구 안내를 먼저 보여준다.

### Officially Grounded
- 조작 명칭은 가능한 한 공식 문서 용어와 맞춘다.
- 내부 UI는 쉬운 한국어를 쓰되, 진단 drawer에는 공식 용어와 API 대응 이름을 같이 보여준다.
- `문서상 존재`, `코드상 존재`, `실기 검증 완료`를 같은 것으로 취급하지 않는다.

## 정보 구조 제안

### RC-01 연결 홈
- 목적: 사용자가 현재 상태를 바로 이해하고 안전한 다음 행동을 선택하게 한다.
- 표시:
  - 연결 상태
  - 모드
  - Drag 상태
  - Tool/User
  - Safety/Fault
  - 현재 속도 preset
- CTA:
  - `연결`
  - `연결 해제`
  - `서보 켜기`
  - `동기화`
  - `에러 초기화`

### RC-02 조작
- 탭:
  - `쉬운 조작`
  - `관절`
  - `TCP`
  - `티칭`
  - `진단`
- `쉬운 조작`은 초보자용 home/ready/folded/zero와 큰 버튼 중심으로 구성한다.
- `관절`은 slider + numeric + ring handle을 묶고, 현재/목표/예상 차이를 보여준다.
- `TCP`는 좌표 입력보다 `목표 카드 + 미리보기` 중심으로 재구성한다.

### RC-03 프리뷰
- 현재 포즈와 목표 포즈를 동시에 본다.
- `ghost robot`, `예상 경로`, `EE trail`, `Frame Gizmo`, `위험 링크 강조`를 같은 레이어 안에서 토글한다.

### RC-04 티칭
- 포인트 저장
- 포인트 이름 붙이기
- 시퀀스 재생
- export/import
- 이후 단계에서 TPD 유사 기록/재생으로 확장

### RC-05 도움말
- 현재 패널과 선택된 버튼에 맞춰 짧은 한국어 도움말을 띄운다.
- 예:
  - `MoveJ`: 관절 기준으로 이동합니다.
  - `MoveL`: 공구 끝이 직선에 가깝게 이동합니다.
  - `Sync`: 실제 로봇 상태를 Unity 화면에 맞춥니다.

## 공식 기능 1:1 매칭 설계

| 공식 기능 | 공식 의미 | Desktop 배치 | Tablet 배치 | Unity 버튼/라벨 |
|---|---|---|---|---|
| Control area | Enable / Start / Stop / Pause | 상단 상태 바 | 상단 상태 바 | `서보 켜기`, `시작`, `정지`, `일시정지/재개` |
| Status bar | robot state / error / speed / mode / tool | 상단 상태 바 + 우측 상태 패널 | 상단 상태 바 + 하단 상태 시트 | `자동`, `수동`, `Drag`, `Fault`, `Safety`, `Speed`, `Tool`, `Wobj` |
| Menu bar | Initial / Base / Safety / Peripheral / Program / Points / Status / Log / Application / Process Package / System | 좌측 전역 메뉴 | 햄버거 드로어 | `초기`, `기본`, `안전`, `주변장치`, `프로그램`, `포인트`, `상태`, `로그`, `앱`, `공정`, `시스템` |
| 3D scene operation | coordinate display / trajectory / tool model | 중앙 3D 상단 오버레이 | 중앙 3D 상단 오버레이 | `Base 축`, `Tool 축`, `Trajectory`, `툴 모델`, `카메라 리셋` |
| TCP | Base / Tool / Wobj jog | 좌측 `TCP` 탭 | 하단 `TCP` 탭 | `Base`, `Tool`, `Wobj`, `증분 이동량`, `X+/-`, `Y+/-`, `Z+/-`, `RX+/-`, `RY+/-`, `RZ+/-` |
| Joint Jog - single axis | 단일축 조그 | 좌측 `관절` 탭 | 하단 `관절` 탭 | `J1-`, `J1+` ... `J6-`, `J6+` |
| Joint Jog - multi-axis linkage | 6 slider + Restore + Apply | 좌측 `관절` 탭 | 하단 `관절` 탭 | `J1~J6`, `복원`, `적용` |
| Move | Cartesian pose + Calculate + Move | 좌측 `포인트 이동` 카드 | 하단 `포인트 이동` 카드 | `관절 위치 계산`, `이 포인트로 이동` |
| Teaching point record | quick point / named point | 하단 보조 모듈 + 우측 패널 | 하단 보조 모듈 | `빠른 포인트 저장`, `이름 저장`, `포인트 목록`, `불러오기`, `삭제` |
| I/O | DO/AO/tool DO control | 하단 보조 모듈 | 하단 탭 | `I/O`, `DO ON/OFF`, `AO 값 적용`, `Tool DO` |
| TPD | record / stop / edit / playback | 하단 보조 모듈 | 하단 탭 | `TPD`, `기록 시작`, `기록 중지`, `재생`, `편집` |
| FT / Eaxis / Gripper status | support status | 우측 상태 패널 하단 | 하단 상태 시트 | `FT`, `외부축`, `그리퍼`, `상태 보기` |

## Desktop 와이어프레임

```text
┌───────────────────────────────────────────────────────────────────────────────────────────────┐
│ [FAIRINO 로고] [전역메뉴]  로봇: FR5   연결됨/안됨   Tool 01   Wobj 00   Load 01   Speed 30 │
│ [서보 켜기] [시작] [정지] [일시정지/재개] [자동] [수동] [Sync] [오류 초기화] [E-Stop 안내] │
└───────────────────────────────────────────────────────────────────────────────────────────────┘

┌───────────────────────┬──────────────────────────────────────────────┬──────────────────────┐
│ 좌측 전역/조작 패널   │                 중앙 3D 뷰                  │ 우측 상태/설명 패널  │
│                       │                                              │                      │
│ 전역 메뉴             │  3D 툴바:                                   │ 상태 요약            │
│ `초기`                │  [Base 축] [Tool 축] [Trajectory] [툴모델]  │ `Stopped/Running`    │
│ `기본`                │  [고스트 미리보기] [충돌경고] [카메라리셋]  │ `자동/수동/Drag`     │
│ `안전`                │                                              │ `연결 상태`          │
│ `주변장치`            │  현재 로봇                                  │ `Fault/Safety`       │
│ `프로그램`            │  목표 고스트                                │ `Tool/Wobj/Load`     │
│ `포인트`              │  예상 경로                                  │                      │
│ `상태`                │  Base/Tool/Wobj 축                          │ Why It Moved         │
│ `로그`                │  위험 링크 하이라이트                       │ `왜 이렇게 움직였는지`│
│ `앱`                  │                                              │                      │
│ `공정`                │                                              │ 추천 다음 행동       │
│ `시스템`              │                                              │ `먼저 Sync 하세요`   │
│                       │                                              │ `이 포즈는 특이점...`│
│ 작업 탭               │                                              │                      │
│ [쉬운 조작]           │                                              │ 진단/도움            │
│ [TCP]                 │                                              │ [세션 리포트]        │
│ [관절]                │                                              │ [로그 보기]          │
│ [포인트 이동]         │                                              │ [도움말]             │
│ [티칭]                │                                              │                      │
└───────────────────────┴──────────────────────────────────────────────┴──────────────────────┘

┌───────────────────────────────────────────────────────────────────────────────────────────────┐
│ 보조 모듈 바: [포인트] [I/O] [TPD] [외부축] [FT] [그리퍼] [Diagnostics] [Help]              │
└───────────────────────────────────────────────────────────────────────────────────────────────┘
```

### Desktop 패널별 버튼 이름
- 상단 상태 바
  - `서보 켜기`
  - `시작`
  - `정지`
  - `일시정지/재개`
  - `자동`
  - `수동`
  - `Sync`
  - `오류 초기화`
- `쉬운 조작`
  - `Home`
  - `Ready`
  - `Folded`
  - `Zero`
  - `현재 자세 동기화`
  - `미리보기`
  - `실제 이동`
- `TCP`
  - 좌표계 탭: `Base`, `Tool`, `Wobj`
  - 이동량: `증분 이동량`
  - 이동 버튼: `X+`, `X-`, `Y+`, `Y-`, `Z+`, `Z-`, `RX+`, `RX-`, `RY+`, `RY-`, `RZ+`, `RZ-`
  - 값 표시: `X`, `Y`, `Z`, `RX`, `RY`, `RZ`
- `관절`
  - 단일축 조그: `J1-`, `J1+`, `J2-`, `J2+`, `J3-`, `J3+`, `J4-`, `J4+`, `J5-`, `J5+`, `J6-`, `J6+`
  - 다축 연계: `J1`, `J2`, `J3`, `J4`, `J5`, `J6`
  - 하단 액션: `복원`, `적용`
- `포인트 이동`
  - `X`, `Y`, `Z`, `RX`, `RY`, `RZ`
  - `관절 위치 계산`
  - `이 포인트로 이동`
  - `복원`
- `티칭`
  - `빠른 포인트 저장`
  - `이름 저장`
  - `포인트 목록`
  - `선택 불러오기`
  - `선택 삭제`
  - `TPD 기록 시작`
  - `TPD 기록 중지`
  - `TPD 재생`
  - `TPD 편집`
- `I/O`
  - `DO1 ON/OFF`
  - `DO2 ON/OFF`
  - `AO 값 적용`
  - `Tool DO ON/OFF`

## Tablet 와이어프레임

```text
┌─────────────────────────────────────────────────────────────┐
│ [로고] FR5   연결상태   Speed 30   [서보] [시작] [정지]    │
│ Tool 01  Wobj 00  Load 01  Fault/Safety                    │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                        3D 뷰포트                            │
│   현재 로봇 / 목표 고스트 / 예상 경로 / 위험 하이라이트     │
│   상단 오버레이: [Base축] [Tool축] [Trajectory] [고스트]    │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ 하단 탭                                                     │
│ [쉬운 조작] [TCP] [관절] [포인트 이동] [티칭] [상태]       │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│ 현재 선택 탭의 바텀시트                                     │
│ 예: TCP                                                     │
│ [Base] [Tool] [Wobj]                                        │
│ 증분 이동량  [슬라이더] [30]                                │
│ X+ X-  Y+ Y-  Z+ Z-                                         │
│ RX+ RX-  RY+ RY-  RZ+ RZ-                                   │
│ [미리보기] [실제 이동]                                      │
└─────────────────────────────────────────────────────────────┘
```

### Tablet 패널별 버튼 이름
- 상단 고정 버튼
  - `서보`
  - `시작`
  - `정지`
  - `Sync`
  - `오류 초기화`
- 하단 탭
  - `쉬운 조작`
  - `TCP`
  - `관절`
  - `포인트 이동`
  - `티칭`
  - `상태`
- `쉬운 조작`
  - `Home`
  - `Ready`
  - `Folded`
  - `Zero`
  - `미리보기`
  - `실제 이동`
- `상태`
  - `Fault`
  - `Safety`
  - `Tool/Wobj`
  - `Why It Moved`
  - `추천 복구`

## 라벨 표준
- `Enable` -> `서보 켜기`
- `Start` -> `시작`
- `Stop` -> `정지`
- `Pause/Resume` -> `일시정지/재개`
- `Manual` -> `수동`
- `Automatic` -> `자동`
- `Base Jog` -> `Base 이동`
- `Tool Jog` -> `Tool 이동`
- `Wobj Jog` -> `Wobj 이동`
- `Single-axis jog` -> `단일축 조그`
- `Multi-axis linkage` -> `다축 연계`
- `Move` -> `포인트 이동`
- `Teaching point` -> `티칭 포인트`
- `TPD` -> `TPD 기록`
- `Gripper` -> `그리퍼`

## 디자인 해석
- SimMachine의 `중앙 3D + 좌측 작업 패널 + 상단 상태 + 보조 기능 바` 구조는 유지한다.
- SimMachine처럼 상단/우측/하단에 기능을 과하게 분산하지 않고, 역할별 패널로 재조합한다.
- 공식 문서의 `Apply`, `Restore`, `Calculate joint position`, `Move to this point`는 개념을 유지하되 더 쉬운 한글로 보여준다.
- 공식 문서에 없는 차별화 요소로 `고스트 미리보기`, `위험 경고`, `Why It Moved`, `친절한 한국어 팝업`을 추가한다.

## 범위 우선순위

### 필수
- `연결 / 해제 / 서보 켜기 / Sync`
- `상태 표시`
  - 연결 상태
  - 현재 모드
  - Fault / Safety
  - Tool / Wobj
  - 속도
- `Base / Tool / Wobj`
- `TCP 조그`
  - X/Y/Z
  - RX/RY/RZ
  - 증분 이동량
- `단일축 조그`
  - J1~J6 `- / +`
- `다축 연계`
  - J1~J6 slider
  - `복원`
  - `적용`
- `포인트 이동`
  - Cartesian 값 입력
  - `관절 위치 계산`
  - `이 포인트로 이동`
- `미리보기`
  - 목표 자세
  - 예상 경로
  - 현재/목표 비교
- `위험 경고`
  - 도달 불가
  - 특이점
  - joint limit
  - 큰 자세 차이
- `한국어 안내 팝업`
  - 확인
  - 경고
  - 복구
- `정지 버튼`
- `태블릿 대응 기본 레이아웃`

### 선택
- `빠른 포인트 저장`
- `이름 저장 포인트`
- `포인트 목록 / 불러오기 / 삭제`
- `TPD 최소 버전`
  - 기록 시작
  - 기록 중지
  - 재생
- `I/O 최소 버전`
  - DO
  - AO
  - Tool DO
- `그리퍼 상태 보기`
- `Why It Moved`
- `세션 리포트`
- `강사용 데모 모드`
- `고스트 로봇`
- `Trajectory on/off`

### 제외
- `Mounting`
- `World TCP`
- `Ext. TCP`
- `Workpiece TCP`의 고급 설정
- `Payload` 상세 보정 화면
- `Joint` 설정 페이지
- `I/O setup` 고급 설정
- `Home point` 설정 관리자 화면
- `Configuration file`
- `Safe Stop` 상세 설정
- `Safe speed` 상세 설정
- `I/O safety`
- `Emergency stop` 설정 페이지
- `Protective stop` 설정 페이지
- `Safety plane`
- `Interference area`
- `Reduction Mode`
- `Daemon`
- `Direction limit`
- `Robot limit`
- `Motion Configuration`
- `Rewind`
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
- `Program / Graphical / Node editor`
- `로그/시스템 유지보수 전체`

## Unity 전용 Program 탭 설계

### 핵심 방향
- SimMachine의 `Coding / Graphical / Node Graph / Points`를 그대로 복제하지 않는다.
- Unity에서는 `코드 IDE`보다 `3D 기반 동작 시퀀서 + 교육형 디버거`로 재해석한다.
- 메인 구조는 `좌측 시퀀스 리스트 + 중앙 3D 프리뷰 + 우측 상세 속성 패널 + 하단 검증 바`로 둔다.

### 상단 바
- `새 시퀀스`
- `불러오기`
- `저장`
- `복제`
- `삭제`
- `실행`
- `한 단계 실행`
- `시뮬레이션`
- `정지`
- `리셋`

### 좌측 시퀀스 리스트
- 각 줄은 블록 1개
- 표시 항목:
  - 아이콘
  - 블록 이름
  - 목표 포인트/값 요약
  - 속도
  - 경고 배지
- 각 줄 액션:
  - `위로`
  - `아래로`
  - `복제`
  - `삭제`
  - `비활성화`

### 블록 종류
- `Home 이동`
- `포인트 이동`
- `직선 이동`
- `관절 이동`
- `대기`
- `그리퍼 열기`
- `그리퍼 닫기`
- `I/O ON`
- `I/O OFF`
- `조건`
- `반복`
- `분기`
- `복귀`
- `정지`
- `사용자 메시지`

### 우측 상세 속성 패널
- 공통:
  - `블록 이름`
  - `설명`
  - `활성/비활성`
- 모션 블록:
  - `Motion Type`
  - `Target`
  - `Speed`
  - `Acceleration`
  - `Blend`
  - `Tool`
  - `Wobj`
  - `Approach Offset`
  - `Retreat Offset`
- 대기 블록:
  - `시간(ms)`
  - `조건 대기`
- I/O 블록:
  - `채널`
  - `값`
  - `출력 방식`
- 그리퍼 블록:
  - `Position`
  - `Speed`
  - `Force/Torque`
- 조건/반복:
  - `입력 조건`
  - `비교 연산`
  - `반복 횟수`
  - `탈출 조건`

### 중앙 3D 프리뷰
- 현재 로봇
- 선택 블록 목표 고스트
- 이전 블록 → 현재 블록 경로
- 위험 구간 색상 강조
- 축 표시 토글
- 충돌/특이점/도달 불가 뱃지

### 하단 검증 바
- `경로 검증`
- `충돌 체크`
- `도달 가능성`
- `특이점 체크`
- `joint limit 체크`
- `예상 실행 시간`
- `실행 리포트`

### 초보자 모드
- 보이는 블록:
  - `Home 이동`
  - `포인트 이동`
  - `직선 이동`
  - `대기`
  - `그리퍼 열기/닫기`
- 편집 가능 항목:
  - `목표`
  - `속도`
  - `대기 시간`
- 큰 버튼 위주
- 경고 문구를 쉬운 한국어로 표시

### 고급 모드
- 모든 블록 표시
- blend, tool, wobj, io, 조건식 편집 허용
- 세부 파라미터 표출
- 로그/검증 정보 확장

### Unity에서만 넣기 좋은 기능
- 블록 선택 시 3D 즉시 하이라이트
- 속성 수정 시 경로 실시간 갱신
- 잘못된 블록은 빨간 outline
- 실행 전 전체 애니메이션 미리보기
- `왜 실패하는지` 설명 패널
- `현재 시퀀스 vs 수정 시퀀스` 비교 재생

### Program 탭 범위 해석
- `Coding` -> 제외
- `Graphical` -> 제외
- `Node Graph` -> 제외
- `Points` -> 일부 채택

즉 V1 Program 탭은 `Points + Motion Sequence + Preview` 중심으로 축소한다.

## Unity 전용 Status 탭 설계

### SimMachine 구조 해석
- `Status`
  - `Log`
  - `Query`
- `Log`는 날짜, 레벨 필터, 검색, 시스템 로그 테이블 중심
- `Query`는 차트 표시, trajectory data, waveform time, 데이터 프레임 선택 중심

### 핵심 방향
- SimMachine의 운영자용 로그 뷰어와 파형 조회기를 그대로 복제하지 않는다.
- Unity에서는 `학습형 상태 요약 + 핵심 진단 + 세션 리포트`로 재해석한다.

### V1에 넣을 것
- `요약 카드`
  - 연결 상태
  - 현재 모드
  - Fault / Safety
  - Tool / Wobj
  - Speed
- `최근 이벤트`
  - 연결
  - 서보 켜기
  - Sync
  - MoveJ
  - MoveL
  - 오류
- `최근 명령`
  - 어떤 포즈/시퀀스를 실행했는지
- `왜 안 되는지`
  - 복구 추천
- `세션 리포트 내보내기`

### V2에 넣을 것
- `간단 차트`
  - joint actual vs target
  - tcp actual vs target
- `trajectory replay`
- `fault timeline`

### 제외할 것
- SimMachine 수준의 시스템 로그 테이블 전체
- 전문 파형 조회기 전체
- 복잡한 filter / restore / chart builder
- 운영자용 Modbus / 프로그램 로그 상세 분석

### UI 구조
- 상단:
  - `상태`
  - `이벤트`
  - `리포트`
- 본문:
  - `StatusSummaryCard`
  - `RecentEventsList`
  - `LastCommandCard`
  - `RecoveryGuideCard`
- 하단:
  - `리포트 내보내기`
  - `진단 보기`

### Tablet 구조
- `상태 요약`과 `최근 이벤트`를 우선
- 상세 로그는 바텀시트 또는 drawer로 이동

## Unity 전용 Application 탭 해석

### SimMachine 구조 해석
- `Application`
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

### 핵심 판단
- Application 탭은 일반 조작/학습 UI가 아니라 `산업 공정 패키지 + 확장 툴 앱 묶음`에 가깝다.
- 따라서 메인 소프트 티칭패드 V1에는 넣지 않는다.

### V1 판단
- `Application 전체` -> 제외

### 후속 검토 후보
- `Data recording`
  - Unity `세션 리포트` / `학습 기록`으로 재해석 가능
- `Drag locking`
  - 안전/조작 보조 설정으로 후속 검토 가능
- `Intersection Generation`
  - 교육용 기하 도구로 재해석 가능
- `Palletization`
  - 고급 교육 시나리오 패키지로 후속 검토
- `Conveyor Tracking`
  - 산업 데모 모드에서만 검토

### 제외 이유
- 구현 비용이 크다
- 현재 목표인 `초보자용 조작기`와 맞지 않는다
- 산업 도메인별 전용 워크플로우가 많다
- V1 범위를 크게 흐린다

## 사용자 친화 팝업 규칙

### 팝업 종류
- `안내 팝업`: 기능 설명, 첫 사용 가이드
- `확인 팝업`: Live 실행 전 최종 확인
- `경고 팝업`: 도달 불가, 특이점, 속도 과다, DryRun Off
- `복구 팝업`: 연결 끊김, fault, safety stop 후 다음 행동 안내

### 팝업 문장 구조
1. 현재 상태
2. 왜 이런 상황인지
3. 추천 행동 1개
4. 필요 시 추가 선택지

### 예시
- `목표 자세가 현재 자세와 많이 다릅니다.`
- `이동 전에 미리보기에서 경로와 충돌 위험을 확인하세요.`
- `추천: 먼저 [미리보기 실행]을 눌러 확인한 뒤 [실제 이동]을 진행하세요.`

## 공식 제약 기반 필수 팝업
- `연결 전 팝업`
  - 컨트롤러 전원, 네트워크, IP, 작업 공간 확인
- `Enable 전 팝업`
  - 주변 간섭 없음, Drag 해제, fault 없음 안내
- `MoveJ / MoveL 전 팝업`
  - 현재 Tool/User 문맥, 속도 preset, 목표 차이, 위험 경고 표시
- `TPD/Teaching 계열 팝업`
  - manual mode / drag mode / 시작 자세 / 기록 한계 안내
- `연결 끊김 / safety stop 복구 팝업`
  - 현재 상태, 추정 원인, 추천 복구 순서 안내

## 태블릿 정책

### Desktop
- 3D + 연결 + 조작 + 상태를 동시 배치한다.
- 진단 drawer는 우측 슬라이드 패널로 둔다.

### Tablet
- 3D 뷰포트 우선
- 하단 탭 전환형 조작
- 긴 slider rail은 접이식 아코디언 또는 페이지드 rail로 분리
- 상태 패널은 compact card로 요약하고 자세한 내용은 바텀 시트로 연다

### Phone
- 정식 지원 대상이 아니다.
- 필요한 경우 읽기 전용 또는 데모 전용으로 제한한다.

## 컴포넌트 / 폴더 구조 제안

### App
- `Assets/Scripts/App/Fairino/Connection/`
  - 연결 수명주기
  - reconnect
  - safety/fault sync
- `Assets/Scripts/App/Fairino/Motion/`
  - MoveJ / MoveL
  - preview request
  - speed policy
- `Assets/Scripts/App/Fairino/Teaching/`
  - waypoint
  - sequence runner
  - export/import
  - future TPD adapter
- `Assets/Scripts/App/Fairino/Diagnostics/`
  - log
  - evidence pack
  - health summary

### UI
- `Assets/Scripts/UI/RobotControl/Connection/`
  - connection panel
  - safety summary card
- `Assets/Scripts/UI/RobotControl/Motion/`
  - easy motion panel
  - joint panel
  - tcp panel
  - preview legend
- `Assets/Scripts/UI/RobotControl/Teaching/`
  - point manager
  - sequence panel
  - future TPD panel
- `Assets/Scripts/UI/RobotControl/Diagnostics/`
  - diagnostics drawer
  - session report view
- `Assets/Scripts/UI/RobotControl/Help/`
  - beginner popup
  - inline coach
  - glossary chips

### Visualization
- `Assets/Scripts/Visualization/RobotControl/Preview/`
  - ghost robot
  - predicted path
  - collision highlight
- `Assets/Scripts/Visualization/RobotControl/Overlays/`
  - frame gizmo
  - trail
  - displacement arrow
  - target markers

## 구현 단계 제안

### Phase A. 제품 기준선 정리
- 기능 매트릭스 문서 고정
- 초보자 모드 / 강사 모드 / 운영자 모드 정의
- Live 위험 정책과 팝업 규칙 확정
- 공식 문서의 용어와 내부 한국어 용어 매핑 표 작성

### Phase B. RobotControl L3 정리
- `RobotControl` 전체를 디자인 토큰 기준으로 정리
- `TryBindExistingLayout(...)` 기반 authored-first 경로를 패널 전체에 확대
- UI 하드코딩 제거
- 기능별 하위 폴더 분리 시작
- 진단 drawer에 공식 API 대응 이름 표시

### Phase C. 초보자용 UX 추가
- `쉬운 조작` 탭 추가
- 큰 버튼 프리셋
- 친절한 한국어 팝업
- 상태별 복구 안내 카드
- 처음 쓰는 사람용 guided overlay

### Phase D. Unity 차별화 프리뷰
- ghost preview
- 예상 경로 표시
- 도달 가능 / 불가능 배지
- singularity / limit / collision warning
- 현재 vs 목표 비교 카드

### Phase E. 태블릿 최적화
- 2-column desktop / 1+bottom-sheet tablet 레이아웃
- 대형 버튼 모드
- slider rail 스크롤/접기 최적화
- 태블릿 회전 방향 가이드

### Phase F. teaching 강화
- point manager 개선
- sequence replay / compare
- session 기록
- 이후 TPD 유사 기능 검토

### Phase G. 실기 운영 강화
- I/O
- gripper
- 로그 수집
- 현장 점검 리포트
- 운영자 모드

## 구현 시 공식 문서와 같이 관리할 체크리스트
- 각 기능 UI 옆에 `공식 기능명`과 `내부 쉬운 이름`을 매핑한다.
- SDK 메서드가 있는 기능은 문서 옆에 대표 API를 기록한다.
- 공식 문서상 기능이 있어도 현장 미검증이면 `실기 검증 필요` 뱃지를 유지한다.
- SimMachine으로 재현 가능한 절차와 실제 하드웨어가 필요한 절차를 분리한다.

## 차별화 포인트

### 제조사 패드보다 더 좋아야 하는 부분
- `시각적 이해`: 로봇이 왜 그렇게 움직이는지 보인다.
- `실수 예방`: 실행 전에 위험이 먼저 보인다.
- `교육성`: 초보자가 두려움 없이 배운다.
- `복기`: 이전 시도와 현재 시도를 비교할 수 있다.
- `강의 활용`: 강사가 설명하기 쉬운 화면 구조를 가진다.

### Unity로만 줄 수 있는 가치
- 3D 고스트/오버레이
- 장면 기반 guided overlay
- 실시간 설명 레이어
- 로봇/목표/위험 구역 동시 시각화
- 교육용 애니메이션
- 시도 기록과 재생

## 지금 추가하면 특히 좋은 기능
- `목표 자세 점수`: 현재 자세가 목표에 얼마나 가까운지 점수화
- `안전 시작 체크리스트`: 연결 전 체크 3단계
- `실수 방지 Undo`: 마지막 프리뷰/마지막 포인트 되돌리기
- `강사용 데모 모드`: 설명 자막 + 큰 강조 + 학생용 숨김 기능
- `작업 카드`: Pick, Place, Inspect, Home Recovery 같은 카드형 시나리오
- `현장 친화 큰 버튼 모드`: 태블릿 + 장갑 환경 대응
- `세션 리포트`: 어떤 포즈를 저장했고 어떤 fault가 났는지 자동 정리

## Success Criteria
- 초보자 사용자가 설명 없이도 `연결 -> 동기화 -> 프리뷰 -> 안전 확인 -> 작은 이동`을 수행할 수 있다.
- 태블릿에서 3D 뷰와 기본 조작이 답답하지 않다.
- 위험 상황에서 사용자가 다음 행동을 텍스트만 읽고도 이해할 수 있다.
- 제조사 패드에 없는 `미리보기`, `설명`, `복기` 가치를 명확히 체감할 수 있다.
