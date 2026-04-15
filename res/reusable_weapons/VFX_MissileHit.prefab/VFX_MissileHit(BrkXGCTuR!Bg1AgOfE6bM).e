13
6296422055939
473089675180945 1744302645252957900
{
  "name": "VFX_MissileHit",
  "local_enabled": true,
  "local_position": {

  },
  "local_rotation": 0,
  "local_scale": {
    "X": 1,
    "Y": 1
  }
},
{
  "cid": 1,
  "aoid": "473143790271407:1744302659521466100",
  "component_type": "Internal_Component",
  "internal_component_type": "Spine_Animator",
  "data": {
    "skeleton_data_asset": "anims/reusable-weapons/SKI100_mech_missile_spine/SKI100_mech_missile.spine",
    "ordered_skins": [
      "default"
    ],
    "skeleton_scale": {
      "X": 2,
      "Y": 2
    }
  }
},
{
  "cid": 2,
  "aoid": "474939298577713:1744303132942544600",
  "component_type": "Mono_Component",
  "mono_component_type": "SimpleVFX",
  "data": {
    "Lifetime": 0.5000000000000000,
    "Skeleton": "473143790271407:1744302659521466100",
    "InitialAnimationName": "hit_effect"
  }
}
