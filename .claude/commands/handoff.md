# /handoff

세션을 넘기거나 pause 하기 전 `meta/session-handoff`를 적용한다.

## Steps

1. 다음 세션이 가장 먼저 읽을 문서 1~3개를 고른다.
2. blocker와 현재 범위 잠금 조건을 적는다.
3. 다음 액션과 첫 검증을 구체적으로 남긴다.
4. active status 문서와 handoff 문서가 어긋나지 않는지 확인한다.
