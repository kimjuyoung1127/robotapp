# /status-copy-review — 운영자 상태문구 점검

Pendant V3 운영자 상태 문구가 SSOT를 따르는지 빠르게 점검할 때 사용한다.

## 먼저 볼 문서

- `docs/ref/product/roadmap/fr5-status-copy-ssot.md`

## 목표

- 상태문구가 일반인 기준 표현인지 확인
- raw debug token이 운영자 UI 경로에 섞이지 않았는지 확인
- 연결/위치 확인/도구/작업 기준/좌표 기준/실제 이동/잠금 이유/실시간 추적 상태가 화면 경로에 남아 있는지 확인

## 실행 순서

### 1. 금지 토큰 검사

```bash
scripts/tests/check_status_copy_tokens.sh
```

### 2. 핵심 노출 경로 확인

```bash
rg -n "대표 상태|현재 위치 읽음|도구 설정|작업 기준|좌표 기준|실제 이동|실시간 추적 상태" \
  Assets/Scripts/App/Fairino/RobotControlV3RuntimeController.cs \
  Assets/Scripts/App/Fairino/RobotControlV3RuntimeSnapshot.cs \
  Assets/Scripts/UI/RobotControlV3 \
  Assets/UI/PendantV3
```

### 3. live gate와 같이 볼 때

```bash
scripts/tests/run_fr5_live_checks.sh --live --no-edit-tests
```

## 핵심 판정 기준

- `ReadbackOnly`, `ready=False`, `coordSystem=Base`, `tool=1`, `user=1`, `dry-run simulation`이 운영자 UI 경로에 직접 남지 않는다.
- `대표 상태`, `현재 위치 읽음`, `도구 설정`, `작업 기준`, `좌표 기준`, `실제 이동`, `실시간 추적 상태`가 코드 경로에 남아 있다.
- 상태문구 수정은 가능하면 `RobotControlV3OperatorStatusCopy` 한 곳에서 시작한다.
