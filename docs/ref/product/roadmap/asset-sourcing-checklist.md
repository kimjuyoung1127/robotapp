# Asset Sourcing Checklist

## Purpose
- 프로젝트에 필요한 3D 에셋(로봇 모델, 환경, UI 아이콘 등)의 소싱 상태를 추적한다.

## Parent Doc
- [PRODUCT-ROADMAP](../../PRODUCT-ROADMAP.md)

## When To Read
- 새 로봇 모델 또는 에셋을 추가하려 할 때
- 에셋 라이선스/출처를 확인할 때

## Locked Decisions
- 에셋 소싱 우선순위는 `Open Source > Asset Store (Free) > Custom` 순서를 따른다
- 모든 에셋은 라이선스 호환성 확인 후 등록한다
- 에셋 큐레이션 결과는 `docs/ref/asset-curation-map.md`에 기록한다

## Downstream Sync
- `docs/ref/asset-curation-map.md`
- `docs/ref/asset-validation-report.md`
- `docs/ref/asset-registry.md`

## Last Updated
- 2026-03-12 (KST)

## 3D Robot Models

| 로봇 | 소스 | 라이선스 | 상태 | 비고 |
|------|------|---------|------|------|
| SCARA (generic) | ScaraRobot.prefab (내장) | Internal | Done | donor mesh 기반 렌더링 |
| Fanuc CRX-10iA/L | 미정 | - | Pending | URDF 기반 import 검토 중 |
| igus REBEL | 미정 | - | Pending | 교육용 로봇, 공개 모델 탐색 필요 |
| UR5 | 미정 | - | Pending | universal_robot URDF 공개 |
| Puma 560 | 미정 | - | Pending | classic reference, 공개 모델 다수 |
| Franka Emika Panda | 미정 | - | Pending | franka_description 공개 |

## UI / Icon Assets

| 에셋 | 소스 | 상태 |
|------|------|------|
| Navigation icons | Unity built-in | Done |
| Robot card thumbnails | placeholder color | Done (MVP) |
| Lesson step illustrations | 미정 | Pending |

## Environment Assets

| 에셋 | 소스 | 상태 |
|------|------|------|
| Grid floor | URP default | Done |
| Workspace envelope mesh | 미정 | Pending |
