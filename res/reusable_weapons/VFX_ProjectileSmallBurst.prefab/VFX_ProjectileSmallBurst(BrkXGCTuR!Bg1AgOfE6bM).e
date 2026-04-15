13
2838973382660
473089675180945 1744302645252957900
{
  "name": "VFX_ProjectileSmallBurst",
  "local_enabled": true,
  "local_position": {
    "X": -0.2507627010345459,
    "Y": 6.4015569686889648
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
    "skeleton_data_asset": "anims/reusable-weapons/Hit_Effect/hit_effect.spine",
    "ordered_skins": [
      "default"
    ]
  }
},
{
  "cid": 2,
  "aoid": "474939298577713:1744303132942544600",
  "component_type": "Mono_Component",
  "mono_component_type": "SimpleVFX",
  "data": {
    "Lifetime": 0.2500000000000000,
    "Skeleton": "473143790271407:1744302659521466100",
    "InitialAnimationName": "hit_effect"
  }
}
