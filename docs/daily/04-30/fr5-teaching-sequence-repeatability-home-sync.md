# FR5 Teaching Sequence Repeatability Home Sync

Date: 2026-04-30 (KST)

## Summary

- teaching point live SSOT와 skill에 `save / apply / run` 앞 `Sync + RefreshLiveEvidence` 확인 단계를 고정했다.
- operator가 manual mode에서 실제 로봇 자세를 바꾼 경우, 그 synced pose를 먼저 `Home` point로 저장한 뒤 `2-point / multi-axis` repeatability를 시작하는 규칙을 추가했다.
- 같은 날짜 후속 field run에서 Unity/live를 다시 세운 뒤 `Home` 기준 custom repeatability live re-smoke까지 닫았다.

## Why

- teaching repeatability는 point 저장/호출 성공만으로는 부족하고, 각 단계에서 Unity pose가 최신 controller readback과 실제로 맞는지 확인해야 안전하다.
- manual mode에서 로봇을 직접 움직인 뒤 바로 sequence를 저장/실행하면, stale pose가 저장되거나 이전 기준점으로 돌아갈 위험이 있다.

## Locked Procedure

1. `현재 위치 읽기`
2. `SyncCurrentStateForDebug()`
3. `RefreshLiveEvidenceForDebug()`
4. Unity joints/TCP와 refreshed `latest-state.json`이 일치하는지 확인
5. manual mode에서 바뀐 현재 자세를 `Home` point로 저장
6. 그 다음에만 `2-point` 또는 `multi-axis` teaching sequence repeatability 시작
7. 각 `save / apply / run` 뒤 다시 `Sync + RefreshLiveEvidence`

## Session Reality

- `latest-state.json`에는 direct live truth가 남아 있었고, current pose snapshot도 읽을 수 있었다.
- current manual pose를 기준으로 아래 staging 이름을 waypoint 저장소에 심어뒀다.
  - point: `QA0430_MANUAL_HOME_T6`
  - sequence: `QA0430_HOME_2PT_T6`
  - sequence: `QA0430_HOME_MULTIAXIS_T5_T6`
- 초기에는 Unity IPC 복구 이슈로 같은 세션 재확인이 막혔지만, 후속 재기동 뒤 아래 순서를 실제로 다시 태웠다.
  - `QA0430_MANUAL_HOME_T6` recall
  - `QA0430_HOME_2PT_T6` live 1회 실행
  - `QA0430_HOME_MULTIAXIS_T5_T6` live 1회 실행
  - 각 단계 `post-sync + RefreshLiveEvidence`
- `QA0430_HOME_2PT_T6`는 same-session 2회 반복으로 green을 확인했다.
- `QA0430_HOME_MULTIAXIS_T5_T6`는 confirm 직후 즉시 summary보다 `sleep + post-sync` truth가 중요했고, 최종 post-sync에서는 `Home` joints로 복귀를 확인했다.
- 따라서 오늘 기준 custom `Home` repeatability는 `one-shot live green`으로 본다.

## Next

- next:
  - broad named sequence generalization 여부 판단
  - live loop는 계속 잠금 유지
  - 필요하면 multi-cycle repeatability를 추가로 쌓기
