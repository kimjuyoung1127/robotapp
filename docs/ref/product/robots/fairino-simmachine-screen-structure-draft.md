# FAIRINO SimMachine 화면 구조 초안

## Purpose
- FAIRINO SimMachine VM 이미지와 Software 패키지 분석을 바탕으로 WebApp의 화면 구조 초안을 정리한다.
- 실제 UI를 띄우기 전에도 메뉴 구조, 페이지 책임, `RobotControl`에 가져올 만한 UI 패턴을 빠르게 파악할 수 있게 한다.

## Parent Doc
- [fairino-fr5-integration-reference.md](./fairino-fr5-integration-reference.md)
- [fairino-teaching-pad-feature-matrix.md](./fairino-teaching-pad-feature-matrix.md)
- [robotcontrol-soft-teaching-pad.md](../ux/robotcontrol-soft-teaching-pad.md)

## When To Read
- SimMachine/WebApp 메뉴 구조를 빠르게 파악할 때
- 실제 제조사 화면을 기준으로 Unity `RobotControl` IA를 짤 때
- 어떤 기능이 한 화면에 묶이는지 가늠할 때

## Last Updated
- 2026-03-31 (KST)

## 분석 기준

### 확인한 로컬 자산
- VM 이미지:
  - `C:\Users\ezen601\Downloads\FAIRINO_SimMachine_v3.9.3\FAIRINO_SimMachine\FAIRINO_SimMachine.vmx`
  - `...FAIRINO_SimMachine.vmdk`
  - `...fr_get_vm_net.bat`
- 소프트웨어 패키지:
  - `C:\Users\ezen601\Downloads\FAIRINO SimMachine Software-V3.9.3-20260210\software\software.tar.gz`

### 분석한 내부 경로
- `web/frontend/index.html`
- `web/frontend/login.html`
- `web/frontend/pages/*.html`
- `web/frontend/js/pages/index.js`
- `web/file_factory/file/frontend/lang/ko.json`

## 패키지 성격 요약
- VM 이미지는 실제 SimMachine WebApp을 띄워서 화면을 직접 보는 용도다.
- Software 패키지는 실행 파일보다 `web/controller 업그레이드 번들`에 가깝다.
- 따라서 화면 구조 문서화는:
  - `VM = 실제 화면 확인`
  - `Software = 메뉴/기능/리소스 구조 분석`
  로 나눠서 접근하는 것이 맞다.

## 실행 환경 메모
- 현재 세션에서 VMware 실행 파일은 확인되지 않았다.
- 따라서 이 세션에서는 VM을 직접 부팅하지 못했고, 로컬 패키지 구조 분석까지 진행했다.
- 실제 화면 확인은 아래 체크리스트에 따라 수동 부팅이 필요하다.

## VM 실행 체크리스트
1. VMware Workstation을 설치한다.
2. `C:\Users\ezen601\Downloads\FAIRINO_SimMachine_v3.9.3\FAIRINO_SimMachine\FAIRINO_SimMachine.vmx`를 연다.
3. `Power on this virtual machine`으로 부팅한다.
4. 같은 폴더의 `fr_get_vm_net.bat`를 실행해 VM IP를 확인한다.
5. Windows 브라우저에서 해당 IP로 접속한다.
6. 아래 화면을 우선 캡처한다.
   - 메인/Initial 화면
   - Joint Jog 화면
   - Cartesian/Base-Tool-Wobj 화면
   - TPD/Teaching 화면
   - I/O 또는 Gripper 화면

## 프런트엔드 기술 스택 추정
- AngularJS SPA
- Bootstrap + AdminLTE 기반 레이아웃
- Three.js + URDFLoader + OrbitControls
- Blockly + Ace editor
- 다국어 JSON

즉 SimMachine은 단순 제어 패널이 아니라:
- 3D 시뮬레이션
- 프로그램 코딩
- 그래픽 프로그래밍
- 노드 그래프
- teaching/TPD
- 주변장치
- 시스템/안전 설정
이 하나의 웹앱 안에 같이 들어 있는 구조다.

## 엔트리 포인트
- `frontend/index.html`
  - 메인 SPA 진입점
  - `ng-app="frApp"`
  - `ng-controller="indexCtrl"`
- `frontend/login.html`
  - 로그인 전용 페이지

## 화면 라우트 구조
`web/frontend/js/pages/index.js`의 `routeConfigFn($routeProvider)` 기준:

| route | template | 해석 |
|---|---|---|
| `/monitor` | `pages/monitor.html` | 메인 운전/모니터링 화면 |
| `/programteach` | `pages/program_teach.html` | 코드 기반 teaching/programming |
| `/teachingmanagement` | `pages/teaching_management.html` | teaching point / TPD / 관리 화면 |
| `/graphicalprogramming` | `pages/graphical_programming.html` | 그래픽 프로그래밍 |
| `/nodeeditor` | `pages/node_editor.html` | 노드 그래프 프로그래밍 |
| `/peripheral` | `pages/peripheral_setting.html` | 주변장치 / gripper / IO / axle |
| `/safeset` | `pages/safeset.html` | 안전 설정 |
| `/robotsetting` | `pages/robot_setting.html` | 로봇 설정 |
| `/systemsetting` | `pages/system_setting.html` | 시스템 설정 |
| `/auxiliary` | `pages/auxiliary_application.html` | 보조 기능 / 앱류 |
| `/process` | `pages/process.html` | 공정 패키지 / 프로세스 |
| `/log` | `pages/log.html` | 로그 |
| `/frcap` | `pages/frcap.html` | FRCap 진입 |
| `/frcap-app/:id` | `pages/frcap_app.html` | 특정 FRCap 앱 |

## 구조적으로 중요한 관찰
- `/monitor`와 `/teachingmanagement`만 `showRobotSettingFixed()`를 호출한다.
- 즉 제조사 WebApp도 모든 페이지에서 같은 3D 조작 패널을 띄우는 게 아니라,
  - `모니터링/직접 조작`
  - `티칭 관리`
  같은 특정 화면에서만 고정 조작 패널을 강화하는 구조일 가능성이 크다.

## 메인 화면 역할 추정

### 1. Monitor
- 사실상 제조사 `Initial`/메인 운전 화면에 가깝다.
- 3D 로봇 뷰
- 상태 바
- 현재 위치/속도/모드
- Base/Tool/Wobj 조작
- joint/tcp 이동
- I/O / TPD / gripper 진입

이 화면이 Unity `RobotControl`이 가장 많이 참고해야 하는 원본에 가깝다.

### 2. Teaching Management
- teaching point, TPD, 시뮬레이션 재현, trajectory 편집 쪽의 관리 허브로 보인다.
- `TPD record`, `TPD edit`, `setTPDLocation`, `TPDCfgDI`, `TPDCfgDO` 같은 키가 함께 존재한다.

### 3. Program Teach / Graphical Programming / Node Editor
- 제조사 패드는 단순 jog UI가 아니라 `프로그램 제작` 기능을 별도 큰 화면으로 분리하고 있다.
- 따라서 Unity V1에서는 이 기능을 억지로 같은 화면에 넣지 말고, 후속 범위로 빼는 게 맞다.

### 4. Peripheral / Safety / Robot/System Setting
- 주변장치, 안전, 로봇 설정, 시스템 설정이 개별 라우트로 분리되어 있다.
- 제조사도 운영 기능과 조작 기능을 물리적으로 분리하고 있다는 점이 중요하다.

## ko.json에서 드러나는 조작 구조
다국어 키를 보면 메인 조작 패널의 핵심 묶음이 꽤 선명하다.

### 좌표/이동
- `Base`
- `Tool`
- `Wobj`
- `_joint_space_motion`
- `_single_joint_move`
- `_multi_joints_move`
- `_compute_joints_pose`
- `_current_robot_drag_status`

### 모드/상태
- `_auto_mode`
- `_manual_mode`
- `_drag_mode`
- `_running_speed`
- `_target_speed`
- `_safe_speed_mode`

### 티칭/TPD
- `_tpd_record`
- `_tpd_edit`
- `setTPDLocation`
- `TPDCfgDI`
- `TPDCfgDO`
- `TPD 추적`

### 주변장치
- `_setup_io`
- `_io_cfg`
- `_gripper`
- `_activate_gripper`
- `SmartTool DO`

즉 제조사 메인 UX는 크게:
- `직접 조작`
- `상태/모드`
- `TPD/Teaching`
- `I/O/Gripper`
를 한 작업 공간 안에서 빠르게 넘나드는 형태로 보인다.

## IA 초안

### Tier 1
- 메인 조작
- 프로그래밍
- 티칭 관리
- 주변장치
- 안전
- 시스템
- 로그

### Tier 2
- 메인 조작
  - Base / Tool / Wobj
  - Joint jog
  - Cartesian move
  - Drag
  - 상태/속도
- 티칭 관리
  - 포인트
  - TPD record
  - TPD edit
  - simulation reproduction
- 주변장치
  - I/O
  - gripper
  - external axis
  - SmartTool

## Unity에 가져올 때의 해석

### 참고해야 할 것
- `3D를 중앙에 크게 두는 것`
- `좌측에 현재 작업 패널을 두는 것`
- `Base / Tool / Wobj` 탭 구조
- `상단 상태/속도/실행 상태`
- `우측 퀵 메뉴 또는 세로 툴바`

### 그대로 베끼지 말아야 할 것
- 너무 많은 기능을 한 화면에 노출하는 방식
- 초보자에게 과밀한 수치/설정 중심 구조
- 운영자용 기능과 입문자용 기능을 같은 밀도로 보여주는 방식

## Unity용 재구성 제안

### 우리 V1에 직접 반영할 부분
- 중앙 대형 3D
- 좌측 `쉬운 조작 / 관절 / TCP / 티칭` 탭
- `Base / Tool / Wobj` 서브탭
- 상단 연결/상태/Stop

### 우리 쪽에서 더 좋아져야 하는 부분
- ghost preview
- 예상 경로
- 위험 사전 경고
- Why It Moved
- 친절한 한국어 복구 팝업
- 태블릿 큰 버튼 모드

## 확인되면 좋은 실제 화면 캡처 우선순위
1. `monitor.html` 메인 화면
2. `teaching_management.html`
3. `peripheral_setting.html`
4. `safeset.html`

이 4개면 `V1에 가져올 구조`와 `후속 운영 기능`의 경계가 거의 결정된다.

## 현재 결론
- 패키지 분석만으로도 SimMachine은 `3D 중심 조작 + teaching/TPD + 주변장치 + 안전/시스템`이 분리된 대형 SPA 구조임을 확인했다.
- Unity `RobotControl`은 이 중 `monitor` 성격의 핵심 조작 경험만 먼저 가져오고, 나머지는 단계적으로 붙이는 것이 맞다.
- 실제 VM 화면 확인은 이 초안 문서를 시각적으로 보정하는 마지막 단계다.

## 실제 캡처 반영 메모
- 실제 SimMachine 캡처에서 아래 구조가 확인되었다.
  - 좌측 대메뉴: `초기 설정`, `프로그램`, `상태 정보`, `보조 응용`, `시스템 설정`
  - 상단 제어 바: `Start`, `Stop`, `Pause`, 상태 `Stopped`, 속도 `30`
  - 좌측 컨텍스트 패널: `Cartesian Move`, `Single Joint Jog`, `Multi Joint`, `Point Move`
  - 중앙 대형 3D 로봇 뷰
  - 상단 3D 모드 툴바
  - 우측 세로형 기능 진입 바
  - 하단 보조 모듈 바: `I/O`, `TPD`, `FT`, `RCM`
- 따라서 Unity 설계에서도 `중앙 3D + 좌측 현재 작업 패널 + 상단 상태 + 하단 모듈 바` 구조를 기본으로 삼는 것이 타당하다.

## 한글 표시 점검 메모
- 패키지 내부에는 `web/file_factory/file/frontend/lang/ko.json`이 실제로 존재한다.
- `login.html` 기준 언어 선택 드롭다운이 있고 `한국어` 항목도 코드상 확인된다.
- `login.js`에는 `set_sys_language` 호출과 `langCode`, `langJsonData` 저장 로직이 존재한다.
- 따라서 SimMachine WebApp은 구조적으로 한국어 지원이 가능하며, 현재 화면 깨짐은 `한국어 리소스 부재`보다는 `현재 세션 언어/폰트/렌더링 문제`일 가능성이 높다.
- 우선 점검 순서:
  1. 로그인 화면 우하단 지구본 메뉴에서 `한국어` 선택
  2. 새로고침 후 유지 여부 확인
  3. 시스템 설정의 `시스템 언어` 항목 확인
  4. 필요 시 VM 내 한글 폰트 상태 확인
