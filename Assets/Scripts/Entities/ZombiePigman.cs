using System;
using System.Collections.Generic;

public class ZombiePigman : Monster
{
    public override float maxHealth { get; } = 20;

    public override List<ItemStack> GetDrops()
    {
        //Drop a random amount of a certain item
        List<ItemStack> result = new();
        Random r = new(SeedGenerator.SeedByWorldLocation(Location));

        result.Add(new ItemStack(Material.Rotten_Flesh, r.Next(0, 1 + 1)));
        if(r.NextDouble() < 0.025d)
            result.Add(new ItemStack(Material.Gold_Ingot, 1));

        return result;
    }

    public override EntityController GetController()
    {
        return new ZombiePigmanController(this);
    }
}