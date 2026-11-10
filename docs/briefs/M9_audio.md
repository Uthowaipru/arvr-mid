# Brief — M9 Audio

**Role:** all sound in the game. A new module; nothing else needs to change for it to work.

## Owns (may edit)
`Scripts/Audio/**` (new folder). Everything else is read-only.

## First steps (agreed with M0)
1. Create `Scripts/Audio/RC.Audio.asmdef` (name `RC.Audio`, root namespace `VehicleSystem`, references `RC.Core`, `RC.Vehicle`, `RC.World`, `RC.Match`; Auto Referenced on) **in the same commit as the first script**: an asmdef with no scripts gives a warning.
2. Ask M0 (change request) to add `RC.Audio` to the references of `RC.UI`, `RC.Game` and `RC.Editor` once it compiles.

## May read / call
Core, Vehicle, World, Match, through **public events and read-only members only**: `VehicleImpactSensor.Impact`, `VehicleHealth.Damaged/Destroyed/Respawned`, `VehicleBoost.BoostStarted/BoostEnded`, `VehicleRecovery.Recovered`, `Pickup.Collected`, `TrafficVehicle.Knocked`, `WrongWayTracker.WrongWayChanged`, `MatchManager.StateChanged/PointsAwarded/Finished`, `VehicleController.ForwardSpeed/IsGrounded/HandbrakeAmount`, `VehicleController.All` / `VehicleEnabled`.

## Must not
Change any game state or edit another module's files. Reference UI or Game. Add a Unity `AudioListener` if the scene already has one. Block the main thread loading clips (use import settings below).
If you need an event that does not exist, write a change request; do not poll with `FindObject…` per frame.

## Design
- **`AudioBank`** (ScriptableObject): entries `id`, `clips[]` (random pick), `volume`, `pitchMin/pitchMax`, `spatial`, `maxVoices`, `cooldown`. Edited in the Inspector, no code change to add a sound.
- **`AudioManager`**: pooled `AudioSource`s (e.g. 16 voices), `Play(id)`, `Play(id, position)`, volumes (master, sfx, music, engine) and `Muted`, saved in PlayerPrefs under `VehicleSystem.Audio.*`, event `VolumesChanged`.
- **`AudioBootstrap`**: `[RuntimeInitializeOnLoadMethod]` creates the manager if missing and adds `VehicleAudio` to each car through `VehicleController.VehicleEnabled`, like the rendering integration does for drift effects. `MatchAudio` and `WorldAudio` subscribe to match and world events.
- **Engine loop** per car: pitch and volume from `ForwardSpeed` (and a lower, simpler version for bots; far bots can be silent to save voices). The local player's car is non-spatial; others are 3D with a short range.
- **Mobile rules:** short effects as mono, 22–44 kHz, Vorbis, *Decompress On Load*; music *Streaming*; loops *Compressed In Memory*; a hard voice limit; no per-frame allocations; no `AudioSource` created at runtime during play (pool it).

## Public surface you promise to keep
`CONTRACTS.md` §K. Sound ids are permanent strings; keep the list in `Audio/AUDIO_IDS.md`.

## Possible next work (not decided)
Music per chapter; ducking music under big events; a volume sliders panel (UI conversation hosts it, you provide `AudioManager` properties); licensing notes for every clip in `Audio/CREDITS.md`.

## Delivery
Changed files only with `.meta`, a note of what was **not** compiled/tested, a `CHANGELOG.md` line.

## Test checklist
1. No errors; all scenes open. 2. Engine pitch rises with speed; stops with the car. 3. Impact sound differs for light and hard hits. 4. Boost start/end, pickup, traffic knocked, destroyed, respawn each play once.
5. Countdown ticks, match finish jingle, points chime. 6. Never more than the voice limit playing; no clicks or hitches on a low-end phone (profile it). 7. Mute and volumes persist after restart.
8. Removing `RC.Audio` from the project does not break any other module's compile.
