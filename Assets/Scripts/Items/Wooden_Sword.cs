﻿public class Wooden_Sword : Sword
{
    public override Tool_Level tool_level { get; } = Tool_Level.Wooden;
    public override int maxDurability { get; } = 59;
    public override float entityDamage { get; } = 4;
}