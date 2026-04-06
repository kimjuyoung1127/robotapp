# Pendant V3 unityctl Recipes

## Date
- 2026-04-06 (KST)

## Summary
- Teaching Pendant V3 문서에 `unityctl` 명령 레시피를 구조적으로 추가했다.
- 목적은 사람과 AI 에이전트가 같은 세션 부트스트랩, 같은 검증 루프, 같은 증빙 산출물로 움직이게 만드는 것이다.

## Updated Docs
- `docs/ref/product/pendant-v3/unityctl-recipes.md`
- `docs/ref/product/pendant-v3/implementation-plan.md`
- `docs/ref/product/pendant-v3/README.md`

## Added Recipes
- Session Bootstrap
- Recipe 0A: Infrastructure Assets
- Recipe 0B-1C: Shell And Layout
- Recipe 2A-2D: Panel UI
- Recipe 3A-3C: Binder And Mock Flow
- Recipe 3D Viewport Verification
- Diagnostics Recipe
- Preflight Recipe
- Workflow Verify Bundle

## Why
- V3는 UI Toolkit 셸, 3D viewport, 입력, 포커스, Mock 종단 검증이 모두 얽혀 있어 `무슨 명령을 어떤 순서로 쓸지`를 문서에 고정할수록 회귀 범위가 줄어든다.
- 특히 `editor select`, `status`, `check`, `scene snapshot`, `screenshot capture`, `project validate`, `doctor`, `build --dry-run`, `workflow verify`를 Phase별로 고정한 것이 핵심이다.
