# Deep Blue

**마지막 갱신:** 2026-10-17 · **현재 단계:** 기능 구현본은 Windows r17 미리보기입니다. 현재 수집 목록은 Quaternius 35종과 Fish Alive 2종으로 총 37종이며, 저장 장면의 37개 보상과 3D 프리팹·도감 아이콘이 모두 연결된 것을 다시 대조했습니다. 사용자는 r17에서 이동·시점·일시정지, 포획·판매·장비 구매와 재실행 저장, 캐스팅·입질·포획 효과음, 창 모드·4:3 UI, 실패 후 재시도를 확인했습니다. 전체 진행률은 가중 추정 94.50%(반올림 95%)입니다. 남은 도감·사망 재시작 통합 확인과 초보자 플레이 세션은 사용자의 요청에 따라 QA 재개 시 진행합니다. 배경 음악은 제외합니다.

Deep Blue는 Unity 6과 URP로 만드는 3D 1인칭 낚시 게임입니다. Unity 초보자도 따라올 수 있도록 기능을 작은 단계로 만들고, 코드가 하는 일과 에디터에서 확인할 방법을 함께 기록합니다.

## 프로젝트 정보

- **엔진:** Unity `6000.3.12f1 LTS`
- **렌더링:** URP
- **입력:** Input System `1.19.0`
- **주 개발 장면:** `Assets/Scenes/DeepBlueFishing.unity`
- **원본 장면:** `Assets/Scenes/SampleScene.unity`는 보존되어 있습니다.
- **목표:** 다음 주 일요일인 **2026년 10월 18일**까지 낚시 코어 루프, 필수 메뉴, Windows 빌드 검증과 문서 정리 완료를 목표로 진행

게임 범위와 요구사항은 [SPEC.md](SPEC.md), 실행 순서는 [TASKS.md](TASKS.md)에서 관리합니다.

## 무료 3D 물고기 37종 연결 — 2026-10-17

[Quaternius Animated Cute Fish](https://quaternius.com/packs/cutefish.html) CC0 1.0 번들 35종과 [Fish Alive - FREE Samples](https://assetstore.unity.com/packages/3d/characters/animals/fish/fish-alive-free-samples-362806) 2종을 사용합니다. 출처 및 사용 범위는 [Quaternius 출처 기록](Assets/Archive/QuaterniusFish/ASSET_SOURCE.md)과 [Fish Alive 출처 기록](Docs/FISH_ALIVE_ASSET_SOURCE.md)을 확인하세요.

- 활성 보상은 37종이며 각 항목이 실제 3D 리소스와 연결됩니다. Quaternius 프리팹 35개와 Fish Alive 프리팹 2개는 `Assets/Resources/DeepBlueFish/` 아래에서 불러옵니다.
- 도감 아이콘도 활성 목록의 37개 파일과 일대일로 연결됩니다. 물고기는 6개씩 7페이지에 표시합니다. 예전 12개 종의 저장 기록은 현재 어획 목록에 섞지 않고 보관 기록으로 유지합니다.
- 두 흰동가리는 클래식 Quaternius 모델과 Fish Alive 모델의 서로 다른 외형으로 각각 수집할 수 있습니다.
- 게임용 무게 범위와 출현 가중치로 작은 어종에서 대형 어종 순으로 수집 난이도를 구성하고, 처음 만나는 종은 출현 보정을 받습니다. 서식지 시뮬레이션은 포함하지 않습니다.
- Unity Play에서 37종 전부의 프리팹·Animator·클립·SkinnedMeshRenderer를 조과 Showcase로 불러오는 검증이 통과했습니다. 현재 파일·씬 대조에서도 보상 37개, 프리팹 37개, 아이콘 37개 및 누락 0개입니다. 전체 목록은 [37종 에셋 연결표](Docs/FISH_ROSTER.md)에서 확인할 수 있습니다.

저폴리 스타일의 무료 에셋이라 실사형 모델은 아닙니다. 현재 프로젝트는 별도 유료 애니메이션 플러그인이나 모델 구독이 필요하지 않습니다.

## 지금까지 진행한 내용

### 재미 검토와 첫 체험 목표 — 2026-10-08

2026-10-08 당시에는 **낚시 코어 루프를 갖춘 프로토타입**으로 평가했습니다. 이후 릴링 변화, 수집 목표, 개체 무게 기록과 상점이 추가됐습니다. 목표 체험 길이는 **15~20분**이며, 초보자 세션이 아직 진행되지 않아 실제 재미·체류 시간은 검증되지 않았습니다.

[The Angler](https://store.steampowered.com/app/1408610/Call_of_the_Wild_The_Angler/)는 어종 발견과 물고기의 저항에 대응하는 과정을, [Cast n Chill](https://store.steampowered.com/app/3483740/Cast_n_Chill/)은 수집과 점진적인 성장을 소개합니다. 이를 참고한 이번 프로젝트의 판단은 **작은 목표 → 낚시 중 판단 → 기록 갱신**을 한 장소에서 먼저 완성하는 것입니다.

| 부족했던 부분 | 이번 보강 | 플레이어가 느낄 변화의 목표 |
| --- | --- | --- |
| 다음에 할 일이 모호함 | 첫 1마리 → 3종 → 총 5마리 → 6종 → 12종 수집 목표 | 화면에서 다음 도전을 바로 알 수 있음 |
| 같은 물고기는 횟수만 증가 | 개체 무게·어종별 개인 최고 기록 | 중복 어종도 더 큰 개체를 노리는 이유가 생김 |
| 감기/쉬기 속도가 일정 | 안정 → 0.75초 예고 → 강한 저항과 행동 안내 | 화면 신호를 보고 감거나 쉬는 선택을 함 |
| 결과를 연속 클릭으로 넘기기 쉬움 | 결과 표시 후 0.8초 입력 유예 | 이름·무게·첫 발견·목표 달성을 읽을 시간 확보 |

무게 범위는 게임용 기본안입니다. 작은 개체는 자주, 큰 개체는 상대적으로 적게 나오며 종마다 범위가 다릅니다. 기존 획득 횟수는 유지되고 과거 기록에 없던 무게를 임의로 채우지 않습니다. 무게 기록은 다음 성공부터 저장됩니다.

**처음 접하는 플레이어 3~5명에게 확인할 기준(아직 미검증):**

- 별도 설명 없이 1분 안에 캐스팅하고 3분 안에 첫 물고기 획득을 시도할 수 있는지
- 3번 낚시한 뒤 ‘언제 버튼을 놓는지’와 실패 이유를 말할 수 있는지
- 15분 뒤 어떤 수집 목표나 최고 기록에 도전 중인지 알고, 자발적으로 한 번 더 낚시하는지
- 조작·글자·소리·불편한 대기 때문에 그만두는 지점이 있는지

시간만으로 재미를 판정하지 않습니다. 직접 관찰한 막힘과 재도전 의사를 기록한 뒤 입질 대기·장력·목표를 조정합니다. 장시간 콘텐츠와 실제 청취·물리 키보드 플레이는 별도 남은 작업입니다.

**구현 확인:** Editor에서 정상 입력 이벤트로 큰입배스 **2.42kg**을 낚았고 새 Play 세션에서도 횟수·최고 무게·목표가 유지됐습니다. 저항 예고 중 Esc 메뉴를 약 2초 열어도 예고 시간 0.75초·장력·진행도가 변하지 않았습니다. 계속 감아 줄이 끊기는 경우와 챔질 시간 초과에서 저장 기록이 증가하지 않았습니다. 별도 임시 데이터로 첫 발견·목표 달성·중복 저장 방지·작은 개체의 기록 보존·새 최고 기록과 목표 마지막 단계도 확인했습니다.

720p 도감, 1080p 목표/성공/저항 화면, 1024×768 도감/목표/실패 화면에서 잘림이 없었습니다. Windows r4는 오류 0건으로 빌드됐고 압축 해제 후 실제 타이틀→도감→시작·수집 목표와 기존 큰입배스 횟수 유지까지 확인했습니다. Windows에서는 새 무게가 없는 이전 기록을 정확히 안내합니다. Editor와 Windows 저장소는 별도입니다.

[검증 요약](Evidence/2026-10-08/PlayExperience/acceptance-summary.json) · [2.42kg 성공 화면](Evidence/2026-10-08/PlayExperience/05-normal-catch.png) · [저항 안내](Evidence/2026-10-08/PlayExperience/04-surge.png) · [Windows 목표 화면](Evidence/2026-10-08/PlayExperience/19-windows-r4-goal.png)

초보자를 위한 코드 역할: `DeepBlueFishDetails`가 무게 범위, `DeepBlueCollectionProgress`가 다음 목표, `DeepBlueProfile`이 저장, `FishingGameController`가 저항과 결과, `FishingHUD`/`DeepBlueMenus`가 화면 표시를 맡습니다. 새 파일도 Unity가 자동으로 읽으므로 별도 오브젝트 연결은 필요 없습니다.

### Steam 조사와 물고기 6종 추가

[Fishing Planet](https://store.steampowered.com/app/380600/Fishing_Planet/), [The Angler](https://store.steampowered.com/app/1408610/Call_of_the_Wild_The_Angler/), [Ultimate Fishing Simulator](https://store.steampowered.com/app/468920/Ultimate_Fishing_Simulator/)의 어종 구성과 확장 사례를 조사했습니다. 이를 참고해 현재 호수에 어울리고 외형을 구별하기 쉬운 6종을 선정했습니다. 자세한 출처와 선정 이유는 [조사 문서](Docs/FISH_RESEARCH_2026-10-08.md)에 있습니다.

| 새 물고기 | 그림에서 구분되는 특징 | 이미지 |
| --- | --- | --- |
| 옐로퍼치 | 금색 몸통, 세로 줄무늬 | [PNG](Assets/Art/UI/Fish_YellowPerch.png) |
| 월아이 | 큰 눈과 길쭉한 몸 | [PNG](Assets/Art/UI/Fish_Walleye.png) |
| 브룩송어 | 주황색 배와 작은 반점 | [PNG](Assets/Art/UI/Fish_BrookTrout.png) |
| 레이크철갑상어 | 긴 주둥이, 몸의 골판 | [PNG](Assets/Art/UI/Fish_LakeSturgeon.png) |
| 블랙크래피 | 넓은 은색 몸과 검은 얼룩 | [PNG](Assets/Art/UI/Fish_BlackCrappie.png) |
| 버봇 | 턱수염과 낮고 긴 지느러미 | [PNG](Assets/Art/UI/Fish_Burbot.png) |

- 내장 imagegen으로 투명 배경 PNG 6개(각 1536×1024)를 새로 만들고 Unity Sprite로 연결했습니다. [전체 생성 프롬프트와 파일 기록](Assets/Art/UI/FishExpansionManifest.json)을 보관합니다.
- 기존 6종과 새 6종 모두 낚시의 무작위 보상에 포함됩니다. 어종 추가 당시에는 몸무게·전투 변화가 없었으며 후속 DB-035에서 개체 무게와 저항 변화를 추가했습니다.
- 도감의 **이전 / 다음** 버튼으로 두 페이지를 볼 수 있습니다. 기존 그림 이름과 저장 키를 유지하므로 기존 획득 기록도 이어집니다.
- Editor에서 입력 이벤트로 정상 캐스팅·Space 챔질·릴링을 진행해 **버봇 1마리**를 낚았습니다. 이름/그림 일치, 성공 시 1회 증가, 도감 표시 및 Play 재시작 후 유지 확인. 기존 Editor 기록인 강꼬치고기 1·블루길 1·채널메기 2도 유지됐습니다. Editor와 Windows 저장 기록은 별도입니다.
- 도감의 720p·1080p·1024×768 화면과 페이지 경계 버튼을 확인했습니다. 새 6종을 각각 무작위 낚시로 모두 얻은 검증은 아니며 12종 전체 참조는 카탈로그에서 확인했습니다.
- Windows r3 압축본을 새로 풀어 실행했습니다. 기존 큰입배스 1마리 기록이 유지됐고, 12종 합계와 두 페이지의 그림·이름 및 이전/다음 이동을 확인했습니다. [Windows 새 어종 화면](Evidence/2026-10-08/FishExpansion/15-windows-r3-page-two.png)
- 다시 연결할 때는 Play를 멈추고 **Deep Blue → Configure Fish Rewards**를 사용합니다. 물고기 목록만 갱신하므로 환경 전체를 다시 생성할 필요가 없습니다.
- [검증 기록](Evidence/2026-10-08/FishExpansion/), [버봇 성공 화면](Evidence/2026-10-08/FishExpansion/07-burbot-caught.png), [12종 도감의 두 번째 페이지](Evidence/2026-10-08/FishExpansion/08-burbot-journal-720p.png).

### Windows 미리보기 실행

[Windows 미리보기 r17](Builds/Delivery/DeepBlue-Windows-20261017-preview-r17-collection-pacing/DeepBlue.exe) · 실행 폴더 합계 약 137.2 MiB · 2026-10-17 · 37종 수집·초보자 수집 페이스 조정 포함

1. 링크된 `DeepBlue.exe`는 전달 폴더 안의 실행 파일입니다. 실행할 때는 같은 폴더의 `DeepBlue_Data`, `UnityPlayer.dll`, `MonoBleedingEdge` 등 빌드 파일을 함께 둡니다.
2. 타이틀에서 **낚시 시작**을 누릅니다. 이동·캐스팅·챔질·릴링 안내는 게임 안에서 볼 수 있습니다.
3. 설정·도감·화폐·장비는 이 PC에 저장하며, 위치와 진행 중인 낚시는 저장하지 않습니다.

사용자는 r17에서 주요 조작·포획·상점·재실행 저장과 효과음을 확인했습니다. 도감 열기·사망 후 재시작 통합 흐름 및 초보자 사용성은 남아 있으므로 **QA가 모두 끝난 출시 판정은 아닙니다.** 빌드 폴더에는 180개 파일, 총 143,853,189바이트가 있습니다.

### 코드와 장면에 연결됨

- [x] URP 프로젝트와 Input System 구성
- [x] `DeepBlueFishing` 장면 생성: 땅, 물, 부두, 해안 경계, 햇빛과 낚시 도구 블록아웃
- [x] `Player`에 `CharacterController`, `FirstPersonLook`, `FirstPersonMovement`, `DrowningController` 연결
- [x] 자식 `Main Camera`를 시점 스크립트의 카메라 피벗으로 연결
- [x] `Fishing Systems`에 `FishingGameController`를 연결하고 카메라, 낚싯대 끝, 찌, 낚싯줄, 물고기 아이콘 참조 지정
- [x] 낚시 시작 시 `FishingGameController`가 `FishingHUD`를 자동으로 연결하고 uGUI Canvas 생성
- [x] 물 표면에 맞춘 `Water Death Zone` Trigger와 바닥 충돌용 `Lake Bed (Collision)` 구성
- [x] 명세서와 작업 목록 작성 및 이 README에서 연결
- [x] `DeepBlueMenus`와 전용 배경을 장면에 연결하고 도감·설정 자동 저장 추가
- [x] 굴곡 지형, 소나무 숲, 산 능선, 목재 부두, 물가 쉼터·벤치·랜턴과 새 수면 재질 적용

### Unity에서 확인된 내용

- [x] 2026-10-01 Scene/Game 뷰에서 땅, 물, 부두, 낚시 위치와 낚싯대가 구분되어 보이는 것을 확인
- [x] 시점 입력 보완 코드를 재임포트하고 Console 오류 0건, 경고 0건 확인
- [x] Play 모드에서 한국어 준비 안내, 캐스팅 중 낚싯줄과 찌, 입질 시간 초과 실패 카드 표시 확인
- [x] 실패 화면을 클릭해 장면을 다시 불러오지 않고 준비 상태로 돌아오는 흐름 확인
- [x] 2026-10-08 목표·무게·저항 변화 포함 Windows x64 r4 빌드 성공, 166,878,536바이트(159.1MiB), 오류 0건
- [x] Editor에서 기본 난이도의 Space 챔질·릴링 성공, 도감 1회 기록과 재시작 후 유지 확인
- [x] Editor에서 입질·익사 도중 메뉴를 열면 시간이 멈추고 이동 입력도 차단됨을 확인
- [x] 물 진입·탈출 초기화·10초 익사, 사망 후 물가 재시작 확인
- [x] 타이틀과 도감의 실제 1280×720·1920×1080·1024×768 화면 확인
- [x] Windows에서 시작·도감·설정·타이틀 복귀·종료 버튼 및 포커스 이탈 일시정지 확인
- [x] Windows 음량을 50%로 변경하고 종료·재실행해 유지됨을 확인한 뒤 기본 80%로 복구

- [x] Editor에서 낚시 HUD 상태와 720p·1080p·4:3 주요 화면 배치, 기본 밸런스 성공/실패 확인

### 남은 QA — 사용자의 요청에 따라 연기

- r17에서 도감 열기와 사망 후 재시작을 이어 확인합니다. DB-030은 현재 90%입니다.
- 초보자 3~5명에게 각각 15~20분 플레이를 관찰하고, 막힌 단계와 자발적 재도전 여부를 `Docs/PLAYTEST_FORM_BEGINNER.md`에 기록합니다. DB-036은 현재 20%입니다.
- 사용자는 이동·시점·일시정지, 포획·거래·재실행 저장, 효과음과 음량, 창 모드·4:3 표시, 실패 후 재시도를 이미 확인했습니다. 이 항목은 다시 요청하지 않습니다.

## 완료 조건별 검증 현황 — 2026-10-17

**현재 상태: 기능 구현은 r17까지 진행했고, 통합 QA는 사용자의 요청에 따라 보류 중입니다.** 아래 과거 r2/r3/r4 QA 기록과 최신 r17 사용자 확인을 구분합니다. Windows r17에서 사용자가 확인한 직접 조작·상점·저장·효과음·창 표시·실패 재시도만 최신 확인으로 기록하며, 아직 확인하지 않은 도감 열기와 사망 후 재시작, 초보자 사용성을 완료로 확대하지 않습니다. 현재 가중 진척은 94.50%(반올림 95%)입니다.

| 기준 | Editor에서 확인한 범위 | Windows 실행본 및 남은 범위 |
| --- | --- | --- |
| FR-01 시점·커서 | 회전, 상하 ±80도 제한, 메뉴 커서 전환 | r17 마우스 시점·Escape 일시정지와 재개를 사용자가 확인. 도감 열기 남음 |
| FR-02 이동·충돌 | WASD, 바닥, 최종 외곽 8방향 차단, 물가 왕복 | r17 WASD 이동을 사용자가 확인. 전체 경계 acceptance는 이전 QA 기록 참조 |
| FR-03 낚시 장소 | 부두·수면·찌 구분 | r2 시작 위치와 캐스팅 화면 확인 |
| FR-04 캐스팅 | 충전 거리, 수면 보정 120건, 착수 불가 안내 | r2 기본 캐스팅 확인. 긴 충전·가장자리 보정 확인 필요 |
| FR-05 입질 신호 | 착수 후 대기→입질 및 카운트다운 | r2 입질 알림과 로그 3회 확인 |
| FR-06 챔질 | 기본 5초 안의 Space 챔질과 시간 초과 | QA Space 챔질→릴링 확인. r2 시간 초과 확인. 실제 조작 남음 |
| FR-07 릴링 | 누를 때 진행도·장력 증가, 놓으면 장력 감소 | QA 버튼 유지·놓기에서 진행도 증가·장력 감소 및 성공 확인 |
| FR-08 결과 | 진행도 100% 성공, 장력 100% 줄 끊김 | QA 큰입배스 성공(진행도 1)·줄 끊김(장력 1) 결과 확인 |
| FR-09 반복 | 시간 초과→줄 끊김→성공의 3회 연속 낚시 | QA 성공→줄 끊김→시간 초과 3회와 각각의 준비 복귀. r2 시간 초과 3회 확인 |
| FR-10 HUD | 상태별 안내·막대·결과·익사·사망, 720p/1080p/4:3 | r2 준비·캐스팅·입질·실패 및 QA 성공·줄 끊김·사망 화면 확인. r2 전체 직접 확인 남음 |
| FR-11 보상 | 활성 37종 이름·아이콘·3D 프리팹 연결. Play Mode에서 Showcase 37/37 로드 확인 | r17에서 사용자 포획 확인. 독립 실행본 전체 어종별 결과 표시는 미확인 |
| FR-12 익사 | 물 진입, 탈출 초기화, 10초 사망과 입력 차단 | QA 물 진입→탈출 시 10초 초기화→재진입·익사·사망 입력 차단 확인 |
| FR-13 타이틀·메뉴 | 시작·일시정지·복귀·타이틀 및 입력 차단 | r17 Escape 일시정지·재개 확인. 도감 열기 및 사망 메뉴 통합 흐름은 QA 재개 시 확인 |
| FR-14 설정·안내 | 음량·감도 적용/재로드 유지, 조작법 | r17 캐스팅·입질·포획 효과음과 음량 적절함을 사용자 확인. 배경 음악 제외, 감도 조절 체감은 미확인 |
| FR-15 도감·저장 | 성공만 증가, 실패 미증가, 이전 기록 보관, 6종 단위 7페이지 | r17 실패 후 횟수 미증가와 재시도 확인. 도감 열기·사망 재시작 통합 확인은 QA 재개 시 진행 |
| FR-16 사망·종료 | 익사 후 재시작/입력 복구, 도감·설정 유지 | QA 익사→재시작→위치·타이머·낚시 복구 확인. 이전 일반 빌드 타이틀·종료 확인창 확인 |
| FR-17 환경 | 생성 이미지 6종 적용, 이동·낚시·물·도감 유지 | r2 풍경·캐스팅·메뉴, QA 새 환경 위 이동·성공 낚시·물 판정 확인 |
| FR-18 수집 목표 | 모든 단계·최종 완료 및 첫 달성은 임시 데이터, 정상 성공으로 4/6→5/6 및 재시작 유지 | r4 HUD·도감에서 기존 저장의 1/3 목표 표시. 실제 플레이 관찰 남음 |
| FR-19 개체 기록 | 정상 큰입배스 2.42kg 성공·재시작 유지, 작은 개체·새 기록·실패 규칙은 임시 데이터로 확인 | r4 기존 횟수 유지·무게 미기록 안내 확인. Windows 새 성공 무게 저장은 직접 확인 남음 |
| FR-20 저항 변화 | 정상 입력으로 안정/예고/저항·성공·줄 끊김·메뉴 시간 정지 확인 | r4 빌드에 포함. 실제 키보드/마우스 대응·체감 난이도 남음 |

주요 근거:

- [카메라](Evidence/2026-10-08/Menus/camera-limits.json), [일시정지·챔질·성공](Evidence/2026-10-08/Menus/pause-catch.json), [장력 감소와 세 번째 성공](Evidence/2026-10-08/Menus/hud-third-cast-catch-720p.json), [줄 끊김](Evidence/2026-10-08/Menus/hud-line-break-4x3.json).
- [익사·재시작](Evidence/2026-10-08/Menus/death-retry.json), [화면 크기별 UI](Evidence/2026-10-08/Menus/), [수면 착수 보정](Evidence/2026-10-08/Environment/landing-fix-acceptance.json), [환경](Evidence/2026-10-08/Environment/acceptance-summary.json), [외곽 경계](Evidence/2026-10-08/Boundary/acceptance-summary.json).
- [r2 Windows 연속 플레이](Evidence/2026-10-08/WindowsAcceptance/three-casts.json), [실패 후 도감](Evidence/2026-10-08/WindowsAcceptance/05-journal-after-three-failures.jpg).
- [Windows 개발용 빌드 검증 요약](Evidence/2026-10-08/WindowsQA/acceptance-summary.json), [큰입배스 성공](Evidence/2026-10-08/WindowsQA/03-caught-bass.jpg), [일반 r2 재실행 후 도감](Evidence/2026-10-08/WindowsQA/18-r2-persisted-journal.jpg).
- [37종 도감 확인 화면](Assets/Evidence/2026-10-17/CurrentAcceptance/FullFishRoster-Title-1280x720.png), [초보자 수집 안내 화면](Assets/Evidence/2026-10-17/CurrentAcceptance/CollectionGuide-1280x720-r17-final.png), [37종 에셋 연결표](Docs/FISH_ROSTER.md).

### 일반 r2의 10분 대기 측정

- 2026-10-08 18:12:25~18:22:25(KST), 일반 r2의 낚시 준비 화면에서 같은 프로세스를 약 30초마다 21회 측정했습니다. 측정한 모든 시점에서 응답 상태가 정상이었고 프로세스 종료는 없었습니다.
- 실제 메모리 사용량(Working Set)은 642.4→656.2MiB, 프로세스 전용 메모리(Private Bytes)는 974.7→987.8MiB였습니다. 이 구간의 증가량만으로 메모리 누수 유무를 확정하지 않습니다.
- 10분 대기 뒤 마우스 클릭으로 캐스팅→입질 시간 초과→다음 낚시 준비까지 확인했습니다. 같은 실행 로그에 입질·시간 초과가 각각 1건 기록됐고 C# 예외 검색 결과는 0건입니다. 기존 D3D12 정보 큐 진단은 시작 로그에 남아 있습니다. [측정 요약](Evidence/2026-10-08/ReleaseStability/acceptance-summary.json)과 [원본 수치·화면](Evidence/2026-10-08/ReleaseStability/)을 저장했습니다.
- 현재 PC의 RTX 4070 SUPER 환경에서 Unity Editor도 열린 상태로 측정했습니다. FPS·GPU 사용량·장시간 이동/연속 낚시 부하는 측정하지 않았으며 DB-030의 전체 성능 통과 판정은 아닙니다.

### Windows 개발용 실행본에서 추가 확인한 결과

- 별도 QA 빌드에 Unity의 로컬 검증 연결을 포함하고, 게임의 입력 프레임에 맞춰 키·마우스 이벤트를 보냈습니다. 코드·밸런스·플레이어 위치·낚시 상태를 강제로 바꾸지 않았으며 영구 테스트 스크립트도 추가하지 않았습니다.
- 1회차: 0.8초 충전 후 던지고 Space 챔질, 1.6초 감기와 2.5초 놓기를 반복해 큰입배스 획득. 진행도 1, 결과 장력 약 0.279, 도감 1회 기록.
- 2회차: 계속 감으면 장력 1에서 줄 끊김. 3회차: 입질 중 Esc 메뉴에서 남은 5초가 정지하고 복귀 후 시간 초과. 두 실패 모두 큰입배스 기록은 1회 그대로였습니다.
- Tab 도감 전환, WASD 이동과 바닥 유지, 메뉴 중 W 입력 차단, 좌우 시점 회전 및 상하 ±80도 제한을 확인했습니다. 시점은 반대 방향 입력으로 원래 방향에 복귀했습니다.
- 물 진입→탈출 시 10초 초기화→재진입, 메뉴 중 익사 8.962249초 유지, 익사 후 이동/낚시 차단, 사망 메뉴의 실제 버튼 클릭 후 시작 위치·10초·준비 상태 복구를 확인했습니다.
- QA 게임을 완전히 종료한 뒤 일반 미리보기 r2를 새로 실행했습니다. 타이틀과 도감에 큰입배스 1마리가 남아 있어 DB-028을 완료 처리했습니다. 이 PC 도감에는 검증 중 정상 획득한 큰입배스 1마리가 저장되어 있습니다.
- QA 빌드 오류 0건, Runtime Console 오류·경고 0건. 빌드 경고 1건은 Unity Cloud 심벌 업로드 생략 안내입니다. 첫 이동 검사에서 검증 이벤트끼리 키 상태를 덮어쓴 시도는 제외했고, 하나의 키 상태로 다시 보내 확인했습니다.
- 검증용 프로세스와 로컬 연결 파일은 종료·제거됐고, 프로젝트 연결 설정/Development Build는 비활성입니다. 씬 저장 및 컴파일 오류 없음도 확인했습니다. `Builds/QA-20261008/`는 내부 검증용이며, 당시 사용자 실행용은 일반 r4 압축본이었습니다.

### r2 Windows에서 추가 확인한 결과

- 같은 게임 프로세스에서 세 번 캐스팅했고 입질·시간 초과 로그가 각각 3회 기록됐습니다. 세 번째 실패 뒤에도 다음 낚시 준비 화면으로 복귀했습니다.
- 실패 후 도감은 발견 0/6종·총 0마리였고, 다른 창으로 이동했을 때 자동 일시정지와 도감 버튼도 동작했습니다. 이 결과는 성공 기록의 저장·재실행 검증에는 해당하지 않습니다.
- 검토한 실행 로그에 C# 예외는 없었습니다. 시작 때의 기존 D3D12 정보 큐 진단 메시지는 남아 있습니다.
- 자동 입력으로 챔질을 시도했지만 다음 관찰 시점에는 제한 시간이 지난 상태였습니다. 자동 Esc 입력도 메뉴 전환을 확인하지 못했습니다. 실제 키보드 동작 여부는 이 결과만으로 판단하지 않습니다.
- 코드·밸런스는 변경하지 않았습니다. DB-022의 완료 여부 기록을 갱신했으며, Windows 전체 검증은 DB-030에 남아 있습니다.

## 외곽 낙하 방지 — 2026-10-08

- 맵 뒤쪽 끝으로 걸어 나가면 Player가 계속 떨어지는 문제를 재현해 수정했습니다. 지형 끝 안쪽에 기존 목재 재질을 사용한 울타리와 충돌 경계를 저장했습니다.
- 보행 범위는 x=-142~142m, z=-72~150m입니다. 북쪽은 장식용 산의 뒷면 아래로 들어가지 않도록 산 앞에 울타리를 배치했습니다. 부두와 물 진입 통로는 유지했습니다.
- Editor에서 네 방향과 네 모서리 가까이 Player를 배치한 뒤 각 3초간 정상 W 입력 이벤트를 보냈습니다. 8곳 모두 울타리 앞에서 멈추고 바닥 위를 유지했습니다. Windows 실제 키보드 검증과는 구분합니다.
- 부두 z=10.70m에서는 물 판정 없음 → z=14.70m에서 물 진입 → z=6.70m 복귀 후 익사 타이머 10초 초기화를 재확인했습니다.
- 울타리는 표시 메쉬 1개와 충돌 구간 254개로 구성됩니다. **Deep Blue → Update Forest Boundary**로 외곽 울타리만 다시 만들 수 있으며, **Upgrade Lake Environment**에도 포함됩니다. 재생성 시 기존 울타리에 한 수동 편집은 생성 위치로 돌아갑니다.
- 개선 전 씬은 `Assets/Scenes/Backups/DeepBlueFishing_BeforeBoundary_20261008.unity`에 보존했습니다. 현재 씬의 Player 시작 위치는 (0, 0, 3.5)이며 저장 완료입니다.
- 외곽 수정 당시 빌드: 157,170,248바이트, 약 149.9MiB, 오류 0건. 경고 2건은 기존 Player 원격 제어 및 Cloud 심벌 업로드 안내입니다. r2 압축본의 실제 시작 화면과 한글 HUD도 확인했습니다. 이후 목표·기록·저항 변화가 포함된 r4를 전달했습니다.
- [완료 근거](Evidence/2026-10-08/Boundary/acceptance-summary.json), [8방향 이동 결과](Evidence/2026-10-08/Boundary/02-eight-directions.json), [수정판 Windows 화면](Evidence/2026-10-08/Boundary/09-windows-r2-gameplay.jpg).

## 인게임 환경 개선 — 2026-10-08

### 후속 UI·재시작 점검

- Unity 6에서 HUD 예비 글꼴 `Arial.ttf` 조회가 예외를 내는 것을 확인해 유효한 `LegacyRuntime.ttf`로 수정했습니다. 평소에는 기존처럼 Windows 맑은 고딕을 사용합니다.
- HUD와 메뉴에서 직접 만든 글꼴만 종료 시 정리하도록 했습니다. 공유 기본 글꼴은 삭제하지 않습니다.
- Editor에서 장면을 3회 다시 로드하며 매번 이전 글꼴 2개와 효과음 5개의 제거를 확인했습니다. 새 UI는 한글을 표시했고 Canvas 2개·EventSystem 1개를 유지했습니다. Play 종료 후에도 추적한 자원이 모두 정리됐으며 Console 오류·경고는 0건이었습니다.
- 이 검증은 장면 재로드 API와 시작 버튼 콜백으로 진행한 자원 수명 확인입니다. 실제 키보드 조작 검증으로 간주하지 않습니다. 지원 OS 글꼴이 전혀 없는 PC의 한글 표시도 확인하지 않았습니다.
- 이 점검 시점의 Windows 빌드는 155,955,640바이트(약 148.7MiB), 오류 0건으로 성공했습니다. 경고 2건은 Player 원격 제어 미설정과 Unity Cloud 심벌 업로드 생략 안내입니다. 이후 플레이 보강 포함 r4를 전달했습니다.
- [재시작·배포 확인 기록](Evidence/2026-10-08/Lifecycle/acceptance-summary.json), [압축본 실행 화면](Evidence/2026-10-08/Lifecycle/09-extracted-windows-gameplay.jpg).

생성한 2D 이미지를 실제 3D 물체의 표면에 입혔습니다. 하늘 이미지는 주변을 감싸는 Skybox로 사용하며, 플레이어가 걸어 다니는 지형과 부두·쉼터는 3D로 구성되어 있습니다.

### 추가한 풍경과 구조물

- 굴곡 있는 호숫가와 이끼·자갈 지면, 소나무 숲과 풀, 바위, 먼 산 능선.
- 판자가 구분되는 목재 부두, 고정 못, 기둥과 처진 밧줄, 랜턴, 낚시 도구 상자.
- 물가 목조 쉼터와 벤치. 쉼터는 배경 구조물로, 출입이나 상점 기능은 없습니다.
- 새벽 하늘과 따뜻한 햇빛, 거리 안개, 색 보정, 은은한 랜턴 빛.
- 시간에 따라 움직이는 잔물결과 하늘·주변 환경 반사. 호수 표시 영역과 물 판정/바닥을 130×136m로 함께 맞췄습니다.

### Unity 초보자를 위한 확인 방법

1. 저장된 `Assets/Scenes/DeepBlueFishing.unity`를 열고 Play → **낚시 시작**을 누릅니다. 추가 연결 작업은 필요 없습니다.
2. 부두를 따라 걷고 마우스로 왼쪽을 둘러보면 목조 쉼터가 보입니다. 물에 들어가면 익사 카운트다운이 시작됩니다.
3. Play를 멈추고 Hierarchy의 **Deep Blue Environment**를 펼치면 지형·숲·부두·쉼터를 찾을 수 있습니다.
4. 환경을 다시 생성하려면 Play를 멈춘 상태에서 **Deep Blue → Upgrade Lake Environment**를 실행합니다. 이 메뉴는 해당 환경 묶음을 다시 만들므로 직접 옮긴 환경 오브젝트도 생성 위치로 돌아갑니다.

표면 색과 재질은 `Assets/Art/Environment/Materials/`, 분위기는 `Dawn Grade.asset`와 씬의 **Dawn atmosphere volume**에서 조정할 수 있습니다. 원래 바닥·부두의 충돌은 유지하면서 표시만 새 환경으로 바꿨습니다. 부두 앞은 낚시와 물 진입을 위해 열어 두었습니다.

### imagegen 제작 이미지

| 파일 | 적용 용도 |
| --- | --- |
| `Cedar_Planks.png` | 부두 판자, 목조 쉼터 외벽 |
| `Forest_Shore.png` | 흙·자갈 지면 |
| `Granite_Rock.png` | 바위와 먼 산 |
| `Dawn_Sky.png` | 새벽 파노라마 하늘과 수면 반사 |
| `Pine_Branch.png` | 투명 배경 솔잎 가지로 구성한 소나무 |
| `Moss_Floor.png` | 지형 높이에 따라 섞이는 이끼 |

모두 내장 imagegen으로 새로 생성했습니다. PNG 원본은 [환경 텍스처 폴더](Assets/Art/Environment/Textures/), 각 생성 프롬프트와 모드·투명도 기록은 [GenerationManifest.json](Assets/Art/Environment/GenerationManifest.json)에 있습니다.

### 저장과 확인 결과

- 개선 전 장면: `Assets/Scenes/Backups/DeepBlueFishing_BeforeEnvironment_20261008.unity`. 원본을 별도 씬으로 보존했습니다.
- 부두 z=10.70m에서 물 판정 없음 → z=14.69m에서 물 판정/카운트다운 → z=6.70m 복귀 후 10초 초기화를 확인했습니다.
- 기존 밸런스 그대로 캐스팅, Space 챔질, 버튼을 누르고 놓는 릴링으로 강꼬치고기를 획득했습니다. Editor 도감에 강꼬치고기 1마리와 이전 채널메기 2마리가 표시됩니다.
- 도감이 열릴 때 게임이 정지하고, 환경 위로 메뉴가 정상 표시됐습니다. 씬 저장 후 두 환경 셰이더의 오류 메시지가 없음을 확인했습니다.
- 작업 중 멀리 있는 잎이 사라지는 문제, 재생성 후 메쉬가 갱신되지 않는 문제, 산 아래의 틈을 발견해 수정하고 다시 촬영했습니다.
- [게임 화면](Evidence/2026-10-08/Environment/05-gameplay.png), [목조 쉼터](Evidence/2026-10-08/Environment/10-shelter.png), [낚시 기록](Evidence/2026-10-08/Environment/fishing.json), [이동·물 판정 기록](Evidence/2026-10-08/Environment/movement-water.json).
- 새 환경을 포함한 Windows x64 빌드가 성공했습니다(155,954,616바이트, 약 148.7MiB, 오류 0건). 실제 실행 파일에서 타이틀 → 낚시 시작 후 숲·산·부두·수면·하늘과 HUD를 확인했습니다. [Windows 게임 화면](Evidence/2026-10-08/Environment/12-windows-gameplay.jpg), [빌드 결과](Evidence/2026-10-08/Environment/windows-build.json).
- 나무·바위는 합친 메쉬로 그리며 반사 프로브는 시작할 때 갱신합니다. 장시간 성능과 낮은 사양 PC의 목표 프레임은 아직 측정하지 않았습니다.

## 추가 수정 — 지형에 맞춘 찌 착수 (DB-032)

- 착수 수정 시점의 Windows 빌드에 수정이 포함됐습니다(155,955,128바이트, 오류 0건). 실행 파일에서 캐스팅·시간 초과·클릭 후 준비 복귀를 두 번 확인했습니다. [당시 빌드 기록](Evidence/2026-10-08/Environment/landing-fix-build.json). 이후 r4 미리보기가 전달됐습니다.
- **발견한 문제:** 기존 착수 범위의 좌우 끝은 새 지형이 수면 위로 솟아 있어 찌가 땅 안에 놓일 수 있었습니다.
- **수정:** `FishingGameController`가 수역과 실제 지면 높이를 검사합니다. 정상 위치는 그대로 사용하고, 막힌 위치만 최대 4m 안의 물로 보정합니다. 열린 수면을 찾지 못하면 안내를 표시하고 다시 시도할 수 있습니다.
- **확인:** Unity 지형에 대해 위치·방향·충전량 120개 조합에서 모두 물 위 착수를 확인했습니다. 정면의 짧은/최대 충전은 각각 z=13m/23.5m로 유지됐습니다.
- 우측 물가를 임시 시작 위치로 둔 실제 Play에서 찌가 (24, 수면, 14.5)로 착수했고 Space 챔질·릴링 후 블루길을 획득했습니다. 전체 수역을 임시로 막은 조건에서는 안내 후 클릭으로 준비 상태로 돌아왔습니다.
- 재현용 위치와 장애물은 Play 종료 후 제거됐으며 저장 씬의 시작 위치는 (0, 0, 3.5)입니다. 자동화 테스트 파일은 추가하지 않았습니다.
- 근거: [120개 착수 검사](Evidence/2026-10-08/Environment/landing-surface-audit.json), [물가 낚시 결과](Evidence/2026-10-08/Environment/shore-cast-play.json), [착수 불가 안내](Evidence/2026-10-08/Environment/15-water-blocked-guidance.png).
- Windows 자동 입력으로 유효 챔질과 Esc 전환은 확인하지 못했습니다. 실제 Esc·Tab·WASD·마우스 조작과 효과음 청취를 사용자에게 요청했고, 응답 전에는 전체 acceptance를 완료로 표시하지 않습니다.
- 효과음 다섯 개의 실제 생성 파형도 확인했습니다. 모두 무음이 아니고 최대 진폭은 0.32~0.482, 비정상 수치는 없었습니다. 소스 음량 0.28/전체 음량 0.8이며 실제 스피커 청취와는 별도 확인입니다. [효과음 데이터](Evidence/2026-10-08/Environment/audio-signal-audit.json)

## 게임 화면 UI

`FishingHUD`는 Unity의 Canvas(uGUI)를 실행 중에 만들고 낚시 상태에 맞춰 화면을 갱신합니다. 별도로 Canvas 오브젝트를 손으로 만들지 않아도 됩니다. 현재 장면에서는 `FishingGameController`가 같은 `Fishing Systems` 오브젝트에 HUD를 자동으로 추가하고, 장면 생성 도구도 HUD 스크립트를 함께 연결합니다.

- 왼쪽 아래: 현재 할 일 안내, 캐스팅 힘 또는 릴 진행도·줄 장력 막대
- 화면 가운데: 조준 표시와 입질 알림 및 남은 챔질 시간
- 오른쪽 위: 물에 들어갔을 때 익사까지 남은 시간
- 낚시 결과: 성공/실패 이유, 성공한 물고기의 이름과 그림
- 사망: 물가에서 다시 시작하거나 타이틀로 돌아가는 버튼 표시

화면 크기에 맞춰 크기를 조정하는 Canvas Scaler를 사용합니다. 한국어 글꼴은 Windows의 맑은 고딕을 우선 사용합니다. 메뉴를 열면 기존 낚시 HUD를 숨겨 화면이 겹치지 않게 했습니다. 타이틀·도감·설정·조작법·사망 메뉴의 실제 화면을 확인했습니다.

## Unity에서 열고 조작하기

1. Unity Hub에서 `DeepBlue` 프로젝트 폴더를 Unity `6000.3.12f1`로 엽니다.
2. Project 창에서 `Assets/Scenes/DeepBlueFishing.unity`를 엽니다. `SampleScene`이 아니라 이 장면이 주 개발 장면입니다.
3. 에디터가 스크립트를 임포트하고 컴파일할 때까지 기다립니다. Console에 오류가 없는지 확인합니다.
4. 상단의 Play 버튼을 누르면 타이틀이 표시됩니다. **낚시 시작**을 누르면 커서가 잠기고 물가에서 플레이합니다.
5. `Escape`는 일시정지, `Tab`은 도감입니다. 메뉴 중에는 입질·익사 시간도 멈춥니다. 조작법과 설정은 타이틀 및 일시정지 메뉴에서 열 수 있습니다.
6. Play 중 Hierarchy의 `Fishing Systems` 아래에 `Deep Blue HUD Canvas`, `Deep Blue Menu Canvas`, `Deep Blue EventSystem`이 자동 생성됩니다. Play를 멈추면 사라지는 정상 동작이므로 Canvas를 직접 만들 필요가 없습니다.

| 입력 | 동작 |
| --- | --- |
| `W`, `A`, `S`, `D` | 바라보는 방향을 기준으로 이동 |
| 마우스 이동 | 좌우 및 상하 시점 회전 |
| `Escape` | 일시정지/계속, 하위 메뉴에서 뒤로 |
| `Tab` | 물고기 도감 열기/닫기 |
| 마우스 왼쪽 버튼 누르고 놓기 | 캐스팅 힘을 모은 뒤 찌 던지기 |
| 입질 순간 `Space` 또는 마우스 왼쪽 클릭 | 제한 시간 안에 챔질 |
| 릴링 중 마우스 왼쪽 버튼 누르기 | 물고기를 감아 올리기 |
| 결과 상태에서 마우스 왼쪽 버튼 | 다음 낚시 시작 |

## 낚시 장면의 익사 판정

물 표면을 나타내는 `Lake Water` 아래에 `Water Death Zone`이 배치되어 있습니다. 이 영역은 BoxCollider Trigger와 Kinematic Rigidbody를 사용하므로 플레이어가 물에 들어가도 즉시 멈추지 않고 감지됩니다. 플레이어 아래에는 `Lake Bed (Collision)`이 있어 물속에서 계속 떨어지지 않고 익사 카운트다운을 기다립니다.

- `DrowningController`의 기본 시간은 **10초**입니다. Player의 Inspector에서 `Seconds To Drown` 값을 바꿀 수 있습니다.
- 물 영역에 들어가면 화면에 `익사까지 n.n초`가 표시됩니다.
- 10초 전에 영역을 빠져나오면 카운트다운이 처음부터 다시 시작됩니다.
- 시간이 끝나면 이동, 시점, 낚시 입력이 멈추고 `물가로 돌아갈 시간` 재시작 화면이 표시됩니다.
- 사망 화면의 **물가에서 다시 시작** 버튼은 낚시와 플레이어를 초기화합니다. 도감과 설정은 유지합니다.

씬 생성 도구도 같은 구성을 만들도록 수정했습니다. `Assets/Editor/DeepBlueSceneBuilder.cs`를 다시 실행해 장면을 만들면 익사 판정 영역과 Player 연결도 함께 생성됩니다.

## 주요 코드가 하는 일

- [FirstPersonLook.cs](Assets/Scripts/FirstPersonLook.cs): 마우스 시점 회전과 감도 적용을 처리합니다. 메뉴가 커서 잠금 상태를 전달합니다.
- [FirstPersonMovement.cs](Assets/Scripts/FirstPersonMovement.cs): WASD 입력, 이동 속도, 중력을 처리합니다.
- [FishingGameController.cs](Assets/Scripts/FishingGameController.cs): 캐스팅 충전, 입질 대기, 챔질, 릴링 장력, 성공·실패와 반복 낚시 상태를 처리합니다.
- [FishingHUD.cs](Assets/Scripts/FishingHUD.cs): 상태 안내, 캐스팅/릴링 막대, 입질, 결과, 익사 시간을 Canvas UI로 표시합니다.
- [DeepBlueMenus.cs](Assets/Scripts/DeepBlueMenus.cs): 타이틀·일시정지·설정·조작법·도감·재시작·종료 확인창을 만들고 메뉴 중 게임 입력을 차단합니다.
- [DeepBlueProfile.cs](Assets/Scripts/DeepBlueProfile.cs): 음량·감도·물고기 획득 횟수를 이 PC에 자동 저장합니다. 낚시 도중 위치나 찌 상태는 저장하지 않습니다.
- [DrowningController.cs](Assets/Scripts/DrowningController.cs): 물속 시간을 누적하고, 시간이 끝나면 플레이어와 낚시 조작을 멈춥니다.
- [WaterDeathZone.cs](Assets/Scripts/WaterDeathZone.cs): Trigger 진입·이탈을 Player의 익사 판정에 전달합니다.
- [DeepBlueSceneBuilder.cs](Assets/Editor/DeepBlueSceneBuilder.cs): 기본 낚시 장면을 만드는 Unity Editor 도구입니다. 환경 이미지가 있으면 새 환경도 함께 연결합니다.
- [DeepBlueEnvironmentBuilder.cs](Assets/Editor/DeepBlueEnvironmentBuilder.cs): 3D 지형·구조물·숲과 재질·조명을 만들고 씬을 저장하는 Editor 도구입니다.

Unity의 `CharacterController`는 플레이어 캡슐과 바닥 충돌을 처리하고, 물의 BoxCollider는 `Is Trigger`가 켜져 있어 물리적으로 막지 않고 진입만 감지합니다. Trigger 이벤트에는 물 영역 쪽 Kinematic Rigidbody가 필요합니다.

## 폴더 구성

```text
Assets/
  Art/                         물 텍스처, UI 아이콘, 재질
  Editor/DeepBlueSceneBuilder.cs  기본 낚시 장면 생성 도구
  Scenes/
    DeepBlueFishing.unity      주 개발 장면
    SampleScene.unity          원본 장면 보존
  Scripts/
    FirstPersonLook.cs         1인칭 시점
    FirstPersonMovement.cs     WASD 이동과 중력
    FishingGameController.cs   낚시 진행 상태와 UI용 상태 정보
    FishingHUD.cs              Canvas 기반 게임 화면 HUD
    DrowningController.cs      익사 시간 및 사망 처리
    WaterDeathZone.cs          물 Trigger 감지
Packages/manifest.json         프로젝트 패키지 목록
ProjectSettings/               Unity 버전과 프로젝트 설정
```

## 개발 로드맵

**2026년 10월 18일까지 플레이 가능한 게임 완성**을 목표로 일정을 앞당겼습니다. 필요한 UI를 우선 구현하고, Windows 빌드 acceptance(완료 기준)와 마지막 이틀의 수정 시간을 확보합니다. 아래 일정은 개발 목표이며 실제 검증 결과에 따라 완료를 판단합니다.

| 목표 기간 | 단계 | 완료 기준 |
| --- | --- | --- |
| 10월 8일~11일 | 타이틀·필수 UI | 시작, 일시정지, 설정, 조작법, 도감, 사망 후 재시작 |
| 10월 12일~14일 | 코어 루프·화면 완성도 | 성공·실패·익사, 해상도별 HUD, 효과음과 밸런스 |
| 10월 15일~16일 | Windows 빌드 전체 검증 | FR-01~FR-20 확인 및 발견한 문제 수정 |
| 10월 17일~18일 | 최종 수정 및 전달 | 재검증, 실행 파일과 초보자용 문서 정리 |

**최종 완료 목표일: 2026년 10월 18일(일요일).** Steam 조사는 UI 참고용이며, Steam 상점 출시 절차는 이번 개발 일정에 포함하지 않습니다.

## 타이틀과 UI 참고 조사 — 2026-10-08

Steam 공식 상점 설명을 읽고 Deep Blue의 작은 싱글플레이 범위에 맞게 아래 기능을 선정했습니다. 적용 UI는 이 프로젝트를 위한 설계 판단이며 각 게임의 화면을 그대로 재현한 것이 아닙니다.

| 참고 게임 | 상점에서 확인한 특징 | Deep Blue에 적용한 방향 |
| --- | --- | --- |
| [Ultimate Fishing Simulator](https://store.steampowered.com/app/468920/Ultimate_Fishing_Simulator/) | 여러 낚시 기법, 난이도 선택, 낚은 물고기 보상 | 캐스팅·챔질·릴링을 설명하는 조작법, 상태별 HUD와 결과 안내 |
| [Call of the Wild: The Angler](https://store.steampowered.com/app/1408610/Call_of_the_Wild_The_Angler/) | 자연 속 탐험, 다양한 어종, 싱글플레이 낚시 | 호수 풍경 타이틀, 낚시 시작에 집중한 메뉴, 시점 감도 설정 |
| [WEBFISHING](https://store.steampowered.com/app/3146520/WEBFISHING/) | 편안한 낚시와 생물 수집, 도감 완성 | 기존 여섯 어종의 발견 여부와 획득 횟수를 기록하는 도감 |

### 저장과 연결 방법

- `Fishing Systems`의 `DeepBlueMenus`에 타이틀 배경이 연결되어 있습니다. Play를 누르면 바로 표시됩니다.
- 음량과 감도는 슬라이더로 바꾸며 메뉴를 나갈 때 저장합니다. 획득 기록은 낚시 성공 직후 저장합니다. 재시작과 타이틀 이동으로 지워지지 않습니다.
- 저장은 Unity `PlayerPrefs`를 사용하며 설정과 도감만 포함합니다. 테스트용 Editor 기록과 Windows 게임 기록은 별도일 수 있습니다.
- 새 배경 경로: `Assets/Art/UI/Title/DeepBlue_TitleLake.png`. 기존 여섯 물고기 그림도 도감에서 재사용합니다.

### 타이틀 배경 제작 기록

내장 `imagegen`으로 생성했고 다음 프롬프트를 사용했습니다.

> Use case: stylized-concept. Asset type: original background art for the title screen of Deep Blue, a calm first-person lake fishing game. Wide cinematic landscape 16:9 composition. A tranquil deep turquoise freshwater lake at blue-hour dawn, layered misty pine-covered mountains, warm soft sunrise on the upper right, a small weathered wooden fishing dock and simple fishing rod in the right foreground, delicate water ripples and distant tree silhouettes. Premium painterly 3D game key art, serene atmospheric light, rich deep navy and teal with restrained warm gold highlights. Leave the left 45 percent mostly dark quiet lake and mist with low detail, suitable for a readable title and vertical menu overlay. No humans, no boats, no fish out of water. No typography, no letters, no logos, no buttons, no UI, no watermark. Original art, do not imitate an existing game's exact composition.

### 추가 플레이 검증 — 카메라와 HUD

- Windows 실행 파일에서도 마우스 캐스팅, 입질 알림, 시간 초과 로그와 클릭 후 준비 복귀를 추가 확인했습니다. 자동 Esc 입력은 화면 전환이 관찰되지 않았으며 실제 키보드 확인을 기다립니다. [Windows 관찰 기록](Evidence/2026-10-08/Menus/windows-pointer-observations.json)
- Editor Input System의 마우스·키보드 이벤트로 확인했습니다. 게임 난이도나 낚시 상태를 강제로 변경하지 않았으며 Windows 실제 키보드 검증과는 구분합니다.
- 카메라 좌우 회전과 위아래 ±80도 제한을 확인했습니다. [카메라 결과](Evidence/2026-10-08/Menus/camera-limits.json)
- 1024×768에서 준비·충전·입질·위험 장력·두 실패 화면·익사 표시를 확인했습니다. 한국어 문구와 진행 막대가 패널 안에 들어갑니다. [입질 화면](Evidence/2026-10-08/Menus/18-bite-4x3.png), [위험 장력](Evidence/2026-10-08/Menus/20-reeling-danger-4x3.png), [사망 메뉴](Evidence/2026-10-08/Menus/25-death-4x3.png)
- 최신 메뉴를 적용한 동일 Play 세션에서 시간 초과→줄 끊김→성공을 연속으로 진행했습니다. 1280×720 성공 카드에도 채널메기의 이름과 그림이 짝 맞게 표시됩니다. [세 번째 낚시 결과](Evidence/2026-10-08/Menus/hud-third-cast-catch-720p.json), [성공 화면](Evidence/2026-10-08/Menus/23-catch-720p.png)
- 기존 채널메기 1회 기록은 두 실패 동안 유지됐고, 다음 성공 때 2회로 증가했습니다. 도감을 다시 열거나 사망해도 추가 증가하지 않았습니다.
- 검증 후 Editor Play를 종료하고 원래 1080p Game 뷰와 저장된 장면으로 복구했습니다. 이번 확인으로 게임 코드나 Windows 실행 파일이 바뀌지는 않았습니다.

## 학습 및 문서 원칙

각 새 기능은 코드 파일, 연결할 GameObject, Inspector 설정, Play 모드에서 확인할 방법 순서로 설명합니다. 씬 파일에 연결된 것과 실제 실행을 확인한 것은 따로 기록합니다. 명세 변경은 `SPEC.md`와 `TASKS.md`에도 반영합니다.

## 개발 기록

### 2026-09-28~29

- Unity 프로젝트 설정과 1인칭 시점·이동 스크립트를 준비했습니다.
- 게임 범위 명세서 `SPEC.md`와 실행 작업 문서 `TASKS.md`를 만들었습니다.

### 2026-09-30

- `DeepBlueFishing.unity` 장면에 땅, 물, 부두, 낚시 도구와 플레이어를 구성했습니다. 원본 `SampleScene`은 유지합니다.
- Player 이동·시점 및 FishingGameController 참조를 장면에 연결했습니다.
- `DrowningController`와 `WaterDeathZone`을 추가하고 주 장면의 Player와 수역에 연결했습니다.
- 물속에서 바닥에 멈춰 카운트다운을 기다릴 수 있도록 `Lake Bed (Collision)`을 추가했습니다.
- 사망하면 Player뿐 아니라 별도 `Fishing Systems` 오브젝트의 낚시 입력도 정지하도록 연결했습니다.
- Canvas 기반 `FishingHUD`를 추가해 조작 안내, 캐스팅/릴링 진행도와 장력, 입질 카운트다운, 물고기 결과, 익사 타이머와 사망 화면을 표시하도록 했습니다.
- 이번 변경 이후 Unity Editor 임포트와 Play 모드 확인은 남아 있습니다.

### 2026-10-01

- Unity Scene/Game 뷰에서 물가·부두·낚시 도구와 한국어 준비 HUD를 확인했습니다.
- Play 모드에서 캐스팅, 낚싯줄과 찌 표시, 입질 시간 초과 실패, 클릭 후 준비 상태 복귀를 확인했습니다. 입질 알림이 보일 때 Space 입력도 시도했지만 Console에 시간 초과가 기록됐습니다. Unity Game 뷰의 키보드 포커스 문제인지 게임 입력 코드 문제인지는 아직 분리해 확인하지 못했으며, 유효 챔질과 릴링 성공은 미확인입니다.
- Game 뷰의 첫 마우스 입력에서 커서를 잠그고 잠금 직후 0.12초를 무시하도록 `FirstPersonLook`을 보완했습니다. Unity 재임포트 뒤 첫 포커스 전환에서 큰 시점 점프가 보이지 않았습니다. 지속적인 시점 회전과 WASD 이동·충돌은 확인이 남았습니다.
- 최신 코드로 Unity Editor의 Windows x64 빌드를 다시 완료했습니다. `Builds/Windows/Deep Blue.exe`는 124.9MB입니다. 이전 실행 프로세스는 응답했지만 화면 제어 도구가 게임 창을 제공하지 않아 최신 빌드의 화면 검토는 이어서 합니다.
- 완료 목표를 2026년 10월 31일로 조정하고 남은 검증을 주간 일정으로 다시 나눴습니다.
- 2초 제한에서 Space 챔질 시간 초과가 반복되어 초보자에게 반응 시간을 주도록 5초로 늘렸습니다. 기존 장면에 저장된 2초 값이 코드 기본값을 덮는 것도 발견해 장면 파일과 생성 도구를 함께 수정했습니다. Unity 재임포트 뒤 새 시간 표시와 챔질 성공을 확인합니다.
- 재임포트한 장면에서 입질 알림의 5초 카운트다운과 시간 초과 실패를 확인했습니다. Game 뷰가 포커스됐는데도 자동 Space 입력이 컨트롤러까지 도달하지 않아, 기존 Space 조작을 유지하고 마우스 왼쪽 클릭도 챔질로 받도록 추가했습니다. HUD에도 두 입력을 안내합니다. 재임포트 뒤 왼쪽 클릭으로 릴링과 성공 결과를 검증하고, Space 실제 입력은 따로 확인합니다.
- 이어진 Play 모드 확인에서 왼쪽 클릭 챔질이 릴링 상태로 전환되고 릴 진행도가 증가하는 것을 확인했습니다. 물고기 획득과 줄 끊김 결과까지의 전체 릴링은 아직 검증 중입니다.

### 2026-10-08

- 최종 완료 목표일을 **2026년 10월 31일**로 명시하고, 남은 개발 일정을 10월 8일~31일로 다시 정리했습니다.
- 10월 28일까지 Windows 빌드 전체 acceptance와 문제 수정을 진행하고, 10월 29일~31일에는 수정 후 재검증과 완료 문서 정리를 마칩니다.
- 이후 사용자의 다음 주 완성 요청에 따라 목표를 **10월 18일**로 앞당겼습니다. 위 개발 로드맵이 최신 일정입니다.
- Steam 세 게임을 조사하고 타이틀·일시정지·설정·조작법·도감·사망 후 재시작 UI를 추가했습니다. 배경은 내장 imagegen으로 제작했습니다.
- 설정 슬라이더가 패널 밖으로 늘어나는 문제를 고쳤고, 입력 차단·일시정지·성공 기록·익사 후 재시작을 Editor에서 확인했습니다.
- Windows 빌드 성공: 오류 0건, 경고 2건. 경고는 Player 원격 제어 미설정과 Unity Cloud 디버그 심벌 업로드 생략에 관한 안내입니다. 실행 화면과 메뉴 버튼은 동작했습니다.
- 검증 자료: [타이틀 화면](Evidence/2026-10-08/Menus/01-title.png), [도감 획득 화면](Evidence/2026-10-08/Menus/12-journal-caught.png), [입질 정지·성공 기록](Evidence/2026-10-08/Menus/pause-catch.json), [익사·재시작 기록](Evidence/2026-10-08/Menus/death-retry.json), [빌드 결과](Evidence/2026-10-08/Menus/build-result.json).
