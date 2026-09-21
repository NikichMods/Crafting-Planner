using UnityEngine;

public class UIRect : MonoBehaviour
{
    public void SetAnchor(GameObject go)
    {
    }
}

public class UIPanel : UIRect
{
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
}
