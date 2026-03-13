# FAIRINO FR5 Reference And Skill

## Summary
- FAIRINO FR5 6축 실기 로봇 연동을 위한 공식 source map 문서를 추가했다.
- 이후 같은 작업을 반복할 때 재사용할 수 있도록 프로젝트 전용 skill `fairino-fr5-integration`을 추가했다.
- 공개 레퍼런스 팩과 skill index/matrix도 함께 동기화했다.

## Added
- `docs/ref/product/robots/fairino-fr5-integration-reference.md`
  - FR5 공식 제품/문서/SDK/도면/프로토콜 링크
  - 설치 요구사항, load curve, DH 다운로드, C# SDK 핵심 API 요약
  - Unity UI -> validation -> simulation -> adapter -> live robot 구조 권장안
  - `8083 status feedback` vs `20004 feedback cycle` 구분 메모
- `.claude/skills/kinetutor-guide/content/fairino-fr5-integration/SKILL.md`
  - FR5 문서화, SDK 연결, 상태 피드백, Unity 실기 제어 작업용 재사용 규칙

## Synced
- `docs/ref/product/content/open-robotics-reference-pack.md`
  - FAIRINO Official Docs row 추가
- `CLAUDE.md`
  - skill index와 dependency rule에 `fairino-fr5-integration` 추가
- `docs/status/SKILL-DOC-MATRIX.md`
  - 새 skill/document acceptance 계약 추가

## Notes
- 저장소에는 대용량 ZIP/PDF/SDK 바이너리를 넣지 않고 공식 링크만 남겼다.
- 향후 코드 구현 시에는 FR5 adapter를 `App/UI/Visualization` facade와 분리해 live robot 책임이 섞이지 않게 유지하는 것이 핵심이다.

## Web Recheck Additions
- 공식 웹 재확인 후 아래 가치 포인트를 reference/skill에 추가 반영했다.
- `3D STEP`, `2D DWG`, `SimMachine VMware/Docker`, `ROS1/ROS2/moveIt2` 링크
- `FR5 linear speed 1.7 m/s` 관련 version-dependent note
- 공식 error code table, `GetSafetyCode`, `MotionQueueClear`, controller log/data package download 계열
- `version handshake`와 `offline-first` 운영 가드레일

## Clarification Update
- `FR5 연결 패널`, `IFairinoRobotClient adapter`, `errcode UI 번역`이 무엇인지 쉽게 이해할 수 있도록 reference 문서에 plain-language 설명을 추가했다.
- 이제 문서만 읽어도 `화면`, `SDK 중간 계층`, `오류 메시지 번역`의 역할 차이를 바로 구분할 수 있다.
