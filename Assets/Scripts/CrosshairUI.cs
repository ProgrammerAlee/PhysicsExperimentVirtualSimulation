using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 屏幕中心准星。
/// 挂载在 Canvas 下的一个 GameObject 上（由 WireConnectionSetup 自动创建）。
/// 根据 WireManager 传入的状态改变颜色和大小，给玩家清晰的交互反馈。
///
/// 外观：中心小圆点 + 四条短线（十字缺口准星），纯代码生成，无需图片资源。
/// </summary>
public class CrosshairUI : MonoBehaviour
{
    public enum State
    {
        Normal,      // 白色 — 普通
        Hover,       // 黄色 — 对准可连接点
        Selected,    // 绿色 — 已选中第一个端点，等待选第二个
        WireHover    // 红色 — 对准导线（可删除）
    }

    [Header("尺寸")]
    [Tooltip("中心圆点直径（像素）")]
    public float dotSize     = 8f;
    [Tooltip("短横线长度（像素）")]
    public float lineLength  = 10f;
    [Tooltip("短横线宽度（像素）")]
    public float lineWidth   = 2f;
    [Tooltip("短横线距中心的间距（像素）")]
    public float lineGap     = 5f;

    [Header("颜色")]
    public Color colorNormal      = new Color(1f,   1f,   1f,   0.90f);
    public Color colorHover       = new Color(1f,   0.85f,0f,   1f);
    public Color colorSelected    = new Color(0.1f, 1f,   0.3f, 1f);
    public Color colorWireHover   = new Color(1f,   0.3f, 0.2f, 1f);

    [Header("动画")]
    [Tooltip("状态切换时颜色/大小过渡速度")]
    public float lerpSpeed = 12f;
    [Tooltip("Hover 时准星放大倍率")]
    public float hoverScale = 1.35f;

    // ── 内部子元素引用 ────────────────────────────────────────────────
    private Image dot;
    private Image[] lines = new Image[4]; // 上下左右

    private State currentState  = State.Normal;
    private Color targetColor;
    private float targetScale;

    private Color  lerpedColor = Color.white;
    private float  lerpedScale = 1f;

    // ══════════════════════════════════════════════════════════════════
    void Awake()
    {
        BuildCrosshair();
        targetColor = colorNormal;
        targetScale = 1f;
        lerpedColor = colorNormal;
        lerpedScale = 1f;
    }

    void Update()
    {
        // 平滑过渡颜色和大小
        lerpedColor = Color.Lerp(lerpedColor, targetColor, Time.unscaledDeltaTime * lerpSpeed);
        lerpedScale = Mathf.Lerp(lerpedScale, targetScale, Time.unscaledDeltaTime * lerpSpeed);

        ApplyVisual();
    }

    /// <summary>由 WireManager 每帧调用，传入当前准星状态。</summary>
    public void SetState(State state)
    {
        if (state == currentState) return;
        currentState = state;

        switch (state)
        {
            case State.Normal:
                targetColor = colorNormal;
                targetScale = 1f;
                break;
            case State.Hover:
                targetColor = colorHover;
                targetScale = hoverScale;
                break;
            case State.Selected:
                targetColor = colorSelected;
                targetScale = hoverScale;
                break;
            case State.WireHover:
                targetColor = colorWireHover;
                targetScale = hoverScale;
                break;
        }
    }

    // ── 应用颜色和缩放 ────────────────────────────────────────────────
    private void ApplyVisual()
    {
        if (dot != null) dot.color = lerpedColor;
        foreach (var l in lines)
            if (l != null) l.color = lerpedColor;

        transform.localScale = Vector3.one * lerpedScale;
    }

    // ── 代码构建准星子元素 ────────────────────────────────────────────
    private void BuildCrosshair()
    {
        // 确保有 RectTransform，居中
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;

        // 中心圆点
        dot = CreateImage("Dot", Vector2.zero, new Vector2(dotSize, dotSize));

        // 四条短线：上、下、左、右
        float offset = lineGap + lineLength * 0.5f;

        lines[0] = CreateImage("Line_Up",    new Vector2(0,  offset), new Vector2(lineWidth, lineLength));
        lines[1] = CreateImage("Line_Down",  new Vector2(0, -offset), new Vector2(lineWidth, lineLength));
        lines[2] = CreateImage("Line_Left",  new Vector2(-offset, 0), new Vector2(lineLength, lineWidth));
        lines[3] = CreateImage("Line_Right", new Vector2( offset, 0), new Vector2(lineLength, lineWidth));
    }

    private Image CreateImage(string childName, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(childName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Image img = go.GetComponent<Image>();
        img.raycastTarget = false;

        if (img.sprite == null)
        {
            // 备选：动态创建一个纯白 Texture2D 作为 sprite
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            img.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }
        img.type = Image.Type.Simple;
        img.color = colorNormal;

        return img;
    }
}
