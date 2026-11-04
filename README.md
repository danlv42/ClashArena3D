# ClashArena3D

A small original 3D lane-battler prototype inspired by the readable, toy-like presentation of mobile arena battlers. No Clash Royale assets, characters, logos, or proprietary code are included.

## Requirements
- Unity 2022.3 LTS or newer (3D Core template)
- Windows 10/11 for a standalone build

## Open
1. Clone/download this repository.
2. In Unity Hub choose **Add > Add project from disk** and select this folder.
3. Open `Assets/Scenes/Arena.unity`.
4. Press Play.

The scene and arena are generated using built-in primitives; no external assets are required.

## Controls
- Select one of the four cards at the bottom.
- Click the friendly side of the arena to deploy that unit.
- Units advance and attack enemy troops or towers.
- Elixir regenerates over time.
- Press **R** to restart.

## Build Windows executable
In Unity choose **File > Build Settings > Windows, Mac, Linux > Windows > x86_64 > Build**. Select an output folder such as `Build/ClashArena3D.exe`.

The executable is not checked in because it must be built for the target platform using Unity Editor. The repository contains project source.

## Structure
- `Assets/Scripts/ArenaBootstrap.cs` — generated arena, camera, UI setup
- `Assets/Scripts/BattleGame.cs` — towers, units, deployment, combat and elixir
- `Assets/Scenes/Arena.unity` — startup scene
- `ProjectSettings/ProjectVersion.txt` — editor version
