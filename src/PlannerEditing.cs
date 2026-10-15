// SPDX-License-Identifier: MPL-2.0
using System;
using HarmonyLib;
using Rewired;
using UnityEngine;
namespace CraftingPlanner
{
    internal static class PlannerEditing
    {
        private static Controller _controller;
        private static IGamepadTemplate _template;
        private static int _selected, _exitFrame = -1, _processedFrame = -1;
        private static bool _failed;
        internal static bool Active { get; private set; }
        internal static bool Available { get { return _template != null; } }
        internal static ProjectGoal Selected
        {
            get { var goals = Planner.GetGoals(); return goals.Count == 0 ? null : goals[Math.Max(0, Math.Min(_selected, goals.Count - 1))]; }
        }
        internal static bool IsPlayer(BaseCharacterComponent character)
        {
            return MainGame.me != null && MainGame.me.player != null && MainGame.me.player.components != null && ReferenceEquals(character, MainGame.me.player.components.character);
        }
        internal static void Reset() { Active = false; _selected = 0; _exitFrame = -1; _processedFrame = -1; _controller = null; _template = null; _failed = false; }
        private static void Leave()
        {
            if (!Active) return;
            Active = false; _exitFrame = Time.frameCount;
            Planner.LogLine("event=plan_edit state=closed"); PlannerPresentation.Refresh();
        }
        internal static void Tick()
        {
            if (_failed || _processedFrame == Time.frameCount) return;
            _processedFrame = Time.frameCount;
            try
            {
                if (PlannerCalibration.IsOpen || !LazyInput.gamepad_active || PlannerPresentation.GetView() != PlannerView.Hud || !Planner.HasGoals) { Leave(); return; }
                Controller controller = ReInput.isReady ? ReInput.players.GetPlayer(0).controllers.GetLastActiveController(ControllerType.Joystick) : null;
                if (controller == null || !controller.isConnected)
                {
                    bool changed = _controller != null;
                    _controller = null; _template = null; Leave();
                    if (changed) PlannerPresentation.Refresh();
                    return;
                }
                if (!ReferenceEquals(controller, _controller))
                {
                    _controller = controller; _template = controller.GetTemplate<IGamepadTemplate>();
                    Planner.LogLine("event=plan_input template=" + (_template != null ? "gamepad" : "unavailable"));
                    PlannerPresentation.Refresh();
                }
                if (_template == null) { Leave(); return; }
                bool toggle = _template.rightStick.press.justPressed;
                if (Active && (toggle || LazyInput.GetKeyDown(GameKey.Back))) { Leave(); return; }
                if (!Active)
                {
                    if (!toggle) return;
                    Active = true; _selected = 0;
                    Planner.LogLine("event=plan_edit state=opened input=right_stick"); PlannerPresentation.Refresh();
                    return;
                }
                int count = Planner.GetGoals().Count;
                if (LazyInput.GetKeyDown(GameKey.Up)) { _selected = (_selected + count - 1) % count; PlannerPresentation.Refresh(); }
                else if (LazyInput.GetKeyDown(GameKey.Down)) { _selected = (_selected + 1) % count; PlannerPresentation.Refresh(); }
                if (LazyInput.GetKeyDown(GameKey.Left)) Planner.AdjustPinned(Selected, -1, "hud_left");
                else if (LazyInput.GetKeyDown(GameKey.Right)) Planner.AdjustPinned(Selected, 1, "hud_right");
                if (!Planner.HasGoals) Leave();
            }
            catch (Exception ex)
            {
                Leave(); _failed = true; _template = null;
                Planner.LogLine("event=plan_input result=disabled_for_session error=" + ex);
            }
        }
        internal static bool BlocksWorldInput
        {
            get { return Active || _exitFrame == Time.frameCount || PlannerPresentation.BlocksWorldClick(); }
        }
        internal static bool Horizontal(CraftGUI gui, int delta)
        {
            if (!LazyInput.gamepad_active || PlannerCalibration.IsOpen || gui == null || !gui.is_shown_and_top || gui.gamepad_controller == null) return false;
            var focused = gui.gamepad_controller.focused_item;
            CraftItemGUI item = focused != null ? focused.GetComponent<CraftItemGUI>() : null;
            if (item == null || !Planner.IsSupportedProject(item.current_craft) || (item.amount_buttons != null && item.amount_buttons.activeSelf)) return false;
            Planner.Focus(item.current_craft);
            Planner.AdjustFocusedProject(delta, delta < 0 ? "menu_left" : "menu_right");
            return true; // At zero, still consume the planner direction rather than navigate elsewhere.
        }
    }
    [HarmonyPatch(typeof(BaseCharacterComponent), "UpdatePlayer")]
    internal static class PlannerHudInputPatch
    {
        private static void Prefix(BaseCharacterComponent __instance) { if (PlannerEditing.IsPlayer(__instance)) PlannerEditing.Tick(); }
    }
    [HarmonyPatch(typeof(BaseCharacterComponent), "PlayerControlIsDisabled")]
    internal static class PlannerWorldInputGuard
    {
        private static void Postfix(BaseCharacterComponent __instance, ref bool __result)
        {
            if (!__result && PlannerEditing.IsPlayer(__instance) && PlannerEditing.BlocksWorldInput) __result = true;
        }
    }
}
