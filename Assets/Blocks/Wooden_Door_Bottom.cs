﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wooden_Door_Bottom : Door
{
    public override bool rotate_x { get; } = true;

    public override string open_texture { get; } = "block_wooden_door_bottom_open";
    public override string closed_texture { get; } = "block_wooden_door_bottom_close";

    public static string default_texture = "block_wooden_door_bottom_close";

    public override void BuildTick()
    {
        (location + new Location(0, 1)).SetMaterial(Material.Wooden_Door_Top);
        
        base.BuildTick();
    }
}
