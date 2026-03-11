---
name: scene-scaffold
description: "씬 스캐폴드 — scene, Main.unity, 카메라, 씬 설정, 조명, UI Canvas"
---

## Trigger
Main.unity 씬 생성, 카메라/조명/UI 초기 설정, 씬 계층 구조 설정 요청 시.

## Input Context
- 씬 이름 (기본: Main)
- 필요한 UI 패널 목록
- 로봇 템플릿 (기본: 2DOF_RR)
- 제품 화면 종류 (`Guided Lesson`, `Robot Library`, `Sandbox`, `Instructor Mode`)

## Read First
1. `Assets/Scenes/CLAUDE.md` — 씬 명명 규칙
2. `Assets/Scripts/Visualization/CLAUDE.md` — 좌표 변환 규칙
3. `docs/ref/coordinate-mapping.md` — 로보틱스↔Unity 좌표 매핑
4. `docs/ref/architecture-diagrams.md` — 데이터 흐름
5. `docs/ref/tutor-step-plan.md` — Step Tutor UI 요구사항
6. `docs/ref/product/ux/guided-lesson.md`
7. `docs/ref/product/ux/robot-library.md`
8. `docs/ref/product/ux/sandbox.md`
9. `docs/ref/product/ux/instructor-mode.md`

## Do

### 1단계: 씬 계층 구조 생성
Main.unity 씬에 다음 계층 구조 설정:

```
Main.unity
├── --- MANAGEMENT ---
│   ├── AppController          (App/AppController.cs)
│   └── EventSystem            (Unity UI EventSystem)
│
├── --- CAMERA & LIGHT ---
│   ├── MainCamera             (카메라 + OrbitController)
│   │   └── Position: (0, 2, -5) / Rotation: (20, 0, 0)
│   ├── DirectionalLight       (방향광)
│   │   └── Rotation: (50, -30, 0) / Intensity: 1.0
│   └── AmbientLight           (환경광 설정)
│
├── --- ROBOT ---
│   ├── RobotRoot              (빈 오브젝트, 로봇 계층 부모)
│   │   ├── Base               (바닥 고정 베이스)
│   │   ├── Joint_0            (관절 0)
│   │   │   └── Link_0        (링크 0 메시)
│   │   │       └── Frame_0   (FrameGizmo)
│   │   ├── Joint_1            (관절 1)
│   │   │   └── Link_1        (링크 1 메시)
│   │   │       └── Frame_1   (FrameGizmo)
│   │   └── EndEffector        (엔드이펙터 프레임)
│   │       └── Frame_EE      (FrameGizmo, 강조 표시)
│   └── WorldFrame             (월드 좌표 프레임 표시)
│       └── Frame_World       (FrameGizmo, 항상 원점)
│
├── --- GROUND ---
│   ├── GroundPlane            (바닥면)
│   │   └── Scale: (10, 1, 10) / Material: Grid
│   └── GridOverlay            (격자 표시, 선택적)
│
└── --- UI ---
    └── Canvas                 (Screen Space - Overlay)
        ├── TopBar             (앱 제목, 템플릿 선택 드롭다운)
        ├── LeftPanel          (DH 테이블 + 파라미터 편집)
        │   ├── DHTableEditor  (UI/DHTableEditor.cs)
        │   └── ParameterInfo  (선택된 파라미터 설명)
        ├── RightPanel         (행렬 표시 영역)
        │   ├── MatrixDisplay  (현재 스텝의 행렬)
        │   └── StepTutorPanel (UI/StepTutorPanel.cs)
        ├── BottomBar          (관절 슬라이더)
        │   └── JointSliderPanel (UI/JointSliderPanel.cs)
        └── StepNavigator      (이전/다음 버튼)
```

### 2단계: 카메라 설정
- **위치**: (0, 2, -5) — 로봇을 비스듬히 위에서 바라봄
- **FOV**: 60도
- **Near/Far**: 0.1 / 100
- **OrbitController**: 마우스 드래그로 궤도 회전 (선택적 스크립트)
- **배경색**: 진한 회색 (#2D2D2D) 또는 Skybox

### 3단계: 조명 설정
- **Directional Light**: 메인 조명 (그림자 Soft)
- **환경광**: Gradient 모드, 상단 하늘색, 하단 바닥색
- **로봇 머티리얼**: 관절별 색상 구분 (파랑, 초록, 주황 등)

### 4단계: UI Canvas 설정
- **Canvas Scaler**: Scale With Screen Size, Reference 1920x1080
- **레이아웃**:
  - TopBar: height 60px, 상단 고정
  - LeftPanel: width 350px, 좌측 고정
  - RightPanel: width 400px, 우측 고정
  - BottomBar: height 120px, 하단 고정
  - 중앙: 3D 뷰포트 (나머지 영역)

### 5단계: 좌표 프레임 기즈모
- **프레임 축 색상**: X=빨강, Y=초록, Z=파랑 (RGB = XYZ 관례)
- **축 길이**: 0.2m (기본), EE 프레임: 0.3m
- **좌표 변환**: `docs/ref/coordinate-mapping.md` 규칙 적용
  ```csharp
  Vector3 ToUnity(Vec3D v) => new Vector3((float)v.X, (float)v.Z, (float)v.Y);
  ```

### 6단계: 초기 씬 저장
- Assets/Scenes/Main.unity로 저장
- 씬을 Build Settings에 추가 (인덱스 0)

### 7단계: 제품 화면 scaffold 체크
- `Guided Lesson`: Top/Left/Center/Right/Bottom 5영역 계약 확인
- `Robot Library`: grid + detail drawer + mode routing CTA 배치 확인
- `Sandbox`: joint input rail + history + replay + constraint view 영역 확인
- `Instructor Mode`: lesson jump + focus override + teaching note 영역 확인

## Do Not
1. 비즈니스 로직을 씬 오브젝트에 직접 구현 금지 (스크립트 참조만)
2. 좌표 변환을 Visualization/ 외부에서 수행 금지
3. 하드코딩된 관절 수 사용 금지 (템플릿에서 동적 생성)
4. UI Canvas에서 직접 수학 계산 금지
5. 기존 Main.unity가 있는 경우 덮어쓰기 전 확인 필수

## Validation
- [ ] Main.unity 씬 존재
- [ ] 카메라 위치/회전 올바름
- [ ] 조명 설정 완료
- [ ] UI Canvas + 패널 4개 배치됨
- [ ] RobotRoot 계층 구조 올바름
- [ ] GroundPlane 존재
- [ ] EventSystem 존재
- [ ] Build Settings에 씬 등록
- [ ] 제품 화면 scaffold 체크리스트 반영
- [ ] Unity 컴파일: 에러 0

## Output Template
```
[scene-scaffold 완료]
- 씬: Assets/Scenes/Main.unity
- 계층 구조:
  - Management: AppController, EventSystem
  - Camera: MainCamera (0, 2, -5)
  - Robot: RobotRoot + {n}개 관절 + EndEffector
  - Ground: GroundPlane + GridOverlay
  - UI: Canvas (TopBar, LeftPanel, RightPanel, BottomBar, StepNavigator)
- Build Settings: 인덱스 0 등록
- Unity 컴파일: 에러 0
```
