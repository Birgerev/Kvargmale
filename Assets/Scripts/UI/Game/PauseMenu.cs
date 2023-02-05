﻿using Mirror;
using Steamworks;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool active;
    public GameObject optionsMenuPrefab;

    // Start is called before the first frame update
    private void Start()
    {
        active = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetMenuActive(!active);
    }
    
    public void Invite()
    {
        if(SteamManager.Initialized)
            SteamFriends.ActivateGameOverlayInviteDialog(((MultiplayerManager)NetworkManager.singleton).lobbyId);
    }

    public void Options()
    {
        Instantiate(optionsMenuPrefab);
    }

    public void EnterMenu()
    {
        SetMenuActive(true);
    }

    public void BackToGame()
    {
        SetMenuActive(false);
    }

    public void SetMenuActive(bool setActive)
    {
        active = setActive;

        GetComponent<CanvasGroup>().alpha = active ? 1 : 0;
        GetComponent<CanvasGroup>().interactable = active;
        GetComponent<CanvasGroup>().blocksRaycasts = active;
    }

    public void BackToMainMenu()
    {
        ((MultiplayerManager)NetworkManager.singleton).StopConnection();
    }
}