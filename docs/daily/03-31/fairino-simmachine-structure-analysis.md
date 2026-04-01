# 2026-03-31 — FAIRINO SimMachine 구조 분석

## Summary
- SimMachine VM 이미지와 Software 패키지를 함께 확인해 화면 구조 초안 문서를 추가했다.
- 현재 세션에서는 VMware 실행 파일이 확인되지 않아 직접 부팅 대신 패키지 분석 중심으로 진행했다.

## What Changed
- `docs/ref/product/robots/fairino-simmachine-screen-structure-draft.md`
  - VM 이미지와 Software 패키지의 역할을 분리 정리했다.
  - `frontend/index.html`, `routeConfigFn`, `pages/*.html`, `ko.json` 기준으로 route와 메뉴 구조를 정리했다.
  - Unity `RobotControl`에 어떤 구조를 가져오고 무엇을 단순화할지 초안 해석을 추가했다.
- `docs/status/PROJECT-STATUS.md`
  - SimMachine 구조 분석과 VM 실행 보류 상태를 현재 상태 요약에 반영했다.

## Key Findings
- SimMachine은 Angular 기반 대형 WebApp 구조다.
- 메인 화면 축은 `monitor`, `teaching_management`, `peripheral`, `safeset`, `robotsetting`, `systemsetting`, `log`로 읽힌다.
- ko.json 기준으로 `Base / Tool / Wobj`, `Joint`, `TPD`, `I/O`, `Gripper`, `Manual/Auto/Drag` 키가 모두 확인된다.
- Unity는 이 전체를 한 번에 옮기기보다 `monitor 성격의 핵심 조작 경험`부터 가져오는 편이 맞다.

## VM Note
- 로컬에는 `.vmx`, `.vmdk`, `fr_get_vm_net.bat`가 있는 VM 이미지가 준비돼 있다.
- 하지만 이 세션에서는 VMware 실행 파일을 확인하지 못해 직접 부팅은 하지 않았다.
- 실제 화면 확인은 VMware 설치 후 `vmx open -> power on -> fr_get_vm_net.bat -> 브라우저 접속` 순서로 진행하면 된다.
