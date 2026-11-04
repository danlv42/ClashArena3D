# Pixel Quest — 2D platformer prototype

A fresh Unity 2D platformer prototype. The first build is generated from code at runtime, so the scene does not need manually placed objects or imported art assets.

## Engine
- Unity 2022.3 LTS
- C#
- 2D physics
- Procedural placeholder pixel-art sprites

## Open the project
1. Install Unity Hub and Unity Editor 2022.3 LTS.
2. In Unity Hub, choose **Add project from disk** and select this repository folder.
3. Open the project and wait for scripts to compile.
4. Open **Assets/Scenes/Level01.unity** and press **Play**. The prototype creates its level, player, enemies, camera and HUD automatically.

## Controls
- **A / D** or **Left / Right** — move
- **Space**, **W** or **Up** — jump
- **J** or **Z** — ordinary melee attack
- **K** or **X** — special ability

## Kill Charge ability
- Defeat 10 enemies to charge the special ability.
- The ability is not recharged by time.
- Press K or X when ready to unleash an area burst.
- Using the ability spends 10 kills; kills made by the burst count toward the next charge.

## Prototype scope
This is the first playable mechanics prototype, not a finished game. It currently uses procedural placeholder art and one test level. Character selection, alternate art styles, polished animations, sound, level progression and bosses are planned for later iterations.

## Build for Windows
In Unity, open **File → Build Settings**, select **Windows**, add the active scene to Scenes In Build, then choose **Build**.
