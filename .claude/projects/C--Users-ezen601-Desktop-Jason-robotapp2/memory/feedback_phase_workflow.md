---
name: phase-workflow-review-gate
description: 각 Phase별로 자기리뷰 게이트를 두고, 통과 후 문서 업데이트 → 다음 Phase 진행. 계획은 Opus, 코딩은 Sonnet도 사용
type: feedback
---

각 구현 Phase 완료 시 자기리뷰(self-review) 단계를 반드시 거친다.
자기리뷰 통과 후 관련 문서를 업데이트하고 나서야 다음 Phase로 진행한다.

**Why:** 주인님이 단계별 품질 게이트를 요구. 한꺼번에 전부 바꾸고 나중에 수습하는 패턴 방지.

**How to apply:**
- Phase 완료 → compile check + 자기리뷰 → 문서 업데이트 → 다음 Phase
- 계획/설계: Opus 사용
- 코딩 구현: Sonnet agent도 활용 가능 (model: "sonnet" 파라미터)
- 자기리뷰에서 문제 발견 시 해당 Phase 내에서 수정 완료 후 진행
