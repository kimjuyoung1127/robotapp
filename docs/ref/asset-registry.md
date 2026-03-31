# Asset Registry

| 에셋명 | 출처 | 버전 | 라이선스 | 배치 경로 | 용도 |
|--------|------|------|----------|-----------|------|
| realvirtual package | Unity Asset/Imported Package | unknown | 확인 필요 | removed (2026-03-31) | 삭제 완료. 필요 최소 donor 자산만 Runtime 공용 자산층으로 복구 |
| DemoRealvirtual scene assets | Imported with package | unknown | 확인 필요 | Assets/Vendors/Archive/DemoRealvirtual | 데모 씬 조명/반사 프로브 데이터 |
| Common donor recovery assets | project-owned recovered subset | n/a | repo-local | Assets/Runtime/Robots/Common | SCARA/FANUC/IGUS donor mesh/material/prefab 최소 복구 자산 |
| ScaraRobot.prefab (runtime) | curated runtime prefab | unknown | 확인 필요 | Assets/Runtime/Resources/Robots/ScaraRobot.prefab | 현재 SCARA donor/runtime prefab |
| FanucCRX-10iA_L.prefab (runtime) | curated runtime prefab | unknown | 확인 필요 | Assets/Runtime/Resources/Robots/FanucCRX-10iA_L.prefab | 현재 Fanuc donor/runtime prefab |
| igusRebel.prefab (runtime) | curated runtime prefab | unknown | 확인 필요 | Assets/Runtime/Resources/Robots/igusRebel.prefab | 현재 igus donor/runtime prefab |
| ShootingTarget.prefab | Glowing Rifts | unknown | 확인 필요 | Assets/Runtime/Prefabs/Teaching/Markers/ShootingTarget.prefab | 타깃 마커 curated subset |
| Checkmark_3D_Icon.prefab | HQP Studios | unknown | 확인 필요 | Assets/Runtime/Prefabs/Teaching/Markers/Checkmark_3D_Icon.prefab | 성공 마커 curated subset |
| Warning_3D_Icon.prefab | HQP Studios | unknown | 확인 필요 | Assets/Runtime/Prefabs/Teaching/Markers/Warning_3D_Icon.prefab | 경고 마커 curated subset |
| Heathen flat icon subset | Heathen Engineering | unknown | 확인 필요 | Assets/Runtime/Art/UI/Icons | 화살표/잠금/검색/설명 아이콘 curated subset |

## 운영 규칙
1. `Assets/realvirtual`는 2026-03-31 기준 삭제되었고, 삭제 후 필요한 최소 donor 자산만 `Assets/Runtime/Robots/Common`으로 이관했다.
2. `Assets/Vendors/Archive/HQPStudios`, `Assets/Vendors/Archive/HeathenEngineering`, `Assets/Vendors/Archive/GlowingRifts`는 로컬 vendor source로 두고, 실제 런타임 참조는 curated subset(`Assets/Runtime/Art`, `Assets/Runtime/Prefabs/Teaching`)을 우선 사용한다.
3. 현재 런타임/카탈로그 참조는 `Assets/Runtime/Resources/Robots`와 curated subset을 우선 사용하고, donor 복구용 공용 자산은 `Assets/Runtime/Robots/Common`에 둔다.
4. `Assets/Runtime/Resources`는 런타임 `Resources.Load(...)` 전용 경로다. `Glossary`, `Onboarding`, `Robots`, `TutorSteps`, `UI/Icons`는 이 루트 아래에서 유지한다.
5. `Assets/Vendors/Archive/DemoRealvirtual`는 `realvirtual` 부속 데모/조명 데이터 보관 구역으로 취급하고, 제품 런타임 직접 참조 경로로는 사용하지 않는다.
6. 라이선스/버전 확인 즉시 표 업데이트.
