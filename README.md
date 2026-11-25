# Nexward

A top-down 2D sci-fi survival and tower-defense prototype built in Unity.

**Play in your browser:** [Nexward on Unity Play](https://play.unity.com/en/games/3477e6df-ab5b-440b-b2e4-95ad490ed4b2/finalbuildv1)

## Overview

Defend the Core against increasingly dangerous enemy attacks. Move around the arena, fight enemies yourself, collect resources, construct defensive towers, and invest in upgrades to keep the Core alive.

Nexward currently has two playable modes:

### Timed Survival

- Survive a five-minute assault.
- Enemy pressure escalates during the run, including a storm event.
- Prepare your defenses for the final SiegeBreaker boss encounter.
- Defeat the boss to complete the run.

### Endless Survival

- Fight through progressively larger enemy waves.
- Standard and faster Runner enemies create different combat pressures.
- Enemy durability scales as the run progresses.
- Every tenth wave brings a SiegeBreaker encounter. Defeating it allows the run to continue.
- The run ends when the player is down or the Core is destroyed.

## Core systems

- **Player combat:** Switch between manual aiming and automatic targeting. Improve weapon damage and unlock additional combat abilities.
- **Buildable defenses:** Construct Laser and Tesla towers. Laser towers fire projectiles; Tesla towers chain attacks across nearby enemies.
- **Tower upgrades:** Invest in Power, Speed, and Range paths to specialize each tower.
- **Core defense:** Protect the Core and develop its Dome Defense, Expansion, Regeneration, and Pulse upgrade paths.
- **Player progression:** Earn XP to level up and gain Command Points for upgrades. Combat, Survival, and Mobility paths let you shape your build.
- **Run resources and meta-progression:** Collect Scrap for construction and upgrades. Earn Research Data for persistent Lab unlocks between runs.
- **Run feedback:** HUD elements communicate run status and boss health; end-of-run screens show performance and rewards.
- **Audio and interface:** Scene-based music, gameplay sound effects, UI feedback, and pause/restart navigation.

## Controls

| Input | Action |
| --- | --- |
| WASD / Arrow Keys | Move |
| Left Mouse Button | Fire in Manual weapon mode; place a tower while placing |
| T | Toggle Manual / Automatic weapon mode |
| 1 | Select Laser Tower |
| 2 | Select Tesla Tower |
| B | Enter or cancel tower placement |
| Right Mouse Button / Esc | Cancel tower placement |
| U | Purchase an additional tower slot |
| E | Interact with a nearby tower or the Core |
| R | Open the Player Upgrade panel |
| P / Esc | Pause or resume |
| Space | Dash after unlocking the Mobility Tier 3 upgrade |

When both a tower and the Core are within interaction range, tower interaction takes priority. Some actions are available only after purchasing the relevant upgrade.

## Opening the project

1. Clone or download this repository.
2. Install **Unity 6000.4.11f1** (Unity 6).
3. In Unity Hub, choose **Add** or **Open**, then select the repository's project root — the folder containing `Assets`, `Packages`, and `ProjectSettings`.
4. Open the project and allow Unity Package Manager to resolve the listed dependencies.
5. Open `Assets/NexWard/Scenes/MainMenu.unity` and press Play.

The project build settings include the Main Menu, Endless Arena, and Survival Arena scenes.

## Project structure

```
Assets/
  NexWard/
    Audio/       Music and sound effects
    Prefabs/     Gameplay, enemy, player, tower, and UI prefabs
    Scenes/      MainMenu, EndlessArena, SurvivalArena
    Scripts/
      Combat/    Projectiles, damage interfaces, resource pickups
      Core/      Core health, dome, and upgrade systems
      Endless/   Endless wave progression and run results
      Enemies/   Enemy AI, health, and boss behaviour
      Managers/  Audio management
      Player/    Movement, combat, health, resources, upgrades
      Resources/ Persistent Research Data
      Survival/  Timed Survival progression
      Towers/    Tower placement, attacks, health, and range
      UI/        HUD, menus, and upgrade panels
Packages/        Unity package manifest and lockfile
ProjectSettings/ Unity project and build configuration
```

## Technical notes

- **Engine:** Unity 6 — 6000.4.11f1
- **Target build:** WebGL for browser play
- **Gameplay:** 2D, single-player
- **Input:** Keyboard and mouse

The playable WebGL build is hosted separately on Unity Play so the repository can remain focused on the editable Unity source project. Generated build output is not required to open the source project.

## Evaluation / asset note

This repository is shared as the source project for the Nexward prototype. Any third-party assets included in the project remain subject to their respective creators' licence terms.
