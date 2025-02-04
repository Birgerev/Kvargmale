using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public class LightManager : MonoBehaviour
{
    public const int MaxLightLevel = 15;
    public const int NightLightLevel = 5;
    public const int NetherLightLevel = 7;
    
    public static LightManager Instance;
    public static bool DoLight = true;

    public GameObject lightSourcePrefab;
    public GameObject sunlightSourcePrefab;
    public Dictionary<LightColumn, SunlightSource> sunlightSources = new Dictionary<LightColumn, SunlightSource>();

    private void Start()
    {
        Instance = this;
    }

    #region Area Light Handling
    
    public static void UpdateAllLight()
    {
        foreach (object chunkPos in WorldManager.instance.chunks.Keys)
            UpdateChunkLight((ChunkPosition) chunkPos);
    }
    

    public static void UpdateChunkLight(ChunkPosition chunk)
    {
        UpdateLightInArea(new Location(chunk.worldX, 0, chunk.dimension),
            new Location(chunk.worldX + Chunk.Width, Chunk.Height, chunk.dimension));
    }

    public static async void UpdateLightInArea(Location min, Location max)
    {
        List<LightSource> lightSources = GetLightSourcesForArea(min + new Location(-MaxLightLevel, -MaxLightLevel),
            max + new Location(MaxLightLevel, MaxLightLevel));
        
        List<LightObject> lightObjects = GetLightObjectsForArea(min, max);
        
        List<Location> lightObjectLocations = lightObjects.Select(lightObject => lightObject.GetLocation()).ToList();
        List<LightValues> calculatedLightValues = await Task.Run (() => CalculateLightValuesForLocations(lightObjectLocations, lightSources));

        for (int index = 0; index < lightObjects.Count; index++)
        {
            LightObject lightObject = lightObjects[index];
            
            if(lightObject != null)
                lightObject.UpdateLightLevel(calculatedLightValues[index]);
        }
    }

    public static List<LightValues> CalculateLightValuesForLocations(List<Location> locationsToCalculate, List<LightSource> possibleLightSources)
    {
        List<LightValues> finalLightValues = new List<LightValues>();
        
        foreach (Location locationToCalculate in locationsToCalculate)
        {
            LightValues lightValueAtLocation = GetLightValuesAt(locationToCalculate, possibleLightSources);
            
            finalLightValues.Add(lightValueAtLocation);
        }

        return finalLightValues;
    }

    private static List<LightObject> GetLightObjectsForArea(Location boundingBoxMin, Location boundingBoxMax)
    {
        Collider2D[] lightObjectColliders = Physics2D.OverlapAreaAll(boundingBoxMin.GetPosition(),
            boundingBoxMax.GetPosition());
        List<LightObject> lightObjects = new List<LightObject>();

        foreach (Collider2D lightObjectCollider in lightObjectColliders)
        {
            LightObject lightObject = lightObjectCollider.GetComponent<LightObject>();

            if (lightObject != null)
                lightObjects.Add(lightObject);
        }

        return lightObjects;
    }

    private static List<LightSource> GetLightSourcesForArea(Location boundingBoxMin, Location boundingBoxMax)
    {
        Collider2D[] lightSourceColliders = Physics2D.OverlapAreaAll(boundingBoxMin.GetPosition(),
            boundingBoxMax.GetPosition(),
            LayerMask.GetMask("LightSource"));
        List<LightSource> lightSources = new List<LightSource>();

        foreach (Collider2D lightSourceCollider in lightSourceColliders)
            lightSources.Add(lightSourceCollider.GetComponent<LightSource>());

        return lightSources;
    }
    #endregion
 
    #region Sunlight
    public static bool DoesBlockInfluenceSunlight(Location loc)
    {
        LightColumn column = new LightColumn(loc.x, loc.dimension);

        if (!Instance.sunlightSources.ContainsKey(column))
            return true;
        
        if (loc.y >= Instance.sunlightSources[column].transform.position.y)
            return true;

        return false;
    }

    public static void UpdateSunlightInColumn(LightColumn column, bool updateLight)
    {
        if (Instance.sunlightSources.ContainsKey(column))
        {
            SunlightSource oldSunlightSource = Instance.sunlightSources[column];
            Instance.sunlightSources.Remove(column);

            if (updateLight)
                oldSunlightSource.lightSource.UpdateLightWithinReach();
            
            Destroy(oldSunlightSource.gameObject);
        }

        //Dont create sunlight sources if player is in the nether
        if (column.dimension == Dimension.Nether)
            return;

        Block topmostBlock = Chunk.GetTopmostBlock(column.x, column.dimension, false);

        //Return in case no block was found in column, may be the case in ex void worlds
        if (topmostBlock == null)
            return;

        SunlightSource newSunlightSource =
            SunlightSource.Create(Location.LocationByPosition(topmostBlock.transform.position));

        Instance.sunlightSources.Add(column, newSunlightSource);
        if (updateLight)
            newSunlightSource.lightSource.UpdateLightWithinReach();
    }
    #endregion

    public static void DestroySource(LightSource source)
    {
        Destroy(source.gameObject);
        
        source.UpdateLightWithinReach();
    }

    public static LightValues GetLightValuesAt(Location loc, List<LightSource> knownLightSources = null)
    {
        if (!DoLight)
            return new LightValues(MaxLightLevel);
        
        //If no set of light sources was supplied, automatically find them
        if(knownLightSources == null)
            knownLightSources = GetLightSourcesForArea(
                loc + new Location(-MaxLightLevel, -MaxLightLevel), 
                loc + new Location(MaxLightLevel, MaxLightLevel));
        
        Vector3 pos = loc.GetPosition();
        
        //Find the brightest way to light 
        LightValues brightestRecordedLight = new LightValues();
        Color finalColor = Color.white;
        foreach (LightSource source in knownLightSources)
        {
            Vector3 lightSourcePos = source.GetLocation().GetPosition();
            float sourceDistance = Vector3.Distance(lightSourcePos, pos);
            
            //If Source is outside of sphere of influence, ignore it
            if (sourceDistance > MaxLightLevel)
                continue;
            
            //Get light source values
            LightValues sourceLight = source.lightValues;
            
            //Calculate object light values using this specific source
            LightValues contextLight = sourceLight;
            contextLight.lightLevel = sourceLight.lightLevel - (int) sourceDistance;
            
            //If resulting light level is 0 or less, ignore it
            if(contextLight.lightLevel <= 0)
                continue;
            
            //If resulting light level is the highest one so far, store it
            if (contextLight.lightLevel > brightestRecordedLight.lightLevel)
                brightestRecordedLight = contextLight;
            
            //Blend light source colors, with their context strength as weights
            float contextSourceLightWeight =
                (float)contextLight.lightLevel / Mathf.Max(contextLight.lightLevel, brightestRecordedLight.lightLevel);
            
            finalColor = Color.Lerp(finalColor, sourceLight.sourceColor, contextSourceLightWeight);
        }

        brightestRecordedLight.sourceColor = finalColor;
        return brightestRecordedLight;
    }

    public static void UpdateBlockLight(Location loc)
    {
        UpdateLightInArea(loc, loc);
    }

    public static void UpdateLightObject(LightObject lightObj)
    {
        Location loc = lightObj.GetLocation();
        List<LightSource> lightSources = GetLightSourcesForArea(loc + new Location(-MaxLightLevel, -MaxLightLevel),
            loc + new Location(MaxLightLevel, MaxLightLevel));

        UpdateLightObjectWithSpecificSources(lightObj, lightSources);
    }

    private static void UpdateLightObjectWithSpecificSources(LightObject lightObject, List<LightSource> possibleLightSources)
    {
        LightValues lightValues = GetLightValuesAt(lightObject.GetLocation(), possibleLightSources);

        lightObject.UpdateLightLevel(lightValues);
    }
}

public struct LightColumn
{
    public int x;
    public Dimension dimension;

    public LightColumn(int x, Dimension dim)
    {
        this.x = x;
        dimension = dim;
    }
}

[Serializable]
public struct LightValues
{
    public int lightLevel;
    public Color sourceColor;
    public bool flicker;
    
    public LightValues(int lightLevel)
    {
        this.lightLevel = lightLevel;
        
        sourceColor = Color.white;
        flicker = false;
    }

    public LightValues(int lightLevel, Color sourceColor, bool flicker)
    {
        this.lightLevel = lightLevel;
        this.sourceColor = sourceColor;
        this.flicker = flicker;
    }
}