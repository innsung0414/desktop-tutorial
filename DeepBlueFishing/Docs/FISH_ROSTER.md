# Deep Blue 활성 물고기 에셋 목록

**기준일:** 2026-10-17  
**활성 어종:** 37종  
**목록 원본:** `Assets/Scripts/DeepBlueFishDetails.cs`의 `CurrentSpeciesIds`

이 표는 현재 낚시 보상·도감·3D 조과 연출에 연결된 물고기 목록입니다. 저장 장면 `Assets/Scenes/DeepBlueFishing.unity`의 `fishRewards`도 같은 37개 항목을 사용합니다.

## 에셋 연결

- Quaternius 35종: 모델 `Assets/Resources/DeepBlueFish/Prefabs/<모델명>.prefab`, 아이콘 `Assets/Art/UI/Quaternius/Fish_Asset_<ID>.png`
- Fish Alive 구피·흰동가리 2종: 모델 `Assets/Resources/DeepBlueFish/FishAlive/Guppy.prefab`, `Clownfish.prefab`; 아이콘 `Assets/Art/UI/FishAlive/` 아래 같은 ID의 PNG
- Fish Alive 흰동가리와 Quaternius 흰동가리는 서로 다른 모델 외형이므로 도감에서 별도 수집 항목입니다.
- 이전 12종의 이미지와 저장 ID는 예전 기록 호환용으로 보존됩니다. 현재 낚시 보상 목록에서는 제외되어 있으며, 보관 중인 이전 물고기는 상점에서 판매할 수 있습니다.

| # | 도감 이름 | 원본 ID | 3D 프리팹 파일 |
| ---: | --- | --- | --- |
| 1 | 구피 | `FishAlive_Guppy_CatchIcon` | `FishAlive/Guppy.prefab` |
| 2 | 베타 | `Fish_Asset_Betta` | `Prefabs/Betta.prefab` |
| 3 | 푸른 금붕어 | `Fish_Asset_BlueGoldfish` | `Prefabs/BlueGoldfish.prefab` |
| 4 | 블루탱 | `Fish_Asset_BlueTang` | `Prefabs/BlueTang.prefab` |
| 5 | 나비고기 | `Fish_Asset_ButterflyFish` | `Prefabs/ButterflyFish.prefab` |
| 6 | 카디널피시 | `Fish_Asset_CardinalFish` | `Prefabs/CardinalFish.prefab` |
| 7 | 금붕어 | `Fish_Asset_Goldfish` | `Prefabs/Goldfish.prefab` |
| 8 | 만다린피시 | `Fish_Asset_MandarinFish` | `Prefabs/MandarinFish.prefab` |
| 9 | 무어리시 아이돌 | `Fish_Asset_MoorishIdol` | `Prefabs/MoorishIdol.prefab` |
| 10 | 로열그라마 | `Fish_Asset_RoyalGramma` | `Prefabs/RoyalGramma.prefab` |
| 11 | 탱 | `Fish_Asset_Tang` | `Prefabs/Tang.prefab` |
| 12 | 테트라 | `Fish_Asset_Tetra` | `Prefabs/Tetra.prefab` |
| 13 | 옐로탱 | `Fish_Asset_YellowTang` | `Prefabs/YellowTang.prefab` |
| 14 | 제브라 흰동가리 | `Fish_Asset_ZebraClownFish` | `Prefabs/ZebraClownFish.prefab` |
| 15 | 흰동가리 (클래식) | `Fish_Asset_Clownfish` | `Prefabs/Clownfish.prefab` |
| 16 | 흰동가리 (Fish Alive) | `FishAlive_Clownfish_CatchIcon` | `FishAlive/Clownfish.prefab` |
| 17 | 갑옷메기 | `Fish_Asset_ArmoredCatfish` | `Prefabs/ArmoredCatfish.prefab` |
| 18 | 검은쏠배감펭 | `Fish_Asset_BlackLionFish` | `Prefabs/BlackLionFish.prefab` |
| 19 | 뿔복 | `Fish_Asset_Cowfish` | `Prefabs/Cowfish.prefab` |
| 20 | 가자미 | `Fish_Asset_Flatfish` | `Prefabs/Flatfish.prefab` |
| 21 | 플라워혼 | `Fish_Asset_FlowerHorn` | `Prefabs/FlowerHorn.prefab` |
| 22 | 쏠배감펭 | `Fish_Asset_Lionfish` | `Prefabs/Lionfish.prefab` |
| 23 | 피라냐 | `Fish_Asset_Piranha` | `Prefabs/Piranha.prefab` |
| 24 | 복어 | `Fish_Asset_Puffer` | `Prefabs/Puffer.prefab` |
| 25 | 아귀 | `Fish_Asset_Anglerfish` | `Prefabs/Anglerfish.prefab` |
| 26 | 블롭피시 | `Fish_Asset_Blobfish` | `Prefabs/Blobfish.prefab` |
| 27 | 산호그루퍼 | `Fish_Asset_CoralGrouper` | `Prefabs/CoralGrouper.prefab` |
| 28 | 비단잉어 | `Fish_Asset_Koi` | `Prefabs/Koi.prefab` |
| 29 | 앵무고기 | `Fish_Asset_ParrotFish` | `Prefabs/ParrotFish.prefab` |
| 30 | 붉은도미 | `Fish_Asset_RedSnapper` | `Prefabs/RedSnapper.prefab` |
| 31 | 넙치 | `Fish_Asset_Turbot` | `Prefabs/Turbot.prefab` |
| 32 | 고블린상어 | `Fish_Asset_GoblinShark` | `Prefabs/GoblinShark.prefab` |
| 33 | 혹돔 | `Fish_Asset_Humphead` | `Prefabs/Humphead.prefab` |
| 34 | 상어 | `Fish_Asset_Shark` | `Prefabs/Shark.prefab` |
| 35 | 개복치 | `Fish_Asset_Sunfish` | `Prefabs/Sunfish.prefab` |
| 36 | 황새치 | `Fish_Asset_Swordfish` | `Prefabs/Swordfish.prefab` |
| 37 | 참치 | `Fish_Asset_Tuna` | `Prefabs/Tuna.prefab` |

## 수집·밸런스 구성

- 도감은 6종씩 7페이지에 배치합니다. 목록은 자주 만나는 작은 어종, 중형·희귀 어종, 대형 어종의 흐름으로 정리했습니다.
- 개체 무게 범위, 출현 가중치, 희귀도 표시는 `DeepBlueFishDetails`의 게임용 값으로 관리합니다. 실제 생태 분포나 세계 기록을 뜻하지 않습니다.
- 첫 발견 어종의 출현 보정은 기본 가중치의 4배입니다. 수집 목표는 서로 다른 3종 → 19종 → 30종 → 37종으로 이어집니다.
- 판매 후에도 도감 발견 횟수와 최고 무게는 유지됩니다. 저장 이전의 12종 데이터는 활성 37종과 분리된 보관 기록으로 표시됩니다.

## 점검 결과

- 정적 파일 대조: 고유 어종 ID 37개, 저장 장면 보상 37개, 장면 아이콘 참조 37개, Resources 프리팹 37개, 연결 아이콘 파일 37개, 누락 0개.
- 기존 Unity Play Mode 확인: `DeepBlueFishShowcase.Show` 경로에서 37/37 프리팹을 불러와 Animator·애니메이션 클립·SkinnedMeshRenderer를 확인했습니다. 상세 기록은 `TASKS.md`의 DB-040과 `SPEC.md` 1.72를 참고하세요.
- 현재 진행 문서: [`SPEC.md`](../SPEC.md), [`TASKS.md`](../TASKS.md), Windows 실행본 안내는 [`README.md`](../README.md).
