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
    public void SetAnchor(GameObject go)
    {
    }
}

public class UIPanel : UIRect
{
    public float width { get { return 0f; } }
    public float height { get { return 0f; } }
    public float alpha { get; set; }
    public UIDrawCall.Clipping clipping { get; set; }
    public Vector2 clipOffset { get; set; }
}

public class UIWidget : UIRect
{
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
    public Vector3[] worldCorners { get { return new Vector3[4]; } }
}

public class UILabel : UIWidget
{
    public enum Overflow
    {
        ShrinkContent,
        ClampContent,
        ResizeFreely,
        ResizeHeight
    }

    public string text { get; set; }
    public bool multiLine { get; set; }
    public Overflow overflowMethod { get; set; }
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
