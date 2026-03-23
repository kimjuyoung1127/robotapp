# FR5 Slim Template Extraction

## Summary

- `robottemplete` 루트 직배포용 FR5 슬림 템플릿 경로를 추가했다.
- 기존 full 번들은 `archive/` 보존 전제로 유지한다.
- 슬림 범위는 FR5 prefab, URDF resources, minimal host, ring interaction, export/evidence tooling으로 제한했다.

## Changes

- Runtime
  - `Assets/Scripts/App/FR5TemplateMinimalController.cs`
  - `Assets/Scripts/App/FR5TemplatePoseCatalog.cs`
  - `Assets/Scripts/App/FR5TemplateSlimManifest.cs`
  - `Assets/Scripts/Visualization/Shared/JointRotationHandle.cs`
- Editor
  - `Assets/Editor/KineTutor3D/Fr5TemplateSlimSceneBuilder.cs`
  - `Assets/Editor/KineTutor3D/Fr5TemplateSlimEvidenceCapture.cs`
  - `Assets/Editor/KineTutor3D/ExportFr5SlimTemplatePackage.cs`
- Handoff docs
  - `Assets/Handoff/FR5_Slim_Template/*`
- Tests
  - `Assets/Tests/EditMode/FR5TemplatePoseCatalogTests.cs`
  - `Assets/Tests/EditMode/FR5TemplateSlimManifestTests.cs`

## Validation

- Completed:
  - `unityctl ping --project C:/Users/ezen601/Desktop/Jason/robotapp2 --json`
    - PASS, Unity `6000.0.64f1`
  - `unityctl check --project C:/Users/ezen601/Desktop/Jason/robotapp2 --json`
    - PASS, `scriptCompilationFailed=false`, `assemblies=43`
  - `unityctl test --project C:/Users/ezen601/Desktop/Jason/robotapp2 --mode edit --filter KineTutor3D.Tests.EditMode.FR5TemplatePoseCatalogTests --timeout 180 --json`
    - PASS, `2 passed`
  - `unityctl test --project C:/Users/ezen601/Desktop/Jason/robotapp2 --mode edit --filter KineTutor3D.Tests.EditMode.FR5TemplateSlimManifestTests --timeout 180 --json`
    - PASS, `2 passed`
  - `unity-cli prefab_validate_tool --params '{"path":"Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab"}'`
    - PASS, `missing_scripts=0`, `zero_vertex_meshes=0`
  - `unityctl exec ... ExportFr5SlimTemplatePackage.BuildPackage()`
    - PASS, `C:\Users\ezen601\Desktop\Jason\robottemplete` 루트에 `Assets`, 문서 5종, `.unitypackage`, `.zip` 생성
  - `unityctl exec ... ExecuteMenuItem("KineTutor3D/Export/Capture FR5 Slim Evidence")`
    - PASS, PNG evidence 생성

## Exported Artifacts

- `C:\Users\ezen601\Desktop\Jason\robottemplete\Assets`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\README.md`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\INSTALL.md`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\DEPENDENCIES.md`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\MATERIALS.md`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\CHECKLIST.md`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\FAIRINO_FR5_TEMPLATE_Slim.unitypackage`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\FAIRINO_FR5_TEMPLATE_Slim.zip`

## Evidence

- `C:\Users\ezen601\Desktop\Jason\robottemplete\evidence\fr5-template-neutral.png`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\evidence\fr5-template-ready.png`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\evidence\fr5-template-showcase.png`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\evidence\sequence-frame-00-neutral.png`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\evidence\sequence-frame-01-ready.png`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\evidence\sequence-frame-02-showcase.png`
- `C:\Users\ezen601\Desktop\Jason\robottemplete\evidence\sequence-frame-03-wristturn.png`

## Self-Review

- 원인:
  - 기존 `FR5_RobotControl_Template`는 UI와 RobotControl 계층이 너무 넓게 포함되어 재사용 패키지로는 과포함 상태였다.
- 조치:
  - FR5 링 조작과 3D pose 반영만 담당하는 최소 런타임과 slim manifest/exporter로 분리했다.
- 검증:
  - `unityctl` compile, targeted EditMode tests, prefab validation, export 결과물, PNG evidence로 확인했다.
- 재발 방지:
  - 이후 FR5 템플릿 추출은 `FR5TemplateSlimManifest` 기준으로만 package root를 관리한다.
