# RobotControl 소프트 티칭패드 V1 Backlog

## Purpose
- `RobotControl`을 FR5용 한국어 소프트 티칭패드 V1으로 구현하기 위한 실제 작업 순서표를 정의한다.
- 범위를 작게 고정해 `실기 핵심 + 초보자 가치 + 태블릿 사용성`만 먼저 완성한다.

## Parent Doc
- [PRODUCT-ROADMAP](../../PRODUCT-ROADMAP.md)
- [milestone-backlog.md](./milestone-backlog.md)
- [robotcontrol-soft-teaching-pad.md](../ux/robotcontrol-soft-teaching-pad.md)
- [fairino-teaching-pad-feature-matrix.md](../robots/fairino-teaching-pad-feature-matrix.md)

## When To Read
- `RobotControl` V1 구현 순서를 정할 때
- 큰 계획을 실제 태스크 묶음으로 바꿀 때
- “지금은 넣지 않을 것”을 명확히 고정할 때

## Last Updated
- 2026-04-01 (KST)

## V1 Goal
- 초보자 사용자가 `연결 -> 동기화 -> 프리뷰 -> 안전 확인 -> 작은 MoveJ/MoveL`을 한국어 UI로 수행할 수 있게 한다.
- Desktop + Tablet에서 같은 흐름이 성립해야 한다.
- 제조사 패드의 전체 복제보다 `안전한 프리뷰`, `친절한 안내`, `시각적 이해`를 우선한다.
- 기존 `RobotControl`의 조작 코어는 최대한 재사용하되, old panel 구조는 그대로 확장하지 않는다.

## V1 Scope In
- 연결/해제/Enable/Sync
- 현재 상태 표시
- MoveJ / MoveL
- `쉬운 조작` 큰 버튼 프리셋
- 초보자용 한국어 안내 팝업
- ghost preview / 예상 경로 / 위험 경고의 최소 버전
- authored-first + 디자인 토큰 정리
- 태블릿 기본 대응

## V1 Scope Out
- WebApp 프로그램 load/run/pause/resume
- 정식 TPD 기록/편집
- I/O 패널
- 그리퍼 제어
- 외부축 / force / conveyor
- 초고속 ServoJ / ServoCart 실기 제어
- 역할별 권한 체계 완성
- 다국어

## V1 필수 / 선택 / 제외

### 필수
- 연결 / 해제 / 서보 켜기 / Sync
- 연결 상태 / 모드 / Fault / Safety / Tool / Wobj / 속도 표시
- `Base / Tool / Wobj`
- TCP 조그
- 단일축 조그
- 다축 연계
- 포인트 이동
- Preview 최소 버전
- 위험 경고 최소 버전
- 한국어 안내 팝업
- Stop
- 태블릿 기본 레이아웃

### 선택
- 빠른 포인트 저장
- 이름 저장 포인트
- 포인트 목록 / 불러오기 / 삭제
- Program 탭 최소 버전
  - 포인트 기반 동작 시퀀스
  - 시뮬레이션
  - 선택 블록 상세 편집
- Status 탭 최소 버전
  - 상태 요약
  - 최근 이벤트
  - 세션 리포트
- TPD 최소 버전
- I/O 최소 버전
- 그리퍼 상태 보기
- Why It Moved
- 세션 리포트
- 강사용 데모 모드

### 제외
- Initial/Base/Safety/Peripheral의 설치형 상세 설정
- Program / Graphical / Node editor
- SimMachine식 코드 IDE 복제
- SimMachine식 시스템 로그 테이블 / 파형 조회기 복제
- Application 전체 (`Tool App`, `Process Package`)
- TPD 정식 복제
- 산업별 주변장치 세부 설정
- 안전 상세 설정 페이지
- 유지보수/보드 통신/설정 파일 화면

## Success Criteria
- `RobotControl`이 L3 이상, 태블릿 핵심 플로우는 L4에 근접한다.
- Live 위험 명령은 모두 `preview -> 확인` 단계를 거친다.
- 연결 실패, fault, safety stop, 도달 불가에서 사용자 친화 문구가 나온다.
- 초보자 모드에서 화면 밀도가 낮고 버튼 의미가 명확하다.

## V1 Backlog

| order | priority | item | why | main area | done 기준 |
|---|---|---|---|---|---|
| 1 | P0 | `RobotControl` L3 정리 | 이후 기능을 얹을 공통 바닥을 먼저 만든다 | UI 구조 | authored-first 경로 확대, 하드코딩 축소, 토큰 기준선 확보 |
| 2 | P0 | 연결 홈 재구성 | 초보자가 현재 상태를 한눈에 이해해야 한다 | Connection | 연결/모드/Drag/Tool/User/Safety/Fault를 한국어 요약 카드로 표시 |
| 3 | P0 | `쉬운 조작` 탭 추가 | slider보다 큰 버튼이 초보자에게 빠르다 | Motion UI | `Home/Ready/Folded/Zero/Sync` 큰 버튼과 설명 라벨 제공 |
| 4 | P0 | 한국어 안내/경고/복구 팝업 | 실수 방지와 불안 감소가 V1 핵심이다 | Help UX | 연결 전/Enable 전/Move 전/복구 팝업 동작 |
| 5 | P0 | Preview 최소 버전 | Unity 차별화의 핵심이다 | Visualization | ghost preview 또는 목표 오버레이, 예상 경로, 현재-목표 차이 표시 |
| 6 | P0 | 위험 사전 경고 최소 버전 | 실행 전에 위험을 먼저 알려야 한다 | Validation | 도달 불가, joint limit, 큰 포즈 차이, safety/fault 경고 표시 |
| 7 | P0 | MoveJ / MoveL 실기 플로우 정리 | 실제 조작 가치를 만드는 핵심이다 | Motion runtime | confirm dialog, speed preset, tool/user 표시와 함께 실행 |
| 8 | P0 | 태블릿 레이아웃 1차 | 정식 지원 정책과 맞춰야 한다 | Responsive UI | 3D 우선 레이아웃, 하단 탭, 큰 버튼 모드 기본 동작 |
| 9 | P1 | 초보자 도움말 패널 | 팝업 외에도 지속 설명이 필요하다 | Help UX | 선택한 버튼/패널에 따라 짧은 도움말 갱신 |
| 10 | P1 | teaching point 매니저 정리 | 제조사 대체의 다음 단계다 | Teaching | 포인트 저장/이름/불러오기/삭제/순서 보기 개선 |
| 11 | P1 | 세션 리포트 초안 | 강의/현장 복기에 바로 도움이 된다 | Diagnostics | 최근 명령, 연결 상태, fault, 저장 포인트 요약 export 가능 |
| 12 | P1 | 강사용 데모 모드 초안 | Unity다운 차별화 포인트다 | Presentation UX | 학생용 단순 화면 + 강조 오버레이 최소 버전 |

## 구현 묶음

### 묶음 A: 기반 정리
- 1. `RobotControl` L3 정리
- 2. 연결 홈 재구성

### 묶음 B: 초보자 MVP
- 3. `쉬운 조작` 탭 추가
- 4. 한국어 안내/경고/복구 팝업
- 7. MoveJ / MoveL 실기 플로우 정리

### 묶음 C: Unity 차별화
- 5. Preview 최소 버전
- 6. 위험 사전 경고 최소 버전

### 묶음 D: 배포 사용성
- 8. 태블릿 레이아웃 1차

### 묶음 E: 후속 가치
- 9. 초보자 도움말 패널
- 10. teaching point 매니저 정리
- 11. 세션 리포트 초안
- 12. 강사용 데모 모드 초안

## 추천 구현 순서
1. `RobotControl` L3 정리
2. V2 상태 계약 정리
3. Mock 기준 연결 홈 + 상태 바인딩
4. `쉬운 조작` 탭 추가
5. 한국어 팝업 시스템
6. MoveJ / MoveL 실기 플로우 정리
7. Preview 최소 버전
8. 위험 경고 최소 버전
9. Live 기준 `Connect -> Enable -> small MoveJ -> small MoveL` 검증
10. 태블릿 레이아웃 1차
11. teaching point 매니저 정리
12. 세션 리포트 초안

## 기능별 담당 경계

### App/Fairino
- 연결 수명주기
- 명령 실행
- safety/fault 해석
- preview 요청 데이터

### UI/RobotControl
- 연결 홈
- 쉬운 조작
- 팝업
- 도움말
- teaching point manager
- diagnostics/report view

### Visualization/RobotControl
- ghost preview
- 경로 프리뷰
- 위험 하이라이트
- 비교 오버레이

## 바로 구현하지 않을 것
- `Program run/pause/resume`
- `TPD 정식 복제`
- `I/O + Gripper`
- `ServoJ / ServoCart Live`
- `외부축 / FT`
- `다국어`

이 항목들은 V1을 흐릴 가능성이 크므로, V1 완료 전까지 backlog 상단으로 올리지 않는다.

## 검증 방식
- Compile clean
- Mock 기준 `연결 -> Sync -> 쉬운 조작 -> Preview -> Move` 수동 검증
- Live 기준 `Connect -> Enable -> small MoveJ -> small MoveL` 현장 검증
- Tablet 해상도 수동 검증
- 위험 팝업 시나리오 검증

## 구현 해석 메모
- `UI 먼저`는 화면 mockup만 먼저 만드는 전략이 아니다.
- 가장 안전한 흐름은 `셸 -> 상태 계약 -> Mock 연결 -> Motion -> Preview -> Live -> Tablet`이다.
- 따라서 V2는 `완성된 기존 조작기 위에 새 UI만 얹는 작업`이 아니라, `기존 조작 코어를 재사용하면서 제품 구조를 다시 조립하는 작업`으로 본다.

## One-Line Product Cut
- V1은 `초보자도 안 무서운 한국어 FR5 소프트 티칭패드`를 만드는 범위로 고정한다.
