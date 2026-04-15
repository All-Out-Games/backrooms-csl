13
2838973382657
473089675180945 1744302645252957900
{
  "name": "VFX_PlayerHeal",
  "local_enabled": true,
  "local_position": {
    "X": 23.9811687469482422,
    "Y": 16.4512195587158203
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 1.5000000000000000,
    "Y": 1.5000000000000000
  }
},
{
  "cid": 1,
  "aoid": "473143790271407:1744302659521466100",
  "component_type": "Internal_Component",
  "internal_component_type": "Spine_Animator",
  "data": {
    "skeleton_data_asset": "anims/reusable-weapons/Heal_Effect/heal_effect.spine",
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
    "Lifetime": 1.5000000000000000,
    "Skeleton": "473143790271407:1744302659521466100",
    "InitialAnimationName": "Swirl"
  }
}
