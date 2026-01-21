# RobotAbuse-0001

ForgeFX Programmer's Application - Robot Abuse Mini-Project

## Quick Start

1. Open `Assets/Scenes/MainScene.unity` in Unity 6 (6000.1.1f1)
2. Press Play in the editor

Or load the WebGL build:
1. Navigate to `Builds/WebGL/`
2. Run `python -m http.server 8000`
3. Open http://localhost:8000 in browser
4. Wait 8-10 seconds for Unity to initialize

## Features

| # | Feature | How to Test |
|---|---------|-------------|
| 1 | Camera Movement | Press WASD or arrow keys to orbit around robot |
| 2 | Torso Highlight | Hover mouse over robot body - entire robot highlights yellow |
| 3 | Robot Dragging | Click and drag on torso to move entire robot |
| 4 | Arm Highlight | Hover mouse over right arm - only arm highlights yellow |
| 5 | Arm Detach | Click and drag right arm away from body to detach |
| 6 | Arm Reattach | Drag detached arm close to shoulder to reattach |
| 7 | Status Text | Top of screen shows "Attached" or "Detached" |
| 8 | Unit Test | Run Edit Mode tests in Unity Test Runner |

## Controls

- **WASD / Arrow Keys**: Orbit camera around robot
- **Mouse Scroll**: Zoom in/out
- **Left Click + Drag on Torso**: Move entire robot
- **Left Click + Drag on Arm**: Detach arm (pull away) or reattach (move close)

## Unit Tests

Open Unity Test Runner (Window > General > Test Runner) and run Edit Mode tests:

- `ArmController_StartsAttached` - Verifies arm starts in attached state
- `ArmController_CanBeSetToDetached` - Verifies detachment logic
- `ArmController_CanBeReattached` - Verifies reattachment logic
- `ArmController_InitialStateIsCorrect` - Validates initial state consistency
- `ArmController_StateToggleWorks` - Tests state toggle behavior

## Project Structure

```
Assets/
├── Editor/
│   ├── BuildScript.cs      # WebGL build automation
│   ├── FBXValidator.cs     # Model validation tool
│   └── SceneSetup.cs       # Automated scene creation
├── Models/
│   └── Robot_Toy.fbx       # Robot model with textures
├── Scenes/
│   └── MainScene.unity     # Main game scene
├── Scripts/
│   ├── ArmController.cs    # Arm highlight, detach, reattach
│   ├── CameraController.cs # Orbital camera with WASD/scroll
│   ├── RobotController.cs  # Torso highlight and dragging
│   └── RobotStatus.cs      # Status text display (OnGUI)
└── Tests/
    └── Editor/
        └── ArmControllerTests.cs  # Edit Mode unit tests
```

## Technical Notes

- Uses OnGUI for status text (reliable in WebGL, no UI assembly dependencies)
- MaterialPropertyBlock for efficient highlight color changes
- Physics.Raycast in Update() for reliable WebGL mouse interaction
- MeshColliders auto-added to all mesh renderers for raycasting

## Requirements

- Unity 6 (6000.1.1f1)
- WebGL Build Support module

## Author

Built for ForgeFX programmer's application.
