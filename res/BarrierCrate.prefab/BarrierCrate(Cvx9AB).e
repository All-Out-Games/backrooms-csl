14
889058230273
2949107713
{
  "name": "BarrierCrate",
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
  "aoid": 2949107714,
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "Barriers/BarrierCrate.png",
    "scale": {
      "X": 2,
      "Y": 2
    },
    "mask_in_shadow": true
  }
},
{
  "cid": 2,
  "aoid": 2949107715,
  "component_type": "Internal_Component",
  "internal_component_type": "Navmesh_Loop",
  "data": {
    "points": [
      {
        "X": -0.6250000000000000,
        "Y": -0.8700000047683716
      },
      {
        "X": 0.6250000000000000,
        "Y": -0.8700000047683716
      },
      {
        "X": 0.6250000000000000,
        "Y": 0.6000000238418579
      },
      {
        "X": -0.6250000000000000,
        "Y": 0.6000000238418579
      }
    ],
    "flip_inside_outside": true
  }
},
{
  "cid": 3,
  "aoid": 2949107716,
  "component_type": "Internal_Component",
  "internal_component_type": "Barricade",
  "data": {

  }
},
{
  "cid": 4,
  "aoid": 2949107717,
  "component_type": "Internal_Component",
  "internal_component_type": "Barricade_Survivor_Interactable",
  "data": {
    "radius": 1.3999999761581421,
    "required_hold_time": 0.4499999880790710,
    "priority": 20
  }
},
{
  "cid": 5,
  "aoid": 2949107718,
  "component_type": "Internal_Component",
  "internal_component_type": "Barricade_Monster_Interactable",
  "data": {
    "radius": 1.3999999761581421,
    "required_hold_time": 5,
    "priority": 20
  }
}
