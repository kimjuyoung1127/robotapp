# CHECKLIST

## 1. Compile

- `unityctl check --project C:/Users/ezen601/Desktop/Jason/robotapp2 --json`
- compile error 0 확인

## 2. Scene

- `Assets/Scenes/FR5_Template_Demo.unity` 열기
- Play 진입
- FR5 control prefab 표시 확인

## 3. Ring Interaction

- 관절 링 6개 보이는지 확인
- 링 드래그 시 각도 변화 확인
- 3D pose가 즉시 따라오는지 확인

## 4. Material

- 분홍색 없이 렌더링되는지 확인
- preview/control prefab 모두 정상인지 확인

## 5. Evidence

- 대표샷: `evidence/fr5-template-ready.png`
- 포즈샷: `evidence/fr5-template-neutral.png`
- 포즈샷: `evidence/fr5-template-showcase.png`
- 프레임 시퀀스:
  - `evidence/sequence-frame-00-neutral.png`
  - `evidence/sequence-frame-01-ready.png`
  - `evidence/sequence-frame-02-showcase.png`
  - `evidence/sequence-frame-03-wristturn.png`

## 6. Self Review

- 원인: full template가 과포함 상태였는지 확인
- 조치: slim runtime만 남겼는지 확인
- 검증: `unityctl` compile / EditMode / evidence 경로 확인
- 재발 방지: 다음 export도 slim manifest 기준으로만 생성
