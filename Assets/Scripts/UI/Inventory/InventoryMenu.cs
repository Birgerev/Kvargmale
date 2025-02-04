﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenu : NetworkBehaviour
{
    [SyncVar] public PlayerInstance ownerPlayerInstance;

    public List<Transform> inventorySlotLists = new List<Transform>();
    public PointerSlot pointerSlot;
    public CanvasGroup canvasGroup;
    public Text inventoryTitle;
    private float inventoryAge;
    public readonly SyncDictionary<int, int> inventoryIds = new SyncDictionary<int, int>();
    [SyncVar] public ItemStack pointerItem = new ItemStack();

    public virtual void Update()
    {
        bool ownsInventoryMenu = (PlayerInstance.localPlayerInstance != null &&
                                 PlayerInstance.localPlayerInstance == ownerPlayerInstance);

        canvasGroup.alpha = ownsInventoryMenu ? 1 : 0;
        canvasGroup.interactable = ownsInventoryMenu;
        canvasGroup.blocksRaycasts = ownsInventoryMenu;
        if (ownsInventoryMenu)
        {
            CheckClose();

            inventoryAge += Time.deltaTime;
        }
    }

    public override void OnStartClient()
    {
        ClientInventoryUpdate();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        OpenPlayerInventory();
    }

    public virtual void OpenPlayerInventory()
    {
        inventoryIds.Add(1, ownerPlayerInstance.playerEntity.inventoryId);
    }

    [Command(requiresAuthority = false)]
    public virtual void RequestInventoryUpdate()
    {
        UpdateInventory();
    }

    [Server]
    public virtual void UpdateInventory()
    {
        CallClientInventoryUpdate();
    }

    [ClientRpc]
    public virtual void CallClientInventoryUpdate()
    {
        StartCoroutine(awaitInventoryChangesToUpdate());
    }

    private IEnumerator awaitInventoryChangesToUpdate()
    {
        yield return new WaitForSeconds(0f);
        ClientInventoryUpdate();
    }

    [Client]
    public virtual void ClientInventoryUpdate()
    {
        SetTitle();
        UpdateSlots();
    }

    [Client]
    public virtual void CheckClose()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E)) && inventoryAge > 0.1f)
            foreach (int inventoryId in inventoryIds.Values.ToList())
                Inventory.Get(inventoryId).RequestClose();
    }

    [Server]
    public void Close()
    {
        pointerItem.Drop(Inventory.Get(inventoryIds[0]).holder);
        NetworkServer.Destroy(gameObject);
    }

    [Client]
    public virtual void SetTitle()
    {
        inventoryTitle.text = Inventory.Get(inventoryIds[0]).invName;
    }

    [Client]
    public void UpdateSlots()
    {
        //Update every slot in every inventory
        for (int inventoryIndex = 0; inventoryIndex < inventorySlotLists.Count; inventoryIndex++)
        {
            ItemSlot[] slots = GetSlots(inventoryIndex);

            for (int slotId = 0; slotId < slots.Length; slotId++)
            {
                slots[slotId].item = GetItem(inventoryIndex, slotId);
                slots[slotId].UpdateSlotContents();
            }
        }

        //Update the pointer slot aswell
        pointerSlot.item = pointerItem;
        pointerSlot.UpdateSlotContents();
    }

    [Client]
    public ItemSlot GetSlot(int inventoryIndex, int slot)
    {
        if (slot < GetSlots(inventoryIndex).Length)
            return GetSlots(inventoryIndex)[slot];
        return null;
    }

    [Client]
    public ItemSlot[] GetSlots(int inventoryIndex)
    {
        return inventorySlotLists[inventoryIndex].GetComponentsInChildren<ItemSlot>();
    }

    public ItemStack GetItem(int inventoryIndex, int slot)
    {
        return Inventory.Get(inventoryIds[inventoryIndex]).GetItem(slot);
    }

    [Server]
    public void SetItem(int inventoryIndex, int slot, ItemStack item)
    {
        Inventory.Get(inventoryIds[inventoryIndex]).SetItem(slot, item);
    }

    [Server]
    public void SetPointerItem(ItemStack item)
    {
        pointerItem = item;
    }

    [Client]
    public int GetSlotIndex(ItemSlot slot)
    {
        return slot.transform.GetSiblingIndex();
    }

    [Client]
    public int GetSlotInventoryIndex(ItemSlot slot)
    {
        for (int inventoryIndex = 0; inventoryIndex < inventorySlotLists.Count; inventoryIndex++)
        {
            Transform inventorySlotList = inventorySlotLists[inventoryIndex];

            if (slot.transform.IsChildOf(inventorySlotList))
                return inventoryIndex;
        }

        Debug.LogError("No tracked inventory was found for slot '" + slot.gameObject.name + "' in " + name);
        return -1;
    }

    [Command(requiresAuthority = false)]
    public virtual void CMD_OnClickBackground()
    {
        if (pointerItem.material == Material.Air)
            return;
        
        Player player = ownerPlayerInstance.playerEntity;
        player.GetInventoryHandler().DropItem(pointerItem);
        
        SetPointerItem(new ItemStack(Material.Air));
        UpdateInventory();
    }
    
    [Client]
    public virtual void OnClickSlot(int inventoryIndex, int slotIndex, ClickType clickType)
    {
        switch (clickType)
        {
            case ClickType.LeftClick:
                OnLeftClickSlot(inventoryIndex, slotIndex);
                break;
            case ClickType.RightClick:
                OnRightClickSlot(inventoryIndex, slotIndex);
                break;
            case ClickType.RightHold:
                OnRightDragSlot(inventoryIndex, slotIndex);
                break;
            case ClickType.ShiftClick:
                OnShiftClickSlot(inventoryIndex, slotIndex);
                break;
        }
    }

    [Command(requiresAuthority = false)]
    public virtual void OnRightClickSlot(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);

        //Right Click to leave one item
        if ((slotItem.material == Material.Air || slotItem.material == pointerItem.material)
            && pointerItem.Amount > 0 && slotItem.Amount + 1 <= Inventory.MaxStackSize)
        {
            SlotAction_LeaveOne(inventoryIndex, slotIndex);

            return;
        }

        //Right click to halve
        if ((pointerItem.Amount == 0 || pointerItem.material == Material.Air) && slotItem.Amount > 0)
            SlotAction_Halve(inventoryIndex, slotIndex);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnLeftClickSlot(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);

        //Left click to swap
        if (pointerItem.material == Material.Air || pointerItem.material != slotItem.material)
            SlotAction_Swap(inventoryIndex, slotIndex);
        else
            //Left click to add pointer to slot
            SlotAction_MergeSlot(inventoryIndex, slotIndex);
    }

    [Command(requiresAuthority = false)]
    public virtual void OnRightDragSlot(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);

        //Right Click to leave one item
        if ((slotItem.material == Material.Air || slotItem.material == pointerItem.material)
            && pointerItem.Amount > 0 && slotItem.Amount + 1 <= Inventory.MaxStackSize)
        {
            SlotAction_LeaveOne(inventoryIndex, slotIndex);
            return;
        }
    }
    
    [Command(requiresAuthority = false)]
    public virtual void OnShiftClickSlot(int inventoryIndex, int slotIndex)
    {
        //If two inventories are open
        if(inventoryIds.Keys.Count >= 2)
        {
            SlotAction_MoveStackToSecondInventory(inventoryIndex, slotIndex);
            return;
        }
    }

    [Server]
    public virtual void SlotAction_LeaveOne(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);
        ItemStack newPointerItem = pointerItem;

        if (slotItem.material == newPointerItem.material)
        {
            slotItem.Amount++;
        }
        else
        {
            slotItem = pointerItem;
            slotItem.Amount = 1;
        }

        SetItem(inventoryIndex, slotIndex, slotItem);

        newPointerItem.Amount--;
        SetPointerItem(newPointerItem);

        UpdateInventory();
    }

    [Server]
    public virtual void SlotAction_Halve(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);

        int newPointerAmount = Mathf.CeilToInt(slotItem.Amount / 2f);
        int newSlotAmount = Mathf.FloorToInt(slotItem.Amount / 2f);

        ItemStack newPointerItem = slotItem;
        newPointerItem.Amount = newPointerAmount;
        SetPointerItem(newPointerItem);

        slotItem.Amount = newSlotAmount;
        SetItem(inventoryIndex, slotIndex, slotItem);

        UpdateInventory();
    }

    [Server]
    public virtual void SlotAction_Swap(int inventoryIndex, int slotIndex)
    {
        ItemStack oldSlotItem = GetItem(inventoryIndex, slotIndex);

        SetItem(inventoryIndex, slotIndex, pointerItem);
        SetPointerItem(oldSlotItem);

        UpdateInventory();
    }

    [Server]
    public virtual void SlotAction_MergeSlot(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);
        ItemStack newPointerItem = pointerItem;

        int maxAmountOfItemsToMerge = Inventory.MaxStackSize - slotItem.Amount;
        int amountOfItemsFromPointerToMerge = Mathf.Clamp(newPointerItem.Amount, 0, maxAmountOfItemsToMerge);

        newPointerItem.Amount -= amountOfItemsFromPointerToMerge;
        slotItem.Amount += amountOfItemsFromPointerToMerge;
        SetItem(inventoryIndex, slotIndex, slotItem);
        SetPointerItem(newPointerItem);

        UpdateInventory();
    }
    
    [Server]
    public virtual void SlotAction_MoveStackToSecondInventory(int inventoryIndex, int slotIndex)
    {
        ItemStack slotItem = GetItem(inventoryIndex, slotIndex);
        
        //If current inventory index is 0, other inventory index is 1, and vice versa
        int otherInventoryIndex = (inventoryIndex == 0 ? 1 : 0);
        Inventory otherInventory = Inventory.Get(inventoryIds[otherInventoryIndex]);

        //Try to add stack to other inventory
        if (otherInventory.AddItem(slotItem))
        {
            //If successful, remove stack in this inventory
            slotItem = new ItemStack();
        }
        
        SetItem(inventoryIndex, slotIndex, slotItem);
        UpdateInventory();
    }
}