13
9019431321602
1193449237816964 1746058616294626500
{
  "name": "VFX_MobParalyzedAura",
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
  "aoid": "1193449238264034:1746058616294743100",
  "component_type": "Internal_Component",
  "internal_component_type": "Spine_Animator",
  "data": {
    "skeleton_data_asset": "anims/reusable-weapons/Electrocuted/014ANT_Electrocuted_State_VFX.spine",
    "ordered_skins": [
      "default"
    ]
  }
},
{
  "cid": 3,
  "aoid": "1193449238520876:1746058616294810800",
  "component_type": "Mono_Component",
  "mono_component_type": "LoopingVFX",
  "data": {
    "Lifetime": -1,
    "Skeleton": "1193449238264034:1746058616294743100",
    "InitialAnimationName": "Electrocuted_Effect_Start",
    "LoopAnimationName": "Electrocuted_Effect_Loop",
    "OutroAnimationName": "Electrocuted_Effect_End",
    "OutroAnimationTime": 1.0670000314712524
  }
}
