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

public abstract class UIRect : MonoBehaviour
{
    public class AnchorPoint
    {
        public Transform target;
        public float relative;
        public int absolute;
    }

    public AnchorPoint leftAnchor = new AnchorPoint();
    public AnchorPoint rightAnchor = new AnchorPoint();
    public AnchorPoint bottomAnchor = new AnchorPoint();
    public AnchorPoint topAnchor = new AnchorPoint();

    public float finalAlpha = 1f;

    public virtual float alpha { get; set; }
    public bool isAnchored { get { return false; } }
    public Camera anchorCamera { get { return null; } }
    public abstract Vector3[] localCorners { get; }
    public abstract Vector3[] worldCorners { get; }

    public void SetAnchor(GameObject go)
    {
    }
}

public class UIPanel : UIRect
{
    public int depth { get; set; }
    public UIDrawCall.Clipping clipping { get; set; }
    public Vector4 baseClipRegion { get; set; }
    public Vector4 finalClipRegion { get { return Vector4.zero; } }
    public Vector2 clipOffset { get; set; }
    public float width { get { return 0f; } }
    public float height { get { return 0f; } }

    public override Vector3[] localCorners { get { return new Vector3[4]; } }
    public override Vector3[] worldCorners { get { return new Vector3[4]; } }
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
    public virtual int depth { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public Color color { get; set; }
    public override float alpha { get; set; }
    public bool isVisible { get { return false; } }
    public bool hasVertices { get { return false; } }
    public UIPanel panel;

    public override Vector3[] localCorners { get { return new Vector3[4]; } }
    public override Vector3[] worldCorners { get { return new Vector3[4]; } }
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
    public int fontSize { get; set; }
}

public class UIRoot : MonoBehaviour
{
    public enum Scaling
    {
        Flexible,
        Constrained,
        ConstrainedOnMobiles
    }

    public Scaling scalingStyle;
    public int manualWidth;
    public int manualHeight;

    public int activeHeight { get { return 720; } }
    public float pixelSizeAdjustment { get { return 1f; } }
}

public static class GJL
{
    public static string L(string id)
    {
        return id;
    }
}
