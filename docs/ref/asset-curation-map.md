# Asset Curation Map

## Purpose
- 프로젝트 에셋의 큐레이션 상태를 시각적으로 추적한다.
- 어떤 에셋이 사용 중이고, 어떤 것이 백업/제외 대상인지 한눈에 파악한다.

## Source
- 큐레이션 기준: `docs/ref/product/roadmap/asset-sourcing-checklist.md`
- 검증 결과: `docs/ref/asset-validation-report.md`

## Last Updated
- 2026-03-12 (KST)

## Curation Status

### Active Assets (사용 중)

| 에셋 경로 | 용도 | 상태 |
|-----------|------|------|
| `Assets/Prefabs/ScaraRobot.prefab` | SCARA donor mesh 소스 | Active |
| `Assets/Scenes/Boot.unity` | 부트 씬 | Active |
| `Assets/Scenes/Onboarding.unity` | 온보딩 씬 | Active |
| `Assets/Scenes/MathReadiness.unity` | 수학 준비 학습 씬 | Active |
| `Assets/Scenes/RobotLibrary.unity` | 로봇 라이브러리 씬 | Active |

### Curated Backup (백업 보관)

| 에셋 경로 | 원래 출처 | 백업 위치 | 이유 |
|-----------|----------|-----------|------|
| 내부 패키지 자산 | 다양한 Asset Store / 내부 | `Assets/KineTutor_AssetCuration_BACKUP/` | 미사용 에셋 정리 |

### Excluded (제외)

| 에셋 | 이유 |
|------|------|
| `SceneSelectables.prefab` | vendor asset (realvirtual), 프로젝트 미사용, NullRef 발생 |

## Rules
1. Active 에셋만 빌드에 포함한다.
2. Backup 에셋은 참고용으로만 보관하며 빌드에서 제외한다.
3. Excluded 에셋은 삭제하거나 .gitignore에 추가할 수 있다.
4. 새 에셋 추가 시 이 문서와 `asset-sourcing-checklist.md`를 동시에 업데이트한다.
