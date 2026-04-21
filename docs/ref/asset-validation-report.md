# Asset Validation Report

최종 업데이트: 2026-03-12

## 범위
`Assets/KineTutor_AssetCuration_BACKUP/` 내 프리팹 44개를 당시 `Main.unity` 임시 루트에 instantiate했던 historical 스모크 테스트 기록이다.

## 결과 요약
| 분류 | 개수 |
|------|------|
| Clean instantiate | 43 |
| NullReferenceException | 1 |
| **합계** | **44** |

## 실패 항목

### SceneSelectables.prefab
- **경로**: `Assets/realvirtual/UIPrefabs/SceneSelectables.prefab` (historical, removed)
- **증상**: Instantiate 후 `NullReferenceException` 발생
- **원인**: realvirtual 패키지 내부 스크립트가 런타임 초기화 시 참조하는 컴포넌트/오브젝트가 당시 `Main.unity` 컨텍스트에 없었음
- **분류**: vendor asset (외부 패키지)
- **판단**: **수정 불필요 (Exclude)**
  - KineTutor3D 제품에서 직접 사용하지 않는 vendor UI prefab
  - realvirtual 패키지 자체 씬에서만 동작하도록 설계된 자산
  - 큐레이션 백업에서 제외 대상으로 분류
  - 현재 제품 런타임 donor/robot prefab 참조는 `Assets/Runtime/Resources/Robots` 기준이며, 삭제 후 donor 최소 복구 자산은 `Assets/Runtime/Robots/Common`으로 정리했다

## 후속 조치
- [x] `SceneSelectables.prefab`을 큐레이션 대상에서 제외로 판단 완료
- [ ] `Assets/KineTutor_AssetCuration_BACKUP/`에서 해당 prefab 참조 제거 (optional, 저위험)
