13
287762808835
400308853860792 1744283455157971700
{
  "name": "Loot_Chest",
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
  "aoid": "400363210395199:1744283469490139100",
  "component_type": "Internal_Component",
  "internal_component_type": "Interactable",
  "data": {
    "text": "Loot!",
    "hold_text": "Looting...",
    "required_hold_time": 1
  }
},
{
  "cid": 2,
  "aoid": "400376044678673:1744283472874150300",
  "component_type": "Internal_Component",
  "internal_component_type": "Box_Collider",
  "data": {
    "is_trigger": true,
    "size": {
      "X": 1.5000000000000000,
      "Y": 1.5000000000000000
    }
  }
},
{
  "cid": 3,
  "aoid": "409744146170752:1744285942958359800",
  "component_type": "Mono_Component",
  "mono_component_type": "LootChest",
  "data": {
    "Interactable": "400363210395199:1744283469490139100",
    "Skeleton": "400419654584587:1744283484372759800"
  }
}
