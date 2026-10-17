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
    public string GetIcon() { return id; }
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
    public string icon;

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

public class Inventory { public Item data { get { return null; } } }

public class MultiInventory
{
    public bool RemoveItems(List<Item> items, DestinationType destination, List<string> multiquality, List<Item> used) { return false; }
    public List<Inventory> all { get { return null; } }
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
    public bool just_built;
    public long unique_id;
    public ComponentsManager components { get { return null; } }
    public string obj_id;
    public Item data { get { return null; } }

    public MultiInventory GetMultiInventoryOfWGOWithoutWorldZone(bool duplicate_bags = false)
    {
        return null;
    }
}

// Signatures only, verified on the inspected1.407 placement path.
public class BuildModeLogics
{
    public static WorldGameObject last_build_desk;
#pragma warning disable 0169
    private ObjectCraftDefinition _cd;
    private MultiInventory _multi_inventory;
#pragma warning restore 0169
    private void DoPlace() { }
}
public class FloatingWorldGameObject : MonoBehaviour
{
    public static FloatingWorldGameObject cur_floating { get { return null; } }
    public WorldGameObject wobj { get { return null; } }
}
public class MainGame : MonoBehaviour
{
    public static MainGame me;
    public WorldGameObject player;
}

public class BaseGUI : MonoBehaviour
{
    public delegate void OnAnyWindowStateChanged(BaseGUI gui);
    public static event OnAnyWindowStateChanged on_window_opened { add { } remove { } }
    public static event OnAnyWindowStateChanged on_window_closed { add { } remove { } }
    public bool is_shown_and_top { get { return false; } }
    public ButtonTipsStr button_tips { get { return null; } }
    public GamepadNavigationController gamepad_controller { get { return null; } }
    public static List<BaseGUI> opened_windows = new List<BaseGUI>();
}

public class CraftGUI : BaseGUI
{
    public UITable craft_list_grid;
    public UniversalObjectInfoGUI object_info;
    public CraftItemGUI craft_item_prefab;
    public List<CraftItemGUI> GetItemsList() { return null; }
}
public class UniversalObjectInfoGUI : MonoBehaviour { public UILabel text_header; }

public class CraftItemGUI : MonoBehaviour
{
    public UILabel txt_spend;
    public GameObject amount_buttons;
    public UIWidget selection_frame;
    public UITable ingredients_table;
    public UILabel label_name;
    public BaseItemCellGUI item_gui;
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
    public UILabel zone_name;
    public UILabel zone_descr;
    public UILabel version_label;
    public UILabel day_label;
    public UIPanel panel;
    public Transform time_circle_rotating;
}

public class GameSettings
{
    public void ApplyLanguageChange()
    {
    }

    public static string GetCurrentLanguage()
    {
        return "en";
    }
}

public class GUIElements : MonoBehaviour
{
    public GameGUI game_gui;
    public UIWidget screen_size_widget;
    public static GUIElements me { get { return null; } }
    public HUD hud;
    public CraftGUI craft;
    public ChestGUI chest;
}
public class GameGUI : BaseGUI { }

public class ComponentsManager { public BaseCharacterComponent character { get { return null; } } }
public class BaseCharacterComponent { public bool control_enabled { get; set; } }

public class BaseItemCellGUI : MonoBehaviour { public BaseItemCellElements x1; }
public class BaseItemCellElements { public UI2DSprite icon; }
public class EasySpritesCollection : ScriptableObject
{
    public static Sprite GetSprite(string name, bool noError, string collection) { return null; }
}
public enum GameKey { Work=8, Left=16, Right=17, Up=18, Down=19, Back=21, Option2 = 23, PrevSubTab = 28, NextSubTub = 29 }
public class GamepadNavigationItem : MonoBehaviour { }
public class GamepadNavigationController { public GamepadNavigationItem focused_item { get { return null; } } }
public class GameKeyTip
{
    public static GameKeyTip RightStick(bool active, bool gamepadOnly, bool translate, string text) { return null; }
    public static string Get(GameKey key, string text, bool active, bool gamepadOnly, bool translate, bool previousNewline) { return text; }
}
public class ButtonTipsStr : MonoBehaviour { public UILabel label; }
public class LazyInput : MonoBehaviour
{
    public static bool GetKeyDown(GameKey key) { return false; }
    public static void WaitForRelease(GameKey key) { }
    public static bool gamepad_active { get { return false; } }
    public static event System.Action on_input_changed { add { } remove { } }
}

public static class TooltipsManager
{
    public static void Redraw()
    {
    }
}
