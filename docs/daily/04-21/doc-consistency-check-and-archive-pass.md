# Doc Consistency Check And Archive Pass

## Summary

- 현재 SSOT와 충돌하거나 legacy로 내려간 문서 4개를 `docs/archive/legacy/`로 이동했다.
- active 문서에서 archive 대상 경로를 직접 가리키던 참조도 현재 기준에 맞게 정리했다.

## Archived Docs

- `docs/ref/cli-tools-guide.md` -> `docs/archive/legacy/unity-cli/cli-tools-guide.md`
- `docs/ref/unity-cli-improvement-backlog.md` -> `docs/archive/legacy/unity-cli/unity-cli-improvement-backlog.md`
- `docs/ref/product/ux/page-quality-baseline.md` -> `docs/archive/legacy/page-qa/page-quality-baseline.md`
- `docs/status/PAGE-QA-MATRIX.md` -> `docs/archive/legacy/page-qa/PAGE-QA-MATRIX-2026-03-23.md`

## Why

- `cli-tools-guide`, `unity-cli-improvement-backlog`는 현재 저장소 운영 기준상 `unityctl` 이전 legacy lane이라 active ref에 둘 이유가 약했다.
- `page-quality-baseline`, `PAGE-QA-MATRIX`는 `Home/Main`과 2026-03-23 기준 baseline이 섞여 있어서 현재 runtime/QA 기준과 충돌했다.
- 현재 active QA 기준은 `docs/status/page-qa/README.md`와 page별 runbook 묶음으로 보는 게 더 맞다.

## Reference Updates

- `AGENTS.md`
- `docs/ref/WIREFRAME.md`
- `docs/ref/product/roadmap/current-feature-checklist.md`
- `docs/status/PHASE-EXECUTION-BOARD.md`
- `docs/status/page-qa/README.md`
- `docs/status/PROJECT-STATUS.md`
- `docs/daily/03-12/module-page-qa-matrix-baseline.md`

## Check Result

- archive 대상 4개는 새 경로에서 확인됨
- old active 경로 참조 grep 결과 0건 확인
- `architecture-diagrams.md`는 아직 현재 구조와 일부 historical note를 함께 담고 있지만 active 참조가 많고 2026-03-31 업데이트 이력이 있어 이번 archive 대상에서는 제외했다.
