# Unity/C# 코딩 가이드라인

## 1. 시작 순서
1. 활성 문서 읽기 (START-HERE → master-plan → project-context → 이 파일)
2. 대상 폴더 CLAUDE.md 읽기
3. 기존 패턴 검색 후 구현

## 2. 역할 분담
- **Claude**: 구현, 테스트 작성, 문서 동기화
- **사용자**: 요구사항 정의, 검토, 승인

## 3. 구현 원칙
- 핵심 모듈(Types, Math, Kinematics)은 **순수 C#** — `using UnityEngine` 금지
- Unity API는 경계 모듈(UI, Visualization, App)에서만 사용
- 모든 수치 연산은 `double` — `float` 절대 사용 금지 (Visualization 렌더링 경계 제외)
- XML doc summary는 한국어 1-3줄로 클래스/공개 메서드에 작성
- 공식 참조가 있는 코드는 XML doc에 수학 공식 표기

## 4. 수학 모듈 규칙
- NaN/Infinity 전파 방지: 생성자와 연산자에서 가드
- Factory 메서드 제공: `Identity`, `Zero`
- 연산자 오버로딩: `+`, `-`, `*`, `==`
- 불변(immutable) 선호: `readonly struct` 사용
- 모든 타입에 대응하는 EditMode 테스트 필수

## 5. 기구학 규칙
- DH 공식을 코드 주석에 참조 (예: `A_i = Rz(θ)·Tz(d)·Tx(a)·Rx(α)`)
- 허용 오차: 위치 < 1e-4 m, 회전 < 1e-3 rad
- DHStandard.cs는 **베이스라인** — 명시적 요청 없이 수정 금지
- 새 DH 변형은 별도 파일 (DHModified.cs 등)

## 6. 완료 보고 형식
```
- 범위: [변경 내용]
- 파일: [수정된 파일 목록]
- 검증: [통과한 체크 항목]
- 일일 동기화: [업데이트된 문서]
- 위험 요소: [잠재적 문제]
- 다음 권장: [다음 작업]
```

## 7. 로깅 규칙
- 작업 완료 시 `docs/daily/MM-DD/` 폴더에 로그 작성
- Phase 완료 시 `sprint-docs-sync` 스킬로 일괄 동기화

## 8. 필수 코딩 규칙
1. 수정 전 반드시 현재 파일 내용 읽기
2. 기존 패턴 검색 후 재사용 (중복 금지)
3. 대상 폴더 CLAUDE.md를 먼저 확인
4. 구조 변경과 동작 변경은 별도 커밋

## 9. Unity 전용 규칙
- Assembly Definition 사용: 모듈별 .asmdef 파일
- 순환 참조 금지 (모듈 의존성 방향 준수)
- Inspector 노출 필드는 `[SerializeField]` 사용
- `MonoBehaviour` 상속은 UI/Visualization/App 모듈에서만
- `double→float` 캐스팅은 Visualization/ 렌더링 경계에서만 허용
