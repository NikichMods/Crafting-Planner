// SPDX-License-Identifier: MPL-2.0
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
namespace CraftingPlanner
{
    internal enum PlannerView { Hidden, Hud, Build, Chest }

    internal sealed class PlannerCard : IDisposable
    {
        private readonly GameObject _panelGo;
        private readonly UIPanel _panel;
        private readonly UIWidget _shell, _root;
        private readonly UIWidget _anchor;
        private readonly UILabel _fontTemplate, _measure;
        private readonly UI2DSprite _iconTemplate;
        private readonly UITexture _back, _action;
        private readonly UILabel _footer, _help;
        private readonly List<UILabel> _labels = new List<UILabel>();
        private readonly List<UI2DSprite> _icons = new List<UI2DSprite>();
        private int _labelCursor, _iconCursor, _width, _height;
        private bool _iconWarning;
        private PlannerView _view;
        internal bool IsAlive { get { return _panelGo != null; } }
        private PlannerStyle Style { get { return PlannerStyle.Current; } }
        internal PlannerCard(UIWidget anchor, UILabel fontTemplate, UI2DSprite iconTemplate)
        {
            _anchor = anchor; _fontTemplate = fontTemplate; _iconTemplate = iconTemplate;
            _panelGo = new GameObject("CraftingPlannerPresentation");
            _panelGo.layer = fontTemplate.gameObject.layer;
            _panelGo.SetActive(false);
            // Exact native frame chain: Screen size -> Screen size panel -> UI Root.
            _panelGo.transform.SetParent(anchor.transform.parent, false);
            _panel = _panelGo.AddComponent<UIPanel>();
            _shell = Child("placement", _panelGo.transform).AddComponent<UIWidget>();
            _shell.pivot = UIWidget.Pivot.TopRight;
            _shell.transform.localPosition = Vector3.zero;
            _shell.updateAnchors = UIRect.AnchorUpdate.OnUpdate;
            _root = Child("card", _shell.transform).AddComponent<UIWidget>();
            _root.pivot = UIWidget.Pivot.TopRight;
            _root.transform.localPosition = Vector3.zero;
            _back = Child("soft-backing", _root.transform).AddComponent<UITexture>();
            _back.mainTexture = Texture2D.whiteTexture;
            _back.pivot = UIWidget.Pivot.TopRight;
            _back.transform.localPosition = Vector3.zero; // pivot preserves corners by moving the transform
            _back.depth = 0;
            _back.onPostFill = FadeBacking;
            _measure = Child("measurement", _root.transform).AddComponent<UILabel>();
            _measure.gameObject.SetActive(false);
            _action = Child("storage-action", _root.transform).AddComponent<UITexture>();
            _action.mainTexture = Texture2D.whiteTexture;
            _action.pivot = UIWidget.Pivot.TopRight;
            _action.transform.localPosition = Vector3.zero;
            _action.depth = 1;
            _action.color = new Color(0.1f, 0.095f, 0.085f, 0.22f);
            BoxCollider2D collider = _action.gameObject.AddComponent<BoxCollider2D>();
            UIEventListener.Get(_action.gameObject).onClick = delegate(GameObject go)
            {
                if (PlannerPresentation.GetView() == PlannerView.Chest && GUIElements.me != null)
                    Planner.TakeNeeded(GUIElements.me.chest, "mouse_button");
            };
            _footer = Child("control-hint", _root.transform).AddComponent<UILabel>();
            _help = Child("storage-help", _root.transform).AddComponent<UILabel>();
        }
        private GameObject Child(string name, Transform parent)
        {
            GameObject go = new GameObject(name); go.layer = _panelGo.layer;
            go.transform.SetParent(parent, false); return go;
        }
        internal void Render(PlannerView view)
        {
            if (!IsAlive || _anchor == null || _fontTemplate == null) return;
            _view = view;
            if (view == PlannerView.Hidden || (!Planner.HasGoals && view != PlannerView.Build)) { Hide(); return; }
            Style.Clamp();
            _labelCursor = 0; _iconCursor = 0;
            int pad = (int)Style.Padding, line = (int)Math.Max(Style.Row, Style.Font + 4);
            var rows = Planner.AggregateRequirements().Select(p => new MaterialRow(p.Key, p.Value, Planner.GetPlayerHave(p.Key))).ToList();
            rows.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase));
            int col = (int)Style.Column;
            foreach (string header in new[] { PlannerLocalization.Have, PlannerLocalization.Required, PlannerLocalization.Missing }) col = Math.Max(col, Measure(header) + 4);
            foreach (MaterialRow row in rows)
                foreach (int count in new[] { row.Have, row.Need, row.Missing }) col = Math.Max(col, Measure(count.ToString()) + 4);
            int columnGap = (int)Style.ColumnGap;
            _width = Math.Max((int)Style.Width, 2 * pad + (int)Style.Icon + 8 + 64 + 3 * col + 2 * columnGap);
            int firstCountX = _width - pad - 3 * col - 2 * columnGap;
            int y = pad;
            Label(PlannerLocalization.Plan, pad, y, _width - 2 * pad, line, false, false);
            y += line + (int)Style.TitleGap;
            foreach (ProjectGoal goal in Planner.GetGoals())
            {
                UILabel name = Label(Planner.GetProjectName(goal.Definition), pad, y, _width - 2 * pad - 60, line, false, true);
                int rowHeight = Math.Max(line, (int)Math.Ceiling(name.printedSize.y));
                Label("×" + goal.Quantity, _width - pad - 60, y, 60, rowHeight, true, false);
                y += rowHeight;
            }
            y += 6;
            Label(PlannerLocalization.Have, firstCountX, y, col, line, true, false);
            Label(PlannerLocalization.Required, firstCountX + col + columnGap, y, col, line, true, false);
            Label(PlannerLocalization.Missing, firstCountX + 2 * (col + columnGap), y, col, line, true, false);
            y += line;
            foreach (MaterialRow row in rows)
            {
                int nameX = pad + (int)Style.Icon + 8;
                UILabel name = Label(row.Name, nameX, y, firstCountX - nameX - 8, line, false, true);
                int rowHeight = Math.Max(Math.Max(line, (int)Style.Icon), (int)Math.Ceiling(name.printedSize.y));
                Icon(row.Id, pad, y + (rowHeight - (int)Style.Icon) / 2);
                int textY = y + (rowHeight - line) / 2;
                Label(row.Have.ToString(), firstCountX, textY, col, line, true, false);
                Label(row.Need.ToString(), firstCountX + col + columnGap, textY, col, line, true, false);
                UILabel missing = Label(row.Missing == 0 ? "0" : row.Missing.ToString(), firstCountX + 2 * (col + columnGap), textY, col, line, true, false);
                missing.color = row.Missing == 0 ? new Color(0.72f, 0.80f, 0.62f, Style.TextOpacity) : new Color(0.96f, 0.82f, 0.57f, Style.TextOpacity);
                y += rowHeight + 3;
            }
            for (int i = _labelCursor; i < _labels.Count; i++) _labels[i].gameObject.SetActive(false);
            for (int i = _iconCursor; i < _icons.Count; i++) _icons[i].gameObject.SetActive(false);
            _height = y + pad;
            Place();
            Footer(view, line);
            UIPanel host = PlannerPresentation.HostPanel(view);
            _panel.depth = host != null ? host.depth + 1 : 1;
            _panelGo.SetActive(true);
        }
        private void Place()
        {
            _root.width = _width; _root.height = _height;
            _root.transform.localScale = Vector3.one * Style.Scale;
            _root.transform.localPosition = Vector3.zero;
            _back.width = _width; _back.height = _height;
            _back.transform.localPosition = Vector3.zero;
            _back.color = new Color(0.10f, 0.095f, 0.085f, Style.Opacity);
            _back.MarkAsChanged();
            int w = (int)Math.Ceiling(_width * Style.Scale), h = (int)Math.Ceiling(_height * Style.Scale);
            _shell.width = w; _shell.height = h;
            _shell.SetAnchor(_anchor.gameObject, 1f, -(int)Style.Right - w, 1f, -(int)Style.Top - h, 1f, -(int)Style.Right, 1f, -(int)Style.Top);
        }
        private void Configure(UILabel label, UILabel template, int width, int height, bool right, bool wrap)
        {
            ApplyNativeFont(label, template);
            label.fontSize = (int)Style.Font;
            label.pivot = UIWidget.Pivot.TopLeft;
            label.alignment = right ? NGUIText.Alignment.Right : NGUIText.Alignment.Left;
            label.multiLine = wrap; label.maxLineCount = wrap ? 0 : 1;
            label.overflowMethod = wrap ? UILabel.Overflow.ResizeHeight : UILabel.Overflow.ClampContent;
            label.width = Math.Max(width, 1); label.height = height;
            label.color = new Color(0.94f, 0.90f, 0.80f, Style.TextOpacity);
            label.depth = 2;
        }
        private UILabel Label(string text, int x, int y, int width, int height, bool right, bool wrap)
        {
            if (_labelCursor == _labels.Count) _labels.Add(Child("text-" + _labelCursor, _root.transform).AddComponent<UILabel>());
            UILabel label = _labels[_labelCursor++];
            Configure(label, _fontTemplate, width, height, right, wrap);
            label.transform.localPosition = new Vector3(-_width + x, -y, 0);
            label.text = text; label.gameObject.SetActive(true); return label;
        }
        private int Measure(string text)
        {
            ApplyNativeFont(_measure, _fontTemplate); _measure.fontSize = (int)Style.Font;
            _measure.multiLine = false; _measure.maxLineCount = 1;
            _measure.overflowMethod = UILabel.Overflow.ResizeFreely; _measure.text = text;
            return (int)Math.Ceiling(_measure.printedSize.x);
        }
        private void Icon(string itemId, int x, int y)
        {
            ItemDefinition definition = GameBalance.me.GetDataOrNull<ItemDefinition>(itemId);
            Sprite sprite = definition != null ? EasySpritesCollection.GetSprite(definition.GetIcon(), false, "") : null;
            if (_iconTemplate == null || sprite == null)
            {
                if (!_iconWarning) { Planner.LogLine("event=card_icon result=text_fallback"); _iconWarning = true; }
                return;
            }
            if (_iconCursor == _icons.Count)
            {
                UI2DSprite icon = UnityEngine.Object.Instantiate(_iconTemplate, _root.transform, false);
                icon.gameObject.name = "item-icon-" + _iconCursor; icon.gameObject.layer = _panelGo.layer;
                icon.SetAnchor((GameObject)null); icon.transform.localScale = Vector3.one;
                icon.transform.localRotation = Quaternion.identity; icon.fixedAspect = false;
                icon.pivot = UIWidget.Pivot.TopLeft; icon.depth = 1; _icons.Add(icon);
            }
            UI2DSprite current = _icons[_iconCursor++];
            current.width = current.height = (int)Style.Icon;
            current.sprite2D = sprite; current.transform.localPosition = new Vector3(-_width + x, -y, 0);
            current.gameObject.SetActive(true);
        }
        private void Footer(PlannerView view, int line)
        {
            bool build = view == PlannerView.Build, chest = view == PlannerView.Chest;
            UILabel template = _fontTemplate;
            BaseGUI native = chest ? (BaseGUI)GUIElements.me.chest : GUIElements.me.craft;
            if (native != null && native.button_tips != null && native.button_tips.label != null) template = native.button_tips.label;
            Configure(_footer, template, _width, line, false, true);
            _footer.transform.localPosition = new Vector3(-_width, -_height - Style.FooterGap, 0);
            if (build)
            {
                string left = GameKeyTip.Get(GameKey.PrevSubTab, PlannerLocalization.RemoveGoal, true, true, false, false);
                string right = GameKeyTip.Get(GameKey.NextSubTub, PlannerLocalization.AddGoal, true, true, false, false);
                _footer.text = string.IsNullOrEmpty(left + right) ? "LT " + PlannerLocalization.RemoveGoal + "   RT " + PlannerLocalization.AddGoal : left + "   " + right;
            }
            else if (chest)
            {
                string key = GameKeyTip.Get(GameKey.Option2, PlannerLocalization.TakeNeeded, true, true, false, false);
                _footer.text = string.IsNullOrEmpty(key) ? PlannerLocalization.TakeNeeded : key;
            }
            _footer.gameObject.SetActive(build || chest);
            _action.gameObject.SetActive(chest);
            _help.gameObject.SetActive(chest);
            if (chest)
            {
                int actionHeight = Math.Max(line + 8, (int)Math.Ceiling(_footer.printedSize.y) + 8);
                _action.width = _width; _action.height = actionHeight;
                _action.transform.localPosition = new Vector3(0, -_height - Style.FooterGap + 4, 0);
                BoxCollider2D collider = _action.GetComponent<BoxCollider2D>();
                collider.size = new Vector2(_width, actionHeight);
                collider.offset = new Vector2(-_width * 0.5f, -actionHeight * 0.5f);
                Configure(_help, _fontTemplate, _width, line, false, true);
                _help.text = PlannerLocalization.CollectHelp;
                _help.transform.localPosition = new Vector3(-_width, -_height - Style.FooterGap - actionHeight - 2, 0);
            }
        }
        internal static void ApplyNativeFont(UILabel label, UILabel template)
        {
            if (template.bitmapFont != null) label.bitmapFont = template.bitmapFont; else label.trueTypeFont = template.trueTypeFont;
            label.fontSize = template.fontSize; label.fontStyle = template.fontStyle; label.symbolStyle = template.symbolStyle;
            label.spacingX = template.spacingX; label.spacingY = template.spacingY;
            label.effectStyle = template.effectStyle; label.effectColor = template.effectColor; label.effectDistance = template.effectDistance;
        }
        internal static float BackingStrength(float x, float y) { return Math.Max(0, Math.Min(1, (x + y) * 0.5f)); }
        private void FadeBacking(UIWidget widget, int start, List<Vector3> vertices, List<Vector2> uvs, List<Color> colors)
        {
            for (int i = start; i < vertices.Count; i++)
            {
                Color color = colors[i];
                color.a *= (float)Math.Pow(BackingStrength((vertices[i].x + _width) / _width, (vertices[i].y + _height) / _height), Style.FadePower);
                colors[i] = color;
            }
        }
        internal void Export(Camera camera)
        {
            Planner.LogLine("event=calibration_export format=1 version=" + Plugin.PluginVersion + " view=" + _view + " " + Style.Record() +
                " actual_width=" + _width + " actual_height=" + _height + " screen=" + Screen.width + "x" + Screen.height +
                " card_px=" + Rectangle(_root, camera) + " back_px=" + Rectangle(_back, camera) +
                " font=" + (_fontTemplate.bitmapFont != null ? _fontTemplate.bitmapFont.name : "dynamic") + " language=" + PlannerLocalization.CurrentLanguage);
        }
        private static string Rectangle(UIWidget widget, Camera camera)
        {
            if (camera == null) return "unavailable";
            Vector3[] corners = widget.worldCorners;
            Vector3 a = camera.WorldToScreenPoint(corners[0]), b = camera.WorldToScreenPoint(corners[2]);
            return string.Format(CultureInfo.InvariantCulture, "{0:0.#},{1:0.#},{2:0.#},{3:0.#}", a.x, a.y, b.x, b.y);
        }
        internal void Hide() { if (_panelGo != null) _panelGo.SetActive(false); }
        public void Dispose() { if (_back != null) _back.onPostFill = null; if (_panelGo != null) { _panelGo.SetActive(false); UnityEngine.Object.Destroy(_panelGo); } }
        private sealed class MaterialRow
        {
            internal string Id, Name; internal int Have, Need, Missing;
            internal MaterialRow(string id, int need, int have) { Id = id; Name = Planner.GetItemName(id); Have = have; Need = need; Missing = Math.Max(need - have, 0); }
        }
    }

    internal static class PlannerPresentation
    {
        private static PlannerCard _card;
        private static bool _failed;
        private static PlannerView _lastView = PlannerView.Hidden;
        internal static PlannerView GetView()
        {
            GUIElements gui = GUIElements.me;
            if (gui == null) return PlannerView.Hidden;
            if (BaseGUI.opened_windows.Count > 0)
            {
                BaseGUI top = BaseGUI.opened_windows[BaseGUI.opened_windows.Count - 1];
                if (top == gui.chest) return PlannerView.Chest;
                if (top == gui.craft && Planner.ShouldShowCraftOverlay) return PlannerView.Build;
                return PlannerView.Hidden;
            }
            return gui.hud != null && gui.hud.gameObject.activeInHierarchy ? PlannerView.Hud : PlannerView.Hidden;
        }
        internal static UIPanel HostPanel(PlannerView view)
        {
            GUIElements gui = GUIElements.me;
            if (view == PlannerView.Build && gui.craft != null) return gui.craft.GetComponentInParent<UIPanel>();
            if (view == PlannerView.Chest && gui.chest != null) return gui.chest.GetComponentInParent<UIPanel>();
            return gui.hud != null ? gui.hud.panel : null;
        }
        internal static void Refresh()
        {
            if (_failed) return;
            GUIElements gui = GUIElements.me;
            if (gui == null || gui.screen_size_widget == null || gui.hud == null || gui.hud.zone_name == null || gui.craft == null || gui.craft.craft_item_prefab == null) return;
            try
            {
                if (_card == null || !_card.IsAlive)
                {
                    BaseItemCellGUI cell = gui.craft.craft_item_prefab.item_gui;
                    _card = new PlannerCard(gui.screen_size_widget, gui.hud.zone_name, cell != null && cell.x1 != null ? cell.x1.icon : null);
                }
                PlannerView view = GetView();
                _card.Render(view);
                if (view != _lastView)
                {
                    Planner.LogLine("event=card_view view=" + view + " visible=" + (view != PlannerView.Hidden && (Planner.HasGoals || view == PlannerView.Build)));
                    _lastView = view;
                }
            }
            catch (Exception ex) { _failed = true; Destroy(); Planner.LogLine("event=card result=failed action=disabled_for_session error=" + ex); }
        }
        internal static void Export(Camera camera) { if (_card != null) _card.Export(camera); }
        internal static void Destroy() { if (_card != null) _card.Dispose(); _card = null; }
    }
    // Existing state/native lifecycle callers converge on the same single object.
    internal static class PlannerHud
    {
        internal static void Ensure(HUD hud) { PlannerPresentation.Refresh(); }
        internal static void Refresh() { PlannerPresentation.Refresh(); }
        internal static void RefreshIfVisible() { PlannerPresentation.Refresh(); }
        internal static void OnLanguageChanged() { PlannerPresentation.Refresh(); }
        internal static void Destroy() { PlannerPresentation.Destroy(); }
    }
    internal static class PlannerCraftOverlay
    {
        internal static void Open(CraftGUI gui) { PlannerPresentation.Refresh(); }
        internal static void Refresh() { PlannerPresentation.Refresh(); }
        internal static void Hide() { PlannerPresentation.Refresh(); }
        internal static void OnLanguageChanged() { PlannerPresentation.Refresh(); }
        internal static void Destroy() { PlannerPresentation.Destroy(); }
    }
    internal static class PlannerInputHints
    {
        internal static void Refresh() { PlannerPresentation.Refresh(); }
        internal static void HideCraft() { PlannerPresentation.Refresh(); }
        internal static void HideChest() { PlannerPresentation.Refresh(); }
        internal static void OnInputChanged() { PlannerPresentation.Refresh(); }
        internal static void OnWindowChanged(BaseGUI gui) { PlannerPresentation.Refresh(); }
        internal static void Destroy() { }
    }
}
