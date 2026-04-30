---
title: "Reference Docs Agent Guide"
doc_type: "agent-hub"
status: "active"
domain: "reference"
audience: "agent"
canonical: true
last_updated: "2026-04-29"
---

# docs/ref/

장기 참조 스펙 및 아키텍처 문서.

## 파일 목록
- `architecture-diagrams.md` — Unity 프로젝트 아키텍처
- `csharp-master-harness.md` — C# 작업 운영 하네스 (`unityctl`, 경계, 검증 루프)
- `dh-reference.md` — DH 파라미터 수학 레퍼런스 (기본 변환행렬, Modified DH 포함)
- `coordinate-mapping.md` — 로보틱스 ↔ Unity 좌표 매핑
- `test-reference-values.md` — 검증용 기준값 (2DOF, SCARA, PUMA 560)
- `code-patterns.md` — C# 구현 패턴 (readonly struct, NaN 가드, NUnit 테스트)
- `tutor-step-plan.md` — 8개 튜토리얼 스텝 정의 (Step Tutor UI 스펙)

## 규칙
- 이 폴더는 정식(canonical) 참조 문서만 보관.
- `architecture-diagrams-sync` 자동화가 코드 대비 감사.
