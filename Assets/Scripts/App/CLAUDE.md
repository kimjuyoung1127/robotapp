# App/

애플리케이션 오케스트레이터.

## 파일 (예정)
- `AppController.cs` — 메인 애플리케이션 컨트롤러 (템플릿 선택, 업데이트 루프)

## 규칙
1. 단일 오케스트레이터 패턴 — AppController가 UI↔Kinematics 연결
2. 템플릿 변경 전파: Template → DH Table → Sliders → FK → Visualization
3. 직접 기구학 계산 금지 — `Kinematics/`에 위임
