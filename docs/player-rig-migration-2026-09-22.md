# Player rig baked before publication

The player uses the ordinary `player_composed/player.spine` asset. Its bones,
skins, slots, animations, event timings and atlas references were composed once
in the editor using the same native function as the former runtime merge.
The legacy recipe is preserved in `rig_sources/player-merge.json`, outside the
runtime resource directory. All gameplay scripts and animation track usage are
unchanged by this migration.

Inputs, in order:

1. Base: `reusable_weapons/anims/reusable-weapons/player/013RED_Player_Character.spine`
2. `$AO/schleem/playercharacter.spine` (resolved to `$AO/streamed_character`)
3. `reusable_weapons/anims/reusable-weapons/weapons/013RED_weapons.spine`
4. `Animations/lplayer/playercharacter.spine`

The three project input JSON files match the cooked assets in published primary
version `6ab20436236e3076fa1ad9c8`. Engine source is based on
`88fde025ebf3022d7800c4d8f3e1cb76151a416d`, plus the `rig_merge_bake` authoring
tool. Keep these source rigs and their images: the composed atlas references
their existing texture assets, including derived skin pages.

Native checks compared the old and new assets: seven complete inventory queries,
1,055 skin attachment queries, 334 detailed animation queries, and 982 full
bone/slot layouts sampled at animation starts, midpoints and ends. No differences
or truncated results remained. Both assets passed preprocessing validation.
This comparison does not claim every possible layered animation combination was
played or replace gameplay verification.

This is a snapshot of the rig composition. A change to an input or the shared
engine rig requires a new authoring bake, equivalence check and game publication;
it no longer rebuilds a player rig on each user's device. To regenerate, restore
the archived recipe temporarily under its original resource path, use
`rig_merge_bake` to create a new output bundle, compare and validate the result,
then remove that temporary runtime recipe before publication.

The ordinary cooked rig is 9,303,184 bytes before any HTTP content encoding. That
is an added asset, not a claimed download saving. Local startup, payload and
gameplay measurements, plus published version IDs, will be recorded in the
engine repository's game-rig migration ledger. No production release is claimed
by this source commit alone.
