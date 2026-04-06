# Teaching Pendant V3 Agent Contract

## Purpose
- Teaching Pendant V3를 수정하는 AI 에이전트가 반드시 따라야 하는 작업 계약을 고정한다.
- 이 문서는 V3 문서 묶음의 운영 강제력을 높이기 위한 실행 규약이다.

## Parent Docs
- [README.md](./README.md)
- [implementation-plan.md](./implementation-plan.md)
- [unityctl-recipes.md](./unityctl-recipes.md)

## Related Docs
- [migration-strategy.md](./migration-strategy.md)
- [static-checks.md](./static-checks.md)
- [verify-v3.json](./verify-v3.json)

## Last Updated
- 2026-04-06 (KST)

---

## Mandatory Read Order

V3를 구현하거나 수정하는 에이전트는 아래 순서로 읽고 시작한다.

1. [README.md](./README.md)
2. [AGENT-CONTRACT.md](./AGENT-CONTRACT.md)
3. [implementation-plan.md](./implementation-plan.md)
4. [unityctl-recipes.md](./unityctl-recipes.md)
5. 현재 작업 범위와 직접 연결된 feature 문서

---

## Scope Contract

- 작업 시작 전에 현재 작업이 어느 실행 단위(`0A`~`3C`, `4` 이후)인지 먼저 선언하고 그 범위 안에서만 수정한다.
- 서로 다른 실행 단위를 한 커밋에 섞지 않는다.
- `Phase 4` 채택 결정 전에는 V3를 주 경로로 승격하지 않는다.
- V3 작업 중 기존 V2/uGUI 경로를 임의로 폐기하지 않는다.
- 실행 단위는 `implementation-plan.md`의 `Phase Loop Governance` 순서대로 닫는다.

---

## Verification Contract

- V3 관련 수정 후에는 [unityctl-recipes.md](./unityctl-recipes.md)의 해당 레시피를 반드시 실행한다.
- 세션 시작은 항상 `Session Bootstrap`을 먼저 수행한다.
- `3C` 완료를 주장하려면 [verify-v3.json](./verify-v3.json) 기본 번들에 더해 `Recipe 3A-3C`, `Recipe 3D Viewport Verification`, desktop/tablet screenshot loop를 함께 통과해야 한다.
- `Phase 4` 채택 평가 전에는 최소 `Preflight Recipe`를 통과해야 한다.

---

## Completion Contract

에이전트는 아래 조건을 만족하지 않으면 “완료”라고 말할 수 없다.

1. 현재 실행 단위가 문서상 종료 조건을 만족함
2. 해당 `unityctl` 레시피 실행 결과를 확인함
3. 새 콘솔 치명적 에러가 없음
4. 문서 변경이 발생했다면 같은 턴에 `docs/daily/MM-DD/` 로그를 남김
5. parity review에서 문서/씬/빌드/검증 경로가 어긋나지 않음을 확인함

---

## Forbidden Actions

- `UnityEngine.Input` 직접 사용
- 문서에 잠긴 `PanelSettings`, `InputSystemUIInputModule`, 포커스 규칙, 3D 경계를 임의 변경
- 현재 범위와 무관한 `Phase 5+` 기능 선행 구현
- `implementation-plan.md` 종료 조건을 무시한 완료 주장
- `unityctl` 검증 없이 화면이 된다고 가정하고 마무리
- `PendantV3Binder`에 상태 계산, 위험도 계산, feature 간 orchestration을 누적해서 만능 조정자로 만드는 것
- 하나의 Controller가 둘 이상의 패널 책임을 겸하도록 키우는 것
- `MockFairinoClient`와 `LiveFairinoClient`의 동작 의미를 임의로 다르게 만드는 것

---

## Required Evidence

작업 범위에 따라 아래 증빙 중 필요한 것을 남긴다.

| 범위 | 필수 증빙 |
|------|------|
| `0A` | compile + project validate |
| `0B~1C` | scene snapshot + screenshot |
| `2A~2D` | screenshot + console entries |
| `3A~3C` | edit tests + play smoke + screenshot |
| `4` | preflight + 비교 평가 표 |

---

## Static Check Contract

- 코드/문서 수정 전후로 [static-checks.md](./static-checks.md)의 규칙을 확인한다.
- 가능하면 아래 명령으로 실제 정적 체크를 실행한다.

```powershell
pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1
```

- 자동화되지 않은 정적 체크라도, 위반 가능성이 있으면 응답에서 직접 언급한다.

---

## Escalation Rule

아래 상황이면 에이전트는 바로 멈추고 범위를 재확인해야 한다.

- 현재 작업이 어느 실행 단위인지 문서상 불명확함
- 새 UI 변경이 `PanelSettings`, `Input`, `Focus`, `3D boundary` 잠금 규칙과 충돌함
- `unityctl` 검증 결과가 문서 종료 조건과 모순됨
- feature 문서와 implementation plan의 Phase 매핑이 어긋남
- ViewState가 비대해져 패널마다 필요 없는 필드까지 강하게 결합되기 시작함
- 새 코드가 SRP/OCP/DIP를 깨는 방향으로 concrete 의존을 늘림
