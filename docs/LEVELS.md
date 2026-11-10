# LEVELS — offline levels design and work split

*Version 1 · 2026-10-24. Design only; nothing in this file is built. Source: GDD v0.3 §3 and §12. Contract signatures: `CONTRACTS.md` §J.*

## 1. What a level is

A level is a data asset (`LevelDefinition`) that says: which road (fixed seed), how long, how busy, who else is on the road, which scoring rules, what the player must do, and how many stars each result earns. The same match code runs it; a **`LevelRunner`** adds the objective and the stars on top.

| Field | Meaning | Default / note |
| --- | --- | --- |
| `id` | permanent string, e.g. `ch1_01` | never change after release (progress is saved by id) |
| `displayName`, `chapter`, `order` | for the level select screen | 3 chapters of 10 planned (placeholder) |
| `seed` | fixed road seed | same seed = same road, traffic, pickups |
| `timeLimitSeconds` | match length | |
| `twoWayRoad` | oncoming lanes on/off | per level |
| `trafficDensity` | multiplier on traffic | 1 = normal |
| `botCount`, `botDifficulty` | rivals in the level | 0 allowed |
| `scoringRules` | a `ScoringRules` asset | the one built in Stage 6a |
| `objective`, `objectiveTarget` | see §2 | |
| `star2Score`, `star3Score` | score needed for stars 2 and 3 | |
| `firstCompletionCoins`, `twoStarCoins`, `threeStarCoins` | coin bonuses, paid once each | 100 / 50 / 100 (GDD, tunable) |
| `unlockAfterLevelId`, `unlockMinStars` | unlock rule | empty = unlocked from the start |

## 2. Objectives (all five from the GDD)

| Type | Met when | Fails when |
| --- | --- | --- |
| `ReachScore` | score ≥ target | time runs out |
| `TravelDistance` | driven distance ≥ target (measured by the runner; no finish gate in phase 1) | time runs out |
| `CollectBatteries` | N batteries collected | time runs out |
| `SurviveWithoutDestruction` | time runs out and destroyed count is 0 | the car is destroyed |
| `HighSpeedHits` | N paid side bumps (fast or full-throttle tier) | time runs out |

**Stars:** 1 = objective met · 2 = score ≥ `star2Score` · 3 = score ≥ `star3Score`. A failed level gives 0 stars and no bonus.
**Bonus coins** are paid once per tier: first completion, first time reaching 2 stars, first time reaching 3 stars (so a first perfect clear = 250 coins).

## 3. Flow

1. **Level select** (UI) reads `LevelCatalog` and `LevelProgress` (stars per id) → shows locked/unlocked and stars. Tap sets `LevelSession.Selected` and loads the Game scene.
2. **Game scene start** (wiring, `Game/LevelApplier`): if `LevelSession.Selected` is set, apply it: road seed and `twoWayRoad`, traffic density, bots, `MatchManager.SetRules(level.scoringRules)`, match length; then `MatchManager.Begin()`. If nothing is selected (editor testing, or free play), nothing changes.
3. **During play** `LevelRunner` listens to `MatchManager` events (`PointsAwarded`, `StateChanged`), `Pickup.Collected`, `VehicleHealth.Destroyed` and the player's distance, raises `ObjectiveProgress(current, target)` for the HUD, and calls `MatchManager.Finish()` when the level ends early (objective met or failed).
4. **At the end** `LevelRunner` builds a `LevelResult` (stars, score, bonus coins using `LevelProgress.Record` → previous best), the wallet is credited, and the results flow shows stars and bonus.
5. Unlock check: a level is unlocked when `unlockAfterLevelId` is empty or `LevelProgress.GetStars(thatId) ≥ unlockMinStars`.

## 4. Who builds what

| Piece | Module | Assembly | Notes |
| --- | --- | --- | --- |
| `LevelDefinition`, `LevelCatalog`, `LevelSession`, `LevelRunner`, `LevelResult`, 3 sample levels | M5 Match | `RC.Match` | start here |
| `LevelApplier` | M5 + M0 | `RC.Game` | touches road, traffic and bots: needs their public setters (change requests to M3/M4 if missing) |
| `LevelProgress` | M7 Services | `RC.Services` | strings and ints only (Services must not reference level types) |
| Level select screen, objective line on the HUD, stars + bonus on the results screen | M6 UI | `RC.UI` | after the Match types exist |
| Finish gate in the road (visual, distance objective) | M3 World | `RC.World` | **phase 2**, optional |

Order: M5 types and runner → (M7 `LevelProgress` ∥ M0 `LevelApplier`) → M6 screens. M3's finish gate and the real 30 levels come later (GDD: level design after playtests).

## 5. Test checklist (when built)

1. A sample level starts with its seed: same road on two runs. 2. Each of the five objectives can be met and failed, and each ends the match correctly.
3. Stars: three runs reaching 1, 2 and 3 stars. 4. Bonus coins paid once; replaying does not pay again; improving from 1 to 3 stars pays the 2- and 3-star bonuses once.
5. Progress survives restarting the app. 6. A locked level cannot be opened; finishing the previous one unlocks it. 7. Free play (no selected level) still works exactly as today.
