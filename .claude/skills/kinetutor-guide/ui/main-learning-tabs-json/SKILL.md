---
name: main-learning-tabs-json
description: "Main 탭형 학습 쉘의 로봇별 JSON 콘텐츠를 추가/수정/검증하는 스킬. `Assets/Runtime/Resources/LearningTabs/*.json` 문안 업데이트, 새 로봇용 탭 문서 추가, MainLearningTabsLoader fallback 조정, Main 탭 콘텐츠 schema 변경, JSON 파싱/폴백/Unity MCP 검증이 필요할 때 사용한다."
---

# Main Learning Tabs Json

## Overview

`Main` 학습 쉘은 텍스트/탭 구성만 JSON으로 분리하고, 수학/렌더링/Unity wiring은 코드에 남긴다.
이 스킬은 로봇별 탭 문서를 안전하게 업데이트하고, Unity MCP로 로더/컴파일/콘솔 상태를 검증하는 절차를 고정한다.

## Read First
1. `Assets/Scripts/App/MainLearningTabsLoader.cs`
2. `Assets/Scripts/UI/Data/MainLearningTabsDocument.cs`
3. `Assets/Scripts/UI/MainLearningShellController.cs`
4. `Assets/Runtime/Resources/LearningTabs/*.json`
5. `Assets/Tests/EditMode/MainLearningTabsLoaderTests.cs`

## JSON Scope
- JSON에는 문안과 탭 구조만 넣는다.
- JSON에 넣는 것: `robotId`, `displayTitle`, `heroSummary`, `legend`, `tabs`, `motion`, `forwardKinematics`
- JSON에 넣지 않는 것: GameObject 이름, Unity 컴포넌트 참조, FK 계산, 시각화 geometry, gate/condition, slider range
- 컬렉션은 모두 배열만 사용한다. dictionary는 금지한다.
- 탭 ID는 고정값 `overview`, `motion`, `fk`만 사용한다.

## Update Workflow
1. 대상 로봇 ID를 `RobotCatalog`와 `LearningTabs/<robotId>.json`에서 먼저 확인한다.
2. 기존 JSON이 있으면 같은 구조와 tone을 유지하며 문안만 조정한다.
3. 새 로봇이면 기존 문서 하나를 복제하지 말고, `MainLearningTabsDocument` schema에 맞는 새 문서를 작성한다.
4. schema 변경이 필요하면 먼저 `MainLearningTabsDocument.cs`와 `MainLearningTabsLoader.cs`를 함께 수정한다.
5. shell UI가 새 필드를 실제로 읽는지 `MainLearningShellController.cs`까지 확인한다.
6. 문안 추가 후 로더 테스트, 스크립트 validate, Unity refresh/compile, console check 순서로 검증한다.

## Guardrails
1. `JsonUtility` 제한을 우회하려고 nullable collection이나 dictionary를 도입하지 않는다.
2. JSON이 없거나 깨졌을 때는 반드시 `MainLearningTabsLoader.BuildFallbackDocument()`로 폴백되어야 한다.
3. `MainLearningShellController`가 로직을 소유하고, JSON은 문안만 제공한다는 경계를 깨지 않는다.
4. 새 로봇 JSON을 추가할 때 fallback 기본값(`2DOF_RR`)을 제거하지 않는다.
5. `2DOF_RR`만 rich visuals라는 현재 정책은 JSON이 아니라 코드에서 유지한다.

## Validation Order
1. `validate_script`로 `MainLearningTabsDocument.cs`, `MainLearningTabsLoader.cs`, 필요 시 `MainLearningShellController.cs`를 검사한다.
2. `run_tests` 또는 관련 EditMode 테스트로 `MainLearningTabsLoaderTests`를 실행한다.
3. `refresh_unity`로 `compile=request`, `mode=force`, `scope=scripts`를 호출한다.
4. `read_console`로 `error`와 `warning`를 확인한다.
5. 새 JSON 리소스가 있으면 `Assets/Runtime/Resources/LearningTabs/` 존재와 파일명을 다시 확인한다.

## Acceptance Checks
- 대상 로봇 JSON이 `Resources.Load<TextAsset>("LearningTabs/<robotId>")`로 로드 가능한 이름인지 확인한다.
- 유효 JSON이면 `ParseOrFallback`가 문서를 그대로 반환한다.
- 잘못된 JSON이면 fallback 문서가 반환된다.
- `MainLearningTabsLoaderTests`가 통과한다.
- Unity compile 후 console에 새 에러가 없다.

## Output Template
```
[main-learning-tabs-json 적용]
- 대상 로봇: {robotId}
- JSON 파일: Assets/Runtime/Resources/LearningTabs/{robotId}.json
- schema 변경: {Y/N}
- loader/fallback 수정: {Y/N}
- validate_script: 에러 0
- EditMode 테스트: MainLearningTabsLoaderTests 통과
- Unity refresh/compile: 에러 0
```
