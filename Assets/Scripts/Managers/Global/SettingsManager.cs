using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;
    public static string PlayerName;

    public static Dictionary<string, string> Values = new Dictionary<string, string>();
    
    private static readonly Dictionary<string, string> DefaultValues = new Dictionary<string, string>()
    {
        {"soundCategory_master", "100"},
        {"soundCategory_entities", "100"},
        {"soundCategory_block", "100"},
        {"soundCategory_music", "100"},
        {"multiplayer", "friends"},
    };
    
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        LoadSettings();
        PopulateMissingWithDefaultValues();
        
        InvokeRepeating(nameof(SaveSettings), 5f, 5f);
    }

    public static string GetStringValue(string key)
    {
        if(!DefaultValues.ContainsKey(key))
            Debug.LogWarning("Default Settings Values does not contain an entry for the key: '" + key + "'");

        if (!Values.ContainsKey(key))
        {
            Debug.LogWarning("Tried and failed to fetch settings value with key: '" + key + "', errors might follow");
            return "";
        }

        return Values[key];
    }
    
    public static int GetIntValue(string key)
    {
        string stringValue = GetStringValue(key);

        if(!int.TryParse(stringValue, out var intValue))
            Debug.LogWarning("Failed to parse settings value to int, key: '" + key + "', string: '" + stringValue + "'");

        return intValue;
    }

    public static void OverwriteWithDefaultSettings()
    {
        Values = new Dictionary<string, string>(DefaultValues);
    }

    private void LoadSettings()
    {
        if(!File.Exists(GetPath()))
            return;
        
        string[] lines = File.ReadAllLines(GetPath());

        foreach (var line in lines)
        {
            string key = line.Split('=')[0];
            string value = line.Split('=')[1];

            Values[key] = value;
        }
    }

    private void SaveSettings()
    {
        List<String> lines = new List<string>();
        
        foreach (string keyName in Values.Keys)
        {
            string value = Values[keyName];
            
            lines.Add(keyName + "=" + value);
        }
        
        File.WriteAllLines(GetPath(), lines);
    }
    
    
    private void PopulateMissingWithDefaultValues()
    {
        foreach (string defaultKey in DefaultValues.Keys)
        {
            //if values list does not contain a value that should be assigned
            if (!Values.ContainsKey(defaultKey))
                //Populate it with the default value
                Values[defaultKey] = DefaultValues[defaultKey];
        }

    }
    
    public string GetPath()
    {
        return Application.persistentDataPath + "\\options.txt";
    }
}
