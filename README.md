# Backrooms

An asymmetric multiplayer horror game for [All Out](https://allout.game/). Survivors solve puzzles, repair systems, unlock new areas, and work toward escape while a player-controlled Entity hunts them. The Entity gains abilities as the survivors progress; downed teammates, revives, barricades, and tools give the survivors ways to help one another.

This repository contains the CSL game scripts, scene, and project assets for Backrooms.

## Script architecture

| Start here | Responsibility |
| --- | --- |
| [scripts/main.csl](scripts/main.csl) | Engine and folder imports, `Player` behavior, zone helpers, and presentation such as camera and ambience. |
| [scripts/match/game_manager.csl](scripts/match/game_manager.csl) | `Game_Manager`: lobby admission, roles, survivor head start, the active hunt, progression, and results. |
| [scripts/tasks.csl](scripts/tasks.csl) | Shared objective and task logic. |
| [scripts/generator.csl](scripts/generator.csl), [wire_puzzle.csl](scripts/wire_puzzle.csl), [keypad_puzzle.csl](scripts/keypad_puzzle.csl), [radio_puzzle.csl](scripts/radio_puzzle.csl) | World puzzle and repair interactions. |
| [scripts/door_level.csl](scripts/door_level.csl), [keycard_scanner.csl](scripts/keycard_scanner.csl), [zone3_blast_door.csl](scripts/zone3_blast_door.csl) | Gates between areas and the conditions for opening them. |
| [scripts/match/](scripts/match/) | Entity abilities, survivor equipment, revives, barricades, role selection, and escape interactions. |
| [scripts/reusable_weapons/](scripts/reusable_weapons/) | Shared weapon/projectile behaviors used by the game. |

The round flows through **lobby → survivor head start → active hunt → result → lobby**. The manager owns match-wide progression; player and interactable components hold the state needed by each participant or world object. Begin with the manager for rule changes and the corresponding puzzle or ability file for smaller mechanics.

## How CSL works here

CSL is All Out's statically typed scripting language, compiled by the All Out editor. [ao.project](ao.project) selects CSL with `"csl": true`.

- The entry point imports `core:ao`, `core:dropped_items`, `match`, and `reusable_weapons`. Folder imports make those modules available alongside the root game scripts.
- Classes derived from `Component`, `Interactable`, and `Ability_Base` connect scripts to scene objects, interactions, and player actions. Fields marked `@ao_serialize` are configured in the editor.
- `ao_start` initializes behavior, `ao_update` advances gameplay, and player `ao_late_update` callbacks handle player UI. The same gameplay executes on predicting clients and the authoritative server, with reconciliation handled by All Out.
- A puzzle changes shared task state; the manager uses it to advance the round or unlock an area. Keep that gameplay in the shared simulation and keep player-specific visuals separate. Late-joining clients need presentation rebuilt from synchronized state, rather than relying on startup running again.

[scene/](scene/) stores the world and its component wiring, and [res/](res/) stores project assets. The engine references in `scripts/.ao_core/` and `api_references/` are useful for looking up APIs; game-specific behavior belongs in the files above.

## Open and remix

1. Install and launch the Windows All Out editor through the [Creator Portal guide](https://docs.allout.game/getting-started/first-game).
2. Clone the project:

   ```sh
   git clone https://github.com/All-Out-Games/backrooms-csl.git
   ```

3. Use **Open project** to select the cloned folder containing `ao.project`. Wait for compilation and asset preparation, then press **Play**. Add enough local clients to exercise both the Entity and survivors and satisfy the match's player minimum.
4. For a published remix, create your own game and associate your working copy with its project identity. The checked-in IDs identify the original live and staging games. Review game-specific products, matchmaking destinations, and progression identifiers for your own version.

The All Out editor/runtime and referenced platform assets are required. Saving CSL files triggers compilation; restart the test after scene or startup changes. Further reading: [playtesting](https://docs.allout.game/getting-started/playtesting-your-game), [CSL syntax](https://docs.allout.game/scripting/syntax), and [networking](https://docs.allout.game/scripting/networking-fundamentals).

## License

The code, documentation, and original assets contributed by All Out Games are available under the [MIT License](LICENSE). You may copy, modify, remix, distribute, and use them in commercial games, including games published on All Out. Keep the copyright and license notice with copies or substantial portions.

Separately licensed third-party material retains its own terms. References to platform-provided assets, such as `$AO/...`, do not transfer ownership of those assets or license the All Out engine and services.
