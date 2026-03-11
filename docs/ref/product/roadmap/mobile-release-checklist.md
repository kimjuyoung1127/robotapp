# Mobile Release Checklist

## Purpose
- Android 태블릿 우선, iPad 후속 전략에 맞춘 내부 테스트와 스토어 제출 체크리스트를 정의한다.

## Parent Doc
- [PRODUCT-ROADMAP](../../PRODUCT-ROADMAP.md)

## When To Read
- 모바일 내부 배포, 태블릿 QA, Play/App Store 제출 준비를 할 때

## Locked Decisions
- 첫 실기기 검증은 `Android Tablet` 우선이다
- `iPad/TestFlight`는 Android 태블릿 내부 배포 후속 단계다
- 스마트폰은 제한형 지원으로 남긴다

## Open Questions
- Android internal app sharing과 Play internal testing 중 기본 경로를 어느 쪽으로 둘지

## Downstream Sync
- `docs/ref/PRODUCT-ROADMAP.md`
- `docs/status/PROJECT-STATUS.md`

## Last Updated
- 2026-03-11 (KST)

## Platform Priority
1. Android Tablet
2. iPad
3. WebGL
4. Phone 제한형

## Android Internal Test Checklist
| platform | build_type | signing_ready | metadata_ready | qa_ready | submission_ready |
|---|---|---|---|---|---|
| Android Tablet | Internal APK/AAB | keystore 필요 | 앱 이름/아이콘 기본 필요 | 태블릿 safe area, touch target, orientation, install test | 아니오 |

- 고유 package name
- keystore / signing
- version code 증가 규칙
- tablet portrait/landscape 정책
- install-on-device smoke test
- Guided Lesson 완주
- Sandbox numeric input / replay 검증

## Android Play Submission Checklist
| platform | build_type | signing_ready | metadata_ready | qa_ready | submission_ready |
|---|---|---|---|---|---|
| Android Tablet | Play AAB | Play App Signing 포함 | 스토어 설명, 스크린샷, 개인정보 문서 필요 | 내부 테스트 완료 필요 | 예 |

- Play App Signing
- AAB 업로드
- Internal Testing 또는 Internal App Sharing 기준 정리
- privacy policy
- developer verification 상태 확인

## iPad TestFlight Checklist
| platform | build_type | signing_ready | metadata_ready | qa_ready | submission_ready |
|---|---|---|---|---|---|
| iPad | TestFlight | Apple Team, provisioning, certificate 필요 | 앱 이름/아이콘/빌드 메타데이터 필요 | iPad safe area, orientation, touch QA 필요 | 아니오 |

- bundle identifier
- Apple Developer Team
- provisioning / certificate
- Xcode archive 또는 Unity Build Automation 경로
- iPad UI clipping / safe area 확인

## App Store Submission Checklist
| platform | build_type | signing_ready | metadata_ready | qa_ready | submission_ready |
|---|---|---|---|---|---|
| iPad | App Store | signing 완료 필요 | product page, screenshots, privacy details, accessibility summary 필요 | TestFlight 검증 완료 필요 | 예 |

- App Review Guidelines 확인
- TestFlight beta flow 완료
- product page metadata
- privacy details
- accessibility support summary
- 최신 SDK / Xcode 요구사항 반영

## Official Notes
- Apple은 `2026-04-28`부터 App Store Connect 업로드 앱에 최신 SDK 최소 요구사항을 적용한다.
- Google Play 신규 앱은 `Play App Signing`이 필수다.
- Android developer verification은 2026년 3월부터 전 개발자에게 열렸다.
