# Visualization/

3D 렌더링 헬퍼 (로봇 기구학 시각화).

## 폴더 구조
```
Visualization/
├── Renderer/         ← RobotRenderer facade + donor binding + bounds/rig helpers
├── RobotLibrary/     ← RobotLibrary preview pod/factory
├── RobotControl/     ← RobotControl 전용 시각 드라이버
├── MathReadiness/    ← 각도/길이/격자/단순 팔 teaching visuals
├── Targets/          ← target marker + highlight helpers
├── Shared/           ← 로봇 무관 공용 컴포넌트 (어느 씬에서든 AddComponent로 사용)
│   ├── CoordConverter.cs         — 로보틱스↔Unity 좌표 변환
│   ├── EETrailRenderer.cs       — 공유 EE 궤적 코어 (거리게이팅, FIFO, 그라데이션)
│   ├── EndEffectorTrail.cs      — EETrailRenderer + AppController 이벤트 바인딩 어댑터
│   ├── FrameGizmo.cs            — 단일 좌표 프레임 축 표시
│   ├── FrameGizmoFactory.cs     — 다관절 좌표 프레임 기즈모 관리
│   ├── OrbitCameraController.cs — 궤도 카메라 (좌클릭 회전, 스크롤 줌, 우클릭 팬)
│   ├── SharedLineMaterial.cs    — LineRenderer용 공유 Material 캐시 + 설정 헬퍼
│   ├── DisplacementArrow.cs     — EE 변위 벡터 화살표 (LineRenderer + 원뿔 헤드)
│   ├── JointRotationHandle.cs   — 관절 회전 링 핸들 (마우스 드래그→각도 emit)
│   └── UrdfJointDriver.cs      — 범용 URDF 관절 드라이버 (ArticulationBody 자동탐색, N축)
├── Renderer/RobotRenderer.cs    — 범용 2DOF/SCARA 3D 렌더러
├── RobotControl/FairinoUrdfJointDriver.cs — FR5 URDF Transform 관절 제어
├── RobotLibrary/RobotPreviewFactory.cs    — RobotLibrary 프리뷰 생성
└── ...                          — 역할별 시각화 헬퍼
```

## Shared/ 사용 규칙
- 새 공용 시각화 컴포넌트는 `Shared/`에 작성
- 네임스페이스는 `KineTutor3D.Visualization` 유지 (하위 네임스페이스 불필요)
- 특정 페이지/로봇에 종속되는 코드는 해당 역할 폴더(`RobotControl/`, `MathReadiness/`, `Renderer/` 등)에 유지
- LineRenderer Material은 `SharedLineMaterial.Get()` 사용 (개별 static Material 캐시 금지)
- LineRenderer 기본 설정은 `SharedLineMaterial.ConfigureLineRenderer()` 사용

## 규칙
1. **이 모듈만** `double → float` 캐스팅 수행 (렌더링 경계)
2. 좌표 변환은 `docs/ref/coordinate-mapping.md` 참조
3. 위치 에러 표시 임계값: 1e-4 m
4. 회전 에러 표시 임계값: 1e-3 rad

## 좌표 변환 (로보틱스 → Unity)
```csharp
Vector3 ToUnity(Vec3D v) => new Vector3((float)v.X, (float)v.Z, (float)v.Y);
Vec3D FromUnity(Vector3 v) => new Vec3D(v.x, v.z, v.y);
```
