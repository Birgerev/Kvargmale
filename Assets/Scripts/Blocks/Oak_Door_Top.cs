﻿public class Oak_Door_Top : Oak_Door_Block
{
    public override bool RotateX { get; } = true;

    public override string open_texture { get; } = "oak_door_top_open";
    public override string closed_texture { get; } = "oak_door_top";
}