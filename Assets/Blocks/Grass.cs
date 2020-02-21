﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : Block
{
    public static string default_texture = "block_grass";
    public override float breakTime { get; } = 0.75f;

    public override Tool_Type propperToolType { get; } = Tool_Type.Shovel;

    public override ItemStack GetDrop()
    {
        return new ItemStack(Material.Dirt, 1);
    }

    public override void Tick(bool spread)
    {
        if (Chunk.getBlock(location + new Location(0, 1)) != null)
        {
            //Turn to dirt if covered
            if (Chunk.getBlock(location + new Location(0, 1)).playerCollide)
            {
                Chunk.setBlock(location, Material.Dirt, "", false, false);
            }
        }

        base.Tick(spread);
    }
}
