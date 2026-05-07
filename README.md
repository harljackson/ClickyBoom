# ClickyBoom

A simple Unity clicker game where fruit targets spawn in the play area and the player clicks/taps them before they disappear.

## Gameplay
- Click or tap targets to destroy them and score points.
- The match runs on a timer.
- When time runs out, the game ends and final score is shown.

## Controls
- **Mouse**: Left click to hit targets.
- **Touch**: Tap targets on touch devices.
- **Keyboard shortcuts**:
  - `Space` starts the game from the start panel.
  - `R` restarts after game over.

## Project Structure
- `Assets/Scripts/GameManager.cs`  
  Handles game state, score/timer UI, start/restart/game-over flow, and audio.
- `Assets/Scripts/TargetSpawner.cs`  
  Spawns target prefabs in the configured area and tracks active targets.
- `Assets/Scripts/ClickTarget.cs`  
  Target behaviour (lifetime, animation, score award, destroy effects).
- `Assets/Scripts/ClickRaycaster.cs`  
  Converts pointer input into world raycasts and destroys clicked targets.
- `Assets/Scripts/PointerInput.cs`  
  Shared pointer helper supporting legacy Input + new Input System.

## Unity Setup Checklist
1. Open scene: `Assets/Scenes/ClickyBoom.unity`.
2. Ensure there is one active `GameManager` in scene.
3. Ensure there is one active `TargetSpawner` in scene with target prefabs assigned.
4. Ensure the camera used by `ClickRaycaster` is assigned (or tagged `MainCamera`).
5. Ensure target prefabs have colliders and `ClickTarget` component.
6. Ensure UI references are assigned on `GameManager` (score, timer, start panel/button, game over panel/text, restart button).

## Input Compatibility Notes
`PointerInput` supports both:
- Legacy `UnityEngine.Input` (mouse + touch)
- New Input System (`UnityEngine.InputSystem`) when enabled

This helps avoid click/tap regressions when Input handling mode changes in **Project Settings > Player > Active Input Handling**.

## Troubleshooting
If clicks are not destroying targets:
1. Confirm game is active (start panel hidden and timer counting down).
2. Confirm targets are on a layer included by `ClickRaycaster.clickableLayers`.
3. Confirm target prefab has collider (or child collider) and `ClickTarget`.
4. Confirm `ClickRaycaster` has a valid camera reference.
5. Confirm no compile errors in Console.

## Running
- Open project in Unity Hub.
- Open scene `Assets/Scenes/ClickyBoom.unity`.
- Press Play.

## License
For coursework/educational use.
