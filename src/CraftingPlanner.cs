// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace CraftingPlanner
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "nikich.gyk.craftingplanner";
        public const string PluginName = "Building Planner";
        public const string PluginVersion = "1.0.1";

        private Harmony _harmony;

        private void Awake()
        {
            Planner.Log = Logger;
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll();
            PlannerNpcCards.Install();
            PlannerPlacement.Install();
            PlannerWorldCompletion.Install();
            PlannerInteriorCompletion.Install();
            PlannerPersistence.Install();
            LazyInput.on_input_changed += PlannerInputHints.OnInputChanged;
            BaseGUI.on_window_opened += PlannerInputHints.OnWindowChanged;
            BaseGUI.on_window_closed += PlannerInputHints.OnWindowChanged;
            Logger.LogInfo("CRAFTING_PLANNER event=loaded version=" + PluginVersion);
        }

        private void OnDestroy()
        {
            PlannerNpcCards.Stop();
            PlannerPlacement.Stop();
            PlannerWorldCompletion.Stop();
            PlannerInteriorCompletion.Stop();
            PlannerPersistence.Stop();
            PlannerCalibration.Close();
            PlannerEditing.Reset();
            PlannerInventoryRefresh.Reset();
            PlannerRowControls.DestroyAll();
            PlannerChestControls.Destroy();
            PlannerBuildTips.Destroy();
            LazyInput.on_input_changed -= PlannerInputHints.OnInputChanged;
            BaseGUI.on_window_opened -= PlannerInputHints.OnWindowChanged;
            BaseGUI.on_window_closed -= PlannerInputHints.OnWindowChanged;
            PlannerInputHints.Destroy();
            PlannerCraftOverlay.Destroy();
            PlannerHud.Destroy();
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
        }

#if PLANNER_CALIBRATION
        private void Update() { PlannerCalibration.Update(); }
        private void OnGUI() { PlannerCalibration.Draw(); }
#endif
        private void LateUpdate() { PlannerPlacement.Flush(); PlannerWorldCompletion.Flush(); PlannerInteriorCompletion.Flush(); PlannerInventoryRefresh.Flush(); }
    }

    internal enum ProjectContextKind
    {
        None,
        Builder,
        WorldChange
    }

    internal sealed class ProjectGoal
    {
        internal string Key;
        internal CraftDefinition Definition;
        internal int Quantity;
        internal bool Repeatable;
    }

    internal static class Planner
    {
        internal static ManualLogSource Log;

        private static readonly List<ProjectGoal> Goals = new List<ProjectGoal>();
        private static readonly HashSet<CraftDefinition> BuildDefinitions = new HashSet<CraftDefinition>();

        private static ProjectContextKind _context;
        private static WorldGameObject _contextOwner;
        private static CraftDefinition _focusedProject;
        private static WorldGameObject _currentChest;

        internal static bool CanAdjustFocusedProject
        {
            get { return _focusedProject != null && IsSupportedProject(_focusedProject); }
        }

        internal static bool HasGoals
        {
            get { return Goals.Count > 0; }
        }

        internal static void ResetSession()
        {
            PlannerPlacement.Reset();
            PlannerWorldCompletion.Reset();
            PlannerInteriorCompletion.Reset();
            PlannerCalibration.Close();
            PlannerEditing.Reset();
            PlannerRowControls.RestoreAll();
            PlannerNpcCards.Reset();
            Goals.Clear();
            ClearProjectContext();
            _currentChest = null;
            PlannerCraftOverlay.Hide();
            LogLine("event=session_reset");
            PlannerPersistence.Restore();
            PlannerHud.Refresh();
        }

        internal static void OpenBuildContext(WorldGameObject builder, CraftsInventory craftsInventory)
        {
            _context = ProjectContextKind.Builder;
            _contextOwner = builder;
            _focusedProject = null;
            BuildDefinitions.Clear();

            List<ObjectCraftDefinition> definitions = null;
            try
            {
                definitions = craftsInventory != null ? craftsInventory.GetObjectCraftsList() : null;
            }
            catch (Exception ex)
            {
                LogLine("event=project_context kind=builder result=error error=" + ex.GetType().Name);
            }

            if (definitions != null)
            {
                foreach (ObjectCraftDefinition definition in definitions)
                {
                    if (definition != null)
                    {
                        BuildDefinitions.Add(definition);
                    }
                }
            }

            LogLine(
                "event=project_context kind=builder owner=" + Safe(builder != null ? builder.obj_id : null) +
                " count=" + BuildDefinitions.Count);
        }

        internal static void OpenWorldContext(WorldGameObject owner)
        {
            _context = ProjectContextKind.WorldChange;
            _contextOwner = owner;
            _focusedProject = null;
            BuildDefinitions.Clear();

            LogLine("event=project_context kind=world owner=" + Safe(owner != null ? owner.obj_id : null));
        }

        internal static void ClearProjectContext()
        {
            _context = ProjectContextKind.None;
            _contextOwner = null;
            _focusedProject = null;
            BuildDefinitions.Clear();
        }

        internal static void Focus(CraftDefinition definition)
        {
            _focusedProject = IsSupportedProject(definition) ? definition : null;
            PlannerCraftOverlay.Refresh();
        }

        internal static void Blur(CraftDefinition definition)
        {
            if (_focusedProject == definition)
            {
                _focusedProject = null;
                PlannerCraftOverlay.Refresh();
            }
        }

        internal static bool ShouldShowCraftOverlay
        {
            get { return _context == ProjectContextKind.Builder || _focusedProject != null; }
        }

        internal static bool AdjustFocusedProject(int delta, string trigger)
        {
            CraftDefinition definition = _focusedProject;
            if (definition == null || !IsSupportedProject(definition))
            {
                return false;
            }

            return AdjustDefinition(definition, delta, trigger);
        }

        internal static bool AdjustPinned(ProjectGoal goal, int delta, string trigger)
        {
            ProjectGoal updated;
            if (!PlannerGoalState.AdjustPinned(Goals, goal, delta, out updated)) return false;
            return NotifyAdjustment(updated, delta, trigger);
        }

        internal static int QuantityFor(CraftDefinition definition)
        {
            var goal = Goals.FirstOrDefault(x => x.Key == BuildGoalKey(definition));
            return goal != null ? goal.Quantity : 0;
        }
        internal static ProjectGoal FindPlacement(ObjectCraftDefinition definition, string builder)
        { return PlannerGoalState.FindPlacement(Goals, definition, builder); }
        internal static bool CompletePlacement(PlacementAttempt attempt, bool nativeCompleted, bool becameBuilt)
        {
            ProjectGoal updated;
            if (!attempt.Complete(Goals, nativeCompleted, becameBuilt, out updated)) return false;
            // A logging failure must not lose the pending refresh after state was changed.
            try { LogLine("event=goal_placed key=" + Safe(updated.Key) + " quantity=" + updated.Quantity +
                " object_uid=" + (attempt.Object != null ? attempt.Object.unique_id : 0)); } catch { }
            return true;
        }
        internal static ProjectGoal FindWorldCompletion(CraftDefinition definition, string owner)
        { return PlannerGoalState.FindWorldCompletion(Goals, definition, owner) ?? PlannerGoalState.FindBuilderCompletion(Goals, definition, owner); }
        internal static ProjectGoal FindInteriorCompletion(CraftDefinition definition, string owner)
        { return PlannerGoalState.FindInteriorCompletion(Goals, definition, owner); }
        internal static bool CompleteInteriorProject(InteriorCompletionAttempt attempt)
        {
            ProjectGoal updated;
            if (!attempt.Complete(Goals, out updated)) return false;
            try { LogLine("event=goal_completed family=interior key=" + Safe(updated.Key) + " quantity=" + updated.Quantity + " object_uid=" + attempt.ObjectUid); } catch { }
            return true;
        }
        internal static bool CompleteWorldProject(WorldCompletionAttempt attempt, bool nativeCompleted)
        {
            ProjectGoal updated;
            if (!attempt.Complete(Goals, nativeCompleted, out updated)) return false;
            try { LogLine("event=goal_completed key=" + Safe(updated.Key) + " quantity=" + updated.Quantity +
                " object_uid=" + attempt.ObjectUid); } catch { }
            return true;
        }
        internal static bool AdjustProject(CraftDefinition definition, int delta)
        {
            return IsSupportedProject(definition) && AdjustDefinition(definition, delta, "mouse_project_arrow");
        }
        private static bool AdjustDefinition(CraftDefinition definition, int delta, string trigger)
        {
            ProjectGoal updated;
            if (!PlannerGoalState.Adjust(Goals, definition, BuildGoalKey(definition), IsRepeatable(definition), delta, out updated)) return false;
            return NotifyAdjustment(updated, delta, trigger);
        }
        private static bool NotifyAdjustment(ProjectGoal goal, int delta, string trigger)
        {
            LogLine("event=goal_adjust action=" + (delta > 0 ? "add" : goal.Quantity <= 0 ? "remove" : "decrement") +
                " trigger=" + trigger + " id=" + Safe(goal.Definition.id) + " quantity=" + goal.Quantity + " repeatable=" + goal.Repeatable);
            PlannerRowControls.RefreshAll();
            PlannerPresentation.Refresh();
            return true;
        }

        internal static Dictionary<string, int> AggregateRequirements()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (ProjectGoal goal in Goals)
            {
                if (goal == null || goal.Definition == null || goal.Quantity <= 0)
                {
                    continue;
                }

                List<Item> needs = goal.Definition.needs;
                if (needs == null)
                {
                    continue;
                }

                foreach (Item need in needs)
                {
                    if (need == null || string.IsNullOrEmpty(need.id) || need.value <= 0)
                    {
                        continue;
                    }

                    long amount = (long)need.value * goal.Quantity;
                    int current;
                    result.TryGetValue(need.id, out current);
                    long total = (long)current + amount;
                    result[need.id] = total > int.MaxValue ? int.MaxValue : (int)total;
                }
            }

            return result;
        }

        internal static IList<ProjectGoal> GetGoals()
        {
            return Goals;
        }

        internal static int GetPlayerHave(string itemId)
        {
            if (MainGame.me == null || MainGame.me.player == null || MainGame.me.player.data == null)
            {
                return 0;
            }

            return MainGame.me.player.data.GetTotalCount(itemId, true);
        }

        internal static string GetItemName(string itemId)
        {
            try
            {
                ItemDefinition definition = GameBalance.me.GetDataOrNull<ItemDefinition>(itemId);
                if (definition != null)
                {
                    return definition.GetItemName(true);
                }
            }
            catch
            {
            }

            return itemId;
        }

        internal static void OpenChest(WorldGameObject chest)
        {
            PlannerChestControls.Clear();
            _currentChest = chest;
            LogLine("event=chest_open chest=" + Safe(chest != null ? chest.obj_id : null));
        }

        internal static void CloseChest()
        {
            PlannerChestControls.Clear();
            _currentChest = null;
        }

        internal static bool TakeNeeded(ChestGUI chestGui, string trigger)
        {
            if (_currentChest == null || !HasGoals || MainGame.me == null || MainGame.me.player == null)
            {
                return false;
            }

            WorldGameObject player = MainGame.me.player;
            MultiInventory source = _currentChest.GetMultiInventoryOfWGOWithoutWorldZone(true);
            MultiInventory target = player.GetMultiInventoryOfWGOWithoutWorldZone(true);

            if (source == null || target == null)
            {
                PlannerChestControls.Show(new List<KeyValuePair<string, int>>(), TransferNotice.Unavailable);
                LogLine("event=take_needed result=failed reason=null_inventory trigger=" + trigger);
                return true;
            }

            Dictionary<string, int> required = AggregateRequirements();
            bool movedAnything = false;
            bool hadMissing = false, limitedBySpace = false, failed = false;
            var actualMoves = new List<KeyValuePair<string, int>>();

            foreach (KeyValuePair<string, int> pair in required)
            {
                string itemId = pair.Key;
                int requiredCount = pair.Value;
                int haveBefore = GetPlayerHave(itemId);
                int missing = Math.Max(requiredCount - haveBefore, 0);
                int available = source.GetTotalCount(itemId, MultiInventory.DestinationType.AllFromFirst, true);
                int capacity = target.CanAddCount(itemId, true);
                int requested = Math.Min(missing, Math.Min(available, capacity));
                hadMissing |= missing > 0;
                limitedBySpace |= capacity < Math.Min(missing, available);
                bool success = true;

                if (requested > 0)
                {
                    success = source.MoveItemTo(
                        target,
                        new Item(itemId, requested),
                        requested,
                        false,
                        true);

                    int actual = Math.Max(0, GetPlayerHave(itemId) - haveBefore);
                    if (actual > 0) actualMoves.Add(new KeyValuePair<string, int>(itemId, actual));
                    movedAnything |= actual > 0;
                    failed |= actual < requested;
                }

                LogLine(
                    "event=take_item trigger=" + trigger +
                    " item=" + Safe(itemId) +
                    " required=" + requiredCount +
                    " have=" + haveBefore +
                    " missing=" + missing +
                    " storage=" + available +
                    " capacity=" + capacity +
                    " requested=" + requested +
                    " success=" + success);
            }

            if (movedAnything && chestGui != null)
            {
                if (chestGui.player_panel != null)
                {
                    chestGui.player_panel.Redraw();
                }

                if (chestGui.chest_panel != null)
                {
                    chestGui.chest_panel.Redraw();
                }

                TooltipsManager.Redraw();
            }

            PlannerHud.Refresh();
            TransferNotice notice = failed ? TransferNotice.Unavailable : limitedBySpace ? TransferNotice.NoSpace :
                actualMoves.Count > 0 ? TransferNotice.None : hadMissing ? TransferNotice.NoStock : TransferNotice.Complete;
            PlannerChestControls.Show(actualMoves, notice);
            LogLine("event=take_needed result=complete trigger=" + trigger + " moved_any=" + movedAnything);
            return true;
        }

        internal static bool IsSupportedProject(CraftDefinition definition)
        {
            if (!HasPlainNeeds(definition))
            {
                return false;
            }

            if (_context == ProjectContextKind.Builder)
            {
                return definition is ObjectCraftDefinition && BuildDefinitions.Contains(definition);
            }

            if (_context == ProjectContextKind.WorldChange)
            {
                return !(definition is ObjectCraftDefinition) &&
                       !string.IsNullOrEmpty(definition.change_wgo);
            }

            return false;
        }

        internal static bool HasPlainNeeds(CraftDefinition definition)
        {
            if (definition == null || definition.needs == null || definition.needs.Count == 0)
            {
                return false;
            }

            if (definition.needs_from_wgo != null && definition.needs_from_wgo.Count > 0)
            {
                return false;
            }

            foreach (Item need in definition.needs)
            {
                if (need == null || string.IsNullOrEmpty(need.id) || need.value <= 0)
                {
                    return false;
                }

                if (need.multiquality_items != null && need.multiquality_items.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsRepeatable(CraftDefinition definition)
        {
            if (_context == ProjectContextKind.WorldChange)
                return PlannerGoalState.IsLightingProject(definition, _contextOwner != null ? _contextOwner.obj_id : null);
            ObjectCraftDefinition objectCraft = definition as ObjectCraftDefinition;
            return _context == ProjectContextKind.Builder &&
                   objectCraft != null &&
                   objectCraft.build_type == ObjectCraftDefinition.BuildType.Put &&
                   !definition.one_time_craft;
        }

        private static string BuildGoalKey(CraftDefinition definition)
        {
            string owner = _contextOwner != null ? _contextOwner.obj_id : "";
            return PlannerGoalState.Key(_context, owner, definition.id);
        }

        internal static string GetProjectName(CraftDefinition definition)
        {
            try
            {
                string token = definition.GetNameNonLocalized();
                string localized = GJL.L(token);
                return string.IsNullOrEmpty(localized) ? definition.id : localized;
            }
            catch
            {
                return definition.id;
            }
        }

        private static string Safe(string value)
        {
            return string.IsNullOrEmpty(value)
                ? "null"
                : value.Replace(" ", "_").Replace("=", "_").Replace("\r", "_").Replace("\n", "_");
        }

        internal static void LogLine(string details)
        {
            if (Log != null)
            {
                Log.LogInfo("CRAFTING_PLANNER " + details);
            }
        }
    }

    internal static class PlannerLocalization
    {
        private static readonly Dictionary<string, string[]> Texts = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", new[] { "Building Planner", "Have", "Need", "Missing", "Take needed", "Remove one", "Add one", "Only missing materials from this chest; limited by carrying capacity." } },
            { "de", new[] { "Plan", "Da", "Bedarf", "Fehlt", "Benötigtes nehmen", "Eins entfernen", "Eins hinzufügen", "Nur Fehlendes aus dieser Truhe, soweit Platz ist." } },
            { "fr", new[] { "Plan", "En sac", "Requis", "Manque", "Prendre le nécessaire", "Retirer 1", "Ajouter 1", "Matériaux manquants de ce coffre, selon la place disponible." } },
            { "pt-br", new[] { "Plano", "Tenho", "Preciso", "Falta", "Pegar o necessário", "Remover 1", "Adicionar 1", "Somente o que falta deste baú, conforme o espaço disponível." } },
            { "es", new[] { "Plan", "Tengo", "Necesito", "Falta", "Tomar lo necesario", "Quitar 1", "Añadir 1", "Solo lo que falta de este cofre, según el espacio disponible." } },
            { "ru", new[] { "План строительства", "Есть", "Нужно", "Добрать", "Взять недостающее", "Убрать 1", "Добавить 1", "Только недостающее из этого сундука, насколько хватит места." } },
            { "it", new[] { "Piano", "Ho", "Serve", "Manca", "Prendi il necessario", "Rimuovi 1", "Aggiungi 1", "Solo i materiali mancanti da questo baule, spazio permettendo." } },
            { "pl", new[] { "Plan", "Mam", "Potrzeba", "Brakuje", "Weź potrzebne", "Usuń 1", "Dodaj 1", "Tylko brakujące materiały z tej skrzyni, w granicach pojemności." } },
            { "ja", new[] { "計画", "所持", "必要", "不足", "不足分を取る", "1つ減らす", "1つ追加", "この箱から不足分だけ、持てる範囲で取ります。" } },
            { "zh-cn", new[] { "计划", "持有", "需要", "缺少", "拿取所需材料", "减少1个", "添加1个", "仅从此箱子拿取缺少的材料，以背包容量为限。" } },
            { "ko", new[] { "계획", "보유", "필요", "부족", "필요한 재료 가져오기", "1개 제거", "1개 추가", "이 상자에서 부족한 재료만 가방 공간만큼 가져옵니다." } }
        };
        internal static string Plan { get { return Current[0]; } }
        internal static string Have { get { return Current[1]; } }
        internal static string Required { get { return Current[2]; } }
        internal static string Missing { get { return Current[3]; } }
        internal static string TakeNeeded { get { return Current[4]; } }
        internal static string RemoveGoal { get { return Current[5]; } }
        internal static string AddGoal { get { return Current[6]; } }
        internal static string CollectHelp { get { return Current[7]; } }
        private static readonly Dictionary<string, string[]> EditTexts = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", new[] { "Edit plan", "Done", "Add to planner" } },
            { "de", new[] { "Plan bearbeiten", "Fertig", "Zum Plan hinzufügen" } },
            { "fr", new[] { "Modifier le plan", "Terminé", "Ajouter au plan" } },
            { "pt-br", new[] { "Editar plano", "Concluir", "Adicionar ao plano" } },
            { "es", new[] { "Editar plan", "Listo", "Añadir al plan" } },
            { "ru", new[] { "Изменить план", "Готово", "Добавить в план" } },
            { "it", new[] { "Modifica piano", "Fine", "Aggiungi al piano" } },
            { "pl", new[] { "Edytuj plan", "Gotowe", "Dodaj do planu" } },
            { "ja", new[] { "計画を編集", "完了", "計画に追加" } },
            { "zh-cn", new[] { "编辑计划", "完成", "添加到计划" } },
            { "ko", new[] { "계획 편집", "완료", "계획에 추가" } }
        };
        internal static string EditPlan { get { return CurrentEdit[0]; } }
        internal static string FinishEditing { get { return CurrentEdit[1]; } }
        internal static string PlanColumn { get { return CurrentEdit[2]; } }
        private static readonly Dictionary<string, string[]> TransferTexts = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "en", new[] { "Taken:", "No needed materials here", "Not enough space", "Materials already collected", "Transfer unavailable" } },
            { "de", new[] { "Genommen:", "Hier fehlen die benötigten Materialien", "Nicht genug Platz", "Materialien bereits gesammelt", "Transfer nicht verfügbar" } },
            { "fr", new[] { "Pris :", "Aucun matériau nécessaire ici", "Pas assez de place", "Matériaux déjà réunis", "Transfert indisponible" } },
            { "pt-br", new[] { "Coletado:", "Não há materiais necessários aqui", "Espaço insuficiente", "Materiais já coletados", "Transferência indisponível" } },
            { "es", new[] { "Recogido:", "Aquí no hay materiales necesarios", "No hay espacio suficiente", "Materiales ya reunidos", "Transferencia no disponible" } },
            { "ru", new[] { "Взято:", "Недостающего здесь нет", "Не хватило места", "Материалы уже собраны", "Перенос недоступен" } },
            { "it", new[] { "Preso:", "Qui non ci sono materiali necessari", "Spazio insufficiente", "Materiali già raccolti", "Trasferimento non disponibile" } },
            { "pl", new[] { "Wzięto:", "Brak potrzebnych materiałów tutaj", "Za mało miejsca", "Materiały już zebrane", "Przenoszenie niedostępne" } },
            { "ja", new[] { "取得:", "必要な素材がありません", "空き容量が足りません", "素材は揃っています", "移動できません" } },
            { "zh-cn", new[] { "已拿取：", "这里没有所需材料", "空间不足", "材料已集齐", "无法转移" } },
            { "ko", new[] { "가져옴:", "여기에 필요한 재료가 없습니다", "공간이 부족합니다", "재료를 모두 모았습니다", "옮길 수 없습니다" } }
        };
        private static string[] CurrentTransfer
        {
            get { string[] value; return TransferTexts.TryGetValue(Normalize(CurrentLanguage), out value) ? value : TransferTexts["en"]; }
        }
        internal static string Taken { get { return CurrentTransfer[0]; } }
        internal static string TransferMessage(TransferNotice notice) { return CurrentTransfer[(int)notice]; }
        private static string[] CurrentEdit
        {
            get { string[] value; return EditTexts.TryGetValue(Normalize(CurrentLanguage), out value) ? value : EditTexts["en"]; }
        }

        internal static string CurrentLanguage
        {
            get
            {
                string language = "";
                try
                {
                    language = GameSettings.GetCurrentLanguage();
                }
                catch
                {
                }

                return Normalize(language);
            }
        }

        internal static void ApplyFont(UILabel label)
        {
            if (label == null)
            {
                return;
            }

            try
            {
                GJL.EnsureLabelHasCorrectFont(label, false);
            }
            catch
            {
            }
        }

        internal static void OnLanguageChanged()
        {
            PlannerRowControls.RefreshAll();
            PlannerHud.OnLanguageChanged();
            PlannerCraftOverlay.OnLanguageChanged();
            PlannerInputHints.Refresh();
            Planner.LogLine("event=language_changed language=" + CurrentLanguage);
        }

        private static string[] Current
        {
            get
            {
                string[] value;
                return Texts.TryGetValue(CurrentLanguage, out value) ? value : Texts["en"];
            }
        }

        private static string Normalize(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return "en";
            }

            return language.Trim().ToLowerInvariant().Replace('_', '-');
        }
    }

    [HarmonyPatch(typeof(MainGame), "OnGameStartedPlaying")]
    internal static class SessionResetPatch
    {
        private static void Postfix()
        {
            Planner.ResetSession();
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OpenAsBuild", new Type[] { typeof(WorldGameObject), typeof(CraftsInventory) })]
    internal static class BuildContextPatch
    {
        private static void Prefix(WorldGameObject build_desk, CraftsInventory crafts_inventory)
        {
            Planner.OpenBuildContext(build_desk, crafts_inventory);
        }

        private static void Postfix(CraftGUI __instance)
        {
            PlannerCraftOverlay.Open(__instance);
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OpenCraftList", new Type[] { typeof(WorldGameObject) })]
    internal static class WorldContextPatch
    {
        private static void Prefix(WorldGameObject craftery_wgo)
        {
            Planner.OpenWorldContext(craftery_wgo);
        }

        private static void Postfix(CraftGUI __instance)
        {
            PlannerCraftOverlay.Open(__instance);
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "Hide", new Type[] { typeof(bool) })]
    internal static class CraftGuiHidePatch
    {
        private static void Postfix()
        {
            PlannerRowControls.RestoreAll();
            PlannerCraftOverlay.Hide();
            Planner.ClearProjectContext();
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnMouseOvered")]
    internal static class MouseFocusPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                Planner.Focus(__instance.current_craft);
            }
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnMouseOuted")]
    internal static class MouseBlurPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                Planner.Blur(__instance.current_craft);
            }
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnOver")]
    internal static class GamepadFocusPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                Planner.Focus(__instance.current_craft);
            }
        }
    }

    [HarmonyPatch(typeof(CraftItemGUI), "OnOut")]
    internal static class GamepadBlurPatch
    {
        private static void Postfix(CraftItemGUI __instance)
        {
            if (__instance != null)
            {
                Planner.Blur(__instance.current_craft);
            }
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OnPressedRight")]
    internal static class AddGoalGamepadPatch
    {
        private static bool Prefix(CraftGUI __instance, ref bool __result)
        {
            if (!PlannerEditing.Horizontal(__instance, 1))
            {
                return true;
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(CraftGUI), "OnPressedLeft")]
    internal static class RemoveGoalGamepadPatch
    {
        private static bool Prefix(CraftGUI __instance, ref bool __result)
        {
            if (!PlannerEditing.Horizontal(__instance, -1))
            {
                return true;
            }

            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(GameSettings), "ApplyLanguageChange")]
    internal static class LanguageChangePatch
    {
        private static void Postfix()
        {
            PlannerLocalization.OnLanguageChanged();
        }
    }

    [HarmonyPatch(typeof(HUD), "Init")]
    internal static class HudInitPatch
    {
        private static void Postfix(HUD __instance)
        {
            PlannerHud.Ensure(__instance);
        }
    }

    [HarmonyPatch(typeof(HUD), "Open")]
    internal static class HudOpenPatch
    {
        private static void Postfix(HUD __instance)
        {
            PlannerHud.Ensure(__instance);
            PlannerHud.Refresh();
        }
    }

    [HarmonyPatch(typeof(Item), "AddItem", new Type[] { typeof(Item), typeof(bool) })]
    internal static class PlayerAddItemPatch
    {
        private static void Postfix(Item __instance, bool __result)
        {
            if (__result &&
                MainGame.me != null &&
                MainGame.me.player != null &&
                __instance == MainGame.me.player.data)
            {
                PlannerHud.RefreshIfVisible();
            }
        }
    }

    [HarmonyPatch(typeof(Item), "RemoveItem", new Type[] { typeof(Item), typeof(int), typeof(Item) })]
    internal static class PlayerRemoveItemPatch
    {
        private static void Postfix(Item __instance, bool __result)
        {
            if (__result &&
                MainGame.me != null &&
                MainGame.me.player != null &&
                __instance == MainGame.me.player.data)
            {
                PlannerHud.RefreshIfVisible();
            }
        }
    }

    [HarmonyPatch(typeof(ChestGUI), "Open")]
    internal static class ChestOpenPatch
    {
        private static void Prefix(WorldGameObject chest_obj) { Planner.OpenChest(chest_obj); }
        private static void Postfix() { PlannerInputHints.Refresh(); }
    }

    [HarmonyPatch(typeof(ChestGUI), "Hide")]
    internal static class ChestHidePatch
    {
        private static void Postfix()
        {
            Planner.CloseChest();
            PlannerInputHints.HideChest();
        }
    }

    [HarmonyPatch(typeof(BaseGUI), "OnPressedOption2")]
    internal static class TakeNeededGamepadPatch
    {
        private static bool Prefix(BaseGUI __instance, ref bool __result)
        {
            ChestGUI chest = __instance as ChestGUI;
            if (chest == null)
            {
                return true;
            }

            if (!Planner.TakeNeeded(chest, "gamepad_y"))
            {
                return true;
            }

            __result = true;
            return false;
        }
    }
}
