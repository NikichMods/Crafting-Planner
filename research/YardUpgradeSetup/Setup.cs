// SPDX-License-Identifier: MPL-2.0
// Research setup only. Never starts/completes crafts or changes recipe/world/planner state.
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using UnityEngine;
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]
namespace NikichMods.YardUpgradeResearch
{
    [BepInPlugin("nikich.gyk.yardupgradesetup", "Building Planner Yard Upgrade Setup", "0.1.0")]
    public sealed class Setup : BaseUnityPlugin
    {
        private const string CraftId = "mf_wood_builddesk::upgrade_player_buildzone";
        private Rect _window = new Rect(20,60,470,330);
        private bool _visible, _issued, _failed;
        private string _message = "F8: открыть/закрыть. Выполните улучшение обычной кнопкой игры.";
        private GameSave _grantSave;
        private long _grantPlayer;
        private readonly Dictionary<string,int> _added = new Dictionary<string,int>();
        private readonly Dictionary<string,int> _after = new Dictionary<string,int>();
        private void Awake() { Log("event=ready version=0.1.0 hotkey=F8 mode=explicit_resource_setup host_mvid="+typeof(GameSave).Assembly.ManifestModule.ModuleVersionId); }
        private void Update() { if (Input.GetKeyDown(KeyCode.F8)) _visible = !_visible; }
        private void OnGUI()
        {
            if (!_visible) return;
            _window = GUILayout.Window(GetInstanceID(),_window,Draw,"Тест улучшения двора — F8 закрыть");
        }
        private void Draw(int id)
        {
            GUILayout.Label("Только подготовка ресурсов. Улучшение выполняется через обычное меню двора.");
            GUILayout.Label("Выданные ресурсы и улучшение могут попасть в сохранение. Для отката используйте сейв до теста.");
            GUILayout.Label("Ожидаемые затраты: 10 сложных железных деталей, 8 досок, 4 гвоздя. Кнопка читает фактическую цену игры.");
            bool enabled = GUI.enabled;
            GUI.enabled = enabled && !_failed && !_issued;
            if (GUILayout.Button("Выдать недостающее для улучшения двора",GUILayout.Height(35))) Execute(Grant);
            GUI.enabled = enabled && !_failed && _issued && _added.Count>0;
            if (GUILayout.Button("Убрать выданное до улучшения",GUILayout.Height(30))) Execute(Cleanup);
            GUI.enabled = enabled;
            GUILayout.Label(_message);
            if (GUILayout.Button("Закрыть")) _visible=false;
            GUI.DragWindow(new Rect(0,0,470,25));
        }
        private void Execute(Action action)
        {
            try { action(); }
            catch(Exception ex) { _failed=true; _message="Ошибка подготовки; выдача отключена. Смотрите лог. Не повторяйте выдачу."; Log("event=setup_error action=disable_setup error="+ex); }
        }
        private void Log(string message) { Logger.LogInfo("YARD_UPGRADE_SETUP "+message); }
        private bool Context(out GameSave save, out WorldGameObject player, out MultiInventory inventory)
        {
            save=null; player=null; inventory=null;
            if (MainGame.me==null || MainGame.me.player==null || MainGame.me.save==null) { _message="Сначала загрузите сейв и дождитесь появления персонажа."; return false; }
            save=MainGame.me.save; player=MainGame.me.player;
            inventory=player.GetMultiInventoryOfWGOWithoutWorldZone(true);
            if (inventory==null) { _message="Переносимый инвентарь недоступен."; return false; }
            if (save.completed_one_time_crafts==null) { _message="Состояние улучшения недоступно; выдача запрещена."; return false; }
            if (save.completed_one_time_crafts.Contains(CraftId)) { _message="На этом сейве улучшение уже выполнено. Ресурсы не выдаются."; return false; }
            return true;
        }
        private void Grant()
        {
            GameSave save; WorldGameObject player; MultiInventory inventory;
            if (_issued || !Context(out save,out player,out inventory)) return;
            if (GameBalance.me==null) { _message="Данные игры ещё не загружены."; return; }
            var definition=GameBalance.me.GetDataOrNull<ObjectCraftDefinition>(CraftId);
            if (definition==null || !definition.one_time_craft || definition.build_type!=ObjectCraftDefinition.BuildType.None ||
                definition.end_script!="player_buildzone_upgrade" || definition.needs==null || definition.needs.Count==0 ||
                (definition.needs_from_wgo!=null && definition.needs_from_wgo.Count>0))
            { _message="Проверенное определение улучшения недоступно; выдача запрещена."; return; }
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
            if(missing.Count==0) { _message="Все материалы уже при вас. Можно закрыть F8 и открыть меню двора."; return; }
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
            _message="Материалы добавлены. Закройте F8, закрепите расширение двора и выполните его обычной кнопкой.";
            Log("event=grant_end result=verified synthetic_resources=True");
        }
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
}
