---
name: waypoint-teaching
description: "Use when adding, extending, or debugging Waypoint Teaching & Playback: saving robot poses as waypoints, sequencing them, playing/looping sequences, exporting/importing JSON, coroutine-based runner engine, or teaching UI integration."
---

## Trigger
아래 요청에서 사용:
- `웨이포인트 저장/재생`
- `티칭 플레이백`
- `시퀀스 재생/루프/정지`
- `포즈 익스포트/임포트`
- `웨이포인트 러너 디버깅`
- `Teaching UI 확장`

## Input Context
- 대상 로봇의 관절 수 (FR5 = 6축)
- Mock/Live 모드 여부
- 기존 WaypointStore/WaypointCycleRunner 코드 존재 여부

## Read First
1. `Assets/Scripts/App/Fairino/WaypointStore.cs` — 데이터 모델 + JSON 영속
2. `Assets/Scripts/App/Fairino/WaypointCycleRunner.cs` — 코루틴 재생 엔진
3. `Assets/Scripts/App/Fairino/PresetTransitionAnimator.cs` — 보간 애니메이터 (Runner가 의존)
4. `Assets/Scripts/UI/FairinoJointControlPanel.cs` — Teaching 섹션 UI
5. `Assets/Scripts/App/Fairino/RobotControlSceneCoordinator.cs` — 통합 핸들러

## Do

### 아키텍처 계층
```
WaypointStore (데이터)
  Waypoint { name, jointsDeg[6], tcpMm[6], moveType, speedPreset, dwellSec }
  WaypointSequence { name, created, waypoints[] }
  Save/Load/Delete/Duplicate/Rename/Export/Import
      ↕
WaypointCycleRunner (엔진, MonoBehaviour)
  PlayOnce / PlayLoop / Stop
  코루틴 기반 순차 실행
  PresetTransitionAnimator로 3D 보간
      ↕
JointControlPanel (UI)
  Teaching 섹션: Save Point / Play / Loop / Stop / Undo / Export / Import / Clear All
  8개 이벤트로 Coordinator에 위임
      ↕
RobotControlSceneCoordinator (통합)
  이벤트 바인딩, FK TCP 계산, Runner↔3D 미러 연결
```

### 코루틴 안전 규칙 (필수)
1. **StopAllCoroutines** 사용: `StopCoroutine(activeCoroutine)`는 중첩 코루틴(AnimateToWaypoint)을 정지하지 않음. 반드시 `StopAllCoroutines()`로 전체 정지.
2. **이벤트 핸들러 누수 방지**: AnimateToWaypoint에서 `presetAnimator.OnTransitionComplete`에 구독하는 핸들러를 `activeCompleteHandler` 필드에 저장하고, Stop 시 `CleanupCompleteHandler()`로 확실히 해제.
3. **매 yield 후 상태 체크**: 모든 `yield return` 이후 `if (State != RunState.Running) yield break;` 추가. Stop 응답 지연 방지.
4. **빈 시퀀스 감지**: do-while 루프 시작에서 `waypoints.Length == 0` 체크 → break. 빠지면 yield 없는 무한 루프로 Unity 프리즈.
5. **루프 사이 yield null**: do-while 매 사이클 끝에 `yield return null` 추가. UI 이벤트(Stop 버튼) 처리 기회 보장.
6. **OnSequenceComplete 호출 위치**: do-while 루프 **밖**(최종 완료 시)에서만 호출. 루프 안에서 호출하면 SetTeachingState(false)로 Stop 버튼이 비활성화됨.

### 텔레포트 방지 패턴
```csharp
private double[] lastCompletedAngles;

// RunSequence 시작 시 초기화
lastCompletedAngles = null;

// AnimateToWaypoint에서 이전 완료 위치를 시작점으로 사용
var fromAngles = lastCompletedAngles ?? GetCurrentAnglesFromAnimator();
presetAnimator.StartTransition(fromAngles, wp.jointsDeg, duration);

// 완료 후 기억
lastCompletedAngles = (double[])wp.jointsDeg.Clone();
```

### Clear All 안전 패턴
```csharp
// 반드시 Runner 정지 선행 → 데이터 삭제 → 트레일 정리 → UI 초기화
waypointRunner?.Stop();
WaypointStore.ClearWaypoints(currentSequence);
eeTrailRenderer?.Clear();
displacementArrow?.Clear();
jointControlPanel?.SetTeachingState(false);
```

### JSON 저장 형식
```json
{
  "name": "Pick-and-Place-A",
  "created": "2026-03-15T14:30:00+09:00",
  "waypoints": [
    {
      "name": "W1",
      "jointsDeg": [10, -30, 15, -45, -80, 0],
      "tcpMm": [320.1, 150.3, 480.5, 0.0, -90.0, 0.0],
      "moveType": "MoveJ",
      "speedPreset": "medium",
      "dwellSec": 0.0
    }
  ]
}
```
- 저장 경로: `Application.persistentDataPath/waypoints/{name}.json`
- tcpMm: Save Point 시 FK 자동 계산 (m→mm 변환 + ZYX 오일러각)
- moveType: "MoveJ" (관절) 또는 "MoveL" (직교)
- speedPreset: "slow" / "medium" / "fast"

### UI 이벤트 목록
| 이벤트 | 트리거 | Coordinator 핸들러 |
|---|---|---|
| OnSaveWaypointRequested | Save Point 클릭 | 현재 포즈+TCP+속도 저장 |
| OnPlayRequested | ▶ Play 클릭 | PlayOnce 실행 |
| OnLoopRequested | ⟳ Loop 클릭 | PlayLoop 실행 |
| OnTeachStopRequested | ■ Stop 클릭 | Runner.Stop + UI 초기화 |
| OnExportRequested | Export 클릭 | WaypointStore.Save |
| OnImportRequested | Import 클릭 | WaypointStore.Load |
| OnUndoLastRequested | Undo 클릭 | RemoveLast |
| OnClearAllRequested | Clear All 클릭 | Stop→Clear→Trail→UI |

## Do Not
1. `StopCoroutine(activeCoroutine)` 단독 사용 금지 → 반드시 `StopAllCoroutines()`
2. `OnSequenceComplete`를 do-while 내부에서 호출 금지
3. AnimateToWaypoint에서 `presetAnimator.OnFrameUpdated` 구독 금지 → Coordinator 직접 구독과 중복
4. `GetCurrentAnglesFromAnimator()`를 매 웨이포인트 시작점으로 사용 금지 → `lastCompletedAngles` 패턴 사용
5. Clear All에서 Runner 정지 없이 데이터만 삭제 금지 → 빈 배열 무한 루프 발생

## Validation
1. Save Point 3개 → Play → W1→W2→W3 순서 전이 확인 (텔레포트 없음)
2. Loop → 전체 순환 반복 확인 (마지막만 반복 아님)
3. Loop 중 ■ Stop → 즉시 정지 확인
4. Loop 중 Clear All → 프리즈 없이 정지+초기화 확인
5. Clear All 후 새 포인트 저장 → Play → 새 데이터만 재생 확인 (이전 데이터 잔존 없음)
6. Export → `persistentDataPath/waypoints/` JSON 파일 생성 확인
7. Import → 저장된 시퀀스 로드 + UI 리스트 갱신 확인

## Output Template
```
## Waypoint Teaching 변경 요약
- 변경 파일: {파일 목록}
- 추가/수정 기능: {기능 설명}
- 코루틴 안전 체크: StopAllCoroutines ✓ / 핸들러 해제 ✓ / 빈 시퀀스 감지 ✓
- 테스트: {Validation 체크리스트 결과}
```
