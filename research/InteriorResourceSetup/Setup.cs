// SPDX-License-Identifier: MPL-2.0
// Research setup only. Never starts/completes crafts or changes recipe/world/planner state.
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]
namespace NikichMods.InteriorResourceResearch
{
    [BepInPlugin("nikich.gyk.interiorresourcesetup", "Building Planner Interior Resource Setup", "0.1.0")]
    public sealed class Setup : BaseUnityPlugin
    {
        private static Setup _instance;
        private ObjectCraftDefinition _selected;
        private GameSave _selectedSave;
        private Harmony _harmony;
        private string CraftId { get { return _selected != null ? _selected.id : ""; } }
        private Rect _window = new Rect(20,60,540,360);
        private bool _visible, _issued, _failed;
        private GUIStyle _label;
        private string _message = "Откройте строительное меню и выделите доступный вариант домашнего кухонного стола. Затем F8.";
        private GameSave _grantSave;
        private long _grantPlayer;
        private readonly Dictionary<string,int> _added = new Dictionary<string,int>();
        private readonly Dictionary<string,int> _after = new Dictionary<string,int>();
        private void Awake() { _instance=this; _harmony=new Harmony("nikich.gyk.interiorresourcesetup"); _harmony.PatchAll(typeof(Setup).Assembly); Log("event=ready version=0.1.0 hotkey=F8 mode=explicit_resource_setup host_mvid="+typeof(GameSave).Assembly.ManifestModule.ModuleVersionId); }
        private void Update() { if (Input.GetKeyDown(KeyCode.F8)) _visible = !_visible; }
        private void OnGUI()
        {
            if (!_visible) return;
            _window = GUILayout.Window(GetInstanceID(),_window,Draw,"Тест домашнего интерьера — F8 закрыть");
        }
        private void Draw(int id)
        {
            if (_label==null) _label=new GUIStyle(GUI.skin.label) {wordWrap=true};
            GUILayout.Label("Только подготовка ресурсов для выделенного варианта интерьера. Подтверждайте или отменяйте обычными кнопками игры.",_label);
            GUILayout.Label("Выданные ресурсы и строительство могут попасть в сохранение. Для отката используйте сейв до теста.",_label);
            GUILayout.Label(_selected != null ? "Выбрано: "+GJL.L(_selected.GetNameNonLocalized())+" ("+CraftId+")" : "В домашнем строительном меню выделите доступный кухонный стол, шкаф, кровать или отделку.",_label);
            bool enabled = GUI.enabled;
            GUI.enabled = enabled && !_failed && !_issued && _selected != null;
            if (GUILayout.Button("Выдать недостающее для выбранной постройки",GUILayout.Height(35))) Execute(Grant);
            GUI.enabled = enabled && !_failed && _issued && _added.Count>0;
            if (GUILayout.Button("Убрать выданное до применения",GUILayout.Height(30))) Execute(Cleanup);
            GUI.enabled = enabled;
            GUILayout.Label(_message,_label);
            if (GUILayout.Button("Закрыть")) _visible=false;
            GUI.DragWindow(new Rect(0,0,540,25));
        }
        private void Execute(Action action)
        {
            try { action(); }
            catch(Exception ex) { _failed=true; _message="Ошибка подготовки; выдача отключена. Смотрите лог. Не повторяйте выдачу."; Log("event=setup_error action=disable_setup error="+ex); }
        }
        private void Log(string message) { Logger.LogInfo("INTERIOR_RESOURCE_SETUP "+message); }
        private bool Context(out GameSave save, out WorldGameObject player, out MultiInventory inventory)
        {
            save=null; player=null; inventory=null;
            if (MainGame.me==null || MainGame.me.player==null || MainGame.me.save==null) { _message="Сначала загрузите сейв и дождитесь появления персонажа."; return false; }
            save=MainGame.me.save; player=MainGame.me.player;
            inventory=player.GetMultiInventoryOfWGOWithoutWorldZone(true);
            if (inventory==null) { _message="Переносимый инвентарь недоступен."; return false; }
            if (_selected==null || !ReferenceEquals(save,_selectedSave)) { _message="Сначала выделите постройку на текущем сейве."; return false; }
            return true;
        }
        private void Grant()
        {
            GameSave save; WorldGameObject player; MultiInventory inventory;
            if (_issued || !Context(out save,out player,out inventory)) return;
            if (GameBalance.me==null) { _message="Данные игры ещё не загружены."; return; }
            var definition=GameBalance.me.GetDataOrNull<ObjectCraftDefinition>(CraftId);
            if (definition==null || !Supported(definition) ||
                !ReferenceEquals(definition,_selected) || definition.needs==null || definition.needs.Count==0 ||
                (definition.needs_from_wgo!=null && definition.needs_from_wgo.Count>0))
            { _message="Проверенное определение постройки недоступно; выдача запрещена."; return; }
            var missing=new List<Item>(); var before=new Dictionary<string,int>(); var seen=new HashSet<string>();
            foreach(var need in definition.needs)
            {
                if (need==null || string.IsNullOrEmpty(need.id) || need.value<=0 || !seen.Add(need.id) || (need.multiquality_items!=null && need.multiquality_items.Count>0))
                { _message="Неподдерживаемые требования; выдача запрещена."; return; }
                int have=inventory.GetTotalCount(need.id,MultiInventory.DestinationType.AllFromFirst,true);
                before.Add(need.id,have);
                int count=Math.Max(0,need.value-have);
                if(count>0) missing.Add(new Item(need.id,count));
                Log("event=preview item="+need.id+" required="+need.value+" carried="+have+" missing="+count);
            }
            if(missing.Count==0) { _message="Все материалы уже при вас. Можно закрыть F8 и начать проверку интерьера."; return; }
            if(!inventory.CanAddItems(missing,false)) { _message="Освободите место в инвентаре и повторите. Ничего не выдано."; Log("event=grant result=no_capacity"); return; }
            Log("event=grant_begin craft="+CraftId+" synthetic_resources=True save_persistent=True");
            // Keep a ledger before the mutation; exceptions disable further setup rather than retrying.
            _issued=true; _grantSave=save; _grantPlayer=player.unique_id;
            foreach(var item in missing) { _added.Add(item.id,item.value); _after.Add(item.id,before[item.id]+item.value); }
            bool success=inventory.AddItems(missing,false);
            if(!success)
            {
                bool unchanged=true;
                foreach(var pair in before) unchanged &= inventory.GetTotalCount(pair.Key,MultiInventory.DestinationType.AllFromFirst,true)==pair.Value;
                if(unchanged) { Clear(); _message="Игра отказала в добавлении, инвентарь не изменён."; Log("event=grant_end result=rejected unchanged=True"); return; }
                throw new InvalidOperationException("Native add refused with changed inventory");
            }
            foreach(var pair in _after)
            {
                int actual=inventory.GetTotalCount(pair.Key,MultiInventory.DestinationType.AllFromFirst,true);
                Log("event=grant_item item="+pair.Key+" added="+_added[pair.Key]+" after="+actual+" expected="+pair.Value);
                if(actual!=pair.Value) throw new InvalidOperationException("Grant count mismatch for "+pair.Key);
            }
            _message="Материалы добавлены. Закройте F8. Закрепите этот вариант: сначала отмените выбор, затем повторите и подтвердите. Выдача один раз за запуск.";
            Log("event=grant_end result=verified synthetic_resources=True");
        }
        private static bool Supported(ObjectCraftDefinition definition)
        {
            if (definition==null || definition.build_type!=ObjectCraftDefinition.BuildType.None || !definition.wait_script_callback || !definition.is_auto || definition.one_time_craft || definition.id==null || !definition.id.StartsWith("keeper_room_builddesk::",StringComparison.Ordinal)) return false;
            string script=(definition.end_script??"").Split(':')[0];
            return script=="keeper_bed_place" || script=="keeper_cupboard_place" || script=="keeper_table_place" || script=="keeper_walls_place" || script=="keeper_ornament_place";
        }
        internal static void Select(CraftItemGUI row)
        {
            if (_instance==null || _instance._issued || _instance._failed || row==null) return;
            try
            {
                var definition=row.current_craft as ObjectCraftDefinition;
                if (definition==null || !Supported(definition) || definition.needs==null || definition.needs.Count==0 || MainGame.me==null || MainGame.me.save==null) return;
                if (_instance._selected==definition && ReferenceEquals(_instance._selectedSave,MainGame.me.save)) return;
                _instance._selected=definition; _instance._selectedSave=MainGame.me.save;
                _instance._message="Постройка выбрана. F8 выдаёт только недостающие материалы; технологии и размещение не меняются.";
                _instance.Log("event=selected craft="+definition.id);
            }
            catch(Exception ex) { _instance._failed=true; _instance.Log("event=setup_error phase=select error="+ex); }
        }
        private void OnDestroy() { if (_harmony!=null) _harmony.UnpatchSelf(); if (_instance==this) _instance=null; }
        private void Clear() { _issued=false; _added.Clear(); _after.Clear(); _grantSave=null; _grantPlayer=0; }
        private void Cleanup()
        {
            GameSave save; WorldGameObject player; MultiInventory inventory;
            if(!_issued || !Context(out save,out player,out inventory)) return;
            if(!ReferenceEquals(save,_grantSave) || player.unique_id!=_grantPlayer) { _message="Сейв/персонаж изменился; очистка запрещена. Загрузите сейв до теста для отката."; return; }
            foreach(var pair in _after)
                if(inventory.GetTotalCount(pair.Key,MultiInventory.DestinationType.AllFromFirst,true)!=pair.Value)
                { _message="Количество материалов изменилось. Чтобы не забрать ваши предметы, очистка запрещена; используйте сейв до теста."; return; }
            var remove=new List<Item>(); foreach(var pair in _added) remove.Add(new Item(pair.Key,pair.Value));
            Log("event=cleanup_begin synthetic_resources=True");
            if(!inventory.RemoveItems(remove,MultiInventory.DestinationType.AllFromFirst,null,null)) throw new InvalidOperationException("Native cleanup removal refused");
            foreach(var pair in _after)
                if(inventory.GetTotalCount(pair.Key,MultiInventory.DestinationType.AllFromFirst,true)!=pair.Value-_added[pair.Key]) throw new InvalidOperationException("Cleanup count mismatch");
            Clear(); _message="Выданное убрано, исходные количества восстановлены."; Log("event=cleanup_end result=verified");
        }
    }
    [HarmonyPatch(typeof(CraftItemGUI),"OnMouseOvered")]
    internal static class MouseSelection { private static void Postfix(CraftItemGUI __instance) { Setup.Select(__instance); } }
    [HarmonyPatch(typeof(CraftItemGUI),"OnOver")]
    internal static class GamepadSelection { private static void Postfix(CraftItemGUI __instance) { Setup.Select(__instance); } }
}

