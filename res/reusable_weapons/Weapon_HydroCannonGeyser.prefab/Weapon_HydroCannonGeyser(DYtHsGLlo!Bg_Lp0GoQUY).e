13
9629316677634
953078678862184 1746884957422290200
{
  "name": "Weapon_HydroCannonGeyser",
  "local_enabled": true,
  "local_position": {

  },
  "local_rotation": 0,
  "local_scale": {
    "X": 1,
    "Y": 1
  },
  "spawn_as_networked_entity": true,
  "network_position": true
},
{
  "cid": 1,
  "aoid": "953108859956613:1746884965380122500",
  "component_type": "Internal_Component",
  "internal_component_type": "Spine_Animator",
  "data": {
    "skeleton_data_asset": "anims/reusable-weapons/projectiles/013RED_projectile.spine",
    "ordered_skins": [
      "hydro_cannon"
    ],
    "skeleton_scale": {
      "X": 3,
      "Y": 3
    }
  }
},
{
  "cid": 2,
  "aoid": "953120107592457:1746884968345780500",
  "component_type": "Internal_Component",
  "internal_component_type": "Circle_Collider",
  "data": {
    "is_trigger": true,
    "size": 2
  }
},
{
  "cid": 3,
  "aoid": "953216701398301:1746884993814616900",
  "component_type": "Mono_Component",
  "mono_component_type": "HydroCannonGeyser",
  "data": {
    "Skeleton": "953108859956613:1746884965380122500",
    "Animation": "013RED/Splash",
    "Collider": "953120107592457:1746884968345780500"
  }
}
