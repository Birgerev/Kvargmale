﻿using UnityEngine;
using UnityEngine.UI;

public class LoadingMenu : MonoBehaviour
{
    public static LoadingMenu instance;

    public Image loadingBar;
    public Text loadingTitle;
    public Text loadingStateText;

    public LoadingMenuType type;

    // Update is called once per frame
    private void Update()
    {
        switch (type)
        {
            case LoadingMenuType.LoadWorld:
                LoadWorldMenu();
                break;
            case LoadingMenuType.ConnectServer:
                ConnectMenu();
                break;
            case LoadingMenuType.Dimension:
                ChangeDimensionMenu();
                break;
        }
    }

    public void SetState(string name, float progress)
    {
        loadingStateText.text = name;
        loadingBar.fillAmount = progress;
    }

    public static void Create(LoadingMenuType type)
    {
        if (instance != null)
            return;

        GameObject prefab = Resources.Load<GameObject>("Prefabs/LoadingMenu");
        GameObject obj = Instantiate(prefab);
        LoadingMenu menu = obj.GetComponent<LoadingMenu>();

        menu.type = type;
        instance = menu;
        DontDestroyOnLoad(obj);
        Destroy(obj, 15);
    }

    public void LoadWorldMenu()
    {
        loadingTitle.text = "Loading World";

        int steps = 7;

        if (Player.LocalEntity == null)
        {
            SetState("Creating Player", 1f / steps);
            return;
        }

        Chunk playerChunk = new ChunkPosition(Player.LocalEntity.GetComponent<Player>().Location).GetChunk();
        if (playerChunk == null)
        {
            SetState("Creating Chunks", 2f / steps);
            return;
        }

        if (!playerChunk.areBlocksReady)
        {
            SetState("Generating Chunks", 3f / steps);
            return;
        }

        if (!playerChunk.donePlacingGeneratedBlocks)
        {
            SetState("Placing Blocks", 4f / steps);
            return;
        }

        if (!playerChunk.donePlacingBackgroundBlocks)
        {
            SetState("Placing Background Blocks", 5f / steps);
            return;
        }

        if (!playerChunk.isLightGenerated)
        {
            SetState("Waiting For Light", 6f / steps);
            return;
        }

        SetState("Done!", 1);
        Destroy(gameObject, 1f);
    }

    public void ConnectMenu()
    {
        loadingTitle.text = "Joining World";

        int steps = 7;

        if (PlayerInstance.localPlayerInstance == null)
        {
            SetState("Connecting", 1f / steps);
            return;
        }

        if (Player.LocalEntity == null)
        {
            SetState("Creating Player", 2f / steps);
            return;
        }

        Chunk playerChunk = new ChunkPosition(Player.LocalEntity.GetComponent<Player>().Location).GetChunk();
        if (playerChunk == null)
        {
            SetState("Creating Chunks", 3f / steps);
            return;
        }

        if (!playerChunk.donePlacingGeneratedBlocks)
        {
            SetState("Placing Blocks", 4f / steps);
            return;
        }

        if (!playerChunk.donePlacingBackgroundBlocks)
        {
            SetState("Placing Background Blocks", 5f / steps);
            return;
        }

        if (!playerChunk.isLightGenerated)
        {
            SetState("Waiting For Light", 6f / steps);
            return;
        }

        SetState("Done!", 1);
        Destroy(gameObject, 1f);
    }

    public void ChangeDimensionMenu()
    {
        loadingTitle.text = "Loading Dimension";

        int steps = 3;

        Chunk playerChunk = new ChunkPosition(Player.LocalEntity.GetComponent<Player>().Location).GetChunk();
        if (Player.LocalEntity.teleportingDimension)
        {
            SetState("Loading Chunks", 1f / steps);
            return;
        }

        if (!playerChunk.isLightGenerated)
        {
            SetState("Waiting For Light", 2f / steps);
            return;
        }

        SetState("Done!", 1);
        Destroy(gameObject, 1f);
    }
}

public enum LoadingMenuType
{
    LoadWorld
    , ConnectServer
    , Dimension
}