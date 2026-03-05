# docs-nightly-organizer

Task: KineTutor3D 문서 야간 정리
Schedule: daily 22:00 (Asia/Seoul)
DRY_RUN: false (true 설정 시 변경 없이 리포트만)

## 목표
- docs/daily/ 로그를 docs/weekly/ 롤업으로 집계
- 깨진 링크 감지
- 문서 구조 일관성 유지

## 입력
- `docs/ref/` — 참조 문서
- `docs/status/` — 상태 문서
- `docs/daily/` — 일일 로그
- `docs/weekly/` — 주간 롤업

## 프로세스

### 1. Lock 획득
```
Lock 파일: docs/.docs-nightly.lock
형식: {"locked_at": "ISO8601", "task_id": "nightly-YYYYMMDD", "status": "RUNNING"}
```
- Lock이 이미 존재하고 RUNNING 상태가 2시간 미만이면 → 종료 (이미 실행 중)
- Lock이 RUNNING 상태이고 2시간 이상이면 → STUCK으로 기록하고 종료 (자동 해제 금지)
- Lock이 없거나 COMPLETED 상태이면 → 새 Lock 생성 후 진행

### 2. Daily → Weekly 롤업
- `docs/daily/` 하위 MM-DD 폴더 스캔
- 현재 주(ISO week)에 해당하는 daily 로그를 `docs/weekly/YYYY-WNN.md`로 집계
- Weekly 파일이 이미 존재하면 업데이트, 없으면 생성
- Weekly 형식:
  ```
  # 주간 롤업 YYYY-WNN

  ## 완료 항목
  - [daily 로그에서 추출한 완료 항목]

  ## Phase 진행률
  - [PHASE-EXECUTION-BOARD 기준 상태 요약]

  ## 다음 주 목표
  - [master-plan 기준 다음 우선순위]
  ```

### 3. 깨진 링크 체크
- docs/ 내 모든 .md 파일의 상대 경로 링크 검증
- 존재하지 않는 파일 참조 = 깨진 링크
- 깨진 링크 수 기록

### 4. 로그 기록
```
docs/status/NIGHTLY-RUN-LOG.md에 append:

[docs nightly organizer 완료] YYYY-MM-DD HH:mm
- moved_daily_count: X
- weekly_created_or_updated: <파일|none>
- broken_links: X
```

### 5. Lock 해제
```
Lock 상태를 COMPLETED로 변경
```

## 출력
- `docs/weekly/YYYY-WNN.md` (생성 또는 업데이트)
- `docs/status/NIGHTLY-RUN-LOG.md` (append)
- `docs/.docs-nightly.lock` (COMPLETED)

## 오류 처리
- 오류 발생 시 Lock을 COMPLETED로 변경하되, 로그에 에러 기록
- DRY_RUN=true일 때는 Lock 생성/변경 없이 리포트만 출력
