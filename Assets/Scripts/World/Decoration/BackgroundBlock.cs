using System.Collections.Generic;
using UnityEngine;

public class BackgroundBlock : MonoBehaviour
{
    public const int MaxLengthFromSolid = 30;
    public static Dictionary<Material, Material> viableMaterials = new Dictionary<Material, Material>
    {
        {Material.Stone, Material.Stone}, {Material.Cobblestone, Material.Cobblestone}, {Material.Dirt, Material.Dirt}
        , {Material.Oak_Planks, Material.Oak_Planks}, {Material.Obsidian, Material.Obsidian}
        , {Material.Sand, Material.Sand}, {Material.Sandstone, Material.Sand}, {Material.Grass_Block, Material.Dirt}
        , {Material.Oak_Door_Bottom, Material.Oak_Planks}, {Material.Oak_Door_Top, Material.Oak_Planks}
        , {Material.Oak_Trapdoor, Material.Oak_Planks}, {Material.Farmland_Dry, Material.Dirt}
        , {Material.Farmland_Wet, Material.Dirt}, {Material.Nether_Bricks, Material.Nether_Bricks}
        , {Material.Bedrock, Material.Bedrock}
    };
    
    public static List<Material> transparentBackgrounds = new List<Material>
    {
        {Material.Glass}
    };

    public Material material;

    // Start is called before the first frame update
    private void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/block/" + material.ToString());
    }
}