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
    public List<Item> output;
    public bool one_time_craft;
    public bool hidden;
    public string change_wgo;
    public string end_script;
    public string end_event;
    public string craft_after_finish;
}

public class ObjectCraftDefinition : CraftDefinition
{
    public enum BuildType
    {
        Put = 0,
        Remove = 1,
        None = 2
    }

    public string out_obj;
    public BuildType build_type;
    public List<string> builder_ids;
    public List<string> locked_builders_ids;
    public bool enabled;
    public bool wait_script_callback;
}

public class Item
{
    public string id;
    public int value;
    public List<string> multiquality_items;
}

public class ObjectInteractionDefinition
{
    public string script;
    public string hint;
}

public class ObjectDefinition : BalanceBaseObject
{
    public enum InteractionType
    {
        None = 0,
        Craft = 1,
        RunScript = 2,
        Builder = 4,
        Chest = 5,
        Grave = 6
    }

    public InteractionType interaction_type;
    public bool has_craft;
    public bool check_only_interactions;
    public string attached_script;
    public string work;
    public string craft_after_hp_0;
    public string script_after_hp_0;

    public ObjectInteractionDefinition GetValidInteraction(WorldGameObject wgo)
    {
        return null;
    }
}

public abstract class WorldGameObjectComponentBase
{
    public WorldGameObject wgo { get { return null; } }
}

public class CraftComponent : WorldGameObjectComponentBase
{
    public List<CraftDefinition> crafts = new List<CraftDefinition>();
}

public class ComponentsManager
{
    public CraftComponent craft { get { return null; } }
}

public class WorldGameObject : MonoBehaviour
{
    public string obj_id;
    public ObjectDefinition obj_def;
    public bool is_player { get; set; }
    public ComponentsManager components { get { return null; } }
}

public class BaseGUI : MonoBehaviour
{
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

public class BuildModeLogics
{
}

public class GUIElements : MonoBehaviour
{
}
