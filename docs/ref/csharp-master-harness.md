# KineTutor3D C# Master Harness

> 새 C# 작업 세션에서 먼저 읽는 상위 운영 문서입니다.
> 작업 순서, 경계, 검증 루프를 한곳에 묶고, 구현 디테일은 `docs/ref/code-patterns.md`로 내려갑니다.

---

## 하네스 코어

```text
[컨텍스트 로드] -> [경계 확인] -> [변경 목적 선언] -> [구현] -> [검증] -> [문서 동기화]
```

작업 시작 기본 순서:

1. `AGENTS.md`
2. `docs/ref/architecture-mermaid.md`
3. `CLAUDE.md`
4. `docs/ref/project-flow-code-review.md`
5. `docs/ref/csharp-master-harness.md`
6. `docs/ref/code-patterns.md`
7. 관련 폴더의 `AGENTS.md` / `CLAUDE.md`

---

## 범용 황금 규칙

1. **파일과 계약을 먼저 읽는다.**
   변경 대상 파일, 관련 테스트, 루트 문서를 먼저 읽고 시작한다.
2. **스타일 SSOT는 루트 `.editorconfig`다.**
   파일마다 스타일을 새로 정하지 않고 저장소 규칙을 그대로 따른다.
3. **null 안정성을 기본값으로 둔다.**
   nullable 문맥을 존중하고, 의미 없는 null-forgiving `!` 남발을 피한다.
4. **경계가 다른 코드를 섞지 않는다.**
   `Math`, `Types`, `Kinematics`, `Templates`는 pure C#으로 유지한다.
5. **어셈블리 경계를 같이 관리한다.**
   파일 이동이나 모듈 추가가 생기면 관련 `.asmdef`와 테스트 어셈블리까지 같이 확인한다.
6. **직렬화는 Unity 경계에서만 감싼다.**
   순수 계산 타입은 가능한 한 불변 pure C#으로 두고, Unity 직렬화가 필요한 데이터만 Unity 계층에 둔다.
7. **새 경고를 남기지 않는다.**
   컴파일 경고와 참조 경고를 "나중에"로 넘기지 않는다.
8. **검증 도구는 변경 종류에 맞게 고른다.**
   이 저장소의 Unity 작업은 `unityctl`을 우선 사용하고, `unityctl`에 없는 작업만 MCP로 폴백한다.
9. **반복 패턴은 문서로 승격한다.**
   새 C# 패턴이 반복되면 `docs/ref/code-patterns.md` 또는 관련 로컬 문서를 같이 갱신한다.
10. **파괴적 Git 조작은 명시 요청 없이는 금지다.**

---

## KineTutor3D 적용 규칙

### 1. 계층 책임

- `Assets/Scripts/App`: 상태, 오케스트레이션, 씬 흐름
- `Assets/Scripts/UI`: 화면 구성, 상호작용, 안내 UX
- `Assets/Scripts/Visualization`: 렌더링, donor mesh 바인딩, frame ownership
- `Assets/Scripts/Math`, `Types`, `Kinematics`, `Templates`: 도메인 계산과 로봇 템플릿

파일이 폴더 책임을 넘어서기 시작하면 helper/service/class로 분리하거나 올바른 폴더로 이동한다.

### 2. 파일 작성

- `App`, `UI`, `Visualization` 아래 새 `.cs` 파일은 짧은 폴더 역할 주석으로 시작한다.
- 네이밍, 헤더, 수명주기 패턴은 `docs/ref/code-patterns.md`를 따른다.
- 수학/기구학 타입은 결정적이고 테스트 가능한 pure C# 구조를 유지한다.

### 3. 의존성 방향

- pure C# 계층은 Unity 런타임 계층에 의존하지 않는다.
- UI는 계산을 재구현하지 않고 App/Domain 계층을 호출한다.
- Visualization은 표현 책임에 집중하고 상태 결정은 App 계층에 둔다.

### 4. 런타임 진실값

- 현재 씬 흐름은 `Boot -> Onboarding -> RobotLibrary -> {MathReadiness, Sandbox, RobotControl}` 이다.
- `RobotLibrary`가 메인 진입점이다.
- `Home`, `Main`, `HomeContinueHub`, `MainLearningTabs`는 역사적 이름으로 취급한다.
- 최신 흐름 확인은 `SceneCatalog`, `BootSceneRouter`, `RobotLibraryManager`, `SandboxSceneCoordinator`, `RobotControlSceneCoordinator`를 우선한다.

### 5. 저장소 경계

- 루트 `.editorconfig`는 코드 스타일 기준이다.
- 루트 `Assets/Scripts/KineTutor3D.Runtime.asmdef` 및 하위 `.asmdef`가 어셈블리 경계의 출발점이다.
- `Assets/Vendors`, `Assets/realvirtual` 같은 서드파티/벤더 자산은 명시 요청 없이는 건드리지 않는다.

---

## 검증 매트릭스

### 1. Unity 런타임 코드 (`Assets/Scripts/**`)

기본 검증 순서:

1. `unityctl status --project <project> --wait --json`
2. `unityctl check --project <project> --type compile --json`
3. 변경 영향이 순수 로직이면 `unityctl test --project <project> --mode edit --json`
4. 씬/UX 영향이 있으면 `unityctl play start --project <project> --json` 후 `console get-entries`
5. 확인 후 `unityctl play stop --project <project> --json`

### 2. 씬/UI/시각화 변경

다음 루프를 기본값으로 둔다.

1. `status --wait`
2. `check --type compile`
3. `play start`
4. `console get-entries --limit 50`
5. 필요 시 `exec`, `scene hierarchy`, `scene snapshot`, `screenshot capture`
6. `play stop`

### 3. 에디터 유틸리티 / helper / 자동화 코드

- Unity 프로젝트 안 코드면 우선 `unityctl check --type compile`
- 해당 helper가 런타임 probe에 연결되면 `unityctl exec`로 닫힌 루프를 만든다
- 테스트가 있으면 `unityctl test --mode edit`

### 4. 문서만 바꾼 경우

- `git diff --check`
- 루트 인덱스와 로컬 라우팅 문서가 끊기지 않았는지 확인

### 5. 경고 정책

- 새 compile warning이 생기면 원인을 파악하기 전까지 "정상"으로 간주하지 않는다.
- `.asmdef` 변경이 있었다면 참조 방향이 맞는지와 관련 테스트가 깨지지 않는지 추가로 본다.

---

## Unityctl 운영 루프

### 1. 세션 부트스트랩

```powershell
$unityctl = 'C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe'
$project = 'C:\Users\ezen601\Desktop\Jason\robotapp2'

& $unityctl status --project $project --wait --json
& $unityctl check --project $project --type compile --json
& $unityctl console get-entries --project $project --limit 50 --json
```

### 2. 빠른 C# 루프

```text
문서 확인 -> 대상 파일 읽기 -> 구현 -> compile check -> edit test -> 필요 시 console 확인
```

### 3. 씬/UX 루프

```text
scene open -> play start -> console 확인 -> exec / ui / snapshot -> play stop
```

### 4. 회귀 닫기 루프

```text
console clear -> 재현 -> console get-entries -> 수정 -> compile -> tests -> 재현 재확인
```

---

## 작업 시작 체크리스트

- [ ] `AGENTS.md`, `CLAUDE.md`, 이 하네스, `code-patterns.md`를 읽었다
- [ ] 변경 목적을 한 줄로 적었다
- [ ] 대상 파일과 관련 테스트를 먼저 읽었다
- [ ] 순수 C#인지 Unity 런타임인지 검증 경로를 정했다
- [ ] 폴더 책임과 `.asmdef` 경계를 확인했다

## 작업 완료 체크리스트

- [ ] 관련 compile/test/scene verification을 실행했다
- [ ] 새 경고를 남기지 않았다
- [ ] 문서 또는 라우팅 변화가 있으면 동기화했다
- [ ] 다음 사람이 이어서 볼 수 있게 변경 의도를 남겼다

---

## 공식 문서 근거

### Microsoft Learn

- C# coding conventions:
  [https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- `.editorconfig`와 코드 스타일:
  [https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options)
- Nullable reference types:
  [https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)

### Unity Manual

- Assembly Definition files:
  [https://docs.unity3d.com/6000.0/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html](https://docs.unity3d.com/6000.0/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
- Script compilation:
  [https://docs.unity3d.com/6000.0/Documentation/Manual/script-compilation.html](https://docs.unity3d.com/6000.0/Documentation/Manual/script-compilation.html)
- Unity Test Framework:
  [https://docs.unity3d.com/Packages/com.unity.test-framework@1.5/manual/index.html](https://docs.unity3d.com/Packages/com.unity.test-framework@1.5/manual/index.html)
- Serialization rules:
  [https://docs.unity3d.com/6000.0/Documentation/Manual/script-serialization.html](https://docs.unity3d.com/6000.0/Documentation/Manual/script-serialization.html)
- API Compatibility Level:
  [https://docs.unity3d.com/6000.0/Documentation/Manual/dotnet-profile-support.html](https://docs.unity3d.com/6000.0/Documentation/Manual/dotnet-profile-support.html)

---

## 이 문서에 반영한 해석

- 루트 `.editorconfig`는 스타일 SSOT로 사용한다.
- pure C# 계층은 Unity 직렬화/런타임 제약에서 분리한다.
- Unity 프로젝트의 최종 컴파일/테스트 진실원은 현재 저장소 기준으로 `unityctl` 검증 루프다.
- 구현 디테일은 `docs/ref/code-patterns.md`, 폴더 책임은 `AGENTS.md`와 하위 `CLAUDE.md`에 둔다.
