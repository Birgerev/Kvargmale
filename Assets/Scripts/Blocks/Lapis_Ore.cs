﻿public class Lapis_Ore : Block
{
    public override string[] randomTextures { get; } = {"lapis_ore", "lapis_ore_1"};

    public override float breakTime { get; } = 6;

    public override Tool_Type properToolType { get; } = Tool_Type.Pickaxe;
    public override Tool_Level properToolLevel { get; } = Tool_Level.Iron;
    public override Block_SoundType blockSoundType { get; } = Block_SoundType.Stone;
}