# Competitive Synthesis

## Purpose
- 경쟁제품 분석을 KineTutor3D의 제품 전략, UX, 백로그 판단 기준으로 압축해 관리한다.

## Parent Doc
- [PRD](../../PRD.md)

## When To Read
- 경쟁제품 분석을 제품 방향, UX 우선순위, 기능 제외 원칙으로 반영할 때

## Locked Decisions
- 경쟁제품 분석은 참고 메모가 아니라 `흡수할 장점`, `버릴 단점`, `차별점` 판단 기준으로 사용한다
- KineTutor3D는 경쟁제품을 그대로 복제하지 않고 `guided-first`, `why-it-moved`, `replay`, `tablet first` 차별점을 유지한다
- 공개 이론 자료와 경쟁제품 분석은 문서 역할을 분리한다
- 초보자에게 바로 수식/DH/산업 툴 문맥을 강요하는 진입 방식은 배제한다

## Open Questions
- 향후 교육기관/B2B 판매축이 커질 때 cohort/reporting 비교 항목을 별도 분리할지

## Downstream Sync
- `docs/ref/product/foundation/product-positioning.md`
- `docs/ref/product/foundation/success-metrics.md`
- `docs/ref/product/roadmap/milestone-backlog.md`
- `docs/ref/product/ux/guided-lesson.md`
- `docs/ref/product/ux/sandbox.md`
- `docs/ref/product/ux/instructor-mode.md`

## Last Updated
- 2026-03-11 (KST)

## Synthesis Table
| reference_product | category | core_strength | absorbed_patterns | rejected_patterns | why_it_matters_for_kinetutor3d | target_doc_sync |
|---|---|---|---|---|---|---|
| `Intelitek RoboX` | 교육용 커리큘럼 플랫폼 | scaffolded lesson, progress tracking, instructor-led flow | step gate, student-paced + instructor-led 흐름, 강사용 운영 시점 | 기관/LMS 중심의 무거운 구매형 구조 | Guided Lesson과 Instructor Mode를 교육 현장 친화적으로 만든다 | `product-positioning`, `guided-lesson`, `instructor-mode`, `success-metrics` |
| `RoboDK` | 산업용 실습/시뮬레이션 | hardware-free practice, pick/place examples, hands-on sandbox | 실습형 sandbox, pick foundation, robot comparison | 초심자에게 과한 실무 툴 복잡도 | Sandbox를 실제 실습과 연결하되 입문자 난이도를 유지한다 | `sandbox`, `robot-library`, `milestone-backlog` |
| `UR Academy` | 벤더 guided training | guided flow, real robot 없이도 학습 가능 | scaffolded lesson, step progression, training narrative | 벤더 종속 UI/용어 | Guided Lesson의 학습 흐름 품질을 높인다 | `guided-lesson`, `product-positioning` |
| `ABB / Visual Components` | 산업/교육기관 확장 플랫폼 | 강사용 demo, challenge, 기관용 확장성 | instructor demo, teaching note, future classroom features | 무거운 산업 UI, 공장 전체 시뮬레이션 우선순위 | 강사 친화 기능은 강화하되 범위 팽창을 막는다 | `instructor-mode`, `milestone-backlog`, `release-gates` |
| `CoppeliaSim` | 범용 시뮬레이터 | 확장성 높은 sandbox, 다양한 로봇 참조 | multi-robot sandbox 참조, advanced future inspiration | 초심자 친화성 부족, 범용 툴 복잡도 | Sandbox를 확장 가능하게 하되 제품 UX는 단순하게 유지한다 | `sandbox`, `release-gates` |

## Interpretation Rule
- `absorbed_patterns`는 문서/UX/백로그에 명시적으로 반영한다.
- `rejected_patterns`는 release gate와 scope guardrail에 명시적으로 남긴다.
- 경쟁제품 분석은 기능을 늘리기 위한 근거가 아니라, `무엇을 하지 않을지`를 명확히 하는 근거로도 사용한다.

## Beginner Track Interpretation
- `RoboX`, `UR Academy`에서 보이는 scaffolded 진입 구조는 흡수하되, 완전 초보자를 곧바로 공식/산업 용어로 밀어 넣는 흐름은 배제한다.
- KineTutor3D의 `Lesson 0~3`은 경쟁제품의 장점을 가져오되, `Why It Moved`, trail, target marker 중심의 더 쉬운 직관 진입으로 차별화한다.
