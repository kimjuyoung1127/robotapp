# Pendant V3 Context Panel Density Remediation

Last Updated: 2026-04-15 (KST)

## 문제 정의
- `ContextPanel`은 현재 `CoordStrip`, `StatusCard`, `SafetyDiagnostics`, `ActionHint`, `WhyItMoved`를 한 컬럼에 동시에 노출한다.
- 실제 겹침은 아니지만, 고정 폭 `320px` 안에서 카드 5개가 상시 노출돼 오른쪽 패널이 과밀하게 보인다.
- 특히 `CoordStrip`는 관절 6칸 + TCP 6칸 + `Joint/TCP/Both` 버튼을 모두 포함해 세로 높이를 크게 차지한다.

## 확인된 원인
1. 구조상 겹침 아님
   - `ContextPanel`은 `flex-direction: column`이다.
   - `CoordStripHost`, `StatusCardHost`, `SafetyDiagnosticsHost`는 세로로 순서대로 쌓인다.
2. 체감 과밀의 직접 원인
   - `ContextPanel` 폭이 `320px`로 고정
   - `CoordStrip` 카드 높이가 큼
   - `StatusCard`, `SafetyDiagnostics`, `ActionHint`, `WhyItMoved`가 모두 상시 노출
3. 런타임 확인
   - `ContextPanel` visible, childCount=`5`
   - `CoordStripHost`, `StatusCardHost`, `SafetyDiagnosticsHost`, `ActionHint`, `WhyItMoved` 모두 visible

## 해결 원칙
- 빠른 완화와 구조 개편을 분리한다.
- 이번 사이클은 사용성 압박을 먼저 줄이고, 큰 IA 변경은 후속 페이즈로 미룬다.
- 상태 원천과 binder 책임은 유지한다. 오른쪽 컬럼 문제를 해결한다고 preview/shell SSOT는 건드리지 않는다.

## Phase 계획

### Phase 1. CoordStrip 접기/토글화
- 목표: 오른쪽 컬럼 높이를 즉시 줄인다.
- 범위:
  - `CoordStrip` 헤더에 접기/펼치기 토글 추가
  - 초기 상태는 `expanded`
  - 접힌 상태에서는 좌표 상세 그리드와 `Joint/TCP/Both` 버튼을 숨기고, 헤더만 유지
- 비목표:
  - 저장/persistence 추가
  - 상태 카드 통합
  - 우측 컬럼 IA 변경

### Phase 2. StatusCard + SafetyDiagnostics 정보 재배치
- 목표: 상태 정보 중복을 줄인다.
- 후보:
  - `StatusCard` 안으로 `Safety summary`를 흡수
  - `SafetyDiagnostics`는 fault/warning 시 강조 카드로 축소

#### Phase 2A
- `StatusCard` 상단에 compact safety summary 추가
- `SafetyDiagnostics`는 `warning/fault` 상태에서만 visible
- 정상 상태에서는 오른쪽 컬럼 상시 카드 수를 1개 줄인다

#### Phase 2A 결과
- 완료
- 정상 상태: `StatusCard` summary 유지 + `SafetyDiagnostics` hidden
- fault 상태: `StatusCard` summary를 복구 우선 톤으로 전환 + `SafetyDiagnostics` 재노출

### Phase 3. 오른쪽 컬럼 탭 분리
- 목표: 구조적으로 `상태 모드 / 좌표 모드`를 분리한다.
- 후보:
  - `Status` 탭: `StatusCard + SafetyDiagnostics + ActionHint`
  - `Coordinate` 탭: `CoordStrip + WhyItMoved`

#### Phase 3A
- `ContextPanel` 상단에 `상태 / 좌표` 탭 추가
- 기본값은 `상태`
- `SafetyDiagnostics`, `WhyItMoved`는 탭 상태를 존중해 표시/숨김

#### Phase 3A 결과
- 완료
- `Status` 탭: `StatusCard + SafetyDiagnostics + ActionHint`
- `Coordinate` 탭: `CoordStrip + WhyItMoved`
- play smoke 기준 `mode=Status`, `mode=Coordinate` 전환 확인

#### Phase 3B
- `ContextPanel` 세로 overflow를 `ScrollView`로 흡수
- `ActionHint`, `WhyItMoved` 같은 하단 카드가 flex 압축으로 잘리지 않게 `flex-shrink` 규칙 고정
- visual smoke에서 상단/하단 스크롤 기준 텍스트 잘림이 없는지 확인

#### Phase 3B 결과
- 완료
- `ContextPanelScroll` 추가
- `rc-context-card`는 `flex-shrink: 0`으로 고정
- `rc-context-flex`는 남는 공간 강제 점유를 끄고 실제 카드 높이만 사용
- `Status` 탭 하단 `다음 행동 추천` 본문과 `Coordinate` 탭 하단 `최근 조작 메모` 본문이 스크롤 하단에서 정상 노출되는 것 확인

## 우선순위
1. `Phase 1` CoordStrip 접기/토글화
2. `Phase 3` 오른쪽 컬럼 탭 분리
3. `Phase 2` StatusCard + SafetyDiagnostics 재배치

## 완료 기준

### Phase 1
- `CoordStrip`를 접고 펼칠 수 있다.
- 접힌 상태에서 오른쪽 컬럼의 세로 점유가 눈에 띄게 줄어든다.
- compile green
- UITK click smoke에서 `CoordStripBody` visible 토글이 확인된다.
