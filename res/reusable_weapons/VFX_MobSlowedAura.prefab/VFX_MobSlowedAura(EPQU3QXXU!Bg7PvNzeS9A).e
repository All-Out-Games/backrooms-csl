13
9015136354306
1192992518010324 1746058495871496000
{
  "name": "VFX_MobSlowedAura",
  "local_enabled": true,
  "local_position": {
    "X": 34.7256965637207031,
    "Y": -7.3915395736694336
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 1,
    "Y": 1
  }
},
{
  "cid": 1,
  "aoid": "1192992518407728:1746058495871600500",
  "component_type": "Internal_Component",
  "internal_component_type": "Spine_Animator",
  "data": {
    "skeleton_data_asset": "anims/reusable-weapons/Cold_Aura/cold_aura.spine",
    "ordered_skins": [
      "default"
    ]
  }
},
{
  "cid": 3,
  "aoid": "1192992518620300:1746058495871656400",
  "component_type": "Mono_Component",
  "mono_component_type": "LoopingVFX",
  "data": {
    "Lifetime": -1,
    "Skeleton": "1192992518407728:1746058495871600500",
    "InitialAnimationName": "animation",
    "LoopAnimationName": "animation",
    "OutroAnimationName": "animation",
    "OutroAnimationTime": 1.0670000314712524
  }
}
