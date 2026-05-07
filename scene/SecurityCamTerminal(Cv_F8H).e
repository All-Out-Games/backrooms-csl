14
811748818945
2952290055
{
  "name": "SecurityCamTerminal",
  "local_enabled": true,
  "local_position": {
    "X": -40.7923469543457031,
    "Y": -33.0411949157714844
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 1,
    "Y": 1
  },
  "previous_sibling": 2943971087,
  "next_sibling": 2947660454
},
{
  "cid": 1,
  "aoid": 2936267020,
  "component_type": "Internal_Component",
  "internal_component_type": "Spine_Animator",
  "data": {
    "skeleton_data_asset": "Animations/terminal_spine/terminal.spine",
    "ordered_skins": [

    ],
    "depth_offset": 0.4733982086181641,
    "initial_animation": "idle_off",
    "loop_initial_animation": true
  }
},
{
  "cid": 2,
  "aoid": 2947729013,
  "component_type": "Internal_Component",
  "internal_component_type": "Box_Collider",
  "data": {
    "make_navmesh_loop": true,
    "flip_navmesh_loop": true,
    "size": {
      "X": 1.5228133201599121,
      "Y": 0.9211826324462891
    },
    "offset": {
      "X": -0.0179045200347900,
      "Y": 0.0394086837768555
    }
  }
},
{
  "cid": 3,
  "aoid": 2945168981,
  "component_type": "Internal_Component",
  "internal_component_type": "Security_Camera_Terminal",
  "data": {
    "radius": 2,
    "required_hold_time": 0.6000000238418579,
    "prompt_offset": {
      "Y": 1
    }
  }
}
