using System.Collections.Generic;
using UnityEngine;

public class BalanceBaseObject
{
    public string id;
}

public class CraftDefinition : BalanceBaseObject
{
    public enum CraftType
    {
        None = 0,
        ResourcesBasedCraft = 1,
        Survey = 2,
        MixedCraft = 3,
        Fixing = 4,
        AlchemyDecompose = 5,
        PrayCraft = 6,
        RatBuff = 7,
        RefugeeCampCraft = 8
    }

    public CraftType craft_type;
    public List<Item> needs;
    public List<Item> needs_from_wgo;
}

public class ObjectCraftDefinition : CraftDefinition
{
}

public class Item
{
    public string id;
    public int value;
    public List<Item> inventory;
    public List<string> multiquality_items;

    public Item(string item_id, int item_value)
    {
        id = item_id;
        value = item_value;
    }

    public int GetTotalCount(string item_id, bool count_in_bags = true)
    {
        return 0;
    }
}

public class MultiInventory
{
    public enum DestinationType
    {
        OnlyFirst,
        AllFromFirst,
        AllFromLast
    }

    public int GetTotalCount(string item_id, DestinationType destination = DestinationType.AllFromFirst, bool count_in_bags = true)
    {
        return 0;
    }

    public int CanAddCount(string item_id, bool count_bags = false)
    {
        return 0;
    }

    public bool MoveItemTo(MultiInventory another_inventory, Item item, int count = 0, bool use_only_first_from_inventory = false, bool allow_bag = true)
    {
        return false;
    }
}

public class WorldGameObject : MonoBehaviour
{
    public string obj_id;
    public Item data { get { return null; } }

    public MultiInventory GetMultiInventoryOfWGOWithoutWorldZone(bool duplicate_bags = false)
    {
        return null;
    }

    public MultiInventory GetMultiInventoryForInteraction(List<WorldGameObject> exceptions = null)
    {
        return null;
    }
}

public class MainGame : MonoBehaviour
{
    public static MainGame me;
    public WorldGameObject player;
}

public class BaseGUI : MonoBehaviour
{
}

public class ChestGUI : BaseGUI
{
    public InventoryPanelGUI player_panel;
    public InventoryPanelGUI chest_panel;
}

public class InventoryPanelGUI : MonoBehaviour
{
    public void Redraw()
    {
    }
}

public class CraftItemGUI : MonoBehaviour
{
    public CraftDefinition current_craft { get { return null; } }
}

public class BuildItemGUI : MonoBehaviour
{
    public CraftDefinition definition { get { return null; } }
}

public static class TooltipsManager
{
    public static void Redraw()
    {
    }
}
