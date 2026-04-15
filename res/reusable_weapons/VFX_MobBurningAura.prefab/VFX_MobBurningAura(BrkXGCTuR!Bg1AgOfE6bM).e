13
6296422055937
473089675180945 1744302645252957900
{
  "name": "VFX_MobBurningAura",
  "local_enabled": true,
  "local_position": {
    "X": 1.9312047958374023,
    "Y": -2.7595868110656738
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
    "skeleton_data_asset": "anims/reusable-weapons/Player_VFX_Particles/012MIM_Player_VFX_particles.spine",
    "ordered_skins": [
      "default"
    ]
  }
},
{
  "cid": 3,
  "aoid": "171179841557759:1744825798397102800",
  "component_type": "Mono_Component",
  "mono_component_type": "LoopingVFX",
  "data": {
    "Lifetime": -1,
    "Skeleton": "473143790271407:1744302659521466100",
    "InitialAnimationName": "PE_burned_effect_intro",
    "LoopAnimationName": "PE_burned_effect_loop",
    "OutroAnimationName": "PE_burned_effect_outro",
    "OutroAnimationTime": 1.0670000314712524
  }
}
