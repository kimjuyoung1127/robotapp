# Assets/UI/PendantV3/popups/

Pendant V3 팝업 마크업 폴더.

## 규칙
1. 팝업은 동시에 1개만 띄운다.
2. 포커스는 팝업 내부에 가두고, 닫을 때 호출 버튼으로 복원한다.
3. 버튼 순서는 `취소 -> 확인` 고정이다.
4. 위험 동작, 삭제, 미저장 닫기만 팝업을 강제한다.

## 현재/예정 파일
- `action-confirm.uxml` (서보 확인 body + meta copy)
- `action-reset-confirm.uxml` (오류 초기화 확인 body + meta copy)
- `action-run-confirm.uxml` (실행 확인 body + meta copy)
- `move-confirm.uxml` (이동 실행 확인 body + meta copy)
- `warning-dialog.uxml` (정지/경고 안내 body + meta copy)
- `recovery-dialog.uxml` (복구 순서 안내 body + meta copy)
- `unsaved-confirm.uxml` (2D kickoff 완료)
- `first-run-guide.uxml` (예정)
