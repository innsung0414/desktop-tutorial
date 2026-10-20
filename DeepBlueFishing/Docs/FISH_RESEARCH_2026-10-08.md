# Steam 낚시 게임 조사와 추가 어종 선정

조사일: 2026-10-08. 사용자 요청에 따라 기존 6종에서 12종으로 확장합니다.

## 참고한 게임

| 게임 | 공식 소개에서 확인한 내용 | Deep Blue에 적용할 판단 |
| --- | --- | --- |
| [Fishing Planet](https://store.steampowered.com/app/380600/Fishing_Planet/) | 1인칭 낚시와 다양한 담수·해수 어종을 제공하며 어종별 행동과 환경 조건을 설명합니다. | 현재 호숫가에 어울리는 담수 어종을 추가하고 이름·그림을 명확히 구분합니다. |
| [Call of the Wild: The Angler](https://store.steampowered.com/app/1408610/Call_of_the_Wild_The_Angler/) | 자연 속 탐색과 여러 어종 낚시를 강조합니다. [개발사 Evolution Update](https://cotwtheangler.com/news/evolution-update-out-now/)는 블랙크래피·브룩송어·월아이 등의 추가와 버봇의 기본 지역 도입을 설명합니다. | 기존 장소에서도 새 어종을 발견하는 수집 경험을 확장합니다. |
| [Ultimate Fishing Simulator](https://store.steampowered.com/app/468920/Ultimate_Fishing_Simulator/) | [New Fish Species 확장](https://store.steampowered.com/app/1353980/Ultimate_Fishing_Simulator__New_Fish_Species/)에서 버봇 등 소형 담수어부터 대형 해수어까지 16종을 추가합니다. | 몸통 형태와 무늬 차이가 큰 어종을 함께 구성합니다. |

오른쪽 열은 조사에서 도출한 프로젝트 설계 판단입니다. 다른 게임의 이미지나 UI를 가져오지 않고 imagegen으로 새 그림을 제작합니다. 게임 규모·가격·평점의 우열을 평가한 조사는 아닙니다.

## 이번 추가 6종

| 게임 표시 이름 | 영문 / 학명 | 작은 도감 카드에서 구분할 특징 | 선정 근거 |
| --- | --- | --- | --- |
| 옐로퍼치 | Yellow perch / Perca flavescens | 금색 몸통과 세로 줄무늬, 주황색 아래 지느러미 | 기존 배스·블루길과 색상 차이. Fishing Planet의 [Saint-Croix Lake 소개](https://wiki.fishingplanet.com/Saint-Croix_Lake_-_Michigan)에 등장하는 호수 어종 |
| 월아이 | Walleye / Sander vitreus | 긴 몸통, 큰 눈, 나뉜 등지느러미 | The Angler 확장 사례. 기존 강꼬치고기와 다른 포식어 실루엣 |
| 브룩송어 | Brook trout / Salvelinus fontinalis | 주황색 배, 작은 붉은 반점, 지느러미 흰 테두리 | The Angler 확장 사례. 기존 무지개송어와 색·무늬를 구별 |
| 레이크철갑상어 | Lake sturgeon / Acipenser fulvescens | 긴 주둥이, 몸 옆 골판, 비대칭 꼬리 | Fishing Planet의 위 호수 목록에 등장. 기존 6종과 확연히 다른 외형 |
| 블랙크래피 | Black crappie / Pomoxis nigromaculatus | 은색의 넓은 몸과 불규칙 검은 얼룩 | The Angler 확장 사례. 작은 어종의 수집 다양성 |
| 버봇 | Burbot / Lota lota | 길쭉한 몸, 턱수염 1개, 낮고 긴 등지느러미 | The Angler 및 Ultimate Fishing Simulator 확장 사례. 기존 채널메기와 다른 실루엣 |

그림은 게임용 반사실적 일러스트입니다. 생물학적 도판이나 실제 서식지의 엄밀한 생태 재현으로 사용하지 않습니다.

## 구현 범위

- 원래 6종 뒤에 새 6종을 추가합니다. 원래 Sprite 이름과 저장 키를 유지합니다.
- 기존 낚시 성공 시 무작위 보상 선택에 12종을 연결합니다. 종별 출현 가중치·가격·몸무게·전용 전투 규칙은 이번 추가 범위에 없습니다.
- 도감은 한 페이지에 6장씩 표시하고 이전·다음 버튼으로 총 2페이지를 이동합니다. 발견/획득 합계는 12종 전체를 집계합니다.
- 이미지 원본: `Assets/Art/UI/Fish_*.png`. 새 6종의 전체 생성 프롬프트와 파일 매핑은 `Assets/Art/UI/FishExpansionManifest.json`에 저장합니다.
- 생성 방식: 내장 image_gen, 각 어종 개별 생성, transparent_background=true. 1536×1024 PNG 6개를 프로젝트에 복사했습니다.

검증 상태는 TASKS의 DB-034 및 README의 최신 어종 확장 기록을 따릅니다.
