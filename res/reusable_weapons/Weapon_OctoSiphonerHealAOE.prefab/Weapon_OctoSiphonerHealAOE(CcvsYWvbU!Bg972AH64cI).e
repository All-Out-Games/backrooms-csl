13
6296422055938
689372724393684 1746815426218395400
{
  "name": "Weapon_OctoSiphonerHealAOE",
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
  "aoid": "689398483561303:1746815433010300900",
  "component_type": "Internal_Component",
  "internal_component_type": "Spine_Animator",
  "data": {
    "skeleton_data_asset": "anims/reusable-weapons/aoe_heal_circle/BAT003_heal_regeneration.spine",
    "ordered_skins": [
      "default"
    ],
    "depth_offset": 0.5000000000000000
  }
},
{
  "cid": 2,
  "aoid": "689570780373202:1746815478439706900",
  "component_type": "Mono_Component",
  "mono_component_type": "LoopingVFX",
  "data": {
    "Lifetime": -1,
    "Skeleton": "689398483561303:1746815433010300900",
    "InitialAnimationName": "heal_pulse_intro",
    "LoopAnimationName": "heal_pulse_loop",
    "OutroAnimationName": "heal_pulse_outro",
    "OutroAnimationTime": 1.3329999446868896
  }
}
