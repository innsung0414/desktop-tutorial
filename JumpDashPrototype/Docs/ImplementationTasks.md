# 스펙 완료 기준 구현 작업

## 목표

[게임 스펙](GameSpecification.md)의 완료 기준을 기준으로 기능을 구현하고 검증한다. 완료 기준은 Unity 씬 연결과 실행 동작을 모두 확인해 통과로 표시한다. 친구 기록 비교는 제공 방식이 확정되지 않아 이번 완료 기준에서 제외하고 결정 대기 항목으로 관리한다.

## 완료 기준 추적

| ID | 완료 기준 | 상태 | 확인 내용 |
| --- | --- | --- | --- |
| AC-01 | START 버튼으로 게임 플레이에 진입한다. | 런타임 PASS | 최신 타이틀의 START RIDING 버튼 이벤트로 0.75초 전환 후 실제 플레이를 시작했다. MainScene은 타이틀 진입 시 한 번만 로드하며 START 시 재로드하지 않는다. |
| AC-02 | 자동 전진과 생존 시간 점수가 동작하고, 생존 시간이 길어질수록 플레이 속도가 계속 증가한다. | 코드·씬 설정 PASS · 장시간 생존성 대기 | MainScene의 증가량을 초당 0.055로 저장했다. 코드 식 기준 속도는 시작 5.0, 60초 8.3, 120초 11.6, 180초 14.9이며 상한이 없다. 장애물 생성 간격은 2.4–3.2초 시작, 0.025초/초 감소, 1.35/1.9초 하한을 유지한다. 3분 실전 생존성은 별도 플레이 표본으로 확인한다. |
| AC-03 | 기본 점프와 아이템 획득 후 사용할 수 있는 추가 점프가 동작한다. | 런타임 PASS | Space 입력 후 Player y가 -2.26에서 -1.50으로 상승했다. 장애물 없는 Play에서 자연 생성 아이템 획득으로 HUD가 x1이 되고 one-shot audio가 재생됐다. 공중 Space 입력 뒤 HUD x0 및 DoubleJumpEffect(Clone)을 확인했다. 추가 점프 사용은 결정적 재현을 위해 아이템 복제본을 임시 이동한 별도 런에서 확인했으며 자연 생성 아이템 좌표·높이를 바꾸지 않았다. 획득 연출은 AC-08/20에서 새 절차형 빙정 효과로 런타임 재확인했다. |
| AC-04 | Tab 대시와 쿨타임 UI가 동작한다. | 런타임 PASS | Tab 입력 후 RECHARGING 0.3s와 소진 막대를 확인했다. 쿨타임 만료 뒤 두 번째 Tab으로 다시 대시해 RECHARGING 1.2s가 시작되는 것을 확인했다. |
| AC-05 | Small, Tall, Wide 장애물이 여러 패턴으로 등장하고 충돌을 감지한다. | 코드 수정 완료 · Unity 런타임 재확인 대기 | 생성 때 각 충돌체의 바닥을 CompositeCollider2D 설면에 맞추고, 장애물 그림을 충돌체의 폭·높이에 맞춘다. 기존 3종 패턴과 게임 오버 동작은 이전 런에서 확인했다. |
| AC-06 | 충돌 후 플레이가 멈추고 기록, 게임 오버 UI, 충돌 효과음, 카메라 흔들림이 표시된다. | 런타임 PASS | 충돌 뒤 TIME 00:03에서 점수가 멈추고 RUN OVER 결과 카드가 나타났다. 충돌 직후 CameraShake 동작을 보기 위해 Play 중 진단값을 10초/0.5로 잠시 높였을 때 연속 캡처에서 카메라 위치가 x=24.51485에서 24.34869로 바뀌었다. 진단 뒤 저장 씬의 기본값 0.2초/0.15를 복원했다. `impact hit.wav`가 PlayerController의 hitSound에 연결되고 활성 AudioSource/AudioListener 및 호출 경로를 확인했다. |
| AC-07 | R 키 재도전과 최고 기록 유지가 동작한다. | 런타임 PASS | 최신 UI에서 R 입력 및 RIDE AGAIN 클릭 후 점수가 00:00으로 초기화되고 BEST 01:49가 유지되는 것을 확인했다. |
| AC-08 | 점프, 대시, 충돌, 아이템 획득 효과음과 점프·획득 파티클이 연결돼 있다. | Unity 런타임 PASS | `DoubleJumpItem.OnTriggerEnter2D`에서 획득 처리·효과음 재생 뒤 새 `DoubleJumpPickupBurst.Play`를 호출한다. Play 모드에서 새 효과 43개 SpriteRenderer 구성을 생성했고 [Game 뷰 캡처](Previews/pickup-obstacle-vfx-review.png)에서 확인했다. 기존 효과 프리팹 참조와 주황색 획득 색조는 제거했다. 기본 재생 시간은 0.9초다. |
| AC-09 | 현재 기록과 최고 기록, 더블 점프 횟수, 대시 쿨타임을 화면에서 확인할 수 있다. | 런타임 PASS | 최신 HUD에서 Time/Best, AIR JUMPS x0, DASH READY 카드와 충전 막대를 확인했다. |
| AC-10 | 타이틀 대기 중 실제 활강 장면이 화면 전체를 채우고 그 위에 메뉴와 순위표를 표시한다. | Unity 런타임·화면 PASS | 기존 타이틀 Canvas·EventSystem과 작은 미리보기를 제거했다. 전체 화면 MainScene 위에 메뉴가 표시되고, 추가 30초 대기 중 x=132.973→275.469로 계속 이동하며 점수 0·속도 5·장애물/아이템 0개를 유지했다. |
| AC-11 | Small, Tall, Wide 장애물이 각기 다른 크기의 투명 아트와 맞는 충돌 영역을 사용한다. | Unity Inspector·런타임 PASS | MainScene에서 Small/Tall/Wide 프리팹의 전용 Sprite, Transform 크기, Collider2D 및 생성된 장애물 표시를 확인했다. 타이틀 attract 화면은 직진 전용이라 장애물을 배치하지 않는다. |
| AC-12 | 장애물 패턴이 진행 구간에 따라 점진적으로 늘고 혼합 배치가 반복 등장한다. | 수정 후 Unity 런타임·코드 PASS | 16개 패턴 풀을 유지하고 쉬운 6종은 18초까지, 11종은 42초까지 사용한다. 패턴 안 장애물은 약 11–12 단위 간격으로 나오며, 패턴 묶음 사이 간격은 최소 11단위다. |
| AC-13 | 플레이 HUD에 속도 배수가 보여 난이도 진행을 즉시 확인할 수 있다. | 런타임 PASS | MainScene HUD에서 TRAIL SPEED 배수와 시간·Best·AIR JUMPS·DASH 상태 표시를 확인했다. |
| AC-14 | 로컬 순위표는 생존 시간 내림차순 상위 5개만 유지하고 재실행 후에도 보존한다. | Unity 런타임·로직 PASS | TitleScene에서 이전 최고 기록이 PLAYER 이름으로 표시됐다. 게임 오버 제출로 TESTER 기록이 PLAYER 01:49 아래 2위에 추가되는 것을 확인했고, 별도 임시 기록 RELOAD 01:50은 TitleScene 재로드 뒤 1위로 남았다. 다섯 기록이 찬 경우 더 낮은 기록의 자격 순위가 -1임을 확인했다. 테스트 기록 저장 뒤 원래 PlayerPrefs JSON과 BestScore를 복원했다. |
| AC-15 | 게임 오버 기록이 상위 5위에 들면 닉네임을 입력해야 저장되고, 자격 밖 기록은 저장하지 않는다. | Unity 런타임·로직 PASS | 실제 Game Over에서 자격 기록 안내와 포커스된 닉네임 입력창이 열렸다. TESTER 제출 뒤 저장 완료 문구와 순위 #2를 확인했다. 다섯 자리가 찬 경우 기준 밖 기록은 저장 자격이 없음을 확인했다. |
| AC-16 | 타이틀과 게임 오버에서 같은 로컬 상위 5개 순위표를 보여준다. | Unity 런타임 PASS | TitleScene의 LOCAL ARCADE TOP 5와 Game Over의 ARCADE TOP 5 양쪽에 동일한 PLAYER 기록이 표시되고, 제출 시 Game Over 행과 순위가 갱신됐다. |
| AC-17 | 시작 시 0.75초 메뉴/HUD 전환을 거치며 같은 씬·플레이어·카메라의 연속 이동을 유지한다. | 수정 후 Unity 런타임 PASS | 씬·Player·Camera의 인스턴스 ID가 동일하고 x=275.469→277.359→279.406으로 연속 이동했다. 0.400초에 점수 0/메뉴 alpha 0.455/HUD alpha 0.187, 0.850초에 HUD alpha 1/점수 0.096을 확인했다. 실제 플레이 1.897초에 장애물 0개, 2.198초에 첫 패턴 2개가 생성됐다. |
| AC-18 | 초반 난이도와 상승 속도가 회피 가능하고, 이동 속도는 생존 시간에 따라 계속 오르며 장애물 생성 간격은 설정 하한을 지킨다. | 난이도 조정 PASS · 3분 실전 표본 대기 | 속도 증가량을 초당 0.055로 높이고 패턴 간 최소 간격을 11.5에서 11단위로 줄였다. 16개 패턴은 42초부터 모두 사용하며 생성 간격 하한은 유지한다. 계산상 3분에 초속 14.9에 도달한다. 3분 실전 생존성은 실제 플레이 표본으로 확인한다. |
| AC-19 | 플레이어 피격 영역이 캐릭터 중심부에 맞고, 장애물이 설경과 쉽게 구분된다. | Unity Inspector·Game 뷰 PASS | Player CapsuleCollider2D는 0.40×0.45(월드 단위)로 기존 0.45 폭보다 11% 좁다. 장애물 3종에 주황색 외곽 렌더를 추가하고, Game 뷰에서 Small/Tall/Wide 모두 눈·산 배경과 구분됨을 확인했다. |
| AC-20 | 더블 점프 아이템이 눈에 잘 띄고, 기본 점프로 닿을 수 있으며, 획득 연출이 명확하다. | Unity Game 뷰 PASS · 실제 회수성 기존 확인 | 아이콘 런타임 크기를 기존 대비 약 17% 키우고 어두운 키라인, 금빛·청록 링, 세로 신호 빔과 맥동 후광을 더했다. 1920×1080 Game 뷰에서 실제 프리팹 아이콘이 숲과 설면 앞에 선명하게 분리되는 것을 확인했다. 획득 시 절차형 43 렌더 요소는 기존 Play 검증에서 확인했고, 아이템 높이도 기본 점프로 닿는 범위로 확인했다. [가시성 캡처](Previews/item-visibility-review.png) |
| AC-21 | Small, Tall, Wide의 충돌체와 보이는 그림이 같은 바닥선에 맞는다. | Unity 런타임 PASS | Play 모드 측정에서 CompositeCollider2D 설면 상단과 Small/Tall/Wide Collider2D 바닥이 모두 y=-2.5(차이 0)였다. PNG 하단 투명 여백을 보정한 Small/Wide 그림은 화면에서 눈 표면에 닿고, Tall은 기존 위치를 유지했다. [Game 뷰 캡처](Previews/pickup-obstacle-vfx-review.png) |
| AC-22 | 플레이 중 Escape 일시정지·재개, 설정 저장, 타이틀 복귀와 게임 종료가 동작한다. | 기능 구현·Windows 빌드 PASS · 메뉴 입력 런타임 확인 대기 | MainScene의 GameManager가 PauseMenu를 추가한다. 정지 중 시간 배율은 0이고, 음량·전체 화면 설정은 PlayerPrefs에 저장한다. 사용자가 실행 파일을 실행하지 말라고 요청해 빌드 성공만 확인했고 실제 키·버튼 상호작용은 실행하지 않았다. |
| AC-23 | 타이틀 화면에서 주요 조작법을 확인할 수 있다. | 구현·Windows 빌드 PASS · Game 뷰 확인 대기 | 하단 HOW TO RIDE 카드에 Space 점프, 아이템 획득 후 Space 더블 점프, Tab 대시, Escape 일시정지를 두 줄로 표시한다. 게임은 요청에 따라 실행하지 않았다. |

## 작업 목록

| ID | 작업 | 상태 | 관련 기준 |
| --- | --- | --- | --- |
| T-01 | 시작 시 더블 점프 횟수를 0으로 두고 아이템 획득으로만 횟수를 추가한다. | 완료 | AC-03 |
| T-02 | 장애물 생성 간격을 프레임 시간 대신 플레이 경과 시간으로 조정한다. | 완료 | AC-02 |
| T-03 | 게임 오버 중에도 카메라 흔들림이 끝까지 재생되고 원래 위치로 돌아오게 한다. | 완료 | AC-06 |
| T-04 | 최고 기록 갱신을 즉시 저장하고 게임 오버 처리를 중복 실행하지 않게 한다. | 완료 | AC-06, AC-07 |
| T-05 | 대시 쿨타임 UI가 누락된 참조나 0초 쿨타임에서도 안전하게 표시되게 한다. | 완료 | AC-04 |
| T-06 | Unity 컴파일과 씬·프리팹 참조 검증을 실행하고 결과에 따라 완료 기준 상태를 갱신한다. | 완료: C# 컴파일과 정적 참조 검증이 완료됐고, Unity GUI에서 AC-01–AC-09 런타임 acceptance를 확인했다. | AC-01–AC-09 |
| T-07 | Unity 에디터에서 플레이 acceptance를 다시 실행한다. | 완료: Unity GUI에서 AC-01–AC-09 플레이 acceptance를 확인했다. Pipeline 씬 조회 timeout은 GUI 수동 검증을 막지 않았다. | AC-01–AC-09 |
| T-08 | 기존 시트형 플레이어 이미지를 투명 단일 스프라이트로 다시 그리고 MainScene Player에 연결한다. | 완료: 새 에셋 및 씬 참조 반영 | 비주얼 에셋 |
| T-09 | 기존 안내 문구가 포함된 배경 시트를 글자 없는 겨울 산악 배경으로 교체하고 반복 배경 3개에 연결한다. | 완료: 새 에셋 및 씬 참조 반영 | 비주얼 에셋 |
| T-10 | Player의 월드 충돌 박스를 시각 크기에 맞춰 장애물 충돌과 지면 접촉을 안정화한다. | 완료: 로컬 0.45 박스가 Transform 0.2로 0.09가 되던 설정을 로컬 2.25로 보정 | AC-03, AC-05, AC-06 |
| T-11 | 캐릭터 SpriteRenderer 참조를 복구하고, Player 씬 시작 위치를 지면에 맞춘다. | 수정 완료: 스프라이트 재할당 및 시작 y=-2.26 | AC-01, AC-03 |
| T-12 | Space 점프와 더블 점프 아이템 획득·공중 사용을 Unity 플레이 모드에서 재검증한다. | 완료: 일반 점프 상승, 자연 생성 아이템 획득 후 x1 반영, 공중 추가 점프 후 x0 감소와 효과 생성을 런타임에서 확인했다. 추가 점프를 확인한 별도 결정적 런에서는 생성 아이템 복제본을 임시 이동했고 Play 종료 후 변경은 폐기됐다. 자연 생성 도달성은 T-19에서 별도 확인했다. | AC-03 |
| T-13 | Unity 에디터의 `Worker0 Import Error Code:(4)` 상태 메시지 원인을 확인하고 AssetDatabase를 복구한다. | 완료: 원인은 MainScene SourceAssetDB와 디스크 mtime 불일치였다. Pipeline 연결 후 AssetDatabase ForceSynchronousImport Refresh 및 MainScene ForceUpdate 재임포트를 실행했고, Unity 상태 `ready`, 씬 정상 로드를 확인했다. 로그의 최근 120줄에 새 Worker0/Import error는 없고 과거 mtime 오류만 남아 있다. | 프로젝트 검증 |
| T-14 | 현재 플레이어와 배경 이미지를 기존 분위기에 맞춰 다시 그리고 프로젝트에 적용한다. | 완료: 파일 교체, 씬 GUID 연결, Unity Play Game 뷰 표시 확인 | 비주얼 에셋 |
| T-15 | 더블 점프 아이템 이미지를 제작하고 아이템 프리팹에 연결한다. | 완료: 투명 얼음 결정 아이콘을 단일 Sprite/PPU 1254로 가져오고 DoubleJumpItem SpriteRenderer에 연결. PC 임포트 형식을 RGBA 32 bit로 지정해 압축 경고 제거 | 비주얼 에셋 |
| T-16 | 설면 전경 이미지를 제작하고 반복 Ground 오브젝트에 적용한다. | 완료: 1254×1254 정사각형 설면 Sprite를 501.6 PPU로 임포트했다. 각 20×1 물리 Ground에 자식 `GroundVisual`을 만들고 SpriteRenderer Tiled size 20×2.5, y=-0.75로 배치해 발판 상단 -2.5를 맞추고 화면 하단까지 덮는다. Ground 세그먼트마다 8회 반복하며 Collider 월드 bounds는 20×1로 유지했다. 이번 저장 씬 재확인에서 이미지 `.meta` GUID와 일치하는 Sprite 참조 3개가 확인됐다. Game 뷰 캡처에서 바닥 공백이나 눈에 띄는 세로 이음선이 보이지 않는다. | 비주얼 에셋 |
| T-17 | 장르 레퍼런스를 반영해 타이틀·플레이 HUD·게임 오버 UI를 설계하고 구현한다. | Unity Play acceptance 통과: 타이틀·HUD·게임 오버 카드 표시, START RIDING 전환, RIDE AGAIN 재시작과 Best 보존 확인 | AC-01, AC-06, AC-07, AC-09 |
| T-18 | UI와 충돌 흐름에서 빠질 수 있는 참조를 안전하게 처리하고 게임 오버 이후 플레이 입력을 막는다. | 완료: Game Over에서 Space/Tab 입력 후 Player 위치가 x=19.34653, y=-2.26으로 유지되고 점수도 TIME 00:03에서 멈춤을 확인했다. | AC-03, AC-04, AC-06 |
| T-19 | 자연 생성 더블 점프 아이템의 높이·거리와 플레이어 점프 궤적이 실제 획득 가능한지 확인하고 필요하면 스폰 밸런스를 조정한다. | 완료: 장애물 생성기를 통제 목적으로 끈 Play에서 기본 스폰 좌표·높이의 자연 생성 아이템을 획득했다. HUD x0→x1과 one-shot audio 재생을 확인했으며 스폰 좌표는 수정하지 않았다. | AC-03 |
| T-20 | 타이틀 대기 화면에 입력과 분리된 반복 플레이 미리보기를 만들고 시작 UI와 함께 배치한다. | 완료: 최신 요청에 따라 T-28–T-31에서 전체 화면의 실제 활강 장면으로 대체 | AC-10 |
| T-21 | Small·Tall·Wide 전용 겨울 장애물 스프라이트를 만들고 프리팹 참조, 원점, 충돌 크기를 각각 맞춘다. | 완료: 세 전용 스프라이트·Inspector 크기·충돌 영역 및 표시 확인 | AC-11 |
| T-22 | 16개 장애물 패턴을 데이터 흐름에 추가하고 플레이 시간에 따라 패턴 풀을 점진 확장한다. | 완료: 샘플 생성 확인, 패턴 표와 시간별 확장 로직 검토 | AC-12 |
| T-23 | 기존 인게임 기록·아이템·대시 패널과 맞는 이동 속도 배수 HUD를 추가한다. | 완료: MainScene 런타임에서 속도 카드와 기존 HUD 표시 확인 | AC-09, AC-13 |
| T-24 | 로컬 아케이드 기록 저장소를 만들고 생존 시간 기준 상위 5개만 유지한다. | 완료: Unity 제출 경로에서 정렬 저장 및 다섯 자리 초과 기록 자격 제외 확인 | AC-14 |
| T-25 | 게임 오버 시 자격 기록의 닉네임 입력·제출과 등록 건너뛰기를 제공한다. | 완료: 실제 Game Over 입력창·제출 결과를 확인하고 테스트 기록을 복원 | AC-15 |
| T-26 | 타이틀과 게임 오버에 같은 상위 5개 기록을 표시한다. | 완료: 두 화면의 보드와 제출 후 강조 행 확인 | AC-16 |
| T-27 | 타이틀 attract preview에서 장애물 회피를 제외하고 직진·배경 스크롤만 표현한다. | 완료: 최신 전체 화면 어트랙트 모드에서도 장애물 없는 직진을 유지 | AC-10 |
| T-28 | 이전 타이틀의 저장 UI와 작은 미리보기 생성 코드를 삭제한다. | 완료: TitleScene의 Canvas·EventSystem을 Editor API로 삭제·저장하고 이전 미리보기 코드를 제거 | AC-10, AC-16 |
| T-29 | 전체 화면의 실제 활강 장면 위에 제목·시작 버튼·상위 5개 기록을 배치한다. | 완료: 실제 Game 뷰에서 전체 화면 활강·시작 메뉴·상위 5개 기록 표시 확인 | AC-10 |
| T-30 | 어트랙트 대기 중 기록·속도 상승·장애물·아이템과 조작을 대기시키고 시작 뒤 활성화한다. | 완료: 대기 중 속도 5/점수 0/생성 0, 전환 뒤 속도·점수 증가와 아이템 생성, 플레이 2초 뒤 첫 장애물 패턴 확인. 입력 차단은 분기 검토 | AC-10, AC-17 |
| T-31 | 시작 시 메뉴/HUD를 0.75초 동안 전환하고 같은 씬·플레이어·카메라를 유지한다. | 완료: 페이드 중·후 같은 인스턴스 ID, 연속 증가하는 x와 일정한 y=-2.26, 메뉴/HUD alpha 전환 확인 | AC-17 |
| T-32 | 초기 중첩 UI 배치 뒤 시작 버튼 글자의 렌더링을 보정한다. | 완료: 배치 확정 후 TMP 메시를 갱신하며 새 Play 세션의 Game 뷰에서 START RIDING 글자 표시 확인 | AC-10 |
| T-33 | 시작 직전 긴 프레임이 발생해도 페이드가 즉시 끝나지 않게 한다. | 수정 후 PASS: 시작 시점의 실제 경과 시간을 사용해 0.401초에 메뉴 alpha 0.454·HUD alpha 0.189·점수 0, 0.851초에는 HUD alpha 1과 기록 시작을 확인 | AC-17 |
| T-34 | 반복 지면의 측면 접촉 때문에 직진이 멈추는 문제를 수정한다. | 수정 후 PASS: 3개 20×1 지면을 단일 CompositeCollider2D로 합치고 순환 시 갱신한다. path 1/point 4/전체 60×1, 높이 -2.5 유지. 기존 걸림 지점 x=189.758을 지나 x=297.542까지 연속 이동 확인 | AC-10, AC-17 |
| T-35 | 초반 장애물 밀도·패턴 묶음 겹침·속도 상승을 조정한다. | 과거 설정으로 Unity 런타임 PASS; 속도 상한 제거로 대체됨. 생성 시작·하한과 18/42초 패턴 개방 및 11.5단위 간격을 유지한다. 과거 45초 회피 기록은 초속 8 상한이 있는 설정의 결과다. | AC-02, AC-12, AC-18 |
| T-36 | 플레이어와 Tall 장애물의 충돌 영역을 보이는 중심 형태에 맞춘다. | 완료: Player BoxCollider를 월드 0.40×0.45 Vertical Capsule로 교체해 폭을 11% 줄였다. Tall BoxCollider를 1.05×2.0, offsetY=0.5로 줄여 보이는 스프라이트 상단과 맞췄다. 점프 접지 및 상승 확인 | AC-05, AC-19 |
| T-37 | 파란 겨울 배경에 묻히는 장애물 실루엣을 보강한다. | 완료: Small/Tall/Wide 각 프리팹에 기존 스프라이트와 같은 형상의 주황색 확대 외곽을 추가했다. 1280×720 Play 캡처에서 3종의 따뜻한 외곽이 산·설경 위에 보임을 확인 | AC-19 |
| T-38 | 더블 점프 아이템의 크기·대비·획득 가능 높이와 획득 연출을 보강한다. | 런타임 완료: 이전 1.2배에서 1.4배로 키워 약 17% 확대했고, 회전·부유와 1.1–1.55단위 획득 높이를 유지한다. 런타임의 예전 프리팹 참조와 주황색 획득 색조를 제거하고 중심 아이콘을 살린 이중 링, 코발트 림, 중심 반짝임, 얼음 파편 12개와 눈 결정 24개로 교체했다. Unity Play에서 총 43개 렌더 구성을 확인했다. 예전 `ItemPickupEffect.prefab` 파일은 참조 없는 에셋으로 남아 있으며, Editor AssetDatabase를 통한 정리는 Unity 재연결 후 처리한다. | AC-03, AC-08, AC-20 |
| T-39 | 장애물 충돌체와 그림이 설면에 뜨거나 묻히지 않게 정렬한다. | 완료: 실제 CompositeCollider2D 상단과 Small/Tall/Wide Collider2D 바닥이 모두 y=-2.5로 일치한다. Small/Wide PNG의 투명 하단(약 19–20%)은 각 0.17단위 시각 오프셋으로 보정하고 Tall은 기존 정렬을 유지한다. | AC-05, AC-21 |
| T-40 | 플레이 시간이 길수록 속도가 계속 올라가고 대시가 가속 이점을 유지하게 한다. | 완료: 초속 5에서 0.055/초로 계속 증가하고 상한은 없다. MainScene 설정을 업데이트했으며 패턴 묶음 최소 간격은 11단위로 조정했다. 계산상 60/120/180초 속도는 8.3/11.6/14.9다. 대시는 현재 속도보다 초속 20단위 높은 값을 유지한다. | AC-02, AC-18 |
| T-41 | 더블 점프 아이템이 배경과 겹쳐 놓치는 문제를 줄인다. | 완료: 아이콘을 기존 1.2배 설정에서 1.4배로 키우고, 짙은 코발트 실루엣, 금빛·청록 링, 세로 빔과 맥동 후광을 런타임에 생성한다. Unity에서 외부 코드 변경을 명시적으로 재임포트한 뒤 컴파일 실패 없이 1920×1080 Game 뷰 캡처를 확인했다. 테스트용 아이템·플레이어 설정은 Play 종료로 폐기했다. | AC-20 |
| T-42 | 게임 중 Escape 일시정지 메뉴와 저장되는 기본 설정을 추가한다. | Windows 빌드 PASS: Resume, Settings, Return to Title, Quit Game; 전체 음량 슬라이더와 전체 화면 전환을 제공한다. 정지는 Time.timeScale 0을 사용하며, Escape는 설정에서 일시정지 화면으로 돌아가고 일시정지 화면에서 재개한다. 사용자의 실행 금지 요청에 따라 입력·버튼 런타임 상호작용은 확인하지 않았다. | AC-22 |
| T-44 | 타이틀 화면에 주요 키보드 조작 안내를 표시한다. | Windows 빌드 PASS: 하단 HOW TO RIDE 카드에 점프, 아이템 더블 점프, 대시, Escape 일시정지를 배치했다. 사용자의 이전 요청에 따라 Windows 실행 파일은 실행하지 않아 Game 뷰 확인은 대기한다. | AC-23 |

## 검증 결과와 후속 작업


- 2026-10-11 Unity GUI 에디터 검증: Unity Hub에서 프로젝트를 Unity 6000.3.12f1로 열었다. TitleScene의 START가 MainScene으로 전환되는 것을 확인했다. MainScene 플레이 화면에서 캐릭터와 HUD, 시간 점수 증가, 장애물 충돌 후 Game Over UI, R 재시작 및 저장된 Best 기록 유지를 확인했다. Tab을 누른 뒤 캐릭터 대시 자세와 쿨타임 UI 변화를 확인했다.
- 런타임 검증 중 MainScene Player의 SpriteRenderer가 `Missing (Sprite)`임을 발견했다. `player_redrawn` 임포트 에셋은 정상 Sprite로 존재했으므로 Unity 에디터에서 해당 Sprite subasset을 Player에 다시 할당하고 씬을 저장했다. 이후 Play 화면에 캐릭터가 표시됨을 확인했다.
- 1254×1254 플레이어 이미지가 기본 압축 설정의 4픽셀 배수 조건을 만족하지 않아 Inspector에 압축 경고가 있었다. Sprite Importer Compression을 None으로 변경해 경고를 제거했다. 적용 후에도 Unity 상태 줄에는 `Worker0 Import Error Code:(4)` 문구가 남아 있어 T-13으로 원인 확인을 남겼다.
- MainScene 플레이 초기에 Player Transform y가 0이고 지면은 y=-3(윗면 -2.5)이어서 첫 프레임에 공중 낙하 상태였다. 첫 Space 입력이 지면 접촉 전 무시될 수 있어 초기 위치를 y=-2.26으로 조정했다. 그 시점에는 Space 점프를 별도 캡처로 확인하지 못해 AC-03을 미검증으로 기록했다. 이후 T-12와 T-19에서 기본 점프, 자연 생성 아이템 획득, 추가 점프를 Unity 런타임으로 확인했다.
- Unity의 GUI 에디터는 정상 실행되었지만 batch/headless 실행은 `LicenseClient-USER` IPC 거부와 headless 컴포넌트 미발견으로 계속 차단됐다. 당시 T-07을 부분 완료로 기록했으나, 이후 Unity GUI에서 AC-01–AC-09를 재검증해 완료 상태로 갱신했다.
- 초기 런타임 점검에서 미확인으로 남긴 항목의 이력이다. 이후 점프·아이템 경로는 T-12/T-19, 속도·생성 빈도 가속은 AC-02, 충돌 흔들림은 AC-06, 오디오·파티클은 AC-08에서 GUI 런타임 PASS 근거를 추가했다.

- 초기 점검에서 AC-06 카메라 흔들림이 실패해 T-03을 추가했고 수정했다. CameraShake는 unscaled 시간을 사용하고 흔들림이 끝나면 기준 위치를 복구한다.
- 초기 점검에서 최고 기록의 즉시 저장이 없어 T-04를 추가했고 PlayerPrefs.Save 호출과 중복 게임 오버 방지를 적용했다.
- T-05에서 대시 UI가 누락 참조와 0초 쿨타임을 안전하게 처리하도록 보완했다.
- MSBuild로 Assembly-CSharp 컴파일이 통과했고, 정적 acceptance audit에서 AC-01부터 AC-09까지 통과했다.
- 기존 플레이어 시트는 여러 포즈와 배경색이 한 이미지에 섞여 있어, 헬멧·청록 바이저·파란 재킷·어두운 바지·청록 보드를 유지한 단일 투명 스프라이트 `Assets/Sprites/player_redrawn.png`를 만들었다. 모서리 알파가 0임을 확인하고 MainScene Player의 SpriteRenderer에 연결했다.
- 기존 배경은 레이어 이름과 안내 문구가 들어간 시트였다. 이를 참고해 푸른 하늘·눈 덮인 산맥·전나무가 있는 배경 `Assets/Sprites/background_redrawn.png`를 만들고, 원래 35 유닛 간격과 맞도록 PPU를 372로 설정해 활성 Far 배경 3개에 연결했다. 씬의 어두운 색 틴트도 제거해 생성된 색을 표시한다.
- ImageGen 프롬프트 요약: 기존 겨울색과 붓 터치를 참조해 흰 헬멧·청록 바이저·파란 재킷·어두운 바지·청록 보드의 단일 투명 라이더를 만들고 72% 크기로 중앙 배치했다. 배경은 문구 없는 푸른 하늘·눈 산맥·전나무의 가로 파노라마로 만들고 좌우 경계를 반복에 맞게 다듬었다.
- 2026-10-11 ImageGen 재작업: 현재 런타임 캐릭터와 파노라마를 각각 참조로 사용해 같은 겨울색·장비·캐릭터 자세의 새 그림을 만들고 `Assets/Sprites/player_redrawn.png` 및 `Assets/Sprites/background_redrawn.png`에 적용했다. 플레이어 PNG는 1254×1254이며 모서리 알파 0, 배경 PNG는 2172×724 불투명 이미지다. 두 `.meta`를 유지해 GUID와 기존 MainScene 스프라이트 연결을 보존했다. `unity` CLI가 설치되어 있지 않고 Editor 로그 접근이 거부되어 CLI와 로그로는 확인하지 못했지만, 아래 GUI Game 뷰 확인으로 에셋 표시를 검증했다.
- 재작업 후 Unity Play Game 뷰에서 새 산악 파노라마와 새 플레이어가 모두 렌더되는 것을 확인해 T-14를 완료했다. 플레이 모드 중 R 재도전은 Time 0 및 Best 109 표시로 확인했다. 이어서 Space를 눌렀으나 다음 캡처가 Time 3/Game Over여서 점프 동작 여부와 아이템 획득·공중 사용은 판별할 수 없었다. 그 시도 시점에는 AC-03/T-12가 미검증이었고 Worker0 상태도 남아 T-13이 미해결이었다. 이후 T-12/T-19 런타임 검증 및 T-13 AssetDatabase 복구 완료 기록을 아래에 추가했다.
- 2026-10-11 ImageGen 아이템 에셋: 플레이어 아트의 청색 손그림풍을 참조해 두 개의 위쪽 셰브론이 있는 투명 얼음 결정 아이콘 `Assets/Sprites/double_jump_item.png`를 만들었다. Unity에서 Single Sprite, PPU 1254로 가져오고 `DoubleJumpItem` 프리팹의 SpriteRenderer에 연결했다. 프리팹 자동 저장과 Scene 뷰 표시를 확인했다. 1254×1254 원본의 DXT5 배수 경고를 없애기 위해 Windows/Mac/Linux 플랫폼 형식을 RGBA 32 bit로 설정했다. 당시 남아 있던 AC-03의 실제 획득·추가 점프 검증은 이후 T-12와 T-19에서 완료했다.
- 2026-10-11 AC-03 재시도: ObstacleSpawner를 Play 동안만 비활성화한 상태에서 GameView에 포커스를 둔 Space 입력 후 Player y가 -2.26에서 -1.50으로 변해 점프를 확인했다. 이어서 생성 아이템 복제본 하나를 런타임에서 플레이어 앞 4 유닛, y=0.15로 임시 이동하고 점프해 트리거 획득(HUD x1, ItemPickupEffect(Clone), One shot audio)을 확인했다. 같은 공중 구간에서 Space를 다시 눌러 HUD x0 및 DoubleJumpEffect(Clone)을 확인했다. 이 임시 이동은 Play 종료 시 사라졌으며 자연 생성 좌표에서의 획득성은 검증하지 않았다. ObstacleSpawner를 재활성화하고 MainScene을 저장했다. 해당 시점에는 기본·추가 점프 경로를 확인했고 자연 아이템 획득성은 T-19에 남겼다. 이후 T-19에서 기본 스폰 위치 아이템 획득도 확인했다.
- 재점검에서 Player Transform 스케일이 0.2인데 BoxCollider2D 크기도 로컬 0.45라 실제 충돌 박스가 0.09 유닛임을 확인했다. 장애물과 지면에 대한 충돌 범위가 캐릭터 그림보다 과도하게 작아 T-10을 추가했고, 로컬 크기를 2.25로 올려 월드 크기를 0.45 유닛으로 맞췄다.
- 2026-10-11 설면 전경 시안: 기존 `background_redrawn.png`의 청색 손그림풍을 참조해 설면 상단, 층진 청록 눈, 짙은 청색 얼음 밑면이 이어지는 타일용 이미지를 만들고 `Assets/Sprites/ground_redrawn.png`에 추가했다. Unity 프로젝트의 활성 에디터 상태를 확인한 뒤 Ground 렌더러 연결, 반복 경계 및 Game 뷰 표시를 검증한다.
- 2026-10-11 T-16 Editor 적용 시도: Pipeline 연결 후 설면 이미지의 Single Sprite/Repeat 임포트를 확인했다. `set_import_settings`는 `spriteMeshType`을 지원하지 않았고 `TextureImporter.spriteMeshType` 직접 설정도 Unity 6000.3.12f1 API에서 컴파일되지 않아 그 시도는 씬 변경 전에 중단됐다. 기존 임포트 메시를 유지하고 SpriteRenderer의 Tiled 모드로 적용하는 경로로 전환한다.
- 2026-10-11 T-16 첫 Game 뷰 acceptance 실패: 새 설면은 보였지만 그래픽 하단과 화면 하단 사이에 단색 공백이 남았다. 사후 geometry audit에서 Ground Transform X 스케일이 3으로 커져 SpriteRenderer와 BoxCollider2D의 월드 폭이 60인 점도 발견했다. 스펙을 갱신해 충돌 상단 높이와 기존 폭 20·높이 1을 보존하고, 시각용 빙층을 별도 자식으로 내려 화면 하단까지 잇도록 재작업한다.
- 2026-10-11 T-16 두 번째 검증: 6:5 설면 아트를 `GroundVisual` 자식 SpriteRenderer로 분리해 화면 하단 공백을 없앴다. Ground 루트 스케일은 1, collider serialized size는 20×1, `Physics2D.SyncTransforms()` 후 월드 bounds도 20×1이며 표면 높이 -2.5는 유지됐다. 좌우 이미지 끝 평균 RGB 차이(15.00)가 끝 인접 픽셀 평균(1.94)보다 커 타일 경계는 아직 합격시키지 않고 아트 에지를 재보정한다.
- 2026-10-11 T-16 반복 폭 점검: 기존 3 유닛 아트 타일은 20 유닛 Ground 구간을 정수 반복 횟수로 채우지 않아 구간 경계의 패턴 위상이 어긋날 수 있다. 다음 타일을 2.5×2.5 유닛으로 맞춰 세그먼트당 8회 반복되게 한다.
- 2026-10-11 T-16 재작업 완료: 1254×1254 정사각 타일을 501.6 PPU로 설정해 Sprite bounds 2.5×2.5 유닛으로 맞췄다. 세 Ground 각각은 8회 반복되는 20×2.5 시각 자식을 갖고, 자식 위치 y=-0.75로 최상단을 -2.5에 맞췄다. `Physics2D.SyncTransforms()` 이후 세 루트의 collider world bounds 20×1, collider top -2.5, GroundLooper 폭 20을 검증했다. 1920×1080 Game 뷰에서 화면 하단까지 설면이 이어지고 반복 줄이 보이지 않아 시각 acceptance 통과. 원본 좌우 픽셀 열은 평균 RGB 차이 7.93이지만, 실제 게임 뷰에서는 눈에 띄는 세로 이음선이 없다.
- 2026-10-11 T-13 복구 완료: Unity CLI가 ready 상태인 동안 `AssetDatabase.Refresh(ForceSynchronousImport)` 및 `ImportAsset("Assets/Scenes/MainScene.unity", ForceUpdate | ForceSynchronousImport)`를 실행했다. 씬 에셋 재임포트가 완료됐고, Editor.log 최근 120줄에서 새 Worker0/Import error는 발견되지 않았다. 기존 mtime mismatch 기록은 과거 로그에만 남았다.
- 2026-10-11 T-07 런타임 검증 재개 지연: Pipeline 서버 포트는 reachable이지만 `list_open_scenes`, `get_component_properties`, `eval return 2`가 Main Thread operation timeout(5–30초)으로 끝났다. Unity 프로세스는 살아 있고 Editor.log에는 해당 timeout만 새로 기록돼 추가 씬 조작을 중지하고 에디터 표시/응답 상태 확인을 요청했다.
- 2026-10-11 그라운드 연결 재확인: `ground_redrawn.png.meta` GUID `c47ce3b99e1d2094899dc4dc6bd79177`가 저장된 MainScene의 `Ground_1`, `Ground_2`, `Ground_3` 각각의 자식 `GroundVisual` SpriteRenderer에 연결되어 있다. 세 렌더러 모두 Tiled 20×2.5 및 `m_WasSpriteAssigned: 1`이고, Importer는 Sprite/Single, 501.6 PPU, mipmaps off다. 기존 [Game 뷰 캡처](../Temp/pipeline-screenshots/ground-applied-v3.png)에서 설면은 캐릭터 발판 높이부터 화면 하단까지 이어진다. 이번 재검증의 Pipeline status는 `ready`/reachable이었으나 `list_open_scenes`가 Main Thread operation timeout(15초)으로 실패해 새 라이브 Inspector 확인은 수행하지 못했다. 씬 파일의 저장 내용은 확인했으며, 런타임 미검증 항목을 통과 처리하지 않는다.
- 2026-10-11 타이틀·HUD 개선: `Alto’s Adventure`의 절제된 산악 표현과 `Ski Safari`의 끝없는 하강·파워업·기록 경쟁을 참고해 기존 배경/캐릭터 중심 타이틀, START RIDING, 최고 기록, Space/Tab 안내, 기록 카드·더블 점프 카드·대시 충전 HUD·결과/재도전 카드를 추가했다. 두 Canvas를 1920×1080 기준 `Scale With Screen Size`로 조정한다. 기존 TitleScene의 START 버튼 연결은 유지하고 Enter/Space 시작도 추가했다. 장르 특성에서 참고한 화면 원칙과 배치는 [GameSpecification](GameSpecification.md)의 화면 구성에 반영했다. 출처: [Alto 공식 프레스 키트](https://altosadventure.com/press/spiritofthemountain/), [Ski Safari 공식 페이지](https://skisafari.sleepyzstudios.com/), [Ski Safari 2 App Store](https://apps.apple.com/us/app/ski-safari-2/id952555810).
- 2026-10-11 UI/안정성 코드 점검: 시간 표시를 초 단위 `mm:ss`로 바꾸고 초가 변경될 때만 HUD 텍스트를 갱신한다. 게임 오버 시 R 입력을 제외한 Player 입력을 중단하고, Rigidbody2D·GroundCheck 대체 판정·AudioSource/AudioClip·CameraShake·GameManager·UI 참조 누락으로 인한 NullReference 가능성을 방어했다. 대시 상태 텍스트는 준비/재충전 시간으로 표시하고, 타이틀 시작/게임 재시작이 같은 입력으로 중복 호출되지 않게 막았다. 새 `SnowUIFactory.cs`와 기존 Assembly-CSharp 스크립트를 Unity 6000.3.12f1 번들 Roslyn으로 컴파일해 exit 0을 확인했다. 에디터 Pipeline의 메인 스레드가 씬 조회에서 10초 시간 초과를 반환하므로 실제 Play/Game 뷰와 클릭 acceptance는 아직 검증하지 않았다.
- 2026-10-11 UI acceptance 재시도 실패: Unity 상태는 `ready`, 프로세스는 OS 응답 중이고 최근 Editor.log는 반복된 `/api/exec` Main thread operation timeout(최대 30초)을 기록한다. 이번 `list_open_scenes` 재시도도 10초 timeout으로 실패했다. Unity Editor 안에서 새 Resources 복사본과 동적 UI의 import 및 Game 뷰 렌더링을 확인할 수 없어 AC-01/06/07/09의 새 UI 적용 PASS는 보류한다. 스크립트 소스는 Unity 번들 Roslyn 컴파일러로 다시 exit 0을 확인했다. 저장되지 않은 Editor 작업을 보존하려면 Unity 사용자가 저장 후 Editor를 재시작해 Pipeline main thread를 복구해야 한다.
- 2026-10-11 Editor 오류 조사: Unity Editor 로그에서 `Assets/Scenes/MainScene.unity`의 SourceAssetDB 시간(16:57:56Z)과 디스크 파일 시간(17:02:48Z)이 불일치해 `Worker0 Import Error Code:(4)`가 반복된다는 직접 원인을 확인했다. Unity CLI 및 `com.unity.pipeline` 패키지는 사용자 승인으로 설치했으나, Editor는 패키지 로드/AssetDatabase 갱신 중이며 Pipeline 서버가 아직 접근 불가다. Editor를 활성화해 Refresh/Reimport한 뒤 T-13 및 런타임 acceptance를 재검증한다.
- 친구 기록 비교는 스펙에 제공 방식이 정의되지 않아 결정 대기이며 이번 완료 기준에 포함하지 않는다.
- 2026-10-11 Unity UI 런타임 acceptance 재개: 사용자가 MainScene의 외부 변경 재로드를 승인했다. 첫 Play 화면은 이전 HUD였고 Project 창에도 `SnowUIFactory`가 없었다. Unity 메뉴 `Assets > Refresh` 후 스크립트가 Project 창에 import되고 컴파일 진행이 끝났다. 새 Play에서 타이틀의 JUMP/DASH 아트·최고 기록·START RIDING·조작 안내, MainScene HUD의 Time/Best·AIR JUMPS·DASH READY·쿨타임 바, 충돌 뒤 RUN OVER 결과 카드가 표시됨을 확인했다. START RIDING 클릭은 MainScene으로 이동했고, R 입력과 RIDE AGAIN 클릭은 00:00으로 재시작하며 BEST 01:49를 유지했다. 이 acceptance 시도 캡처만으로는 Space 점프 상승 여부를 판별하지 못해 당시 AC-03/T-12를 미해결로 기록했다. 오디오·파티클·카메라 흔들림도 이 시점에는 미검증이었으며, 이후 아래 런타임 기록에서 각각 확인했다.

- 2026-10-11 추가 런타임 확인: 장애물 생성기를 통제 목적으로 끈 Play에서 자동 전진과 속도 상승을 관찰했다. Inspector 속도는 시작 5.0, 게임 시간 00:22에서 7.734583, 01:00에서 12.21351이었다. 자연 생성 더블 점프 아이템을 획득해 HUD가 x1이 되고 ItemPickupEffect(Clone) 및 One shot audio 런타임 오브젝트가 나타났다. 이전 통제 런의 공중 Space 입력에서 x0 및 DoubleJumpEffect(Clone)을 확인했으므로 T-12/T-19를 완료했다. Tab은 재충전 중 `RECHARGING 0.3s`를 보였고, 회복 뒤 두 번째 입력으로 다시 `RECHARGING 1.2s`에 진입해 AC-04를 런타임 PASS 처리했다. 테스트 종료 후 ObstacleSpawner GameObject를 다시 활성화하고 MainScene을 저장했으며 Hierarchy/Inspector에서 활성 상태를 확인했다.
- 2026-10-11 AC-02 생성 주기 런타임 확인: 충돌을 피하기 위해 Play 시작 전 Player를 화면 위쪽에 두고 Rigidbody2D 중력만 0으로 설정한 통제 런을 사용했다. 실제 생성된 장애물 복제본을 확인하며 활성 ObstacleSpawner의 Inspector 값을 읽었다. 게임 시간 00:11에 Min/Max Spawn Delay는 1.107545/2.107545였고 00:15에는 0.9423639/1.942364로 감소했다. 이 통제 값은 Play 전 설정이며 씬에 저장하지 않고 종료 시 원래 Player 설정으로 복원한다.
- 2026-10-11 AC-05/06 중간 점검: 장애물 생성기를 켠 MainScene에서 Wide 장애물 복제본과 연속 생성 복제본을 확인했고, Time 00:03에 충돌 후 점수가 고정되며 RUN OVER 결과 카드가 표시됐다. Main Camera에는 CameraShake 컴포넌트가 활성화되어 있고 기본값은 Shake Duration 0.2, Magnitude 0.15였다. 짧은 기본 효과 구간은 이 시점 캡처로 분리 관찰하지 못했으며, 뒤이어 수행한 진단 흔들림 런에서 이를 확인했다(아래 acceptance 마무리 참고).
- 2026-10-11 acceptance 마무리: 통제 런의 Hierarchy와 Game 뷰에서 Small/Tall/Wide 장애물 복제본 및 다중 배치를 확인하고 일반 런의 충돌 Game Over를 확인해 AC-05를 통과 처리했다. CameraShake 효과는 충돌 직후 진단 설정(10초/0.5)으로 관찰했으며 연속 캡처 사이 Main Camera Transform x가 24.51485에서 24.34869로 변했다. GameOver에서 Space/Tab 입력 후 Player 위치가 x=19.34653, y=-2.26 그대로이고 TIME 00:03에 고정되어 T-18을 완료했다. `jump arcade.wav`, `whoosh.wav`, `impact hit.wav`가 각각 jump/dash/hit 필드에 연결되고 Player AudioSource와 Main Camera AudioListener가 활성화됨을 확인했다. 기본 카메라 흔들림 설정 0.2초/0.15, Player 시작 위치 y=-2.26 및 Rigidbody2D Gravity Scale 3.5를 복원하고 MainScene을 저장했다. 아이템 획득/더블 점프 효과 복제본과 item One shot audio 런타임 생성은 이전 런에서 확인했으므로 AC-06/08을 런타임 PASS로 마무리한다.

- 2026-10-11 attract·로컬 순위표 acceptance: 기존 TitleScene의 저장 UI 때문에 새 타이틀 생성 로직이 건너뛰어지는 문제를 발견했다. `TitleManager`가 상위 5명 카드가 없는 구 UI(개인 최고기록 카드/미리보기 장애물 포함)를 감지해 교체하도록 수정했다. Unity Play에서 LOCAL ARCADE TOP 5, 장애물 없는 라이더 직진 미리보기, FREE RIDE 시계, 움직이는 설면을 확인하고 실제 START 버튼 이벤트로 MainScene에 진입했다. 실제 충돌 Game Over에서 닉네임 입력창·ARCADE TOP 5 보드를 확인하고 `TESTER` 제출이 2위로 저장/강조되는 것을 확인했다. 다섯 자리를 모두 채운 테스트 데이터로 낮은 기록의 순위 자격은 -1, 더 높은 기록은 1로 계산됐다. 별도 `RELOAD` 테스트 기록이 TitleScene을 다시 불러온 후에도 1위로 남는 것을 확인했다. 모든 검증용 제출을 마친 뒤 PlayerPrefs를 기존 `PLAYER` 109초 기록과 BestScore 109로 복원했다.

- 2026-10-11 추가 acceptance: TitleScene 대기 중 라이더·장애물·타이머가 반복 재생되고 설면 리소스가 표시됨을 확인한 뒤 START RIDING으로 MainScene에 진입했다. MainScene에서 TRAIL SPEED 1.1x, 시간·Best·AIR JUMPS·DASH HUD와 장애물 충돌 후 RUN OVER를 확인했다. Small/Tall/Wide 프리팹의 개별 SpriteRenderer 참조, 크기, Collider2D를 Inspector에서 확인했다. 새 스폰 테이블의 샘플 패턴 출현은 런타임에서 확인했으며, 16개 개별 패턴 전부의 발생은 수동 세션에서 하나씩 검증하지 않고 테이블 및 12초·32초 확장 로직을 코드 검토했다. Resources 폴더에 Ground 이미지를 복제한 뒤 Unity Assets > Refresh를 실행해 타이틀 preview의 설면 렌더링을 확인했다.

- 2026-10-11 전체 화면 타이틀 요청 반영: TitleScene의 이전 Canvas·EventSystem을 삭제하고 bootstrap 카메라·TitleManager만 남겼다. 실제 MainScene을 한 번 로드한 뒤 전체 화면 활강 위에 제목·START RIDING·상위 5개 기록·조작 안내를 얹는다. START는 씬 재로드 없이 0.75초 메뉴/HUD 페이드로 실제 플레이를 시작한다.
- 첫 화면 검증에서 중첩 버튼의 글자가 보이지 않아 레이아웃 확정 후 TMP 메시를 갱신했다. 새 Play 세션의 Game 뷰에서 글자 표시를 재확인해 T-32를 완료했다.
- 첫 전환 검증에서 시작 직전 긴 프레임의 deltaTime이 페이드에 반영돼 조기 종료되는 문제가 있었다. 시작 시점의 실제 경과 시간을 사용하도록 T-33을 수정했다. 최종 연속 측정에서 0.400초에는 메뉴/HUD alpha가 0.455/0.187이고 점수는 0, 0.850초에는 메뉴가 제거되고 HUD alpha 1 및 점수 0.096이었다.
- 장시간 대기 검증에서 x=189.758의 지면 연결부에 캐릭터가 멈췄다. 접촉 법선 (-1, 0)으로 다음 Ground의 측면에 걸리는 원인을 확인해 T-34를 추가했다. 기존 20×1 Ground 3개를 동일 레이어의 단일 CompositeCollider2D로 합쳐 연결부를 없애고, GroundLooper가 구간을 옮길 때 물리 면을 갱신한다. 전체 충돌 영역은 60×1, path 1/point 4이며 설면 높이는 -2.5다. 재검증에서 대기 중 x=132.973→275.469, 시작 후 x=297.542까지 이동했고 y=-2.26을 유지했다. 추가 접지/점프 검사에서 grounded=True, 점프 속도 14, y=-2.26→0.292 상승을 확인했다.
- 최종 시작 전환 검증: 씬 handle -58308, Player ID 66258, Camera ID 66236이 시작 전·중·후 동일했다. 대기 중 점수 0·속도 5·장애물/아이템 0이며 실제 플레이 시작 뒤 속도와 점수가 증가한다. 플레이 1.897초까지 장애물 0개, 2.198초에 첫 패턴 2개가 생성돼 대기 시간이 난이도나 첫 생성 시점에 포함되지 않았다. [연속 측정 기록](Previews/title-transition.csv)과 [전환 중 화면](Previews/title-transition-mid.png)을 보관한다.
- Unity 6000.3.12f1 최종 컴파일은 오류 없이 완료됐고 현재 Console 오류·경고는 0이다. 로컬 순위표에 새 검증 기록을 제출하지 않았으며 기존 PLAYER 109초 기록을 유지한다. 검증 후 Unity는 [완성된 전체 화면 타이틀](Previews/title-fullscreen.png)의 어트랙트 모드로 복귀시킨다.
- 2026-10-11 장애물 난이도·가시성 조정: 플레이 속도 증가율을 0.045로 낮추고 최대 초속 8을 적용했다. 장애물 간격은 2.4–3.2초에서 0.025초/초씩 감소하며 1.35/1.9초 하한에서 멈춘다. 쉬운 6패턴은 18초, 11패턴은 42초까지 유지한다. 같은 패턴의 개별 장애물 간격은 약 11–12 단위로 늘렸고, 생성 위치는 플레이어+20과 이전 묶음 마지막 장애물+11.5 중 더 먼 지점으로 정해 묶음이 겹치지 않게 했다.
- 2026-10-11 후속 조정: 아이템을 20% 키우고 금빛 맥동 외곽광과 부유 동작을 더했다. 아이템 높이를 설면 위 1.1–1.55단위로 조정해 기본 점프 범위에 넣고, 연결된 `ItemPickupEffect`의 방향·팔레트는 유지하면서 재생 인스턴스의 파티클과 2D 확장 플래시를 보강한다. Small/Tall/Wide는 CompositeCollider2D 설면을 기준으로 충돌체 바닥과 아트 크기를 함께 맞춘다. 이동 속도 상한을 제거해 초속 5에서 0.045/초로 계속 증가시키고, 대시가 현재 속도보다 초속 20단위 빠르게 유지되도록 한다. 이 후속 변경의 Unity Play 화면 acceptance는 아직 확인하지 않아 AC-02/05/08/18/20/21을 재확인 대기로 둔다. 현재 실행 환경에서 `unity` CLI가 경로에 없어 live Editor 명령을 사용하지 못했다.
- 2026-10-11 최종 VFX·접지 acceptance: Unity 6000.3.12f1에서 이펙트 소스를 강제 재임포트하고 컴파일 오류·경고 0건을 확인했다. Game 뷰에서 실제 더블 점프 아이콘을 사용해 43개 렌더 구성이 생성됨을 확인하고, 고대비 코발트 테두리 링·비운 중심 후광·눈꽃 반짝임으로 마무리했다. 캡처 시 구성을 읽기 쉽도록 Play 중에만 연출 수명을 임시 연장했으며 기본 코드 수명 0.9초는 바꾸지 않았다. 장애물 세 종류의 충돌체 바닥은 CompositeCollider2D 상단 y=-2.5와 각각 차이 0으로 측정됐다. Small/Wide 전체 SpriteRenderer bounds는 투명 하단 때문에 y=-2.67이지만, 소스 PNG의 불투명 경계와 0.17 보정을 합친 가시 픽셀 경계는 y=-2.5에 닿는다. [검증 캡처](Previews/pickup-obstacle-vfx-review.png). 임시 검증 물체·캔버스 숨김·이동 정지 값을 폐기하고 TitleManager 어트랙트 모드, timeScale 1, Player/Rigidbody 활성 및 타이틀 UI를 복원했다. 기록 데이터는 변경하지 않았다.
- 플레이어 BoxCollider2D를 세로 CapsuleCollider2D로 교체했다. Transform의 월드 배율 0.2를 반영한 실제 영역은 0.40×0.45 단위로 기존 폭 0.45보다 11% 좁다. 기존 지면 접촉과 점프 상승을 다시 확인했다. Tall의 기존 1.05×2.8/offsetY 0.9 충돌 영역은 아트 높이에 맞춘 1.05×2.0/offsetY 0.5로 조정했다.
- Small/Tall/Wide 각 장애물 프리팹에서 기존 스프라이트를 1.14배 키운 주황색 외곽을 그 뒤에 배치했다. 1280×720 실제 Game 뷰에 세 종류를 함께 표시해 하늘색 산·하얀 설면·짙은 숲 위에서도 모두 구분되는 것을 확인했다: [장애물 대비 화면](Previews/obstacle-types.png).
- 제어 없는 첫 확인에서는 초반 Tall 접근 후 약 5초에 충돌했다. 간격·난이도 조정 뒤 지면 접지 확인과 피격 거리 4.5단위 기준 자동 점프를 적용한 런은 Small/Tall/Wide를 회피하면서 45초까지 생존했고, 이 시점은 전체 16패턴 풀 활성 구간이다. 이는 회피 여유를 확인하기 위한 점프 타이밍 시뮬레이션이며 키보드 입력을 사용하는 사람의 플레이 테스트와는 구분해 기록한다. [초반 회피 기록](Previews/obstacle-balance-early.csv), [중간 회피 기록](Previews/obstacle-balance-mid.csv), [42초 이후 기록](Previews/obstacle-balance-late.csv)를 보관한다.
- 검증 런에서는 닉네임 제출을 수행하지 않았다. 기존 순위표의 PLAYER 109초를 유지했고 Unity 콘솔 컴파일 오류·경고는 0이다.
- 2026-10-12 Escape 일시정지 메뉴: GameManager 시작 시 PauseMenu를 추가해 일시정지 중 점수·물리·장애물의 시간 진행을 멈추고 재개, 설정, 타이틀 복귀, 게임 종료 버튼을 제공한다. 설정은 전체 음량과 창/전체 화면을 조절하고 PlayerPrefs에 보존한다. Unity 6000.3.12f1 StandaloneWindows64 빌드가 성공해 `Builds/Windows/JumpDashPrototype.exe` 및 데이터 폴더를 생성했다. 요청에 따라 실행 파일은 실행하지 않아 메뉴 키·버튼의 실제 플레이 상호작용은 검증하지 않았다. Unity Licensing Client handshake/access-token 오류가 로그에 남았지만 Build Result는 Success이고 CLI 종료 코드는 0이었다.
