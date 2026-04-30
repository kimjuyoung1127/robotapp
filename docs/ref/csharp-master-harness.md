# C# Master Harness (FR5UNITY)

이 문서는 프로젝트 내 모든 C# 코드의 품질 기준과 AI 에이전트가 지켜야 할 작성 규칙을 정의하는 SSOT(Source of Truth)입니다.

## 1. AI-Friendly File Header (Mandatory)
모든 `.cs` 파일 최상단에는 아래 구조의 `<ai_context>` 헤더가 포함되어야 합니다. 이는 에이전트의 빠른 스캔과 역할 파악을 돕습니다.

```csharp
/// <ai_context>
/// Component: [모듈명: App/UI/Visualization/Kinematics/Math/Types/Templates]
/// Responsibility: [이 파일의 핵심 책임 한 줄 요약]
/// Dependencies: [주요 의존성 목록]
/// Quality Gate: [통과해야 할 테스트 파일 또는 검증 조건]
/// Navigation: [관련 AGENTS.md 또는 상세 문서 경로]
/// </ai_context>
```

## 2. 모듈 경계 및 제약 사항
| 모듈 | UnityEngine 허용 | 데이터 정밀도 | 비고 |
|------|:----------------:|:------------:|------|
| **Types / Math** | ✗ (Pure C#) | `double` | 기본 산술 및 타입 정의 |
| **Kinematics** | ✗ (Pure C#) | `double` | DH 파라미터 및 FK/IK 로직 |
| **Templates** | ✗ (Pure C#) | `double` | 로봇 프리셋 데이터 |
| **Visualization** | ✓ | `double` -> `float` | 렌더링 및 본 바인딩 |
| **App / UI** | ✓ | - | 상태 관리 및 레이아웃 |

## 3. 코드 품질 게이트 (Quality Gates)
작업 완료 전 반드시 아래 단계를 수행해야 합니다.
1. **컴파일 검증**: `unityctl check --type compile`
2. **단위 테스트**: `Math`, `Kinematics` 수정 시 관련 `EditMode` 테스트 100% 통과 필수.
3. **가독성**: 복잡한 행렬 연산은 반드시 수식의 의미를 주석으로 명시.

## 4. AI 에이전트 전용 지침
- **기존 패턴 우선**: 새로운 유틸리티를 만들기 전 `Assets/Scripts/Math/`와 `Types/`에 유사 기능이 있는지 먼저 검색할 것.
- **Surgical Edit**: 불필요한 리팩토링은 피하고 요청된 기능 구현 및 테스트 보강에 집중할 것.
- **Context Awareness**: `AGENTS.md`에 명시된 폴더별 역할을 엄격히 준수할 것.
