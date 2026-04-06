# FR5 Official Source Register

## Purpose

`RobotControl FR5 V2`에서 사용할 공식 근거를 한곳에 등록한다.

- 기능 정의는 이 문서에 등록된 소스만 사용한다.
- 웹검색 결과는 반드시 `official` 여부를 확인한 뒤 추가한다.
- 비공식 블로그, 포럼, 영상은 여기 등록하지 않는다.

## FAIRINO Official Sources

| source_id | type | title | url | purpose | status |
|---|---|---|---|---|---|
| `fairino-support-download` | `official-doc` | FAIRINO Technical Support Download Section | [fairino.support](https://fairino.support/) | 공식 다운로드 허브, 매뉴얼/SDK/SimMachine 진입점 | registered |
| `fairino-sdk-manual-index` | `official-doc` | SDK Manual | [manual.fairino.support/latest/SDKManual](https://manual.fairino.support/latest/SDKManual/) | SDK API 계열 인덱스 | registered |
| `fairino-teaching-pendant-manual` | `official-doc` | Teaching Pendant Software | [manual.fairino.support/latest/CobotsManual/teaching_pendant_software.html](https://manual.fairino.support/latest/CobotsManual/teaching_pendant_software.html) | teach pendant UX/용어 기준 | registered |
| `fairino-manual-teaching` | `official-doc` | Robot Manual Teaching | [manual.fairino.support/latest/CobotsManual/manual_teaching.html](https://manual.fairino.support/latest/CobotsManual/manual_teaching.html) | jog / manual / teaching 동작 기준 | registered |
| `fairino-csharp-movement` | `official-doc` | C# Robot Movement | [manual.fairino.support/latest/SDKManual/C%23RobotMovement.html](https://manual.fairino.support/latest/SDKManual/C%23RobotMovement.html) | `MoveJ`, `MoveL`, jog 관련 확인 | registered |
| `fairino-csharp-common-settings` | `official-doc` | C# Common Robot Settings | [manual.fairino.support/latest/SDKManual/C%23RobotCommonSettings.html](https://manual.fairino.support/latest/SDKManual/C%23RobotCommonSettings.html) | `SetTcp4RefPoint`, `ComputeTcp4`, `SetToolCoord`, load/TCP 설정 확인 | registered |
| `fairino-coding-local-points` | `official-doc` | Coding - Local Teaching Point | [manual.fairino.support/latest/CobotsManual/coding.html](https://manual.fairino.support/latest/CobotsManual/coding.html) | local teaching point add/run 개념 확인 | registered |
| `fairino-tool-calibration-ui` | `official-doc` | Base / Tool TCP Calibration UI | [manual.fairino.support/latest/CobotsManual/base.html](https://manual.fairino.support/latest/CobotsManual/base.html) | teaching pendant에서 tool TCP 자동/수동 보정 흐름 확인 | registered |
| `fairino-version-intro-tool-tcp` | `official-doc` | Version Intro - Tool TCP Automatic Calibration | [manual.fairino.support/latest/CobotsManual/version_intro.html](https://manual.fairino.support/latest/CobotsManual/version_intro.html) | tool TCP 자동 calibration 기능 존재 여부 확인 | registered |
| `fairino-cnde` | `official-doc` | CNDE Introduction | [manual.fairino.support/latest/RobotCommunication/cnde_introduction.html](https://manual.fairino.support/latest/RobotCommunication/cnde_introduction.html) | 상태 피드백 필드 확인 | registered |
| `fairino-csharp-sdk` | `official-sdk` | FAIRINO C# SDK | [github.com/FAIR-INNOVATION/fairino-csharp-sdk](https://github.com/FAIR-INNOVATION/fairino-csharp-sdk) | C# 시그니처, 샘플, 릴리즈 버전 확인 | registered |

## Competitive Official UI References

| source_id | type | title | url | purpose | status |
|---|---|---|---|---|---|
| `ur-move-menu` | `competitive-official-ref` | Universal Robots Move Menu | [universal-robots.com](https://www.universal-robots.com/manuals/EN/HTML/SW10_10/Content/prod-usr-man/software/PolyScopeX/polyx-introduction/polyx-Move.htm) | jog / waypoint / move affordance 참고 | registered |
| `ur-fixed-waypoint` | `competitive-official-ref` | Universal Robots Fixed Waypoint | [universal-robots.com](https://www.universal-robots.com/manuals/EN/HTML/SW5_19/Content/prod-usr-man/software/PolyScope/content/BasicProgNodes/commandtab_waypoint_fixed_en.htm) | 포인트/웨이포인트 저장 UX 참고 | registered |
| `doosan-jog-plus` | `competitive-official-ref` | Doosan Jog Plus | [manual.doosanrobotics.com](https://manual.doosanrobotics.com/en/user/2.10.3/1.-M-H-Series/jog-plus-jog) | 조인트/카르테시안 jog 조작 UX 참고 | registered |

## Source Use Policy

1. 실기 명령은 `FAIRINO official`이 우선이다.
2. 타사 문서는 `UI affordance` 보강에만 사용한다.
3. `SDK repository release`와 `manual software version`이 다를 수 있으므로 둘 다 기록한다.
4. 버전 차이가 보이면 `open-questions.md`에 먼저 적고, 곧바로 명령 SSOT를 잠그지 않는다.

## Version Notes

- 공개 FAIRINO 다운로드 허브에서는 `FAIRINO-CobotSoftware-QX-V3.8.4-20250718.zip`가 보인다.
- 공개 C# SDK 저장소에서는 `v1.2.5_robot3.9.4` 릴리즈가 보인다.
- 위 차이는 API/동작 드리프트 가능성이 있으므로 `field-needed` 또는 `researching`으로 취급한다.

## Add Rule

새 소스를 추가할 때는 아래 3가지를 반드시 적는다.

1. `official`인지
2. 어떤 항목을 잠그기 위한 소스인지
3. 문서 버전 / SDK 릴리즈 버전
