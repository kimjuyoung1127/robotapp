# Asset Registry

| 에셋명 | 출처 | 버전 | 라이선스 | 배치 경로 | 용도 |
|--------|------|------|----------|-----------|------|
| realvirtual package | Unity Asset/Imported Package | unknown | 확인 필요 | Assets/realvirtual | 로봇/산업 시뮬레이션 자산 소스 |
| DemoRealvirtual scene assets | Imported with package | unknown | 확인 필요 | Assets/DemoRealvirtual | 데모 씬 조명/반사 프로브 데이터 |
| Robot.prefab | realvirtual 3DPrefabs | unknown | 확인 필요 | Assets/realvirtual/3DPrefabs/Robot.prefab | 로봇 메시 추출 후보 |
| ScaraRobot.prefab | realvirtual 3DPrefabs | unknown | 확인 필요 | Assets/realvirtual/3DPrefabs/ScaraRobot.prefab | SCARA 메시 추출 후보 |
| FanucCRX-10iA_L.prefab | realvirtual 3DPrefabs | unknown | 확인 필요 | Assets/realvirtual/3DPrefabs/FanucCRX-10iA_L.prefab | 6DOF 메시 추출 후보 |
| igusRebel.prefab | realvirtual Interfaces | unknown | 확인 필요 | Assets/realvirtual/Interfaces/igusREBEL/igusRebel.prefab | 로봇 메시 추출 후보 |
| ShootingTarget.prefab | Glowing Rifts | unknown | 확인 필요 | Assets/Prefabs/Teaching/Markers/ShootingTarget.prefab | 타깃 마커 curated subset |
| Checkmark_3D_Icon.prefab | HQP Studios | unknown | 확인 필요 | Assets/Prefabs/Teaching/Markers/Checkmark_3D_Icon.prefab | 성공 마커 curated subset |
| Warning_3D_Icon.prefab | HQP Studios | unknown | 확인 필요 | Assets/Prefabs/Teaching/Markers/Warning_3D_Icon.prefab | 경고 마커 curated subset |
| Heathen flat icon subset | Heathen Engineering | unknown | 확인 필요 | Assets/Art/UI/Icons | 화살표/잠금/검색/설명 아이콘 curated subset |

## 운영 규칙
1. `Assets/realvirtual`는 벤더 소스이며 원본 보존.
2. `Assets/HQP Studios`, `Assets/_Heathen Engineering`, `Assets/Glowing Rifts`는 로컬 vendor source로 두고, 실제 런타임 참조는 curated subset(`Assets/Art`, `Assets/Prefabs/Teaching`)을 우선 사용한다.
3. curated subset이 없으면 vendor source 경로를 차선 fallback으로 사용한다.
4. 라이선스/버전 확인 즉시 표 업데이트.
