using UnityEngine;

public class UIWidget : MonoBehaviour
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
}

public class UILabel : UIWidget
{
    public string text { get; set; }
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
