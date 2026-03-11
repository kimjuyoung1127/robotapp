# Workspace Envelope Algorithm Memo

## Purpose
- 로봇 팔의 도달 가능 영역(workspace envelope) 시각화 방법을 비교하고, KineTutor3D에 적합한 구현 전략을 기록한다.
- Phase 5 P0 범위 밖이며, P1 이후 visualization 확장 시 참조하는 연구 메모다.

## Parent Doc
- [current-feature-checklist.md](./current-feature-checklist.md)

## When To Read
- constraint / workspace / singularity 시각화를 구현할 때
- DH 파라미터 변경에 따른 도달 영역 변화를 보여주는 기능을 설계할 때

## Status
- Research only (코드 미반영)
- Phase 5 P0 범위 밖

## Last Updated
- 2026-03-11 (KST)

---

## 1. 교육적 가치

workspace envelope 시각화는 다음 질문에 직관적으로 답한다:
- "링크 길이를 바꾸면 로봇이 닿을 수 있는 영역이 어떻게 변하나요?"
- "관절 한계를 줄이면 작업 영역이 얼마나 좁아지나요?"
- "이 로봇이 왜 특정 위치에 도달하지 못하나요?"

DH 파라미터 변경 → workspace 변화를 실시간으로 보여주면, 파라미터와 물리적 능력의 연결이 즉각 학습된다.

---

## 2. 방법 비교

### 방법 1: Monte Carlo 샘플링

**원리:**
1. 각 관절의 범위 내에서 랜덤 각도 N개 샘플링
2. 각 샘플에 대해 FK 계산 → EE 위치 수집
3. 점 구름(point cloud)을 convex hull 또는 alpha shape로 변환

**장점:**
- 구현이 단순 (FK만 있으면 됨, KineTutor3D에 이미 존재)
- DOF 수에 관계없이 동일한 코드로 동작
- 복잡한 관절 한계, 자기 충돌 등도 자연스럽게 반영 가능
- 비정형 workspace (구멍이 있는 형태 등)도 표현 가능

**단점:**
- 샘플 수가 적으면 경계가 울퉁불퉁
- 6DOF 이상이면 수렴에 필요한 샘플 수가 기하급수적 증가
- 실시간 갱신에는 프레임 분산 또는 Compute Shader 필요

**성능 추정:**
| DOF | 필요 샘플 수 | FK 1회 비용 | 총 시간 (C# single-thread) |
|-----|------------|-----------|------------------------|
| 2 | 5,000 | ~1 us | ~5 ms ✅ |
| 3 | 20,000 | ~1.5 us | ~30 ms ⚠️ |
| 4 | 100,000 | ~2 us | ~200 ms ❌ (프레임 분산 필요) |
| 6 | 500,000+ | ~3 us | ~1.5 s ❌ (Compute Shader 권장) |

**Unity 구현 옵션:**
- **2DOF:** 단일 프레임에 계산 완료 가능, `LineRenderer`로 경계 렌더
- **3~4DOF:** `Coroutine` + 프레임 분산 (매 프레임 1000~5000개씩)
- **6DOF:** `ComputeShader` 또는 `Burst + Job System`

### 방법 2: 해석적/기하학적 방법

**원리:**
- 2DOF: 두 원의 합/차로 도넛(annulus) 계산
  - 외반경 = a1 + a2, 내반경 = |a1 - a2|
  - 관절 한계가 있으면 원호(arc) 영역으로 축소
- 3DOF: 원호를 축 주위로 sweep하여 3D 영역 생성
- N-DOF: 일반적 closed-form 없음

**장점:**
- 2DOF에서 수학적으로 정확한 경계
- 계산 비용 거의 0 (공식 대입만)
- 실시간 갱신 완벽 지원

**단점:**
- 3DOF 이상에서 급격히 복잡해짐
- 비표준 관절 구조(오프셋, prismatic)에서 공식 유도 어려움
- DOF별로 별도 구현 필요 → 유지보수 비용

**2DOF 공식:**
```
외반경 R_max = a1 + a2
내반경 R_min = |a1 - a2|
workspace = { (x, y) : R_min <= sqrt(x^2 + y^2) <= R_max }
```

관절 한계 적용 시:
```
theta1 ∈ [θ1_min, θ1_max]
theta2 ∈ [θ2_min, θ2_max]
→ 원호 영역으로 축소, boundary는 4개 원호의 조합
```

### 방법 3: 경계 추적 (Boundary Tracing)

**원리:**
1. 하나 이상의 관절을 한계값(min/max)에 고정
2. 나머지 관절을 sweep하며 EE 위치 추적
3. 모든 관절 한계 조합에 대해 반복 → 경계 곡선들의 합집합

**장점:**
- Monte Carlo보다 적은 점으로 정확한 경계
- 경계선만 그리므로 렌더링 비용 낮음
- 2~3DOF에서 매우 효과적

**경계점 수 추정:**
| DOF | 조합 수 (D * 2^(D-1)) | 점 수 (k=100) |
|-----|----------------------|--------------|
| 2 | 4 | 400 |
| 3 | 12 | 1,200 |
| 4 | 32 | 3,200 |
| 6 | 192 | 19,200 |

**단점:**
- 관절 한계 조합 수가 2^N → 고DOF에서 조합 폭발
- 자기 충돌이나 특이점 근처에서 경계가 불연속
- 내부 구멍(void)을 놓칠 수 있음
- 경계가 관절 한계가 아닌 Jacobian rank deficiency에서 발생하는 경우 놓침

**2DOF 경계 추적 예시:**
```
경계 = {
  sweep theta2 with theta1 = theta1_min,
  sweep theta2 with theta1 = theta1_max,
  sweep theta1 with theta2 = theta2_min,
  sweep theta1 with theta2 = theta2_max,
  inner arc (R_min),
  outer arc (R_max)
}
```

### 방법 비교 요약

| 기준 | Monte Carlo | 해석적 | 경계 추적 |
|------|-----------|--------|---------|
| 범용성 (임의 DOF) | 우수 | 나쁨 (2-3만) | 양호 |
| 경계 정확도 | 근사 | 정확 | 양호 |
| 내부 영역 표현 | 가능 | 불가 | 불가 |
| 2DOF 계산 비용 | 무시 | 무시 | 무시 |
| 6DOF 계산 비용 | 높음 (50-500ms) | 불가 | 중간 (5-10ms) |
| 구현 복잡도 | 낮음 | 중간 (DOF별) | 낮음-중간 |
| 결정적 출력 | 아니오 | 예 | 예 |
| 교육적 가치 | 낮음 | 높음 | 중간 |
| 제약 조건 처리 | 가능 | 불가 | 부분적 |

---

## 3. KineTutor3D 권장 전략

### Hybrid 접근 (DOF별 최적 방법 선택)

| DOF | 방법 | 근거 |
|-----|------|------|
| 2 | **해석적 (annulus)** | 정확, 즉시, 그 자체가 교육 콘텐츠 |
| 3 | **해석적 (회전체)** | 2DOF 단면을 축 주위로 sweep |
| 4-6 | **경계 추적 + MC 보완** | 경계는 tracing, 내부 밀도는 optional MC |

### Phase 1: 2DOF 해석적 (즉시 구현 가능)

```
방법: 해석적 annulus + 관절 한계 원호
렌더: LineRenderer (외곽선) + 반투명 Mesh (영역)
갱신: DH 파라미터 변경 시 즉시 재계산
비용: 무시 가능
```

**구현 스케치:**
```csharp
// WorkspaceEnvelope2D.cs (static helper, ~60줄)
public static Vector3[] ComputeBoundary(double a1, double a2,
    double theta1Min, double theta1Max,
    double theta2Min, double theta2Max,
    int segments = 64)
{
    // 4개 경계 곡선 생성:
    // 1. theta1 sweep at theta2_min
    // 2. theta1 sweep at theta2_max
    // 3. outer arc at R_max
    // 4. inner arc at R_min
    // → LineRenderer에 전달
}
```

**시각적 표현:**
```
+------------------------------------------+
|                                          |
|        .-~~~-.     workspace             |
|      .'  EE   '.   envelope (반투명)      |
|     /  trail    \                        |
|    |  +---+      |                       |
|    |  | B |======|====o EE               |
|    |  +---+      |                       |
|     \           /                        |
|      '.       .'                         |
|        '-...-'                           |
|                                          |
+------------------------------------------+
```

### Phase 2: N-DOF Monte Carlo (Robot Library 이후)

```
방법: Monte Carlo + alpha shape
렌더: ParticleSystem (점 구름) 또는 Mesh (convex hull)
갱신: 파라미터 변경 시 Coroutine/Job으로 비동기 재계산
비용: DOF에 비례, 프레임 분산 필수
```

### Phase 3: Compute Shader (최적화 필요 시)

```
방법: GPU에서 대량 FK 병렬 계산
렌더: StructuredBuffer → DrawProceduralIndirect
갱신: 매 프레임 가능 (6DOF 100만 샘플 < 5ms on mid-tier GPU)
비용: Shader 작성 + URP 호환 확인 필요
```

---

## 4. 교육 시나리오

| 시나리오 | 어떤 방법 | 학습 효과 |
|---------|---------|---------|
| "링크 길이 a1을 늘려보세요" | 2DOF 해석적 | annulus 외곽이 실시간으로 커지는 것을 관찰 |
| "관절 1의 범위를 ±90도로 제한하면?" | 2DOF 해석적 | workspace가 원에서 부채꼴로 축소 |
| "이 목표점에 도달할 수 있나요?" | 2DOF 해석적 + Target Marker | 목표점이 workspace 안/밖인지 즉시 판별 |
| "SCARA와 2DOF의 workspace 차이는?" | Monte Carlo | 3D workspace를 점 구름으로 비교 |
| "6DOF 로봇의 도달 영역은?" | Monte Carlo + Compute | 3D 볼륨 렌더링 |

---

## 5. 아키텍처 스케치

```
WorkspaceVisualizer (MonoBehaviour)
  |
  +-- IWorkspaceSolver          // Strategy pattern
  |     +-- Analytical2DOF      // annulus, ~60줄
  |     +-- Analytical3DOF      // surface of revolution
  |     +-- BoundaryTracer      // joint-limit sweep
  |     +-- MonteCarloSolver    // random FK sampling
  |
  +-- IWorkspaceRenderer        // Strategy pattern
        +-- LineOutlineRenderer // LineRenderer, 2D용
        +-- MeshSurfaceRenderer // 반투명 procedural Mesh, 3D용
        +-- PointCloudRenderer  // ParticleSystem, heatmap용
```

- DH 파라미터 변경 시에만 재계산 (dirty flag + 1-frame delay)
- `float` 정밀도로 시각화 (double FK 코어에서 변환)
- `DHTableEditor` 이벤트 구독 → `IWorkspaceSolver.Compute()` → `IWorkspaceRenderer.Render()`

### 성능 예산

| DOF | 방법 | 계산 시간 | 메모리 | 프레임 영향 |
|-----|------|---------|-------|-----------|
| 2 | 해석적 | <0.1 ms | ~1 KB | 없음 |
| 3 | 해석적 | ~0.5 ms | ~50 KB | 없음 |
| 4 | 경계 추적 | ~2 ms | ~100 KB | 무시 |
| 6 | 경계 추적 | ~5-10 ms | ~500 KB | 1프레임 스파이크 (허용) |
| 6 | MC 100K | ~50-100 ms | ~2-4 MB | 비동기 필수 |

---

## 6. 참고 자료

### 오픈소스 구현체
| 자료 | 설명 |
|------|------|
| robot-gui (glumb/robot-gui) | 브라우저 기반 workspace envelope 시각화, 색상 코딩된 각도 한계 |
| Peter Corke RTB `plot_workspace()` | Python에서 Monte Carlo 기반 workspace 점 구름 생성 |
| Robotics Toolbox Swift visualizer | three.js 기반 실시간 workspace 렌더 |
| Unity Robotics Visualizations Package | GPU 최적화 점 구름 렌더러 (10M+ 점) |
| VRRW-Unity | Visually Immersive Robot Workspace (Unity 기반) |
| Marginally Clever | 로봇 팔 workspace 계산 튜토리얼 |

### 학술 자료
| 자료 | 핵심 내용 |
|------|---------|
| Cao et al. (Eng. Applications of AI, 2017) | Gaussian Growth 기반 개선 MC — 경계 부근 밀도 보강 |
| Abdel-Malek et al. (2004) | Manifold stratification 기반 해석적 workspace 경계 |
| Iowa Engineering (Malek) | 내부/외부 경계 void 분석 |
| Robot Academy (Corke) | 2-joint planar arm workspace 교육 자료 |
| Mecharithm | Task space / workspace 개념 교육 |
| MDPI Applied Sciences (2020) | 3D Printing layering 개념 기반 workspace 시각화 |

---

## Downstream Sync
- `docs/ref/product/roadmap/current-feature-checklist.md` (constraint / workspace 항목)
- `docs/ref/product/roadmap/milestone-backlog.md` (P1 constraint preview)
- `docs/ref/phase5-implementation-plan.md` (Phase 5D visualization helpers 참조)
