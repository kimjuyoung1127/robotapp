# Phase 0+1 Core Decisions (Official-Docs Anchored)

## Scope
- 범위: Phase 0+1 핵심 (`asmdef`, `tests`, `compile`, `serialization`, `API 호환성`)
- 기준 버전: Unity `6000.0.64f1` 우선, `2022.3 LTS` 보조

## Decision Output Contract
모든 의사결정 출력은 아래 4개 항목을 반드시 포함:
1. `결론`
2. `공식 문서 근거(링크)`
3. `프로젝트 적용 규칙`
4. `버전 차이 메모(필요 시)`

## Core Decision Patterns

### 1) asmdef 필드 결정
- 결론 예시: Types/Math/Kinematics는 `noEngineReferences=true`.
- 근거 링크:
  - https://docs.unity3d.com/Manual/assembly-definition-file-format.html
  - https://docs.unity3d.com/Manual/assembly-definitions-creating.html
- 적용 규칙:
  - 순수 수학/타입 모듈은 UnityEngine 의존 금지.
  - 참조 방향은 DAG로 유지.

### 2) Test Runner 구성 결정
- 결론 예시: EditMode/PlayMode 분리, 테스트 어셈블리 명시.
- 근거 링크:
  - https://docs.unity3d.com/kr/6000.0/Manual/com.unity.test-framework.html
  - https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/index.html
- 적용 규칙:
  - EditMode는 순수 로직 우선, PlayMode는 통합 스모크 중심.

### 3) Script Compilation 정책 결정
- 결론 예시: asmdef 경계 기준으로 재컴파일 범위를 최소화.
- 근거 링크:
  - https://docs.unity3d.com/Manual/compilation-and-code-reload.html
  - https://docs.unity3d.com/Manual/SpecialFolders.html
- 적용 규칙:
  - 폴더 경계와 asmdef를 일치시켜 컴파일 영향 범위를 예측 가능하게 유지.

### 4) Serialization 제한 결정
- 결론 예시: 직렬화 가능 필드 타입/가시성 규칙 사전 검증.
- 근거 링크:
  - https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html
- 적용 규칙:
  - 런타임/인스펙터 동작 차이를 테스트로 확인.

### 5) API 호환성 결정
- 결론 예시: 프로젝트 API 호환성 변경 시 테스트 재검증 필수.
- 근거 링크:
  - https://docs.unity3d.com/kr/6000.0/Manual/dotnet-profile-support.html
- 적용 규칙:
  - API 호환성 변경은 빌드/테스트와 함께 적용.

## Phase Gate Rule
- Phase 0/1에서 asmdef/테스트/컴파일/직렬화 관련 항목은
  공식 문서 근거 링크가 포함되어야만 `Done`으로 처리.
