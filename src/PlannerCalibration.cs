// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
namespace CraftingPlanner
{
    // Temporary candidate-only editor. Production presentation remains native NGUI.
    internal static class PlannerCalibration
    {
#if PLANNER_CALIBRATION
        private static bool _open;
        private static BaseCharacterComponent _character;
        private static bool _control;
        private static UICamera _events;
        private static Camera _camera;
        private static bool _mouse, _touch, _keyboard, _controller;
        private static Vector2 _scroll;
        private static Rect _window = new Rect(20, 30, 550, 680);
        internal static bool IsOpen { get { return _open; } }
        internal static void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F2)) return;
            if (_open) { Close(); return; }
            if (PlannerPresentation.GetView() == PlannerView.Hidden || !Planner.HasGoals) return;
            try
            {
                WorldGameObject player = MainGame.me != null ? MainGame.me.player : null;
                if (player == null || player.components == null) return;
                _character = player.components.character;
                if (_character == null) return;
                _control = _character.control_enabled;
                _events = UICamera.FindCameraForLayer(GUIElements.me.hud.gameObject.layer);
                if (_events == null) { _character = null; return; }
                _camera = _events.GetComponent<Camera>();
                _mouse = _events.useMouse; _touch = _events.useTouch;
                _keyboard = _events.useKeyboard; _controller = _events.useController;
                _open = true;
                _character.control_enabled = false;
                _events.useMouse = _events.useTouch = _events.useKeyboard = _events.useController = false;
                Planner.LogLine("event=calibration_open input_guard=true");
            }
            catch (Exception ex) { Close(); Planner.LogLine("event=calibration_error error=" + ex); }
        }
        internal static void Close()
        {
            if (!_open) return;
            try { PlannerPresentation.Export(_camera); }
            finally
            {
                _open = false;
                if (_events != null) { _events.useMouse = _mouse; _events.useTouch = _touch; _events.useKeyboard = _keyboard; _events.useController = _controller; }
                if (_character != null) _character.control_enabled = _control;
                _events = null; _camera = null; _character = null;
                Planner.LogLine("event=calibration_close input_guard=false");
            }
        }
        internal static void Draw()
        {
            if (!_open) return;
            _window.height = Math.Max(280, Math.Min(720, Screen.height - 60));
            _window = GUI.Window(197031, _window, Window, "Crafting Planner " + Plugin.PluginVersion + " — F2: закрыть");
        }
        private static void Window(int id)
        {
            GUILayout.Label("Временная настройка. Изменения видны сразу во всех контекстах.");
            _scroll = GUILayout.BeginScrollView(_scroll);
            PlannerStyle s = PlannerStyle.Current;
            bool changed = GUI.changed;
            GUI.changed = false;
            GUILayout.Label("Карточка целиком");
            s.Right = Slider("Отступ справа", s.Right, 0, 400);
            s.Top = Slider("Отступ сверху", s.Top, 40, 400);
            s.Width = Slider("Ширина (минимальная)", s.Width, 240, 640);
            s.Scale = Slider("Общий масштаб (шаг 0.01)", s.Scale, .6f, 1.5f, .01f);
            GUILayout.Label("Текст и строки");
            s.CraftFont = GUILayout.Toggle(s.CraftFont, "Шрифт названий строительства (выкл. — шрифт HUD)");
            s.Outline = GUILayout.Toggle(s.Outline, "Обводка текста (выкл. — мягкая тень)");
            s.Font = Slider("Размер основного текста", s.Font, 10, 32);
            s.AlternateNumbers = GUILayout.Toggle(s.AlternateNumbers, "Другой родной шрифт для всех чисел");
            s.AlternateQuantity = GUILayout.Toggle(s.AlternateQuantity, "Другой родной шрифт для ×количества и стрелок");
            s.Row = Slider("Высота строки (минимальная)", s.Row, 18, 48);
            s.Padding = Slider("Внутренние отступы", s.Padding, 4, 24);
            s.Column = Slider("Ширина числовых столбцов", s.Column, 32, 110);
            s.ColumnGap = Slider("Промежуток столбцов", s.ColumnGap, 0, 20);
            s.ColumnInset = Slider("Все числовые столбцы левее", s.ColumnInset, 0, 36);
            GUILayout.Label("Положение блоков");
            s.GoalsOffset = Slider("Цели и материалы вместе: ниже/выше", s.GoalsOffset, -12, 80);
            s.MaterialsGap = Slider("Материалы: отступ от списка целей", s.MaterialsGap, 0, 60);
            s.TitleGap = Slider("Отступ после заголовка", s.TitleGap, 0, 28);
            s.FooterGap = Slider("Отступ подсказки управления", s.FooterGap, 4, 32);
            GUILayout.Label("Заголовок на месте локации");
            s.TitleCraftFont = GUILayout.Toggle(s.TitleCraftFont, "Заголовок: шрифт строительства (выкл. — HUD)");
            s.TitleFont = Slider("Размер заголовка", s.TitleFont, 10, 32);
            GUILayout.Label("Подсказка RS / B справа");
            s.FooterCraftFont = GUILayout.Toggle(s.FooterCraftFont, "Подсказка: шрифт строительства (выкл. — шрифт кнопок)");
            s.FooterFont = Slider("Размер подсказки", s.FooterFont, 10, 32);
            GUILayout.Label("Иконки — независимо от текста и строк");
            s.Icon = Slider("Размер изображений", s.Icon, 8, 80);
            s.IconX = Slider("Иконки: сдвиг вправо", s.IconX, -60, 60);
            s.IconY = Slider("Иконки: сдвиг вниз", s.IconY, -60, 60);
            s.GoalIconSpacing = Slider("Иконки целей: добавка к вертикальному шагу", s.GoalIconSpacing, -20, 20);
            s.MaterialIconSpacing = Slider("Иконки материалов: добавка к вертикальному шагу", s.MaterialIconSpacing, -20, 20);
            GUILayout.Label("Сообщения сундука — весь блок");
            s.FeedbackX = Slider("Сдвиг вправо", s.FeedbackX, -300, 500);
            s.FeedbackY = -Slider("Отступ под строкой действий", -s.FeedbackY, 4, 150);
            s.FeedbackGap = Slider("Промежуток между взятыми предметами", s.FeedbackGap, 0, 40);
            s.FeedbackLineGap = Slider("Промежуток между строками сообщений", s.FeedbackLineGap, 0, 24);
            GUILayout.Label("Мягкая подложка");
            s.Opacity = Slider("Плотность подложки (%)", s.Opacity * 100, 0, 85) / 100;
            s.TextOpacity = Slider("Плотность текста (%)", s.TextOpacity * 100, 50, 100) / 100;
            s.ShadowRadius = Slider("Скругление подложки", s.ShadowRadius, 6, 90);
            s.ShadowFeather = Slider("Мягкость краёв", s.ShadowFeather, 5, 28);
            bool dirty = GUI.changed;
            GUI.changed |= changed;
            GUILayout.EndScrollView();
            if (dirty) { s.Clamp(); PlannerRowControls.RefreshAll(); PlannerPresentation.Refresh(); }
            if (GUILayout.Button("Записать настройки и геометрию в LogOutput.log")) PlannerPresentation.Export(_camera);
            if (GUILayout.Button("Вернуть исходные настройки")) { PlannerStyle.Current = new PlannerStyle(); PlannerRowControls.RefreshAll(); PlannerPresentation.Refresh(); }
            GUILayout.Label("Размеры/отступы — целые единицы UI. Масштаб — шаг 0.01.\nНастройки до выхода; F2 также записывает их в лог.");
            if (GUILayout.Button("Записать и закрыть (F2)")) Close();
            GUI.DragWindow(new Rect(0, 0, 550, 24));
        }
        private static float Slider(string title, float value, float min, float max, float step = 1)
        {
            float next = (float)(Math.Round(GUILayout.HorizontalSlider(value, min, max) / step, MidpointRounding.AwayFromZero) * step);
            GUILayout.Label(title + ": " + next.ToString(step == 1 ? "0" : "0.00", System.Globalization.CultureInfo.InvariantCulture));
            return next;
        }
#else
        internal static bool IsOpen { get { return false; } }
        internal static void Update() { }
        internal static void Draw() { }
        internal static void Close() { }
#endif
    }
#if PLANNER_CALIBRATION
    [HarmonyPatch]
    internal static class CalibrationKeyGuard
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string name in new[] { "GetKey", "GetKeyDown", "AnyClick", "AnyKeyDown" })
                foreach (MethodInfo method in typeof(LazyInput).GetMethods(BindingFlags.Static | BindingFlags.Public))
                    if (method.Name == name && method.ReturnType == typeof(bool)) yield return method;
        }
        private static bool Prefix(ref bool __result)
        { if (!PlannerCalibration.IsOpen) return true; __result = false; return false; }
    }
    [HarmonyPatch]
    internal static class CalibrationDirectionGuard
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string name in new[] { "GetDirection", "GetDirection2" })
                foreach (MethodInfo method in typeof(LazyInput).GetMethods(BindingFlags.Static | BindingFlags.Public))
                    if (method.Name == name && method.ReturnType == typeof(Vector2)) yield return method;
        }
        private static bool Prefix(ref Vector2 __result)
        { if (!PlannerCalibration.IsOpen) return true; __result = Vector2.zero; return false; }
    }
#endif
    [HarmonyPatch(typeof(HUD), "Hide")]
    internal static class HudHidePresentationPatch { private static void Postfix() { PlannerPresentation.Refresh(); } }
    [HarmonyPatch(typeof(GUIElements), "RecalcScreenResolution")]
    internal static class ResolutionPresentationPatch { private static void Postfix() { PlannerRowControls.RefreshAll(); PlannerPresentation.Refresh(); } }
}
