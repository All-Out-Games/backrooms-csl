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

This branch preserves the scripts in published staging version
`6ab1f7514c1aefa488cac9f4`, separately from primary. All 43 rig manifest entries,
the complete merge recipe payload, and all 2,105 scene files match published
primary version `6ab20436236e3076fa1ad9c8`; the same validated ordinary rig and
scene serialization migration therefore apply to staging. `ao.project` selects
only staging game `69fd03b58bbb5b10c523dfc4` to keep publication explicit.

The three project input JSON files match those published cooked inputs. Engine source is based on
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

The current editor also normalizes scene filenames and serializes the sprite
color field as `color` instead of `tint`. A separate audit matched all 2,098
entity IDs and headers, checked the 408 exact field renames, and confirmed all
other scene JSON was unchanged. This serialization update is committed separately.

## Staging publication preparation

The rebuilt native editor compiled this staging project successfully and prepared
the source archive through its normal publishing code. All 73 gameplay scripts
are byte-identical to staging's own published source; all 2,105 scene entries
match this migrated checkout. The 86 rig JSON/atlas source files match the
validated primary migration. No runtime merge recipe or bundled ordinary rig
remains; the runtime manifest references 43 external Spine assets.

Prepared archive SHA-256:
`70aecb428a3bd1e7b7722600724d3d3e6b56604800e63f3880ff4d98abec9fe8`.
Only staging game `69fd03b58bbb5b10c523dfc4` is selected in `ao.project`. The
normal compile/publication and actual hosted-package gameplay validation remain
pending. Do not activate until those checks pass. Source warnings about four
missing generator sound/icon references and soft-deprecated APIs are unchanged.

Rebuilding the same rig sources in this checkout changed all 43 runtime content
IDs. The cache reuses the exact published IDs across restarts, but cross-rebuild
reuse is not established. The observed packed-file differences are recorded in
the engine migration ledger; do not confuse them with the fixed OPFS read bug.
