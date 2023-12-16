﻿public class Diamond_Ore : Block
{
    public override string[] randomTextures { get; } = {"diamond_ore", "diamond_ore_1"};

    public override float breakTime { get; } = 6;

    public override Tool_Type properToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level properToolLevel { get; } = Tool_Level.Iron;
    public override BlockSoundType blockSoundType { get; } = BlockSoundType.Stone;

    public override ItemStack[] GetDrops()
    {
        return new[] { new ItemStack(Material.Diamond)};
    }
}