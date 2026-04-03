#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor 工具脚本，菜单：Tools > Wire System > Setup Wire Connections
///
/// 一键完成：
/// 1. 给 DigitalVoltmeter 的 Terminal_Neg / Terminal_Pos3 / Terminal_Pos15 添加 SphereCollider + WireConnectionPoint
/// 2. 给电池的 Neg_Base / Pos_Base 添加 SphereCollider + WireConnectionPoint
/// 3. 在场景中创建 WireManager 并关联 SimulationUIManager
/// 4. 在 Canvas 中创建屏幕中心准星 CrosshairUI
/// 5. 在 Canvas 中创建接线提示 Text
/// </summary>
public class WireConnectionSetup : EditorWindow
{
    [MenuItem("Tools/Wire System/Setup Wire Connections")]
    public static void ShowWindow()
    {
        GetWindow<WireConnectionSetup>("Wire Connection Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("导线接线系统初始化", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "一键完成以下操作：\n" +
            "1. 给 Terminal_Neg / Terminal_Pos3 / Terminal_Pos15 添加碰撞体和 WireConnectionPoint\n" +
            "2. 给 Neg_Base / Pos_Base 添加碰撞体和 WireConnectionPoint\n" +
            "3. 创建 WireManager，关联 SimulationUIManager\n" +
            "4. 在 Canvas 中创建屏幕中心准星（CrosshairUI）\n" +
            "5. 在 Canvas 中创建接线提示文字",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("执行完整初始化", GUILayout.Height(42)))
            RunSetup();

        EditorGUILayout.Space();
        GUILayout.Label("单独操作", EditorStyles.miniLabel);

        if (GUILayout.Button("仅更新碰撞体和连接点", GUILayout.Height(28)))
            SetupConnectionPoints();

        if (GUILayout.Button("仅重建 WireManager + UI", GUILayout.Height(28)))
            SetupWireManager();
    }

    // ══════════════════════════════════════════════════════════════════
    static void RunSetup()
    {
        Undo.SetCurrentGroupName("Setup Wire Connections");
        int group = Undo.GetCurrentGroup();

        SetupConnectionPoints();
        SetupWireManager();

        Undo.CollapseUndoOperations(group);
        Debug.Log("[WireConnectionSetup] 初始化完成！");
        EditorUtility.DisplayDialog("完成",
            "导线接线系统初始化成功！\n\n" +
            "• 场景中已创建 WireManager\n" +
            "• Canvas 下已创建准星（CrosshairUI）和提示文字\n" +
            "• 请运行游戏验证效果", "OK");
    }

    // ── 给所有端点添加碰撞体 + WireConnectionPoint ──────────────────
    static void SetupConnectionPoints()
    {
        SetupTerminal("Terminal_Neg",   WireConnectionPoint.TerminalType.Voltmeter_Neg,   0.008f);
        SetupTerminal("Terminal_Pos3",  WireConnectionPoint.TerminalType.Voltmeter_Pos3,  0.008f);
        SetupTerminal("Terminal_Pos15", WireConnectionPoint.TerminalType.Voltmeter_Pos15, 0.008f);

        SetupBatteryBase("Neg_Base", WireConnectionPoint.TerminalType.Battery_Neg, 0.015f);
        SetupBatteryBase("Pos_Base", WireConnectionPoint.TerminalType.Battery_Pos, 0.015f);
    }

    static void SetupTerminal(string terminalName, WireConnectionPoint.TerminalType type, float radius)
    {
        GameObject go = FindInHierarchy("DigitalVoltmeter", terminalName)
                     ?? GameObject.Find(terminalName);
        if (go == null) { Debug.LogWarning($"[WireConnectionSetup] 未找到 {terminalName}"); return; }
        AddColliderAndPoint(go, type, radius);
        Debug.Log($"[WireConnectionSetup] {terminalName} → {type}");
    }

    static void SetupBatteryBase(string baseName, WireConnectionPoint.TerminalType type, float radius)
    {
        GameObject go = FindInHierarchy("电池", baseName)
                     ?? GameObject.Find(baseName);
        if (go == null) { Debug.LogWarning($"[WireConnectionSetup] 未找到 {baseName}"); return; }
        AddColliderAndPoint(go, type, radius);
        Debug.Log($"[WireConnectionSetup] {baseName} → {type}");
    }

    static void AddColliderAndPoint(GameObject go, WireConnectionPoint.TerminalType type, float radius)
    {
        SphereCollider sc = go.GetComponent<SphereCollider>();
        if (sc == null) sc = Undo.AddComponent<SphereCollider>(go);
        sc.isTrigger = false;
        sc.radius    = radius;
        sc.center    = Vector3.zero;

        WireConnectionPoint wcp = go.GetComponent<WireConnectionPoint>();
        if (wcp == null) wcp = Undo.AddComponent<WireConnectionPoint>(go);
        wcp.terminalType = type;

        EditorUtility.SetDirty(go);
    }

    // ── 创建 / 更新 WireManager ───────────────────────────────────────
    static void SetupWireManager()
    {
        WireManager wm;
        WireManager existing = Object.FindObjectOfType<WireManager>();

        if (existing == null)
        {
            GameObject go = new GameObject("WireManager");
            Undo.RegisterCreatedObjectUndo(go, "Create WireManager");
            wm = go.AddComponent<WireManager>();
            Debug.Log("[WireConnectionSetup] 已创建 WireManager。");
        }
        else
        {
            wm = existing;
            Debug.Log("[WireConnectionSetup] 使用已有 WireManager。");
        }

        // 关联 SimulationUIManager
        if (wm.uiManager == null)
        {
            var uiMgr = Object.FindObjectOfType<SimulationUIManager>();
            if (uiMgr != null)
            {
                wm.uiManager = uiMgr;
                Debug.Log("[WireConnectionSetup] 已关联 SimulationUIManager。");
            }
            else
                Debug.LogWarning("[WireConnectionSetup] 未找到 SimulationUIManager，请手动指定。");
        }

        // 获取 Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[WireConnectionSetup] 场景中没有 Canvas，跳过 UI 创建。");
            EditorUtility.SetDirty(wm);
            return;
        }

        // 创建准星
        wm.crosshair = SetupCrosshair(canvas, wm.crosshair);

        // 创建提示文字
        wm.hintText = SetupHintText(canvas, wm.hintText);

        EditorUtility.SetDirty(wm);
    }

    // ── 准星 CrosshairUI ──────────────────────────────────────────────
    static CrosshairUI SetupCrosshair(Canvas canvas, CrosshairUI existing)
    {
        if (existing != null) return existing;

        Transform found = canvas.transform.Find("Crosshair");
        GameObject go;
        if (found != null)
        {
            go = found.gameObject;
        }
        else
        {
            go = new GameObject("Crosshair");
            Undo.RegisterCreatedObjectUndo(go, "Create Crosshair");
            go.transform.SetParent(canvas.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            Debug.Log("[WireConnectionSetup] 已在 Canvas 下创建 Crosshair。");
        }

        CrosshairUI crosshair = go.GetComponent<CrosshairUI>();
        if (crosshair == null)
            crosshair = Undo.AddComponent<CrosshairUI>(go);

        EditorUtility.SetDirty(go);
        return crosshair;
    }

    // ── 提示文字 ──────────────────────────────────────────────────────
    static UnityEngine.UI.Text SetupHintText(Canvas canvas, UnityEngine.UI.Text existing)
    {
        if (existing != null) return existing;

        Transform found = canvas.transform.Find("WireHintText");
        GameObject go;
        if (found != null)
        {
            go = found.gameObject;
        }
        else
        {
            go = new GameObject("WireHintText");
            Undo.RegisterCreatedObjectUndo(go, "Create WireHintText");
            go.transform.SetParent(canvas.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 50f);
            rt.sizeDelta = new Vector2(700, 55);

            var text = go.AddComponent<UnityEngine.UI.Text>();
            text.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize  = 17;
            text.color     = new Color(1f, 1f, 0.85f, 0.95f);
            text.alignment = TextAnchor.MiddleCenter;
            text.text      = "靠近接线柱，对准后单击鼠标左键开始接线";

            Debug.Log("[WireConnectionSetup] 已在 Canvas 下创建 WireHintText。");
        }

        EditorUtility.SetDirty(go);
        return go.GetComponent<UnityEngine.UI.Text>();
    }

    // ── 工具方法 ──────────────────────────────────────────────────────
    static GameObject FindInHierarchy(string rootName, string childName)
    {
        GameObject root = GameObject.Find(rootName);
        if (root == null) return null;
        return FindChildRecursive(root.transform, childName);
    }

    static GameObject FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child.gameObject;
            GameObject found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
#endif
