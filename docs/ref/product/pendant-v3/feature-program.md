# 프로그램 편집/실행

## Purpose
- Step 실행, 전체 실행, 기본 블록 삽입의 UI를 정의한다.
- **확정**: 제조사 Lua 프로그램 load/run은 제외 유지. Unity 내부 시퀀스 실행만 Phase 5 블록 에디터로 포함.

## Parent Doc
- [README.md](./README.md)

## Last Updated
- 2026-04-22 (KST)

## SSOT 상태
- **SSOT 분리**: 제조사 Lua/program load/run은 계속 제외한다.
- Unity 내부 `PendantV3Points` teaching sequence 실행은 [teaching-sequence-execution-plan.md](./teaching-sequence-execution-plan.md)에서 관리한다.
- V3 왼쪽 Nav에 별도 `Program` 탭은 추가하지 않는다. 실행/시퀀스 기능은 `NavPoints` 내부 subview로 확장한다.

---

## 현재 SSOT 제외 이유
- 초기 bring-up 경로의 복잡도 관리
- 제조사 Lua 프로그램 시스템과의 경계 정리가 안 됨
- 교육용 앱에서 프로그래밍은 2차 목표

## V3 포함 시 최소 범위 제안

### BottomBar 프로그램 제어
```text
┌─ BottomBar 프로그램 영역 ────────────────────┐
│                                               │
│  [▶ 실행]  [■ 정지]  [⏸ 일시정지]           │
│  [⏮ 이전 스텝]  [다음 스텝 ⏭]              │
│                                               │
│  현재: 스텝 3/8  "MoveJ → Pick-1"            │
│  상태: 실행 중...                              │
│                                               │
└───────────────────────────────────────────────┘
```

### 프로그램 뷰어 (읽기 전용 최소)
```text
┌─ 프로그램 뷰어 ──────────────────────────────┐
│                                               │
│  1.   MoveJ → Home                           │
│  2.   MoveJ → Pick-1                         │
│  3. ▶ Wait 0.5s          ← 현재 실행 라인    │
│  4.   IO: 그리퍼 닫기                         │
│  5.   MoveL → Place-1                         │
│  6.   IO: 그리퍼 열기                         │
│  7.   MoveJ → Home                           │
│                                               │
│  ▶ = 현재 실행 중인 스텝 (AccentPrimary 배경) │
│                                               │
└───────────────────────────────────────────────┘
```

### 편집은 티칭 시퀀스에서
- 프로그램 편집 = [feature-points-teaching.md](./feature-points-teaching.md)의 시퀀스 편집
- 여기서는 실행/모니터링만 담당
- 이렇게 분리하면 SSOT "프로그램 고급기능 제외" 규칙과 충돌하지 않음

---

## 확정 사항
1. 제조사 Lua 프로그램 load/run → **제외** (SSOT 유지)
2. Unity 내부 시퀀스 실행 → **Phase 5 블록 에디터**로 포함
3. 실행 모니터링(BottomBar Step/Play/Stop) → `PendantV3Points` sequence first slice에서 먼저 구현
4. 제조사 프로그램 연동은 **P2 이후** 별도 검토
