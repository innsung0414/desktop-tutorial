# Free animated fish models

> 보관용 출처 기록입니다. Quaternius CC0 fish pack은 이전 47종 구현에 사용됐으며, 2026-10-17 Fish Alive 무료 샘플 두 어종을 활성 게임 콘텐츠로 교체하면서 Models, Controllers, Prefabs를 Assets/Archive/QuaterniusFish로 옮겼습니다. 이 자료는 현재 런타임 Resources나 전달 빌드에 사용하지 않습니다. 과거 어종 대응표는 당시 구현 이력입니다.

- Creator: Quaternius
- Bundle: Animated Cute Fish Pack (52 FBX assets total, including 35 fish models)
- Source: https://poly.pizza/bundle/Animated-Fish-Bundle-44zhHN1UbT
- Creator's pack and license page: https://quaternius.com/packs/cutefish.html
- License: CC0 1.0 / public domain
- Downloaded: 2026-10-17
- Imported fish models (35 of 35 fish in the source bundle): Anglerfish, Armored Catfish, Betta, Black Lion Fish, Blobfish, Blue Goldfish, Blue Tang, Butterfly Fish, Cardinal Fish, Clownfish, Coral Grouper, Cowfish, Flatfish, Flower Horn, Goblin Shark, Goldfish, Humphead, Koi, Lionfish, Mandarin Fish, Moorish Idol, Parrot Fish, Piranha, Puffer, Red Snapper, Royal Gramma, Shark, Sunfish, Swordfish, Tang, Tetra, Tuna, Turbot, Yellow Tang, Zebra Clown Fish.
- Source archive audit: the 52 FBX files consist of these 35 fish, 16 equipment/environment props (boat, four dock pieces, five fishing rods, and six lures), and one worm bait model. The worm is not counted as a fish. Project-to-archive filename comparison found no missing or extra fish models.

The creator page lists the pack as CC0/public domain and free for personal and commercial projects. The imported FBX files include their mesh, materials, and animation data. Unity generates a catch reaction state (preferring the out-of-water clip) that transitions into a looping swim state, with one controller and prefab per fish. The result-screen showcase adds a small procedural wave across the shared six-bone spine after Animator evaluation, so the tail moves most. Journal icons come from each model preview. No paid animation plugin is required.

## In-game coverage

All 35 new reward identities (`Fish_Asset_*`) load their same-named imported prefab. The 12 older reward IDs, names, icons, and saved discovery keys stay compatible and now resolve to these imported animated models as visual stand-ins:

| Existing reward ID | Free animated model |
| --- | --- |
| `Trout_CatchIcon` | `Tuna` |
| `Fish_Bass`, `Fish_YellowPerch`, `Fish_Walleye` | `CoralGrouper` |
| `Fish_Pike` | `Swordfish` |
| `Fish_Bluegill`, `Fish_BlackCrappie` | `Sunfish` |
| `Fish_Carp` | `Koi` |
| `Fish_Catfish`, `Fish_Burbot` | `ArmoredCatfish` |
| `Fish_BrookTrout` | `RedSnapper` |
| `Fish_LakeSturgeon` | `Shark` |

These aliases let the full 47-entry reward list use animated 3D assets without changing old save IDs. They are stylized visual stand-ins, not a claim that the model and legacy fish species are biologically identical. Full runtime appearance checks for every species and screen ratio remain part of DB-038.
