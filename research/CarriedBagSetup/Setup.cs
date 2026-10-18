// SPDX-License-Identifier: MPL-2.0
// Research-only explicit setup and passive native observations. Does not call Planner.
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
[assembly: AssemblyVersion("0.1.0.0")]
[assembly: AssemblyFileVersion("0.1.0.0")]
namespace NikichMods.CarriedBagResearch
{
    [BepInPlugin("nikich.gyk.carriedbagsetup", "Building Planner Carried Bag Setup", "0.1.0")]
    public sealed class Setup : BaseUnityPlugin
    {
        private const string Bag = "bag_builder", Material = "flitch", Filler = "cabbage_crop";
        private static Setup _instance;
        private Harmony _harmony;
        private bool _visible, _issued, _failed, _fillUsed;
        private GameSave _save;
        private long _playerId;
        private WorldGameObject _lastChest;
        private int _fillerBefore, _fillerAdded;
        private Rect _window = new Rect(20, 60, 580, 390);
        private GUIStyle _label;
        private string _message = "Сохранитесь ДО теста. Загрузите персонажа, освободите две ячейки и закройте игровые меню.";
        private string _report = "Выдача: сумка строителя с 5 досками внутри + 25 досок отдельно.";
        private string _lastSnapshot;
        private void Awake()
        {
            _instance = this;
            try { _harmony = new Harmony("nikich.gyk.carriedbagsetup"); _harmony.PatchAll(typeof(Setup).Assembly); Log("event=ready version=0.1.0 hotkey=F8 synthetic_setup=explicit host_mvid=" + typeof(Item).Assembly.ManifestModule.ModuleVersionId); }
            catch (Exception ex) { Fail(ex); if (_harmony != null) _harmony.UnpatchSelf(); }
        }
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F8)) return;
            _visible = !_visible;
            if (_visible && _issued) Run(() => Snapshot("manual_F8", true));
        }
        private void OnGUI()
        {
            if (_visible) _window = GUILayout.Window(GetInstanceID(), _window, Draw, "Проверка переносной сумки — F8 закрыть");
        }
        private void Draw(int id)
        {
            if (_label == null) _label = new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUILayout.Label("Временный помощник. Не сохраняйтесь и не спите после выдачи. Полный откат: загрузите сохранение ДО теста.", _label);
            bool enabled = GUI.enabled;
            GUI.enabled = enabled && !_failed && !_issued;
            if (GUILayout.Button("Выдать сумку с 5 досками + 25 отдельно", GUILayout.Height(34))) Run(Grant);
            GUI.enabled = enabled && !_failed && _issued && !_fillUsed;
            if (GUILayout.Button("Заполнить основной инвентарь тестовой капустой", GUILayout.Height(30))) Run(Fill);
            GUI.enabled = enabled && !_failed && _issued && _fillerAdded > 0;
            if (GUILayout.Button("Убрать добавленную капусту", GUILayout.Height(28))) Run(RemoveFiller);
            GUI.enabled = enabled && !_failed && _issued;
            if (GUILayout.Button("Записать текущее состояние в лог", GUILayout.Height(28))) Run(() => Snapshot("manual_button", true));
            GUI.enabled = enabled;
            GUILayout.Label(_message, _label);
            GUILayout.Label(_report, _label);
            GUILayout.Label("Наполнение капустой тоже может попасть в сейв. Для полного удаления сумки, досок и капусты перезагрузите исходное сохранение без сохранения теста.", _label);
            if (GUILayout.Button("Закрыть")) _visible = false;
            GUI.DragWindow(new Rect(0, 0, 580, 25));
        }
        private void Log(string text) { Logger.LogInfo("CARRIED_BAG_SETUP " + text); }
        private void Fail(Exception ex)
        {
            _failed = true;
            _message = "Помощник отключён после ошибки. Не повторяйте выдачу; загрузите сейв ДО теста и сообщите результат.";
            Log("event=setup_error action=disable_helper production_unchanged=True error=" + ex);
        }
        private void Run(Action action) { if (_failed) return; try { action(); } catch (Exception ex) { Fail(ex); } }
        private bool Context(out WorldGameObject player)
        {
            player = MainGame.me != null ? MainGame.me.player : null;
            if (player == null || player.data == null || MainGame.me.save == null)
            { _message = "Сначала загрузите сохранение и дождитесь персонажа."; return false; }
            if (_issued && (!ReferenceEquals(_save, MainGame.me.save) || player.unique_id != _playerId))
            { _message = "Сохранение/персонаж сменились. Выдача и заполнение не повторяются; тест завершён. Можно закрыть игру."; return false; }
            return true;
        }
        private void Grant()
        {
            WorldGameObject player;
            if (_issued || !Context(out player)) return;
            if (GameBalance.me == null) { _message = "Баланс игры ещё не загружен."; return; }
            foreach (Item item in player.data.inventory)
                if (item != null && item.is_bag) { _message = "Для изолированной проверки сначала положите существующие сумки в сундук."; return; }
            var bag = new Item(Bag, 1);
            var sample = new Item(Material, 5);
            var filler = new Item(Filler, 1);
            if (!bag.is_bag || bag.inventory_size != 16 || !sample.CanBeInsertedInBag(bag) || filler.CanBeInsertedInBag(bag))
                throw new InvalidOperationException("Native bag/filter definitions differ from inspected 1.407");
            if (!bag.AddItem(sample, false) || bag.GetTotalCount(Material, true) != 5)
                throw new InvalidOperationException("Native detached bag initialization failed before grant");
            var loose = new Item(Material, 25);
            if (!player.data.CanAddItems(new List<Item> { bag, loose }, false))
            { _message = "Освободите две обычные ячейки. Ничего не выдано."; Log("event=grant result=no_capacity"); return; }
            int before = player.data.GetTotalCount(Material, true);
            _issued = true; _save = MainGame.me.save; _playerId = player.unique_id;
            Log("event=grant_begin synthetic_resources=True save_persistent=True carried_before=" + before);
            if (!player.data.AddItem(bag, false) || !player.data.AddItem(loose, false))
                throw new InvalidOperationException("Native grant refused; reload pre-test save");
            if (player.data.GetTotalCount(Material, true) != before + 30 || BagCount(player.data) != 5)
                throw new InvalidOperationException("Native grant count mismatch; reload pre-test save");
            _message = "Выдано: сумка с 5 досками + 25 отдельно. Перемещайте обычными кнопками игры. F8 записывает состояние при открытии.";
            Snapshot("grant_verified", true);
        }
        private void Fill()
        {
            WorldGameObject player;
            if (!_issued || _fillUsed || !Context(out player)) return;
            if (BagCount(player.data) <= 0 || player.data.GetTotalCount(Material, false) != 0)
            { _message = "Сумка с досками должна быть при персонаже. Все отдельные доски сначала переложите в сундук."; return; }
            int count = player.data.CanAddCount(Filler, true, false);
            if (count <= 0) { _message = "Основной инвентарь уже заполнен. Проверьте снимок и переходите к сундуку."; Snapshot("fill_not_needed", true); return; }
            if (count > 10000) throw new InvalidOperationException("Unexpected native ordinary capacity");
            _fillerBefore = player.data.GetTotalCount(Filler, false);
            _fillUsed = true; _fillerAdded = count;
            Log("event=fill_begin synthetic_resources=True save_persistent=True added=" + count + " before=" + _fillerBefore);
            if (!player.data.AddItem(new Item(Filler, count), false) || player.data.GetTotalCount(Filler, false) != _fillerBefore + count)
                throw new InvalidOperationException("Native filler add mismatch; reload pre-test save");
            if (player.data.CanAddCount(Material, true, false) != 0)
                throw new InvalidOperationException("Ordinary inventory still accepts material; bag-only test not ready");
            _message = "Основной инвентарь заполнен; капуста в сумку строителя не помещается. Закройте F8, откройте сундук и нажмите «Взять необходимое».";
            Snapshot("main_full_verified", true);
        }
        private void RemoveFiller()
        {
            WorldGameObject player;
            if (_fillerAdded <= 0 || !Context(out player)) return;
            if (player.data.GetTotalCount(Filler, false) != _fillerBefore + _fillerAdded)
            { _message = "Количество капусты изменилось: автоматическая очистка запрещена. Загрузите сейв ДО теста."; return; }
            if (!player.data.RemoveItem(Filler, _fillerAdded, null) || player.data.GetTotalCount(Filler, false) != _fillerBefore)
                throw new InvalidOperationException("Native filler cleanup mismatch; reload pre-test save");
            Log("event=filler_cleanup result=verified removed=" + _fillerAdded);
            _fillerAdded = 0;
            _message = "Добавленная капуста убрана. Для полного отката сумки, досок и изменений плана загрузите сейв ДО теста без сохранения.";
            Snapshot("filler_removed", true);
        }
        private static int BagCount(Item owner)
        {
            int count = 0;
            if (owner != null && owner.inventory != null)
                foreach (Item item in owner.inventory)
                    if (item != null && item.id == Bag && item.is_bag) count += item.GetTotalCount(Material, true);
            return count;
        }
        private void Snapshot(string cause, bool force)
        {
            WorldGameObject player;
            if (!_issued || !Context(out player)) return;
            int main = player.data.GetTotalCount(Material, false);
            int carried = player.data.GetTotalCount(Material, true);
            int ownBag = BagCount(player.data);
            int mainCapacity = player.data.CanAddCount(Material, true, false);
            int carriedCapacity = player.GetMultiInventoryOfWGOWithoutWorldZone(true).CanAddCount(Material, true);
            string chest = _lastChest == null ? "none" : _lastChest.unique_id.ToString();
            int chestCount = _lastChest == null ? -1 : _lastChest.data.GetTotalCount(Material, true);
            int chestBag = _lastChest == null ? -1 : BagCount(_lastChest.data);
            string state = "item=" + Material + " main=" + main + " carried=" + carried + " carried_builder_bag=" + ownBag +
                " main_capacity=" + mainCapacity + " carried_capacity=" + carriedCapacity + " last_chest_uid=" + chest +
                " chest_total=" + chestCount + " chest_builder_bag=" + chestBag;
            _report = "Доски: основной " + main + ", всего при себе " + carried + ", в переносимой сумке " + ownBag +
                ". Вместимость для досок: основной " + mainCapacity + ", с сумкой " + carriedCapacity + ".";
            if (force || state != _lastSnapshot) Log("event=snapshot cause=" + cause + " " + state);
            _lastSnapshot = state;
        }
        internal static void Chest(WorldGameObject chest)
        {
            if (_instance == null || _instance._failed) return;
            _instance.Run(() => { _instance._lastChest = chest; if (_instance._issued) _instance.Snapshot("chest_open", true); });
        }
        internal static void Moved(bool result)
        {
            if (_instance == null || !_instance._issued || _instance._failed) return;
            _instance.Run(() => _instance.Snapshot("native_move_" + result, false));
        }
        private void OnDestroy() { if (_harmony != null) _harmony.UnpatchSelf(); if (_instance == this) _instance = null; }
    }
    [HarmonyPatch(typeof(ChestGUI), "Open")]
    internal static class ChestObservation { private static void Postfix(WorldGameObject chest_obj) { Setup.Chest(chest_obj); } }
    [HarmonyPatch(typeof(MultiInventory), "MoveItemTo", new Type[] { typeof(MultiInventory), typeof(Item), typeof(int), typeof(bool), typeof(bool) })]
    internal static class MoveObservation { private static void Postfix(bool __result) { Setup.Moved(__result); } }
}
