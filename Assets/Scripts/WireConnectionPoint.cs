using UnityEngine;

/// <summary>
/// 导线连接点组件，挂载在可连接导线的终端节点上。
/// 包含端点类型标识，以及高亮视觉反馈。
/// </summary>
public class WireConnectionPoint : MonoBehaviour
{
    public enum TerminalType
    {
        Battery_Neg,        // 电池负极
        Battery_Pos,        // 电池正极
        Voltmeter_Neg,      // 电压表负极
        Voltmeter_Pos3,     // 电压表正极3V
        Voltmeter_Pos15,    // 电压表正极15V
        Resistor_Left,      // 应变片电阻左端
        Resistor_Right,     // 应变片电阻右端
        Rheostat_A,         // 滑动变阻器接线柱A（左下）
        Rheostat_B,         // 滑动变阻器接线柱B（右下）
        Rheostat_C,         // 滑动变阻器接线柱C（左上）
        Rheostat_D,         // 滑动变阻器接线柱D（右上）
        Switch_Pos,         // 开关正极（或进线端）
        Switch_Neg          // 开关负极（或出线端）
    }

    [Header("端点配置")]
    public TerminalType terminalType;

    [Header("视觉反馈")]
    [Tooltip("悬停或选中时的高亮颜色")]
    public Color hoverColor = new Color(1f, 0.85f, 0f, 1f);   // 金黄色
    public Color selectedColor = new Color(0f, 1f, 0.4f, 1f); // 绿色
    public Color defaultColor = Color.white;

    // 内部状态
    private Renderer[] renderers;
    private Color[] originalColors;
    private bool isHovered = false;
    private bool isSelected = false;

    void Awake()
    {
        // 缓存所有子 Renderer
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
                originalColors[i] = renderers[i].material.color;
        }
    }

    /// <summary>设置悬停高亮状态</summary>
    public void SetHover(bool hover)
    {
        if (isSelected) return; // 已选中时不改变高亮
        isHovered = hover;
        UpdateVisual();
    }

    /// <summary>设置选中状态</summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || renderers[i].material == null) continue;
            if (isSelected)
                renderers[i].material.color = selectedColor;
            else if (isHovered)
                renderers[i].material.color = hoverColor;
            else
                renderers[i].material.color = originalColors[i];
        }
    }

    /// <summary>返回端点世界坐标（用于导线起止点）</summary>
    public Vector3 GetConnectionWorldPosition()
    {
        return transform.position;
    }

    /// <summary>判断是否为负极</summary>
    public bool IsNegative()
    {
        return terminalType == TerminalType.Battery_Neg ||
               terminalType == TerminalType.Voltmeter_Neg ||
               terminalType == TerminalType.Switch_Neg;
    }
}
