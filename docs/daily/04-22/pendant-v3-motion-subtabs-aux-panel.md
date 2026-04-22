# Pendant V3 Motion Subtabs Aux Panel

## 변경 요약

- `NavMotion`의 조작 모드를 상단 독립 WorkTabBar가 아니라 `ViewportHost > ControlDockHost` 내부 subtab으로 이동했다.
- 사용자 노출 라벨은 `기본 / 관절 / TCP / 좌표`로 단순화했다.
- 보조패널 표시 순서는 `조작 subtab/선택 모드 콘텐츠 -> TCP 3D 방향조작 -> 보기 옵션(Base/Tool/Path/Ghost/Bound/Coll/Cam) -> 설명/선택 파츠 정보`로 잠갔다.
- Desktop 폭 우선순위는 `메인패널 > 보조패널 > 컨텐츠패널`로 잠갔다.
- `NavPoints`에서는 조작 subtab을 숨기고, 티칭 전용 `포인트 / 시퀀스 / 함수` subview만 보이게 잠갔다.
- `기본`은 EasyMotion, `좌표`는 기존 직접 좌표 이동(PointMove) 경로다.

## 검증

- `unityctl check --type compile`: PASS
- `RunAuxPanelOrderMatrixForDebug()`: `2/2 PASS`
- `GetPanelWidthHierarchySummaryForDebug()`: `main=489.3; aux=360.0; context=320.0; hierarchy=main>aux>context`
- `RunMotionTabExposureMatrixForDebug()`: `6/6 PASS`
- `RunPointMoveSurfaceSeparationMatrixForDebug()`: `2/2 PASS`
- `RunTeachingSubviewActualClickMatrixForDebug()`: `13/13 PASS`
- `RunTeachingSequenceMatrixForDebug()`: `34/34 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `8/8 PASS`
- `RunTeachingPathRecordingLoopMatrixForDebug()`: PASS
- `RunTabletBottomActualClickMatrixForDebug()`: `16/16 PASS`
- `RunTcpJogVisualMotionMatrixForDebug()`: PASS
- `RunRobotLinkedButtonSimulationAuditForDebug()`: `74/74 PASS`
- `RunActualUiClickMatrixForDebug()`: `113/113 PASS`

## 실제 QA 리스트

1. `조작` 클릭 시 보조패널 첫 줄에 `기본 / 관절 / TCP / 좌표`가 보이는지 확인.
2. `조작` 보조패널 순서가 `기본/관절/TCP/좌표 -> 선택 조작 UI -> 3D방향조작 -> Base/Tool/Path/Ghost/Bound/Coll/Cam -> 설명/선택 파츠 정보`인지 확인.
3. `기본`에서 Home/Ready/Folded/Zero 미리보기 후 적용 시 로봇이 움직이는지 확인.
4. `기본`에서 그리퍼 열기/닫기 버튼 feedback과 visual 상태가 바뀌는지 확인.
5. `관절`에서 J1 슬라이더를 움직이면 고스트/경로가 나오고, 적용 시 현재 로봇 자세가 바뀌는지 확인.
6. `관절`에서 `단일축 조그` 모드로 바꾸고 J1-/J1+ 버튼이 preview를 바꾸는지 확인.
7. `TCP`에서 X+ 미리보기/적용 시 로봇 TCP 값과 3D 로봇 위치가 바뀌는지 확인.
8. `TCP`에서 Base/Tool/User 좌표 버튼을 눌렀을 때 좌표계 chip/summary가 바뀌는지 확인.
9. `좌표`에서 MoveJ/MoveL을 전환하고 좌표 직접 입력 미리보기/적용이 되는지 확인.
10. 좌측 `포인트` 클릭 시 `기본 / 관절 / TCP / 좌표`는 사라지고 `포인트 / 시퀀스 / 함수`만 보이는지 확인.
11. `포인트`에서 현재 위치 저장 후 목록 row의 이동/미리보기/수정 버튼이 보이는지 확인.
12. `포인트 > 함수`에서 포인트 후보 추가, 후보 초기화, 함수 만들기, 함수 실행, 함수 선택부터 실행, 이름 변경, 복사, 삭제가 각각 동작하는지 확인.
13. `포인트 > 시퀀스`에서 `기록 시작` 클릭 후 샘플 카운트가 갱신되는지 확인.
14. 조작 탭에서 TCP/관절 이동을 몇 번 만든 뒤 `포인트 > 시퀀스 > 기록 중지`를 눌렀을 때 저장 샘플 수가 2개 이상인지 확인.
15. `기록 재생`은 1회 재생으로 끝나는지, `기록 루프`는 반복 재생되고 Stop으로 멈추는지 확인.
16. `반복 ON/OFF` 시퀀스 루프와 `기록 루프` 버튼을 서로 헷갈리지 않게 label/feedback이 구분되는지 확인.
17. 태블릿 하단 탭에서 쉬운조작/관절/TCP/포인트/I/O/상태/도움이 모두 눌리는지 확인.
18. 태블릿 각 하단 탭 클릭 시 sheet title/summary/content가 해당 탭과 맞게 바뀌고, 다시 돌아왔을 때 이전 조작 상태가 꼬이지 않는지 확인.
19. `조작 > 좌표`는 직접 좌표 이동 화면이고, 좌측 `포인트`는 티칭 포인트 저장/시퀀스/함수 화면이라는 제목/내용 차이가 명확한지 확인.

## 남은 주의점

- Live 연속 hold jog는 아직 열지 않는다. v1은 Unity/Mock preview/apply 중심이다.
- 콘솔에 남는 `unityctl IPC connection error`는 테스트 러너 잡음으로 보고, 프로젝트 compile/runtime failure로 보지 않는다.
