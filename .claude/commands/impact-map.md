# /impact-map

교차 모듈 수정이나 live risk가 있는 변경 전에는 `meta/change-impact-map`으로 범위를 좁힌다.

## Steps

1. 중심 변경 파일 또는 기능 표면을 하나 정한다.
2. code companions와 doc companions를 나눈다.
3. 검증을 `compile`, `test`, `play/live`, `manual evidence`로 나눠 적는다.
4. `scene flow`, `operator copy`, `FR5 live truth` 리스크가 있으면 별도로 적는다.
