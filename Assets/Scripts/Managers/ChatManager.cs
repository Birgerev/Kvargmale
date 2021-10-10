using System;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager instance;

    public void Start()
    {
        instance = this;
    }

    [Command(requiresAuthority = false)]
    public void RequestSendMessage(string text, NetworkConnectionToClient sender = null)
    {
        PlayerInstance player = sender.identity.GetComponent<PlayerInstance>();

        if (text.StartsWith("/"))
        {
            ExcecuteCommand(text.Split(' '), player);
            return;
        }

        AddMessage("<" + player.playerName + "> " + text);
    }

    [Server]
    public void ExcecuteCommand(string[] args, PlayerInstance player)
    {
        Debug.Log("executing command> " + args[0]);

        if (args[0].Equals("/give"))
            try
            {
                ItemStack item = new ItemStack((Material) Enum.Parse(typeof(Material), args[1]), int.Parse(args[2]));

                player.playerEntity.GetComponent<Player>().GetInventory().AddItem(item);
                AddMessage("Gave " + player.playerName + " " + item.Amount + " " + item.material + "'s");
            }
            catch (Exception e)
            {
                AddMessage("/give command failed");
                Debug.LogError("chat error: " + e.StackTrace);
            }
            //structure true 27 15
        if (args[0].Equals("/stork"))
            AddMessage("meeeeeee");
        
        if (args[0].Equals("/spawn"))
            try
            {
                string entityType = args[1];

                Entity entity = Entity.Spawn(entityType);
                entity.Teleport(player.playerEntity.GetComponent<Player>().Location);
                AddMessage("Spawned " + entityType);
            }
            catch (Exception e)
            {
                AddMessage("/spawn command failed");
                Debug.LogError("chat error: " + e.StackTrace);
            }
        
        if (args[0].Equals("/structure"))
            try
            {
                bool includeAir = args[1].ToLower().Equals("true");
                int xSize = int.Parse(args[2]);
                int ySize = int.Parse(args[3]);
                Location playerLocation = player.playerEntity.GetComponent<Player>().Location;
                string savePath = Application.dataPath + "\\..\\savedStructure.txt";
                List<string> lines = new List<string>();

                AddMessage("Starting Structure Cloner");
                for (int localX = 0; localX < xSize; localX++)
                {
                    for (int localY = 0; localY < ySize; localY++)
                    {
                        Location worldLocation = playerLocation + new Location(localX, localY);
                        Material mat = worldLocation.GetMaterial();
                        BlockData data = worldLocation.GetData();
                        
                        if(mat == Material.Air && !includeAir)
                            continue;
                        
                        lines.Add(mat.ToString() + "*" + localX + "," + localY + "*" + data.ToString());
                    }
                }
                
                File.WriteAllLines(savePath, lines);
                AddMessage("Structure saved to: " + savePath);
            }
            catch (Exception e)
            {
                AddMessage("/spawn command failed");
                Debug.LogError("chat error: " + e.StackTrace);
            }

        if (args[0].Equals("/help"))
        {
            AddMessage("---- Commands ----");
            AddMessage("/give <Material> <Amount>");
            AddMessage("/spawn <Entity Type>");
            AddMessage("/structure <Air> <x-size> <y-size>");
        }
    }

    [ClientRpc]
    public void AddMessage(string text)
    {
        Debug.Log("Chat> " + text);
        ChatMenu.instance.AddMessage(text);
    }

    [ClientRpc]
    public void AddMessagePlayer(string text, PlayerInstance player)
    {
        if (PlayerInstance.localPlayerInstance == player)
        {
            Debug.Log("Chat> " + text);
            ChatMenu.instance.AddMessage(text);
        }
    }
}