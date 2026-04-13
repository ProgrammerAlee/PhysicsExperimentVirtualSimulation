using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 修复 DigitalVoltmeter Prefab 的 URP 材质问题。
/// GenerateVoltmeter.cs 使用 Standard Shader 创建材质，但 URP 项目不支持。
/// 本脚本将所有 MeshRenderer 更换为正确的 URP Lit 材质。
/// 菜单：Physics Lab → Fix Voltmeter Materials (修复电压表材质)
/// </summary>
public class FixVoltmeterMaterials
{
    private const string URP_LIT = "Universal Render Pipeline/Lit";
    private const string MAT_DIR = "Assets/Materials/PhysicsLab";
    private const string PREFAB_PATH = "Assets/Prefabs/DigitalVoltmeter.prefab";

    // ── 颜色常量 ──────────────────────────────────────────────────────────
    static readonly Color C_BLACK       = new Color(0.05f, 0.05f, 0.05f);
    static readonly Color C_RED         = new Color(0.80f, 0.10f, 0.10f);
    static readonly Color C_DARK_RED    = new Color(0.10f, 0.05f, 0.05f); // 屏幕底色
    static readonly Color C_TEAL       = new Color(0.10f, 0.60f, 0.50f); // 面板绿色
    static readonly Color C_HOLE_BLACK  = new Color(0.02f, 0.02f, 0.02f);

    static Dictionary<string, Material> s_cache = new Dictionary<string, Material>();

    [MenuItem("Physics Lab/Fix Voltmeter Materials (修复电压表材质)", false, 11)]
    public static void Execute()
    {
        s_cache.Clear();

        // 确认 shader 可用
        if (Shader.Find(URP_LIT) == null)
        {
            EditorUtility.DisplayDialog("错误",
                $"找不到 Shader：{URP_LIT}\n请确保项目已正确配置 URP。", "确定");
            return;
        }

        // 确保材质目录存在
        if (!Directory.Exists(MAT_DIR))
            Directory.CreateDirectory(MAT_DIR);
        AssetDatabase.Refresh();

        // 加载 Prefab
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_PATH);
        if (prefabAsset == null)
        {
            EditorUtility.DisplayDialog("错误", $"未找到 Prefab：{PREFAB_PATH}", "确定");
            return;
        }

        int total = 0;
        using (var scope = new PrefabUtility.EditPrefabContentsScope(PREFAB_PATH))
        {
            foreach (MeshRenderer mr in
                scope.prefabContentsRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                Material mat = ResolveMaterial(mr.gameObject);
                if (mat != null)
                {
                    mr.sharedMaterial = mat;
                    total++;
                }
                else
                {
                    Debug.LogWarning($"[FixVoltmeter] 未匹配节点: {GetPath(mr.transform)}，使用黑色默认材质");
                    mr.sharedMaterial = GetMat("VoltmeterBlack", C_BLACK, 0.4f, 0.1f);
                    total++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string msg = $"✅ 电压表材质修复完成！\n• 更新 MeshRenderer：{total} 个\n材质保存于：{MAT_DIR}";
        Debug.Log("[FixVoltmeter] " + msg);
        EditorUtility.DisplayDialog("Physics Lab - 电压表材质修复", msg, "确定");
    }

    // ─────────────────────────────────────────────────────────────────────
    //  根据节点名称分配对应 URP 材质
    // ─────────────────────────────────────────────────────────────────────
    static Material ResolveMaterial(GameObject go)
    {
        string n = go.name;
        string nl = n.ToLower();

        // 1. 孔（接线柱顶部圆孔）
        if (nl == "hole")
            return GetMat("VoltmeterHole", C_HOLE_BLACK, 0.1f, 0f);

        // 2. 屏幕（显示区域 Quad）
        if (nl == "screen")
            return GetMat("VoltmeterScreen", C_DARK_RED, 0.1f, 0f);

        // 3. 接线面板（绿色 Quad）
        if (nl == "terminalpanel")
            return GetMat("VoltmeterPanel", C_TEAL, 0.35f, 0.05f);

        // 4. 正极接线柱（红色）
        if (nl.StartsWith("terminal_pos"))
            return GetMat("VoltmeterTerminalRed", C_RED, 0.55f, 0.3f);

        // 5. 负极接线柱（黑色）
        if (nl == "terminal_neg")
            return GetMat("VoltmeterTerminalBlack", C_BLACK, 0.55f, 0.3f);

        // 6. 主体（BaseBody / SlantBody 等方块）— 黑色机壳
        return GetMat("VoltmeterBlack", C_BLACK, 0.4f, 0.1f);
    }

    // ─────────────────────────────────────────────────────────────────────
    //  创建或复用 URP Lit 材质
    // ─────────────────────────────────────────────────────────────────────
    static Material GetMat(string id, Color color, float smooth, float metallic)
    {
        if (s_cache.TryGetValue(id, out Material cached)) return cached;

        string path = $"{MAT_DIR}/{id}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find(URP_LIT)) { name = id };
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smooth);
        mat.SetFloat("_Metallic", metallic);
        mat.color = color; // 兼容旧版访问

        EditorUtility.SetDirty(mat);
        s_cache[id] = mat;
        return mat;
    }

    static string GetPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetPath(t.parent) + "/" + t.name;
    }
}
