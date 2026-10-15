using UnityEngine;

public class UIDrawCall
{
    public enum Clipping
    {
        None = 0,
        TextureMask = 1,
        SoftClip = 3,
        ConstrainButDontClip = 4
    }
}

public class UIRect : MonoBehaviour
{
    public class AnchorPoint { public int absolute; }
    public AnchorPoint leftAnchor = new AnchorPoint(), rightAnchor = new AnchorPoint();
    public void ResetAndUpdateAnchors() { }
    public enum AnchorUpdate { OnEnable = 0, OnUpdate = 1, OnStart = 2 }
    public AnchorUpdate updateAnchors;
    public void SetAnchor(GameObject go)
    {
    }
    public void SetAnchor(GameObject go, float leftRelative, int leftAbsolute, float bottomRelative, int bottomAbsolute, float rightRelative, int rightAbsolute, float topRelative, int topAbsolute) { }
}

public class UIPanel : UIRect
{
    public int depth { get; set; }
    public float width { get { return 0f; } }
    public float height { get { return 0f; } }
    public float alpha { get; set; }
    public UIDrawCall.Clipping clipping { get; set; }
    public Vector2 clipOffset { get; set; }
}

public class UIWidget : UIRect
{
    public void MarkAsChanged() { }
    public delegate void OnPostFillCallback(UIWidget widget, int start, System.Collections.Generic.List<Vector3> vertices, System.Collections.Generic.List<Vector2> uvs, System.Collections.Generic.List<Color> colors);
    public OnPostFillCallback onPostFill;
    public Color color { get; set; }
    public enum Pivot
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }

    public Pivot pivot { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public float alpha { get; set; }
    public int depth { get; set; }
    public bool isVisible { get { return true; } }
    public Vector3[] localCorners { get { return new Vector3[4]; } }
    public Vector3[] worldCorners { get { return new Vector3[4]; } }
}

public class UILabel : UIWidget
{
    public Vector2 printedSize { get { return Vector2.zero; } }
    public enum Effect { None = 0, Shadow = 1, Outline = 2, Outline8 = 3 }
    public enum Overflow
    {
        ShrinkContent,
        ClampContent,
        ResizeFreely,
        ResizeHeight
    }

    public string text { get; set; }
    public UIFont bitmapFont { get; set; }
    public Font trueTypeFont { get; set; }
    public int fontSize { get; set; }
    public FontStyle fontStyle { get; set; }
    public NGUIText.Alignment alignment { get; set; }
    public NGUIText.SymbolStyle symbolStyle { get; set; }
    public int spacingX { get; set; }
    public int spacingY { get; set; }
    public Effect effectStyle { get; set; }
    public Color effectColor { get; set; }
    public Vector2 effectDistance { get; set; }
    public int maxLineCount { get; set; }
    public bool multiLine { get; set; }
    public Overflow overflowMethod { get; set; }
}

public class UIFont : MonoBehaviour { }
public class UITable : MonoBehaviour { }
public class UIEventListener : MonoBehaviour
{
    public delegate void VoidDelegate(GameObject go);
    public VoidDelegate onClick;
    public delegate void BoolDelegate(GameObject go, bool state);
    public BoolDelegate onHover;
    public static UIEventListener Get(GameObject go) { return null; }
}
public class UICamera : MonoBehaviour
{
    public static GameObject hoveredObject { get { return null; } }
    public bool useMouse;
    public bool useTouch;
    public bool useKeyboard;
    public bool useController;
    public static UICamera FindCameraForLayer(int layer) { return null; }
}

namespace Rewired
{
    public interface IGamepadTemplate : IControllerTemplate { IControllerTemplateThumbStick rightStick { get; } }
}
public static class NGUIText
{
    public enum Alignment { Automatic = 0, Left = 1, Center = 2, Right = 3, Justified = 4 }
    public enum SymbolStyle { None = 0, Normal = 1, Colored = 2, NoEffect = 3 }
}
public class UIBasicSprite : UIWidget { }
public class UITexture : UIBasicSprite { public Texture mainTexture { get; set; } }
public class UI2DSprite : UIBasicSprite
{
    public Material material { get; set; }
    public Shader shader { get; set; }
    public Sprite sprite2D { get; set; }
    public bool fixedAspect { get; set; }
}

public class UIRoot : MonoBehaviour
{
    public int activeHeight { get { return 720; } }
}

public static class GJL
{
    public static string L(string id)
    {
        return id;
    }

    public static void EnsureLabelHasCorrectFont(UILabel label, bool do_cache)
    {
    }
}
