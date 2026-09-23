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

The current editor also normalizes scene filenames and serializes the sprite
color field as `color` instead of `tint`. A separate audit matched all 2,098
entity IDs and headers, checked the 408 exact field renames, and confirmed all
other scene JSON was unchanged. This serialization update is committed separately.

## Publication and payload validation

The first inactive compiled candidate is `6ab323a8236e3076fa1bc807`; it has not
replaced active primary `6ab20436236e3076fa1ad9c8`. Its 43 ordinary rigs retain
their atlas and packed skeleton bytes while omitting source JSON from runtime
files. Full native equivalence, local Chrome movement and five-player match
startup checks passed. Its larger game-data download cancels much of the compute
saving on a slow network, so activation is held while packaging is improved.

Engine publisher commit `cd134d3dbb22ca2bf05167201d25f1ed3f0ff4d0` on
`codex/game-rig-migrations` leaves ordinary rigs out of `bundled_assets` for
merge-free games. The same manifest IDs fetch the same asset files through the
existing cache. A local diagnostic passed cold rendering/movement and, after
restarting Chrome with HTTP cache cleared, reused all 43 rigs with zero rig
downloads. Normal editor package generation also passed. These checks do not
establish the candidate's final cold speed or production gameplay.

The next prepared source applies only that bundle rule to the exact previous
published source; scripts, scenes, authoring resources, manifests and all other
archive entries remain byte-identical. SHA-256:
`b38b5afa9e7311a1fa5d67fd356f415423c9c307ec84ad628978b857a13ad6d5`.
Normal compilation, hosted-package validation and measurements must pass before
activating it. The engine repository's migration ledger records version IDs and
release status. Do not treat this audit commit as production activation.

The source was uploaded once as inactive `6ab33b55236e3076fa1bf207` on
2026-09-23 at 02:37:23.099 UTC. Normal P48 compilation succeeded, build hash
`e817b8553238da9c`, 8,470,536 bytes. Independent download verified the exact source
hash. The compiled manifest, scripts, serialized entities, terrain, scene config
and retained bundled assets match the preceding baked candidate byte for byte.
Only ordinary rig bundle records are removed and the normal packed-scene cache
is regenerated (7,088 bytes larger); no source scene change occurred.

Two canonical full-dev Chrome cold runs per package, CPU6 and slow4g, measured
74.970 s with bundled rigs versus 75.959 s with separate rig assets. Encoded
through-spawn transfer was approximately 41.05 MB on both. This is a small
measured cold-load cost, not a cold speedup. Game-data transfer fell from about
10.67 to 2.85 MB; the external rig files transfer once and can then be reused.
The actual hosted candidate passed Chrome restart with HTTP cleared and OPFS
retained: 43 rig requests / 7.81 MB cold, zero rig requests on restart, rendering
and movement on both runs, no page exceptions or runtime compositions.

A fresh five-client local round passed on the exact hosted candidate. Both roles
rendered and moved; monster Q input triggered bite sound, pose and cooldown, and
survivor R input produced danger pings. Scream remained locked, so that ability
and the full twenty-minute round were not verified. This supplements the full
native rig equivalence checks; it is not a production retention result.

The existing candidate was activated at approximately 2026-09-23 03:00:32 UTC.
Independent production readback confirms `6ab33b55236e3076fa1bf207` active.
Actual public production Chrome sessions, using unchanged Client-Web #635 and
no client/game-data overrides, both spawned, rendered and moved with no page
exceptions or runtime merges. Cold entry fetched all 43 rigs (7,813,376 encoded
bytes); browser restart with HTTP cleared and OPFS retained fetched zero rigs
and zero game assets. These are functional/cache checks, not production timing
or retention measurements. Keep prior active `6ab20436236e3076fa1ad9c8` and the
first inactive bake for rollback. The staging branch retains its own scripts
and is a separate unfinished publication. Engine audits contain the raw evidence
locations and explicit cold-load tradeoff.
