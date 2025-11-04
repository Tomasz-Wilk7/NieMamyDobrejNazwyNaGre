# 2D Roguelike (Unity)

A minimal, modular starting point for a 2D roguelike built in Unity. This README explains purpose, how to run the project, basic gameplay expectations, and how to contribute. No implementation specifics or asset details are included.

## Overview
A procedurally generated dungeon crawler with permadeath, simple combat, and progression via items or upgrades. Designed to be extendable and easy to iterate on.

## Key Features
- Procedural dungeon generation
- Turn-based or real-time movement (configurable)
- Simple combat and enemy AI
- Item pickups, inventory, and upgrades
- Basic UI for health, items, and score/progress

## Requirements
- Unity (recommended LTS version)
- Git (for cloning and collaboration)

## Quickstart
1. Clone the repository:
    git clone <repo-url>
2. Open the project in Unity Hub and let it import assets.
3. Open the main scene (e.g., Scenes/Main.unity).
4. Press Play to run the game in the editor.

## Controls (example)
- Move: WASD or Arrow keys
- Attack / Use: Left mouse button or E
- Interact: E or Enter
- Pause/Menu: Esc

Adjust control bindings via Unity Input or the new Input System as preferred.

## Project Structure (suggested)
- Assets/
  - Scenes/        — game scenes
  - Scripts/       — gameplay, systems, managers
  - Prefabs/       — player, enemies, items, tiles
  - Art/           — sprites and UI elements
  - Audio/         — sound effects and music
- Docs/            — design notes and asset lists
- Tests/           — automated tests (if any)

## Development Notes
- Keep systems decoupled: e.g., input → player controller → combat system.
- Use ScriptableObjects for shared data (items, stats, enemy definitions).
- Keep scene setup simple and data-driven to enable fast iteration.
- Add editor tools to speed up level testing and debugging.

## Contributing
- Open issues for bugs or feature requests.
- Use feature branches named feature/short-description.
- Create pull requests with a concise description and testing notes.
- Write small, focused commits and keep PRs reviewable.

## License
This project is released under the MIT License. See LICENSE for details.

## Contact
Create issues or PRs on the repository for feedback or questions.

Enjoy building!