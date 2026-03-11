# 03-11 Product Doc Expansion

## 반영 범위
- `.claude/skills/` 기존 skill 6종 보강
- `.claude/skills/kinetutor-guide/content/robotics-reference-to-lesson/SKILL.md` 추가
- `docs/ref/product/ux/guided-lesson.md`
- `docs/ref/product/ux/robot-library.md`
- `docs/ref/product/ux/sandbox.md`
- `docs/ref/product/content/concept-to-ui-map.md`
- `docs/ref/product/content/open-robotics-reference-pack.md`
- `docs/ref/product/content/llm-teaching-strategy.md`
- `docs/ref/product/roadmap/mobile-release-checklist.md`
- `AGENTS.md`, `CLAUDE.md`, `docs/status/SKILL-DOC-MATRIX.md`

## 핵심 내용
1. `robot.md`에서 lesson adaptation, DH/MDH warning, joint highlight, pose history, pick foundation 규칙을 추출해 skill에 반영
2. Guided Lesson/Sandbox 문서에 joint numeric input, why-it-moved, snapshot/sequence 저장 계약을 추가
3. 공개 robotics reference pack과 LLM teaching strategy, 모바일 릴리스 체크리스트를 새 leaf 문서로 추가
4. Android 태블릿 우선, iPad 후속 배포 우선순위를 문서에 고정

## 검증 메모
- 비공개 강의자료 원본 파일명/경로/이미지 직접 기록 없음
- `theta` read-only 규칙 유지
- 새 skill routing이 루트 인덱스와 매트릭스에 반영됨
