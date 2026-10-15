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
        private readonly UITexture _back;
        private readonly UI2DSprite _header;
        private Texture2D _backTexture;
        private int _backWidth, _backHeight, _bodyTop;
        private float _backRadius, _backFeather;
        private readonly UILabel _footer;
        private readonly List<UILabel> _labels = new List<UILabel>();
        private readonly List<UI2DSprite> _icons = new List<UI2DSprite>();
        private readonly List<UITexture> _arrows = new List<UITexture>();
        private readonly Camera _mouseCamera;
        private int _arrowCursor;
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
            UICamera events = UICamera.FindCameraForLayer(_panelGo.layer);
            _mouseCamera = events != null ? events.GetComponent<Camera>() : null;
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
            UI2DSprite nativeHeader = fontTemplate.transform.parent.GetComponent<UI2DSprite>();
            if (nativeHeader != null)
            {
                _header = Child("native-title", _root.transform).AddComponent<UI2DSprite>();
                _header.sprite2D = nativeHeader.sprite2D;
                _header.material = nativeHeader.material; _header.shader = nativeHeader.shader;
                _header.pivot = UIWidget.Pivot.TopRight; _header.transform.localPosition = Vector3.zero;
                _header.depth = 1; _header.color = nativeHeader.color;
            }
            _measure = Child("measurement", _root.transform).AddComponent<UILabel>();
            _measure.gameObject.SetActive(false);
            _footer = Child("control-hint", _root.transform).AddComponent<UILabel>();
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
            _labelCursor = 0; _iconCursor = 0; _arrowCursor = 0;
            foreach (UITexture arrow in _arrows) UIEventListener.Get(arrow.gameObject).onHover = null;
            int pad = (int)Style.Padding, line = (int)Math.Max(Style.Row, Style.Font + 4);
            var rows = Planner.AggregateRequirements().Select(p => new MaterialRow(p.Key, p.Value, Planner.GetPlayerHave(p.Key))).ToList();
            rows.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));
            int col = (int)Style.Column;
            foreach (string header in new[] { PlannerLocalization.Required, PlannerLocalization.Have, PlannerLocalization.Missing }) col = Math.Max(col, Measure(header, Math.Max(11, (int)Style.Font - 3)) + 4);
            foreach (MaterialRow row in rows)
                foreach (int count in new[] { row.Have, row.Need, row.Missing }) col = Math.Max(col, Measure(count.ToString()) + 4);
            int columnGap = (int)Style.ColumnGap;
            _width = Math.Max((int)Style.Width, 2 * pad + (int)Style.Icon + 8 + 3 * col + 2 * columnGap);
            int firstCountX = _width - pad - 3 * col - 2 * columnGap;
            int y = pad;
            int titleHeight = Math.Max(32, line);
            Center(PlannerLocalization.Plan, pad, y, _width - 2 * pad, titleHeight);
            y += titleHeight + (int)Style.TitleGap;
            _bodyTop = y;
            foreach (ProjectGoal goal in Planner.GetGoals())
            {
                int nameX = pad + (int)Style.Icon + 4;
                int slotX = _width - pad - PlannerLayout.QuantitySlotWidth;
                UILabel name = Label(Planner.GetProjectName(goal.Definition), nameX, y, slotX - nameX - 4, line, false, true);
                int rowHeight = Math.Max((int)Style.Icon, Math.Max(line, (int)Math.Ceiling(name.printedSize.y)));
                SpriteIcon(goal.Definition.icon, pad, y + (rowHeight - (int)Style.Icon) / 2);
                Center("×" + goal.Quantity, PlannerLayout.QuantityX(slotX), y + (rowHeight - line) / 2, PlannerLayout.QuantityWidth, line).overflowMethod = UILabel.Overflow.ShrinkContent;
                if (PlannerEditing.Active && ReferenceEquals(PlannerEditing.Selected, goal)) name.color = new Color(1, .94f, .72f, Style.TextOpacity);
                if (_mouseCamera != null && (!LazyInput.gamepad_active || PlannerEditing.Active))
                {
                    Arrow(goal, -1, slotX, y + (rowHeight - line) / 2, line);
                    if (goal.Repeatable) Arrow(goal, 1, slotX + PlannerLayout.ArrowWidth + PlannerLayout.QuantityWidth, y + (rowHeight - line) / 2, line);
                }
                y += rowHeight;
            }
            y += 6;
            Center(PlannerLocalization.Required, firstCountX, y, col, line).fontSize = Math.Max(11, (int)Style.Font - 3);
            Center(PlannerLocalization.Have, firstCountX + col + columnGap, y, col, line).fontSize = Math.Max(11, (int)Style.Font - 3);
            Center(PlannerLocalization.Missing, firstCountX + 2 * (col + columnGap), y, col, line).fontSize = Math.Max(11, (int)Style.Font - 3);
            y += line;
            foreach (MaterialRow row in rows)
            {
                int rowHeight = Math.Max(line, (int)Style.Icon);
                if (!Icon(row.Id, pad, y + (rowHeight - (int)Style.Icon) / 2))
                {
                    // A missing native image must never silently erase the ingredient identity.
                    UILabel fallback = Label(row.Name, pad, y, _width - 2 * pad, line, false, true);
                    y += Math.Max(line, (int)Math.Ceiling(fallback.printedSize.y));
                }
                int textY = y + (rowHeight - line) / 2;
                Center(row.Need.ToString(), firstCountX, textY, col, line);
                Center(row.Have.ToString(), firstCountX + col + columnGap, textY, col, line);
                UILabel missing = Center(row.Missing.ToString(), firstCountX + 2 * (col + columnGap), textY, col, line);
                missing.color = row.Missing == 0 ? new Color(0.533f, 0.843f, 0.486f, Style.TextOpacity) : new Color(0.894f, 0.467f, 0.400f, Style.TextOpacity);
                y += rowHeight + 3;
            }
            for (int i = _labelCursor; i < _labels.Count; i++) _labels[i].gameObject.SetActive(false);
            for (int i = _iconCursor; i < _icons.Count; i++) _icons[i].gameObject.SetActive(false);
            for (int i = _arrowCursor; i < _arrows.Count; i++) _arrows[i].gameObject.SetActive(false);
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
            UpdateBacking();
            if (_header != null) { _header.width = _width; _header.height = _bodyTop - (int)Style.TitleGap; _header.transform.localPosition = Vector3.zero; }
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
            label.effectStyle = UILabel.Effect.Outline;
            label.effectColor = new Color(.065f, .065f, .055f, .8f);
            label.effectDistance = new Vector2(1, 1);
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
        private UILabel Center(string text, int x, int y, int width, int height)
        {
            UILabel label = Label(text, x, y, width, height, false, false);
            label.alignment = NGUIText.Alignment.Center;
            label.pivot = UIWidget.Pivot.Center;
            label.transform.localPosition = new Vector3(-_width + x + width * .5f, -y - height * .5f, 0);
            return label;
        }
        private void Arrow(ProjectGoal goal, int delta, int x, int y, int height)
        {
            if (_arrowCursor == _arrows.Count)
            {
                UITexture created = Child("plan-arrow-" + _arrowCursor, _root.transform).AddComponent<UITexture>();
                created.mainTexture = Texture2D.whiteTexture; created.pivot = UIWidget.Pivot.TopLeft; created.depth = 1;
                created.gameObject.AddComponent<BoxCollider2D>(); _arrows.Add(created);
            }
            UITexture button = _arrows[_arrowCursor++];
            button.width = PlannerLayout.ArrowWidth; button.height = height;
            button.transform.localPosition = new Vector3(-_width + x, -y, 0);
            button.color = new Color(.2f, .2f, .18f, .04f); button.gameObject.SetActive(true);
            BoxCollider2D collider = button.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(button.width, height); collider.offset = new Vector2(button.width * .5f, -height * .5f);
            UILabel glyph = Center(delta < 0 ? "<" : ">", x, y, button.width, height);
            Color muted = new Color(.70f, .70f, .65f, Style.TextOpacity); glyph.color = muted;
            UIEventListener listener = UIEventListener.Get(button.gameObject);
            listener.onHover = delegate(GameObject go, bool over)
            {
                glyph.color = over ? new Color(1, .94f, .72f, Style.TextOpacity) : muted;
                button.color = new Color(.2f, .2f, .18f, over ? .25f : .04f);
            };
            listener.onClick = delegate(GameObject go)
            {
                if (!PlannerCalibration.IsOpen && PlannerPresentation.GetView() != PlannerView.Hidden) Planner.AdjustPinned(goal, delta, "mouse_plan_arrow");
            };
            listener.onHover(button.gameObject, UICamera.hoveredObject == button.gameObject);
        }
        internal bool BlocksWorldClick()
        {
            if (_panelGo == null || !_panelGo.activeInHierarchy || _mouseCamera == null || !Input.GetMouseButton(0)) return false;
            Vector3 pointer = Input.mousePosition;
            foreach (UITexture arrow in _arrows)
            {
                if (!arrow.gameObject.activeInHierarchy) continue;
                Vector3[] corners = arrow.worldCorners;
                Vector3 a = _mouseCamera.WorldToScreenPoint(corners[0]), b = _mouseCamera.WorldToScreenPoint(corners[2]);
                if (pointer.x >= Math.Min(a.x, b.x) && pointer.x <= Math.Max(a.x, b.x) && pointer.y >= Math.Min(a.y, b.y) && pointer.y <= Math.Max(a.y, b.y)) return true;
            }
            return false;
        }
        private int Measure(string text, int font = 0)
        {
            ApplyNativeFont(_measure, _fontTemplate); _measure.fontSize = font > 0 ? font : (int)Style.Font;
            _measure.multiLine = false; _measure.maxLineCount = 1;
            _measure.overflowMethod = UILabel.Overflow.ResizeFreely; _measure.text = text;
            return (int)Math.Ceiling(_measure.printedSize.x);
        }
        private bool Icon(string itemId, int x, int y)
        {
            ItemDefinition definition = GameBalance.me.GetDataOrNull<ItemDefinition>(itemId);
            return SpriteIcon(definition != null ? definition.GetIcon() : null, x, y);
        }
        private bool SpriteIcon(string iconName, int x, int y)
        {
            Sprite sprite = string.IsNullOrEmpty(iconName) ? null : EasySpritesCollection.GetSprite(iconName, false, "");
            if (_iconTemplate == null || sprite == null)
            {
                if (!_iconWarning) { Planner.LogLine("event=card_icon result=text_fallback"); _iconWarning = true; }
                return false;
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
            return true;
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
                string left = GameKeyTip.Get(GameKey.Left, PlannerLocalization.RemoveGoal, true, true, false, false);
                string right = GameKeyTip.Get(GameKey.Right, PlannerLocalization.AddGoal, true, true, false, false);
                _footer.text = LazyInput.gamepad_active ? left + "   " + right : "";
            }
            else if (chest)
            {
                _footer.text = ""; // Storage action and feedback live beside the native chest controls.
            }
            else if (view == PlannerView.Hud && LazyInput.gamepad_active && PlannerEditing.Available)
                _footer.text = PlannerEditing.Active ? GameKeyTip.Get(GameKey.Up, "", true, true, false, false) + GameKeyTip.Get(GameKey.Down, "", true, true, false, false) +
                    "   " + GameKeyTip.Get(GameKey.Left, "", true, true, false, false) + GameKeyTip.Get(GameKey.Right, "", true, true, false, false) +
                    "   " + GameKeyTip.Get(GameKey.Back, PlannerLocalization.FinishEditing, true, true, false, false) : GameKeyTip.RightStick(true, true, false, PlannerLocalization.EditPlan).ToString();
            else _footer.text = "";
            _footer.gameObject.SetActive(!string.IsNullOrEmpty(_footer.text));
        }
        internal static void ApplyNativeFont(UILabel label, UILabel template)
        {
            if (template.bitmapFont != null) label.bitmapFont = template.bitmapFont; else label.trueTypeFont = template.trueTypeFont;
            label.fontSize = template.fontSize; label.fontStyle = template.fontStyle; label.symbolStyle = template.symbolStyle;
            label.spacingX = template.spacingX; label.spacingY = template.spacingY;
            label.effectStyle = template.effectStyle; label.effectColor = template.effectColor; label.effectDistance = template.effectDistance;
        }
        private void UpdateBacking()
        {
            int feather = (int)Math.Ceiling(Style.ShadowFeather);
            int bodyHeight = Math.Max(1, _height - _bodyTop);
            int width = _width + 2 * feather, height = bodyHeight + 2 * feather;
            if (_backTexture == null || width != _backWidth || height != _backHeight || _backRadius != Style.ShadowRadius || _backFeather != Style.ShadowFeather)
            {
                int pixelWidth = Math.Min(width, 1024), pixelHeight = Math.Min(height, 1024);
                Texture2D texture = new Texture2D(pixelWidth, pixelHeight, TextureFormat.RGBA32, false);
                texture.name = "CraftingPlannerRoundedShadow"; texture.wrapMode = TextureWrapMode.Clamp; texture.filterMode = FilterMode.Bilinear;
                Color32[] pixels = new Color32[pixelWidth * pixelHeight];
                for (int y = 0; y < pixelHeight; y++) for (int x = 0; x < pixelWidth; x++)
                {
                    float alpha = PlannerLayout.BackingAlpha((x + .5f) * width / pixelWidth - feather, (y + .5f) * height / pixelHeight - feather, _width, bodyHeight, Style.ShadowRadius, Style.ShadowFeather);
                    pixels[y * pixelWidth + x] = new Color32(255, 255, 255, (byte)Math.Round(alpha * 255));
                }
                texture.SetPixels32(pixels); texture.Apply(false, true);
                Texture2D previous = _backTexture; _backTexture = texture; _back.mainTexture = texture;
                if (previous != null) UnityEngine.Object.Destroy(previous);
                _backWidth = width; _backHeight = height; _backRadius = Style.ShadowRadius; _backFeather = Style.ShadowFeather;
            }
            _back.width = width; _back.height = height;
            _back.transform.localPosition = new Vector3(feather, -_bodyTop + feather, 0);
            _back.color = new Color(.07f, .067f, .055f, Style.Opacity);
        }
        internal void Export(Camera camera)
        {
            Planner.LogLine("event=calibration_export format=2 version=" + Plugin.PluginVersion + " view=" + _view + " " + Style.Record() +
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
        public void Dispose() { if (_panelGo != null) { _panelGo.SetActive(false); UnityEngine.Object.Destroy(_panelGo); } if (_backTexture != null) UnityEngine.Object.Destroy(_backTexture); }
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
            PlannerChestControls.Refresh();
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
        internal static bool BlocksWorldClick() { return GetView() == PlannerView.Hud && _card != null && _card.BlocksWorldClick(); }
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
        internal static void Open(CraftGUI gui) { PlannerRowControls.RefreshAll(); PlannerPresentation.Refresh(); }
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
