using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 导线管理器：
/// 1. 从屏幕中心（准星）发射射线，对准连接点后单击鼠标左键选择。
/// 2. 选中第一个端点后，再对准第二个端点单击完成连线（直线）。
/// 3. 对准导线单击可删除。
/// 4. 右键取消当前选择。
/// 5. 非游戏模式（主菜单激活）时暂停交互。
/// </summary>
public class WireManager : MonoBehaviour
{
    // ─── 导线外观 ──────────────────────────────────────────────────────
    [Header("导线外观")]
    public Color positiveWireColor = new Color(0.85f, 0.1f, 0.1f);
    public Color negativeWireColor = new Color(0.08f, 0.08f, 0.08f);
    [Tooltip("导线粗细（米）")]
    public float wireWidth = 0.006f;

    // ─── 交互配置 ─────────────────────────────────────────────────────
    [Header("交互配置")]
    [Tooltip("准星射线最大检测距离（米）")]
    public float raycastDistance = 5f;
    [Tooltip("SimulationUIManager 引用，用于判断是否处于游戏模式")]
    public SimulationUIManager uiManager;

    // ─── UI 引用 ──────────────────────────────────────────────────────
    [Header("UI 引用（由 WireConnectionSetup 自动填充）")]
    public UnityEngine.UI.Text hintText;
    public CrosshairUI crosshair;

    // ─── 内部状态 ─────────────────────────────────────────────────────
    private Camera mainCam;
    private WireConnectionPoint firstSelected = null;
    private WireConnectionPoint hoveredPoint  = null;

    private class WireRecord
    {
        public WireConnectionPoint pointA;
        public WireConnectionPoint pointB;
        public GameObject wireObject;
    }
    private List<WireRecord> wires = new List<WireRecord>();

    // ══════════════════════════════════════════════════════════════════
    void Start()
    {
        mainCam = Camera.main;
        SetHint("靠近接线柱，对准后单击鼠标左键开始接线");
    }

    void Update()
    {
        if (!IsGameActive()) return;

        HandleCrosshairRaycast();

        if (Input.GetMouseButtonDown(0))
            HandleClick();

        if (Input.GetMouseButtonDown(1) && firstSelected != null)
        {
            CancelSelection();
            SetHint("已取消选择");
        }
    }

    // ── 是否处于游戏模式 ──────────────────────────────────────────────
    private bool IsGameActive()
    {
        if (uiManager == null) return true;
        var field = typeof(SimulationUIManager).GetField(
            "mainPageUI",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field == null) return true;
        var go = field.GetValue(uiManager) as GameObject;
        return go == null || !go.activeSelf;
    }

    // ── 每帧：从屏幕中心发射射线，更新准星与悬停高亮 ─────────────────
    private void HandleCrosshairRaycast()
    {
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        WireConnectionPoint newHover = null;
        bool aimingWire = false;

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
        {
            newHover = hit.collider.GetComponentInParent<WireConnectionPoint>();
            if (newHover == null)
                aimingWire = FindWireByCollider(hit.collider) != null;
        }

        // 更新悬停高亮
        if (newHover != hoveredPoint)
        {
            if (hoveredPoint != null && hoveredPoint != firstSelected)
                hoveredPoint.SetHover(false);
            hoveredPoint = newHover;
            if (hoveredPoint != null && hoveredPoint != firstSelected)
                hoveredPoint.SetHover(true);
        }

        // 通知准星
        if (crosshair != null)
        {
            if (firstSelected != null)
                crosshair.SetState(CrosshairUI.State.Selected);
            else if (newHover != null)
                crosshair.SetState(CrosshairUI.State.Hover);
            else if (aimingWire)
                crosshair.SetState(CrosshairUI.State.WireHover);
            else
                crosshair.SetState(CrosshairUI.State.Normal);
        }
    }

    // ── 点击处理 ──────────────────────────────────────────────────────
    private void HandleClick()
    {
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance)) return;

        // 点到导线 → 删除
        WireRecord hitWire = FindWireByCollider(hit.collider);
        if (hitWire != null)
        {
            DeleteWire(hitWire);
            return;
        }

        // 点到连接点
        WireConnectionPoint point = hit.collider.GetComponentInParent<WireConnectionPoint>();
        if (point == null) return;

        if (firstSelected == null)
        {
            firstSelected = point;
            firstSelected.SetSelected(true);
            SetHint($"已选 [{GetTerminalName(point.terminalType)}]  →  再对准另一个端点单击完成连线\n右键取消");
        }
        else
        {
            if (point == firstSelected)
            {
                CancelSelection();
                SetHint("已取消选择");
                return;
            }

            if (IsAlreadyConnected(firstSelected, point))
            {
                SetHint("这两个端点已连接，请选择其他端点");
                CancelSelection();
                return;
            }

            CreateWire(firstSelected, point);
            CancelSelection();
            SetHint("导线已连接！对准导线单击可删除  |  继续选择端点接线");
        }
    }

    private void CancelSelection()
    {
        if (firstSelected != null)
        {
            firstSelected.SetSelected(false);
            firstSelected = null;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // 导线创建（直线）
    // ═══════════════════════════════════════════════════════════════════
    private void CreateWire(WireConnectionPoint a, WireConnectionPoint b)
    {
        bool isPositive = !a.IsNegative() || !b.IsNegative();
        Color wireColor = isPositive ? positiveWireColor : negativeWireColor;

        GameObject wireObj = new GameObject($"Wire_{a.terminalType}_{b.terminalType}");

        LineRenderer lr = wireObj.AddComponent<LineRenderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = wireColor;
        mat.SetFloat("_Glossiness", 0.3f);
        mat.SetFloat("_Metallic", 0.1f);
        lr.material = mat;
        lr.startWidth    = wireWidth;
        lr.endWidth      = wireWidth;
        lr.useWorldSpace = true;
        lr.numCapVertices    = 5;
        lr.numCornerVertices = 5;
        lr.positionCount = 2;   // 直线：仅起点和终点

        lr.SetPosition(0, a.GetConnectionWorldPosition());
        lr.SetPosition(1, b.GetConnectionWorldPosition());

        // MeshCollider 用于点击检测
        MeshCollider mc = wireObj.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        lr.BakeMesh(mesh, mainCam, true);
        mc.sharedMesh = mesh;

        // WireUpdater 每帧同步端点位置
        WireUpdater updater = wireObj.AddComponent<WireUpdater>();
        updater.Initialize(lr, mc, a, b, mainCam);

        wires.Add(new WireRecord { pointA = a, pointB = b, wireObject = wireObj });
    }

    // ═══════════════════════════════════════════════════════════════════
    // 导线删除
    // ═══════════════════════════════════════════════════════════════════
    private WireRecord FindWireByCollider(Collider col)
    {
        foreach (var w in wires)
        {
            if (w.wireObject == null) continue;
            var mc = w.wireObject.GetComponent<MeshCollider>();
            if (mc != null && mc == col) return w;
        }
        return null;
    }

    private void DeleteWire(WireRecord record)
    {
        wires.Remove(record);
        if (record.wireObject != null)
            Destroy(record.wireObject);
        SetHint("导线已删除  |  对准端点继续接线");
    }

    // ═══════════════════════════════════════════════════════════════════
    // 工具
    // ═══════════════════════════════════════════════════════════════════
    private bool IsAlreadyConnected(WireConnectionPoint a, WireConnectionPoint b)
    {
        foreach (var w in wires)
            if ((w.pointA == a && w.pointB == b) || (w.pointA == b && w.pointB == a))
                return true;
        return false;
    }

    private void SetHint(string msg)
    {
        if (hintText != null) hintText.text = msg;
    }

    private static string GetTerminalName(WireConnectionPoint.TerminalType t)
    {
        switch (t)
        {
            case WireConnectionPoint.TerminalType.Battery_Neg:     return "电池 负极";
            case WireConnectionPoint.TerminalType.Battery_Pos:     return "电池 正极";
            case WireConnectionPoint.TerminalType.Voltmeter_Neg:   return "电压表 COM(-)";
            case WireConnectionPoint.TerminalType.Voltmeter_Pos3:  return "电压表 3V(+)";
            case WireConnectionPoint.TerminalType.Voltmeter_Pos15: return "电压表 15V(+)";
            default: return t.ToString();
        }
    }
}
