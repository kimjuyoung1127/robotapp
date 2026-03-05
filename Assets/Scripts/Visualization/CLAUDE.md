# Visualization/

3D 렌더링 헬퍼 (로봇 기구학 시각화).

## 파일 (예정)
- `FrameGizmo.cs` — 좌표 프레임 축 표시
- `VectorArrow.cs` — 3D 벡터 화살표 렌더링
- `StepAnimator.cs` — 단계별 애니메이션 컨트롤러

## 규칙
1. **이 모듈만** `double → float` 캐스팅 수행 (렌더링 경계)
2. 좌표 변환은 `docs/ref/coordinate-mapping.md` 참조
3. 위치 에러 표시 임계값: 1e-4 m
4. 회전 에러 표시 임계값: 1e-3 rad

## 좌표 변환 (로보틱스 → Unity)
```csharp
Vector3 ToUnity(Vec3D v) => new Vector3((float)v.X, (float)v.Z, (float)v.Y);
Vec3D FromUnity(Vector3 v) => new Vec3D(v.x, v.z, v.y);
```
