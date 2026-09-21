using System.Collections.Generic;
using UnityEngine;

public class BalanceBaseObject
{
    public string id;
}

public class GameBalanceBase : ScriptableObject
{
    public T GetDataOrNull<T>(string id) where T : BalanceBaseObject
    {
        return null;
    }
}

public class GameBalance : GameBalanceBase
{
    public static GameBalance me { get { return null; } }
}

public class ItemDefinition : BalanceBaseObject
{
    public string GetItemName(bool localized = true)
    {
        return id;
    }
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
    public bool one_time_craft;
    public string change_wgo;

    public string GetNameNonLocalized()
    {
        return id;
    }
}

public class ObjectCraftDefinition : CraftDefinition
{
    public enum BuildType
    {
        Put = 0,
        Remove = 1,
        None = 2
    }

    public BuildType build_type;
    public string out_obj;
}

public class Item
{
    public string id;
    public int value;
    public List<string> multiquality_items;
    public List<Item> inventory;

    public Item(string itemId, int itemValue)
    {
        id = itemId;
        value = itemValue;
    }

    public bool AddItem(Item item, bool bags_allowed = true)
    {
        return false;
    }

    public bool RemoveItem(Item item, int count = 0, Item try_from_bag = null)
    {
        return false;
    }

    public int GetTotalCount(string itemId, bool count_in_bags = true)
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

    public int GetTotalCount(string itemId, DestinationType destination = DestinationType.AllFromFirst, bool count_in_bags = true)
    {
        return 0;
    }

    public int CanAddCount(string itemId, bool count_bags = false)
    {
        return 0;
    }

    public bool MoveItemTo(MultiInventory anotherInventory, Item item, int count = 0, bool use_only_first_from_inventory = false, bool allow_bag = true)
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
}

public class MainGame : MonoBehaviour
{
    public static MainGame me;
    public WorldGameObject player;
}

public class BaseGUI : MonoBehaviour
{
    public static List<BaseGUI> opened_windows = new List<BaseGUI>();
}

public class CraftGUI : BaseGUI
{
}

public class CraftItemGUI : MonoBehaviour
{
    public CraftDefinition current_craft { get { return null; } }
}

public class CraftsInventory
{
    public List<ObjectCraftDefinition> GetObjectCraftsList()
    {
        return new List<ObjectCraftDefinition>();
    }
}

public class InventoryPanelGUI : MonoBehaviour
{
    public void Redraw()
    {
    }
}

public class ChestGUI : BaseGUI
{
    public InventoryPanelGUI player_panel;
    public InventoryPanelGUI chest_panel;
}

public class HUD : MonoBehaviour
{
    public UILabel zone_descr;
    public UILabel version_label;
}

public class GUIElements : MonoBehaviour
{
    public static GUIElements me { get { return null; } }
    public HUD hud;
}

public static class TooltipsManager
{
    public static void Redraw()
    {
    }
}
