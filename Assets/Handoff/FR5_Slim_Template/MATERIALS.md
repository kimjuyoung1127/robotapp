# MATERIALS

## 포함 머티리얼

- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Preview.mat`
- `Assets/Runtime/Robots/FAIRINO_FR5/Materials/*`

## 라인 렌더러 머티리얼

- 링 핸들은 별도 `.mat` 에셋을 쓰지 않습니다.
- `SharedLineMaterial`이 런타임에 `Sprites/Default` 기반 머티리얼을 생성합니다.

## 분홍색 머티리얼 점검 순서

1. 대상 프로젝트가 URP인지 확인합니다.
2. `FAIRINO_FR5_Preview.mat`가 함께 import되었는지 확인합니다.
3. `Assets/Runtime/Robots/FAIRINO_FR5/Materials/`가 통째로 복사되었는지 확인합니다.
4. `.meta` 파일이 함께 유지되었는지 확인합니다.
5. URDF Importer 패키지가 빠지지 않았는지 확인합니다.

## 보존 규칙

- `Assets/Runtime/Robots/FAIRINO_FR5/Materials`와 `.meta`는 분리하지 않습니다.
- `FAIRINO_FR5_Preview.mat`는 control prefab과 별개로 함께 보존합니다.
- 경로를 바꾸면 prefab reference가 깨질 수 있으므로 상대 경로를 유지합니다.
