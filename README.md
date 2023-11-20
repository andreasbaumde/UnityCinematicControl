# UnityCinematicControl

UnityCinematicControl is a lightweight Unity script designed to enhance your game development experience. It allows you to effortlessly capture game footage for trailers, cutscenes, or devlogs. This tool is perfect for developers looking to add cinematic flair to their games with ease and flexibility.

## Features

- **Real-Time Speed Controls**: Adjust movement and rotation speeds in real-time.
- **Fly-To Function**: Instantly move to a specific point in your scene with the press of a button.
- **Real-Time FOV Controls**: Dynamically change the field of view, perfect for creating dolly zoom effects.
- **Gamepad Input Compatibility**: Every setting and function is accessible via gamepad inputs.
- **Easing Toggle and Adjustment**: Customize the easing times and toggle easing on or off.
- **Live Camera UI**: A compact UI that displays all relevant settings and information.
- **Toggleable UI Elements**: Includes a toggleable info UI and crosshair.
- **Y-Axis Movement Restriction**: Option to restrict movement along the y-axis.
- **New Input System Utilization**: Built to work with Unity's new input system.
- **Camera Speed Modes**: Different speed modes available using modifier inputs (trigger buttons).

## Requirements

- A gamepad is required (mouse & keyboard currently not supported). Feel free to modify the script to add this support.
- Your project must have the new input system package imported and activated.

## Installation

1. Ensure you have a camera in your scene tagged as `MainCamera`.
2. Add the `CameraController` component to this game object.
3. Adjust the LayerMask field to mark the layers the fly-to function can fly to.
4. Drag the canvas prefab into your scene to use the camera UI.
5. Ensure you have TextMesh Pro in your project for the UI text elements.
6. Include the sprite for the crosshair in your project.

## Keybinds

- **Movement**: Use the left stick to move at normal speed.
- **Fly-To**: Press the left stick to activate the fly-to function.
- **Camera Rotation**: Use the right stick to look around/change camera rotation.
- **Toggle Easing**: Press the right stick to toggle easing.
- **Vertical Movement**: Use `A` to move up and `B` to move down.
- **Field of View (FOV)**: `X` to increase and `Y` to decrease FOV.
- **Speed Modifiers**: Use `LT` + left stick for slow speed (0.5x), `RT` + left stick for fast speed (2.5x).
- **Speed Adjustments**: `LB`/`RB` to decrease/increase movement speed.
- **Vertical Speed Adjustment**: `D-pad Up` + `LB`/`RB` to adjust vertical movement speed.
- **Rotation Speed Adjustment**: `D-pad Right` + `LB`/`RB` to adjust rotation speed.
- **Easing Time Adjustment**: `D-pad Down` + `LB`/`RB` to adjust easing time.
- **FOV Change Rate**: `D-pad Left` + `LB`/`RB` to adjust the rate of FOV change.
- **Y-Axis Movement Toggle**: Double-tap `D-pad Right` to toggle y-axis movement restriction.
- **UI Visibility**: Press `Select` to toggle UI visibility.
- **Reset FOV**: Press `Start` to reset FOV to its initial value.

## Contributing

Feel free to fork, modify, and send pull requests to improve this tool. Your contributions are greatly appreciated!

---

Happy game developing!
