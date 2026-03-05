---
name: asmdef-setup
description: "Assembly Definition 설정 — assembly definition, asmdef, 모듈 경계, 컴파일 격리"
---

## Trigger
Assembly Definition 파일 생성/수정 요청 시, 또는 모듈 경계 설정이 필요할 때.

## Input Context
- 대상 모듈 (전체 또는 특정 모듈)
- Unity 버전 (기본: Unity 6)

## Read First
1. `docs/ref/architecture-diagrams.md` — 9개 asmdef 구조 및 의존성
2. `CLAUDE.md` — 모듈 경계 규칙
3. `ai-context/project-context.md` — 모듈별 UnityEngine 허용 정책
4. `ai-context/coding-guideline.md` — pure C# 경계 규칙

## Do (엄격한 순서)

### 1단계: 의존성 그래프 확인
`docs/ref/architecture-diagrams.md`의 Assembly Definition 구조 확인:
```
KineTutor3D.Types        → (없음)
KineTutor3D.Math         → Types
KineTutor3D.Kinematics   → Types, Math
KineTutor3D.Templates    → Types, Math, Kinematics
KineTutor3D.UI           → Types, Templates, UnityEngine.UI
KineTutor3D.Visualization→ Types, Math, UnityEngine
KineTutor3D.App          → 전체
KineTutor3D.Tests.EditMode → Types, Math, Kinematics + nunit.framework, UnityEngine.TestRunner
KineTutor3D.Tests.PlayMode → 전체 + nunit.framework, UnityEngine.TestRunner
```

### 2단계: .asmdef 파일 생성
각 모듈 폴더에 `.asmdef` JSON 파일 생성:

**순수 C# 모듈 (Types, Math, Kinematics):**
```json
{
    "name": "KineTutor3D.{Module}",
    "rootNamespace": "KineTutor3D.{Module}",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": true
}
```
- `noEngineReferences: true` — UnityEngine 참조 차단 (핵심!)

**Unity 모듈 (UI, Visualization, App, Templates):**
```json
{
    "name": "KineTutor3D.{Module}",
    "rootNamespace": "KineTutor3D.{Module}",
    "references": ["KineTutor3D.Types", ...],
    "noEngineReferences": false
}
```

**테스트 모듈:**
```json
{
    "name": "KineTutor3D.Tests.EditMode",
    "rootNamespace": "KineTutor3D.Tests.EditMode",
    "references": [
        "KineTutor3D.Types",
        "KineTutor3D.Math",
        "KineTutor3D.Kinematics",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "optionalUnityReferences": ["TestAssemblies"],
    "overrideReferences": true,
    "precompiledReferences": ["nunit.framework.dll"],
    "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

### 3단계: 순환 참조 검증
의존성 방향 확인 (모두 상위→하위 방향):
- Types ← Math ← Kinematics ← Templates
- Types ← UI, Types ← Visualization
- 역방향 참조 없음

### 4단계: 컴파일 확인
Unity Editor에서 컴파일 에러 0 확인.

## Do Not
1. `noEngineReferences: true` 모듈(Types, Math, Kinematics)에 UnityEngine 참조 추가 금지
2. 순환 참조 생성 금지 (A→B→A)
3. 테스트 어셈블리에서 `nunit.framework` 참조 누락 금지
4. `autoReferenced: false`를 일반 모듈에 설정 금지 (테스트 전용)
5. 기존 .asmdef가 있는 경우 덮어쓰기 전 확인 필수

## Validation
- [ ] 9개 .asmdef 파일 모두 생성됨
- [ ] Types/Math/Kinematics: `noEngineReferences: true`
- [ ] 순환 참조 없음 (의존성 그래프가 DAG)
- [ ] 테스트 어셈블리: `nunit.framework.dll` + `UNITY_INCLUDE_TESTS`
- [ ] Unity 컴파일: 에러 0
- [ ] 각 .asmdef의 `rootNamespace`가 폴더 구조와 일치

## Output Template
```
[asmdef-setup 완료]
- 생성된 .asmdef: {n}/9
- noEngineReferences 모듈: Types, Math, Kinematics
- 순환 참조: 없음
- Unity 컴파일: 에러 0
- 경로 목록:
  - Assets/Scripts/Types/KineTutor3D.Types.asmdef
  - Assets/Scripts/Math/KineTutor3D.Math.asmdef
  - Assets/Scripts/Kinematics/KineTutor3D.Kinematics.asmdef
  - Assets/Scripts/Templates/KineTutor3D.Templates.asmdef
  - Assets/Scripts/UI/KineTutor3D.UI.asmdef
  - Assets/Scripts/Visualization/KineTutor3D.Visualization.asmdef
  - Assets/Scripts/App/KineTutor3D.App.asmdef
  - Assets/Tests/EditMode/KineTutor3D.Tests.EditMode.asmdef
  - Assets/Tests/PlayMode/KineTutor3D.Tests.PlayMode.asmdef
```
