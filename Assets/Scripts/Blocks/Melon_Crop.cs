﻿using System.Collections.Generic;

public class Melon_Crop : Crop
{
    private string melonLeftTexture { get; set; } = "melon_crop_left";
    private string melonRightTexture { get; set; } = "melon_crop_right";

    public override string[] crop_textures { get; } =
    {
        "melon_crop", "melon_crop_1", "melon_crop_2", "melon_crop_3", 
        "melon_crop_4", "melon_crop_5"
    };

    public override Material seed { get; } = Material.Melon_Seeds;
    public override Material result { get; } = Material.Air;
    
    private List<Material> viableMelonBlocks { get; } = new List<Material>
    {
        Material.Grass_Block, Material.Dirt, Material.Farmland_Dry, Material.Farmland_Wet
    };

    public override void Tick()
    {
        UpdateRender();
        
        base.Tick();
    }

    public override void Grow()
    {
        if (GetStage() >= GetAmountOfStages() - 1)
        {
            Location leftLoc = location + new Location(-1, 0);
            Location rightLoc = location + new Location(1, 0);
            if (CanGrowMelon(leftLoc))
            {
                leftLoc.SetMaterial(Material.Melon).Tick();
                return;
            }
            if (CanGrowMelon(rightLoc))
            {
                rightLoc.SetMaterial(Material.Melon).Tick();
                return;
            }
        }
        
        base.Grow();
    }

    protected override string GetTextureName()
    {
        if (GetStage() >= GetAmountOfStages() - 1)
        {
            if ((location + new Location(-1, 0)).GetMaterial() == Material.Melon)
                return melonLeftTexture;
            if ((location + new Location(1, 0)).GetMaterial() == Material.Melon)
                return melonRightTexture;
        }
        
        return crop_textures[GetStage()];
    }

    private bool CanGrowMelon(Location loc)
    {
        Location belowLoc = loc + new Location(0, -1);

        return (loc.GetMaterial() == Material.Air && viableMelonBlocks.Contains(belowLoc.GetMaterial()));
    }
}