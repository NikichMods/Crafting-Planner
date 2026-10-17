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
        internal static bool PreviewDone { get; private set; }
        internal static bool IsOpen { get { return _open; } }
        internal static void Update()
        {
            if (!Input.GetKeyDown(KeyCode.F2)) return;
            if (_open) { Close(); return; }
            PlannerView view = PlannerPresentation.GetView();
            if (view == PlannerView.Hidden || (!Planner.HasGoals && view != PlannerView.Build)) return;
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
                PlannerPresentation.Refresh();
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
                PlannerPresentation.Refresh();
            }
        }
        internal static void Draw()
        {
            if (!_open) return;
            _window.height = Math.Max(280, Math.Min(720, Screen.height - 60));
            _window = GUI.Window(197031, _window, Window, "Building Planner " + Plugin.PluginVersion + " — F2: закрыть");
        }
        private static void Window(int id)
        {
            GUILayout.Label("Временная настройка. Изменения видны сразу во всех контекстах.");
            _scroll = GUILayout.BeginScrollView(_scroll);
            PlannerStyle s = PlannerStyle.Current;
            bool changed = GUI.changed;
            GUI.changed = false;
            GUILayout.Label("Карточка целиком");
            s.Right = Slider(nameof(s.Right), "Отступ справа", s.Right, 0, 400);
            s.Top = Slider(nameof(s.Top), "Отступ сверху", s.Top, 40, 400);
            s.Width = Slider(nameof(s.Width), "Ширина (минимальная)", s.Width, 240, 640);
            s.Scale = Slider(nameof(s.Scale), "Общий масштаб (шаг 0.01)", s.Scale, .6f, 1.5f, .01f);
            GUILayout.Label("Текст и строки");
            s.CraftFont = GUILayout.Toggle(s.CraftFont, "Шрифт названий строительства (выкл. — шрифт HUD)");
            s.Outline = GUILayout.Toggle(s.Outline, "Обводка текста (выкл. — мягкая тень)");
            s.Font = Slider(nameof(s.Font), "Размер основного текста", s.Font, 10, 32);
            s.AlternateNumbers = GUILayout.Toggle(s.AlternateNumbers, "Другой родной шрифт для всех чисел");
            s.AlternateQuantity = GUILayout.Toggle(s.AlternateQuantity, "Другой родной шрифт для количества и стрелок");
            s.QuantityArrowGap = Slider(nameof(s.QuantityArrowGap), "Счётчики везде: отступ стрелок от числа", s.QuantityArrowGap, 0, 40);
            s.Row = Slider(nameof(s.Row), "Высота строки (минимальная)", s.Row, 18, 48);
            s.Padding = Slider(nameof(s.Padding), "Внутренние отступы", s.Padding, 4, 24);
            s.Column = Slider(nameof(s.Column), "Ширина числовых столбцов", s.Column, 32, 110);
            s.ColumnGap = Slider(nameof(s.ColumnGap), "Промежуток столбцов", s.ColumnGap, 0, 20);
            s.ColumnInset = Slider(nameof(s.ColumnInset), "Все числовые столбцы левее", s.ColumnInset, 0, 36);
            GUILayout.Label("Положение блоков");
            s.GoalsOffset = Slider(nameof(s.GoalsOffset), "Цели и материалы вместе: ниже/выше", s.GoalsOffset, -12, 80);
            s.MaterialsGap = Slider(nameof(s.MaterialsGap), "Материалы: отступ от списка целей", s.MaterialsGap, 0, 60);
            s.TitleGap = Slider(nameof(s.TitleGap), "Отступ после заголовка", s.TitleGap, 0, 28);
            s.FooterGap = Slider(nameof(s.FooterGap), "Отступ подсказки управления", s.FooterGap, 4, 32);
            GUILayout.Label("Заголовок на месте локации");
            s.TitleCraftFont = GUILayout.Toggle(s.TitleCraftFont, "Заголовок: шрифт строительства (выкл. — HUD)");
            s.TitleFont = Slider(nameof(s.TitleFont), "Размер заголовка", s.TitleFont, 10, 32);
            s.TitleY = Slider(nameof(s.TitleY), "Текст заголовка: сдвиг вверх", s.TitleY, -300, 300);
            GUILayout.Label("Подсказка RS / B справа");
            s.FooterCraftFont = GUILayout.Toggle(s.FooterCraftFont, "Подсказка: шрифт строительства (выкл. — шрифт кнопок)");
            s.FooterFont = Slider(nameof(s.FooterFont), "Размер подсказки", s.FooterFont, 10, 32);
            PreviewDone = GUILayout.Toggle(PreviewDone, "Предпросмотр B / Готово (выкл. — RS / Изменить план)");
            s.FooterX = Slider(nameof(s.FooterX), "Подсказка: сдвиг вправо", s.FooterX, -300, 300);
            s.FooterY = Slider(nameof(s.FooterY), "Подсказка: сдвиг вверх", s.FooterY, -300, 300);
            s.FooterIconSize = Slider(nameof(s.FooterIconSize), "Размер значка RS / B", s.FooterIconSize, 10, 48);
            s.FooterOutline = GUILayout.Toggle(s.FooterOutline, "Обводка подсказки (выкл. — тень)");
            s.FooterShadowSize = Slider(nameof(s.FooterShadowSize), "Размер тени подсказки", s.FooterShadowSize, 0, 4);
            s.FooterShadowOpacity = Slider(nameof(s.FooterShadowOpacity), "Плотность тени подсказки (%)", s.FooterShadowOpacity * 100, 0, 100) / 100;
            GUILayout.Label("Меню строительства / ремонта — столбец плана");
            s.MenuX = Slider(nameof(s.MenuX), "Весь столбец: вправо", s.MenuX, -300, 300);
            s.MenuY = Slider(nameof(s.MenuY), "Весь столбец: вверх", s.MenuY, -300, 300);
            s.MenuMinusX = Slider(nameof(s.MenuMinusX), "Левая стрелка: вправо", s.MenuMinusX, -300, 300);
            s.MenuMinusY = Slider(nameof(s.MenuMinusY), "Левая стрелка: вверх", s.MenuMinusY, -300, 300);
            s.MenuQuantityX = Slider(nameof(s.MenuQuantityX), "Количество: вправо", s.MenuQuantityX, -300, 300);
            s.MenuQuantityY = Slider(nameof(s.MenuQuantityY), "Количество: вверх", s.MenuQuantityY, -300, 300);
            s.MenuPlusX = Slider(nameof(s.MenuPlusX), "Правая стрелка: вправо", s.MenuPlusX, -300, 300);
            s.MenuPlusY = Slider(nameof(s.MenuPlusY), "Правая стрелка: вверх", s.MenuPlusY, -300, 300);
            s.MenuHudFont = GUILayout.Toggle(s.MenuHudFont, "Шрифт столбца: HUD (выкл. — шрифт строительства)");
            s.MenuFont = Slider(nameof(s.MenuFont), "Размер шрифта столбца", s.MenuFont, 10, 32);
            s.MenuOutline = GUILayout.Toggle(s.MenuOutline, "Обводка столбца (выкл. — тень)");
            s.MenuShadowSize = Slider(nameof(s.MenuShadowSize), "Размер тени столбца", s.MenuShadowSize, 0, 4);
            s.MenuShadowOpacity = Slider(nameof(s.MenuShadowOpacity), "Плотность тени столбца (%)", s.MenuShadowOpacity * 100, 0, 100) / 100;
            s.MenuHideBorder = GUILayout.Toggle(s.MenuHideBorder, "Убрать внутреннюю правую границу родных строк");
            GUILayout.Label("Меню — энергия / игровые затраты");
            s.MenuSpendX = Slider(nameof(s.MenuSpendX), "Игровые затраты: вправо", s.MenuSpendX, -300, 300);
            s.MenuSpendY = Slider(nameof(s.MenuSpendY), "Игровые затраты: вверх", s.MenuSpendY, -300, 300);
            GUILayout.Label("Меню — материалы в строках БЕЗ игровых затрат");
            s.IngredientsX = Slider(nameof(s.IngredientsX), "Материалы без затрат: вправо", s.IngredientsX, -300, 300);
            s.IngredientsY = Slider(nameof(s.IngredientsY), "Материалы без затрат: вверх", s.IngredientsY, -300, 300);
            GUILayout.Label("Меню — материалы в строках С игровыми затратами");
            s.CostIngredientsX = Slider(nameof(s.CostIngredientsX), "Материалы рядом с затратами: вправо", s.CostIngredientsX, -300, 300);
            s.CostIngredientsY = Slider(nameof(s.CostIngredientsY), "Материалы рядом с затратами: вверх", s.CostIngredientsY, -300, 300);
            GUILayout.Label("Меню — общий ряд управления внизу");
            s.MenuCaptionX = Slider(nameof(s.MenuCaptionX), "Весь ряд: вправо", s.MenuCaptionX, -300, 300);
            s.MenuCaptionY = Slider(nameof(s.MenuCaptionY), "Весь ряд: вверх", s.MenuCaptionY, -300, 300);
            s.MenuCaptionFont = Slider(nameof(s.MenuCaptionFont), "Размер подписи Добавить в план", s.MenuCaptionFont, 10, 32);
            s.MenuActionGap = Slider(nameof(s.MenuActionGap), "Одинаковый промежуток между действиями меню", s.MenuActionGap, 0, 40);
            GUILayout.Label("Иконки — независимо от текста и строк");
            s.Icon = Slider(nameof(s.Icon), "Размер изображений", s.Icon, 8, 80);
            s.IconX = Slider(nameof(s.IconX), "Иконки: сдвиг вправо", s.IconX, -60, 60);
            s.IconY = Slider(nameof(s.IconY), "Иконки: сдвиг вниз", s.IconY, -60, 60);
            s.GoalIconSpacing = Slider(nameof(s.GoalIconSpacing), "Иконки целей: добавка к вертикальному шагу", s.GoalIconSpacing, -20, 20);
            s.MaterialIconSpacing = Slider(nameof(s.MaterialIconSpacing), "Иконки материалов: добавка к вертикальному шагу", s.MaterialIconSpacing, -20, 20);
            GUILayout.Label("Сообщения сундука — весь блок");
            s.ChestRowX = Slider(nameof(s.ChestRowX), "Весь ряд A / X / B / Y (F): вправо", s.ChestRowX, -600, 600);
            s.ChestRowY = Slider(nameof(s.ChestRowY), "Весь ряд подсказок: вверх", s.ChestRowY, -300, 300);
            s.ChestActionGap = Slider(nameof(s.ChestActionGap), "Одинаковый промежуток между действиями", s.ChestActionGap, 0, 40);
            s.FeedbackX = Slider(nameof(s.FeedbackX), "Результат переноса: вправо", s.FeedbackX, -600, 1000);
            s.FeedbackY = Slider(nameof(s.FeedbackY), "Результат переноса: вверх", s.FeedbackY, -300, 150);
            s.FeedbackGap = Slider(nameof(s.FeedbackGap), "Промежуток между взятыми предметами", s.FeedbackGap, -8, 40);
            s.FeedbackLineGap = Slider(nameof(s.FeedbackLineGap), "Промежуток между строками сообщений", s.FeedbackLineGap, 0, 24);
            GUILayout.Label("Мягкая подложка");
            s.Opacity = Slider(nameof(s.Opacity), "Плотность подложки (%)", s.Opacity * 100, 0, 85) / 100;
            s.TextOpacity = Slider(nameof(s.TextOpacity), "Плотность текста (%)", s.TextOpacity * 100, 50, 100) / 100;
            s.ShadowRadius = Slider(nameof(s.ShadowRadius), "Скругление подложки", s.ShadowRadius, 6, 90);
            s.ShadowFeather = Slider(nameof(s.ShadowFeather), "Мягкость краёв", s.ShadowFeather, 5, 28);
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
        private static float Slider(string field, string title, float value, float min, float max, float step = 1)
        {
            GUILayout.Label(title + ": " + value.ToString(step == 1 ? "0" : "0.00", System.Globalization.CultureInfo.InvariantCulture));
            GUILayout.BeginHorizontal();
            float next = (float)(Math.Round(GUILayout.HorizontalSlider(value, min, max) / step, MidpointRounding.AwayFromZero) * step);
            if (GUILayout.Button("Сброс", GUILayout.Width(60)))
            {
                next = PlannerStyle.BaselineValue(field);
                GUI.changed = true;
            }
            GUILayout.EndHorizontal();
            return next;
        }
#else
        internal static bool IsOpen { get { return false; } }
        internal static bool PreviewDone { get { return false; } }
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
