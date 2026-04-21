# Unity-CLI 개선 백로그

> Archive Note: 이 문서는 legacy unity-cli 개선 백로그라 archive로 이동했다.

> Navigation 리팩터링(2026-03-23) 과정에서 발견된 unity-cli 도구 개선점.
> 모든 구현 작업 완료 후 별도로 개선할 용도.

## 발견된 버그 (리팩터링 중 즉시 수정)

| 파일 | 문제 | 상태 |
|------|------|------|
| `BuildSettingsTool.cs` line 18 | 하드코딩된 씬 목록에 Home/Main 남아있음 | **수정 완료** (2026-03-23) |
| `SceneValidateTool.cs` line 25 | 하드코딩된 씬 목록에 Home/Main 남아있음 | **수정 완료** (2026-03-23) |
| `QaPrepTool.cs` line 44 | returning user의 next_scene이 "Home"으로 지정 | **수정 완료** (2026-03-23) |
| `SessionContextTool.cs` line 66 | expectedNextScene 기본값이 "Home" | **수정 완료** (2026-03-23) |

## Gap 1: SceneId ↔ Build Settings 일관성 검사 도구

**문제**: SceneId enum 값과 Build Settings 인덱스가 일치하는지 자동 검증하는 도구 없음.
누군가 Build Settings 순서를 바꾸거나 SceneId만 수정하면 런타임에 잘못된 씬이 로드됨.

**제안**: `SceneIdConsistencyTool`
- SceneId enum 파싱 → Build Settings 비교
- 인덱스 불일치, 누락, 추가된 씬 리포트
- `unity-cli sceneid_consistency_tool`

## Gap 2: 하드코딩된 씬 목록 자동 동기화

**문제**: BuildSettingsTool, SceneValidateTool 등 여러 도구에 씬 이름이 **하드코딩**되어 있어서, 씬을 추가/삭제하면 도구도 수동 수정 필요.

**제안**: `SceneCatalog.cs`나 Build Settings에서 씬 목록을 동적으로 읽도록 리팩터링.
- 하드코딩 `string[]` → `SceneCatalog.GetAllEntries()` 또는 `EditorBuildSettings.scenes` 사용
- 도구끼리 씬 목록이 자동으로 일치

## Gap 3: Dead Code 감지 도구

**문제**: HomeContinueHub*, MainLearningShell* 삭제 시 참조하는 파일을 수동 grep으로 확인해야 했음.
도구가 자동으로 "이 클래스를 참조하는 파일 N개" 리포트를 해줬으면 더 안전했음.

**제안**: `DeadCodeDetectorTool`
- 지정 클래스명으로 전체 `.cs` 파일 grep
- 참조 카운트 0이면 dead code 후보로 리포트
- `unity-cli dead_code_tool --params '{"class":"HomeContinueHubController"}'`

## Gap 4: Cross-File Enum 참조 검증 도구

**문제**: `SceneId.Home`을 삭제한 후 전체 코드에서 참조를 수동 검색.
매직 넘버 `LoadScene(2)` 같은 하드코딩이 있으면 발견 못함.

**제안**: `EnumRefValidatorTool`
- 특정 enum 타입의 모든 값에 대해 코드베이스 참조 검색
- 삭제된 enum 값을 여전히 참조하는 코드 감지
- 매직 넘버(숫자 리터럴) 사용도 경고

## Gap 5: 리팩터링 영향 분석 도구

**문제**: "Home 씬을 삭제하면 어떤 파일이 영향받는가?"를 미리 알 수 없음.
탐색 에이전트를 돌려서 수동으로 파악해야 했음.

**제안**: `ImpactAnalysisTool`
- 심볼(클래스/메서드/enum값)을 입력하면 의존 파일 목록 + 영향 범위 출력
- `unity-cli impact_analysis_tool --params '{"symbol":"SceneId.Home"}'`

## 우선순위

| 순위 | 개선 | 이유 |
|------|------|------|
| 1 | Gap 2: 하드코딩 씬 목록 제거 | 매번 씬 변경할 때 같은 실수 반복 |
| 2 | Gap 1: SceneId ↔ Build Settings 일관성 | 런타임 버그 예방 |
| 3 | Gap 5: 영향 분석 도구 | 리팩터링 계획 시간 단축 |
| 4 | Gap 3: Dead Code 감지 | 코드 정리 자동화 |
| 5 | Gap 4: Enum 참조 검증 | 안전망 |
