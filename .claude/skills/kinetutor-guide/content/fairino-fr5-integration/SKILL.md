---
name: fairino-fr5-integration
description: "Use when working on FAIRINO FR5 6-axis robot integration in this repo: collecting official FR5 docs, mapping Unity UI inputs to the FAIRINO C# SDK, planning real-robot control and safety, documenting FR5 hardware/DH/protocol details, or building adapters around MoveJ/MoveL/ServoJ/ServoCart and status feedback."
---

## Trigger
아래 요청에서 사용:
- `FAIRINO FR5 문서 정리`
- `FR5 실기 로봇 연결`
- `Unity에서 FAIRINO 제어`
- `Fairino C# SDK 붙이기`
- `FR5 상태 피드백/프로토콜 확인`

## Read First
1. `docs/ref/product/robots/fairino-fr5-integration-reference.md`
2. `docs/ref/product/robots/robot-model-library-spec.md`
3. `docs/ref/product/content/open-robotics-reference-pack.md`
4. `docs/ref/architecture-mermaid.md`

## Do
1. FAIRINO 공식 출처만 우선 사용한다.
2. FR5 관련 자료를 `hardware / installation / DH / drawings / C# SDK / status feedback / command protocol`로 나눠서 정리한다.
3. Unity 쪽 설계를 `UI -> validation -> simulation -> adapter -> live robot` 계층으로 분리한다.
4. 실기 제어 요청에서는 먼저 `MoveJ` 기반의 안전한 joint move path를 제안하고, 필요할 때만 `MoveL`, `ServoJ`, `ServoCart`로 확장한다.
5. 상태 동기화 작업에서는 `GetActualJointPosDegree`, `GetRobotRealTimeState`, `SetStatePeriod`, `GetStatePeriod` 활용 여부를 검토한다.
6. `8083 status protocol`과 `20004 feedback cycle`을 같은 것으로 단정하지 말고 별도 검증 항목으로 남긴다.
7. 연결 설계 시 `SDK version`, `robot software version`, `hardware version`, `firmware version`, `control box type` 확인 단계를 넣는다.
8. errcode 처리에서는 공식 error code table과 `GetSafetyCode`, `MotionQueueClear`, controller log download API 활용 여부를 함께 검토한다.
9. 실제 로봇 전 단계에서는 `SimMachine`, STEP model, DWG/drawings를 활용한 offline 검증 경로를 우선 제안한다.
10. 문서화 시 정확한 공식 URL을 함께 남긴다.

## Do Not
1. 비공식 블로그, 포럼, 재배포 문서를 source of truth로 채택하지 않는다.
2. ZIP/PDF/SDK 바이너리를 저장소에 그대로 추가하지 않는다.
3. Unity UI 이벤트에서 SDK를 직접 호출하게 만들지 않는다.
4. joint limit, payload, mode, connection state 검증 없이 실제 로봇 명령을 보내지 않는다.
5. 일반 운영 UI에 firmware/OS upgrade 기능을 섞지 않는다.

## Validation
- [ ] 공식 FAIRINO 링크가 포함됨
- [ ] FR5 하드웨어/SDK/프로토콜 구분이 명확함
- [ ] Unity simulation과 live robot adapter 책임이 분리됨
- [ ] 8083 vs 20004 구분 메모가 남아 있음
- [ ] 실기 제어 전에 validation step이 설계됨
- [ ] version handshake, errcode handling, offline test path가 포함됨

## Output Template
```
[fairino-fr5-integration 완료]
- 대상: {FR5 문서화 / Unity 제어 / SDK 연결 / 상태 피드백}
- 공식 출처: {사용한 링크}
- 하드웨어 baseline: {payload / reach / repeatability / installation}
- SDK 경로: {MoveJ / MoveL / ServoJ / ServoCart / status APIs}
- 추가 가치: {SimMachine / STEP / DWG / version notes / errcode / logs}
- Unity 구조: {UI -> validation -> simulation -> adapter -> live robot}
- 주의사항: {8083 vs 20004, safety, repo binary policy}
```
