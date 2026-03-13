# Module Log: P0/P1 Reprioritization (Home + Sandbox)

Date: 2026-03-12 (KST)

## Summary
- 실제 파일 기준으로 `Robot Library MVP`, `SCARA`, `Sandbox baseline`은 이미 존재하는 기반으로 재분류했다.
- 차기 우선순위를 `Home / Continue Hub -> Sandbox polish -> resume / session context -> tablet 4DOF input usability -> snapshot lite`로 재정렬했다.
- 온보딩 `건너뛰기`는 target 기준 late-step direct jump 대신 `Home / Continue Hub`로 연결하도록 정책을 수정했다.

## Why
- 현재 런타임은 `Boot -> Onboarding -> Main` 중심이지만, 실제 파일에는 `RobotLibrary.unity`, `Sandbox.unity`, `SCARA` 지원이 이미 들어와 있다.
- 따라서 다음 우선순위는 새 로봇 추가보다 `재진입 허브`, `샌드박스 완성도`, `멀티로봇 복귀 맥락`, `태블릿 4DOF 사용성` 쪽이 더 중요하다.

## Updated Docs
- `docs/ref/WIREFRAME.md`
- `docs/ref/PRODUCT-ROADMAP.md`
- `docs/ref/USER-FLOW.md`
- `docs/ref/tutor-step-plan.md`
- `docs/ref/product/ux/information-architecture.md`
- `docs/ref/product/ux/guided-lesson.md`
- `docs/ref/product/ux/sandbox.md`
- `docs/ref/product/ux/tablet-first-policy.md`
- `docs/ref/product/roadmap/current-feature-checklist.md`
- `docs/ref/product/roadmap/milestone-backlog.md`
- `docs/ref/product/roadmap/release-gates.md`
- `docs/ref/phase5-implementation-plan.md`
- `docs/status/PROJECT-STATUS.md`
- `docs/status/PHASE-EXECUTION-BOARD.md`
- `docs/status/PRODUCT-DOC-BOARD.md`
- `CLAUDE.md`
- `ai-context/master-plan.md`

## Notes
- 이번 패스는 문서 동기화만 수행했다.
- 현재 runtime fallback의 `Onboarding -> Core Step 8` 경로는 남아 있지만, 문서상 차기 target은 `Home / Continue Hub`로 잠갔다.
