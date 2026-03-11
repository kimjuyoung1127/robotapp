# URDF Reference Collection

## Purpose
- Phase 5F+ Robot Library 확장 시 사용할 표준 로봇 URDF 소스를 사전 수집한다.
- 코드 변경 없이 레퍼런스만 보관하는 문서다.

## Parent Doc
- [robot-template-expansion.md](./robot-template-expansion.md)
- [robot-model-library-spec.md](./robot-model-library-spec.md)

## When To Read
- Robot Library에 새 로봇을 추가할 때
- URDF Importer 통합을 검토할 때
- DH 파라미터 검증용 기준 로봇이 필요할 때

## Status
- Research / 사전 수집 단계 (코드 미반영)
- Phase 5 P0 범위 밖, Phase 5F 이후 활용 예정

## Last Updated
- 2026-03-11 (KST)

---

## 1. UR5 (Universal Robots)

### Source
- **Repo:** `ros-industrial/universal_robot` (BSD-3-Clause)
- **Branch:** `melodic-devel` / `ros2`
- **Path:** `ur_description/urdf/ur5.urdf.xacro`
- **GitHub:** https://github.com/ros-industrial/universal_robot

### Alternative Sources
- `UniversalRobots/Universal_Robots_ROS2_Description` (BSD-3-Clause)
  - https://github.com/UniversalRobots/Universal_Robots_ROS2_Description
  - 공식 UR 제공, ROS2 native
- Peter Corke Robotics Toolbox: `roboticstoolbox/models/DH/UR5.py` (DH 파라미터 직접 정의)

### DH Parameters (Standard, from `config/ur5/physical_parameters.yaml`)
| Link | theta | d (m) | a (m) | alpha (rad) |
|------|-------|-------|-------|-------------|
| 1 | q1 | 0.089159 | 0 | pi/2 |
| 2 | q2 | 0 | -0.425 | 0 |
| 3 | q3 | 0 | -0.39225 | 0 |
| 4 | q4 | 0.10915 | 0 | pi/2 |
| 5 | q5 | 0.09465 | 0 | -pi/2 |
| 6 | q6 | 0.0823 | 0 | 0 |

### Joint Limits (from `config/ur5/joint_limits.yaml`)
| Joint | effort (N-m) | position (deg) | velocity (deg/s) |
|-------|-------------|---------------|-----------------|
| 1-2 | 150 | +/-360 | 180 |
| 3 | 150 | +/-180 | 180 |
| 4-6 | 28 | +/-360 | 180 |

### Metadata
- DOF: 6
- Type: Articulated (6R)
- Convention: Standard DH / MDH (both available)
- Difficulty: Medium
- Payload: 5 kg
- Reach: 850 mm
- Mass: base=4.0, shoulder=3.7, upper_arm=8.393, forearm=2.275, wrist1/2=1.219, wrist3=0.1879 kg
- Format: xacro + YAML configs (xacro 처리 필요)

### KineTutor3D Integration Notes
- 6DOF이므로 Phase 5F 이후 Robot Library의 "Demo / View Only" 카드 후보
- URDF에 mesh 파일(`.dae`, `.stl`)이 포함되어 있어 Unity URDF Importer로 직접 로드 가능
- DH 파라미터가 교과서 표준이라 FK 검증 기준값으로도 사용 가능

---

## 2. Puma 560

### Source
- **Repo:** `petercorke/robotics-toolbox-python` (MIT)
- **Path:** `roboticstoolbox/models/DH/Puma560.py`
- **GitHub:** https://github.com/petercorke/robotics-toolbox-python
- **URDF:** `roboticstoolbox/models/URDF/puma560/` (mesh + URDF 포함)

### Alternative Sources
- Peter Corke "Robotics, Vision & Control" 교과서 부록
- ROS legacy: `unimation_puma560_description` (커뮤니티 유지보수)

### DH Parameters (Standard, Corke convention, from `roboticstoolbox/models/DH/Puma560.py`)
| Link | theta | d (m) | a (m) | alpha (rad) |
|------|-------|-------|-------|-------------|
| 1 | q1 | 0.6731 | 0 | pi/2 |
| 2 | q2 | 0 | 0.4318 | 0 |
| 3 | q3 | 0.15005 | 0.0203 | -pi/2 |
| 4 | q4 | 0.4318 | 0 | pi/2 |
| 5 | q5 | 0 | 0 | -pi/2 |
| 6 | q6 | 0 | 0 | 0 |

### Named Configurations (Corke)
| Name | Values | Description |
|------|--------|-------------|
| qz | [0,0,0,0,0,0] | zero position |
| qr | [0,pi/2,-pi/2,0,0,0] | ready position |
| qs | [0,0,-pi/2,0,0,0] | stretch position |
| qn | [0,pi/4,pi,0,pi/4,0] | nominal position |

### Metadata
- DOF: 6
- Type: Articulated (6R), classic academic reference
- Convention: Standard DH (Corke)
- Difficulty: Medium
- 교과서 기준 로봇 (Robotics, Vision & Control / Introduction to Robotics by Craig)
- Format: standalone URDF (xacro 불필요, 바로 사용 가능)
- Original URDF source: `nimasarli/puma560_description`

### KineTutor3D Integration Notes
- 로보틱스 교육의 "Hello World" 로봇 — 거의 모든 교과서가 이 로봇으로 FK/IK 예제를 설명
- 교수/강사가 가장 친숙한 로봇이므로 Instructor Mode 데모에 적합
- mesh 품질이 낮을 수 있어 Unity에서 대체 visual이 필요할 수 있음
- `test-reference-values.md`에 이미 Puma 560 FK 기준값이 존재

---

## 3. Franka Emika Panda

### Source
- **Repo:** `frankaemika/franka_ros` (Apache-2.0)
- **Branch:** `develop`
- **Path:** `franka_description/robots/panda/panda.urdf.xacro`
- **GitHub:** https://github.com/frankaemika/franka_ros

### Alternative Sources
- `frankaemika/franka_description` (standalone, Apache-2.0)
  - https://github.com/frankaemika/franka_description
- MoveIt 2 config: `moveit/moveit_resources` (`panda_description`)

### DH Parameters (MDH, Franka official)
| Link | theta | d (m) | a (m) | alpha (rad) |
|------|-------|-------|-------|-------------|
| 1 | q1 | 0.333 | 0 | 0 |
| 2 | q2 | 0 | 0 | -pi/2 |
| 3 | q3 | 0.316 | 0 | pi/2 |
| 4 | q4 | 0 | 0.0825 | pi/2 |
| 5 | q5 | 0.384 | -0.0825 | -pi/2 |
| 6 | q6 | 0 | 0 | pi/2 |
| 7 | q7 | 0.107 | 0.088 | pi/2 |

### Metadata
- DOF: 7 (redundant)
- Type: Articulated (7R), collaborative
- Convention: Modified DH (MDH)
- Difficulty: Hard
- Payload: 3 kg
- Reach: 855 mm
- 특이점: 7DOF redundancy → null space 개념 설명에 적합

### KineTutor3D Integration Notes
- 7DOF이므로 Phase 5F 범위가 아니라 장기 확장 후보
- MDH convention → `robot-model-library-spec.md`의 convention badge 기능이 필요
- mesh 품질이 높고 URDF 구조가 깔끔해서 URDF Importer 테스트 1호 후보
- 연구용 로봇으로 많이 쓰여 대학 수업 타겟에 적합

---

## 4. URDF Import 전략 (Phase 5F+)

### 권장 도구
- **Unity URDF Importer** (`com.unity.robotics.urdf-importer`)
  - https://github.com/Unity-Technologies/URDF-Importer
  - PhysX ArticulationBody 기반, URP 호환
  - `Import Robot from URDF` 메뉴로 바로 사용 가능

### 경량 대안
- **NASA JPL urdf-loaders** (Unity + three.js)
  - https://github.com/gkjohnson/urdf-loaders
  - PhysX 의존 없이 시각화 전용 로드
  - KineTutor3D처럼 FK만 필요한 경우에 적합

### 통합 시 주의사항
1. URDF의 joint limits를 `RobotTemplate.GetJointLimit()`으로 매핑해야 함
2. URDF의 mesh 경로는 `.dae`/`.stl` → Unity에서 `.fbx`/`.obj` 변환 필요할 수 있음
3. URDF의 coordinate convention (ROS: X-forward, Z-up) → Unity (Z-forward, Y-up) 변환은 `CoordConverter`에 이미 구현된 패턴 활용
4. xacro → URDF 변환은 빌드 타임에 Python으로 처리하거나, 사전 변환된 `.urdf`를 사용

### 로봇 추가 워크플로우 (예상)
```
1. URDF 파일 확보 (GitHub 또는 official)
2. xacro → urdf 변환 (필요 시)
3. Unity URDF Importer로 프리팹 생성
4. DH 파라미터 → RobotTemplate C# 클래스로 정의
5. RobotMetadata ScriptableObject 생성
6. Robot Library 카드에 등록
7. FK 검증 테스트 추가
```

---

## Downstream Sync
- `docs/ref/product/robots/robot-template-expansion.md`
- `docs/ref/product/robots/robot-model-library-spec.md`
- `docs/ref/PRODUCT-ROADMAP.md` (Phase 5F 참조)
