using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 修复 URP 升级后三个物理实验器件的材质问题：
///   1. 重建 Prefab 资产上的 URP Lit 材质
///   2. 清除场景中预制体实例上的旧材质 PropertyModification 覆盖，
///      使其回退到使用 Prefab 上已修好的材质
/// 菜单：Physics Lab → Fix URP Materials (修复URP材质)
/// </summary>
public class FixURPMaterials
{
    const string URP_LIT = "Universal Render Pipeline/Lit";
    const string MAT_DIR = "Assets/Materials/PhysicsLab";

    // ── 颜色常量 ──────────────────────────────────────────────────────────
    static readonly Color C_GREEN        = new Color(0.45f, 0.82f, 0.15f);
    static readonly Color C_GREEN_SLIDER = new Color(0.40f, 0.78f, 0.12f);
    static readonly Color C_GREEN_KNOB   = new Color(0.30f, 0.70f, 0.10f);
    static readonly Color C_BLACK        = new Color(0.08f, 0.08f, 0.08f);
    static readonly Color C_RED          = new Color(0.85f, 0.15f, 0.10f);
    static readonly Color C_METAL        = new Color(0.80f, 0.80f, 0.82f);
    static readonly Color C_METAL_DARK   = new Color(0.25f, 0.25f, 0.25f);
    static readonly Color C_COIL         = new Color(0.72f, 0.72f, 0.72f);
    static readonly Color C_RING         = new Color(0.55f, 0.55f, 0.55f);
    static readonly Color C_RESISTOR     = new Color(0.92f, 0.90f, 0.82f);
    static readonly Color C_HOLE         = new Color(0.02f, 0.02f, 0.02f);
    static readonly Color C_BAND_BROWN   = new Color(0.60f, 0.30f, 0.00f);
    static readonly Color C_BAND_BLACK   = new Color(0.10f, 0.10f, 0.10f);
    static readonly Color C_BAND_ORANGE  = new Color(1.00f, 0.60f, 0.00f);

    // ── 材质缓存 ──────────────────────────────────────────────────────────
    static Dictionary<string, Material> s_cache = new Dictionary<string, Material>();

    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Physics Lab/Fix URP Materials (修复URP材质)", false, 10)]
    public static void Execute()
    {
        s_cache.Clear();

        // 确保材质目录存在
        if (!Directory.Exists(MAT_DIR))
            Directory.CreateDirectory(MAT_DIR);
        AssetDatabase.Refresh();

        // 预先验证 shader
        if (Shader.Find(URP_LIT) == null)
        {
            EditorUtility.DisplayDialog("错误",
                $"找不到 Shader：{URP_LIT}\n请确保项目已正确配置 URP。", "确定");
            return;
        }

        string[] prefabNames = {
            "FixedResistor_定值电阻",
            "SlidingRheostat_滑动变阻器",
            "BatterySwitch_电池开关"
        };

        // ── 第一步：修复 Prefab 资产 ──────────────────────────────────────
        int prefabTotal = 0;
        foreach (string name in prefabNames)
        {
            string path = $"Assets/Prefabs/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                Debug.LogWarning($"[FixURP] 未找到: {path}");
                continue;
            }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                foreach (MeshRenderer mr in
                    scope.prefabContentsRoot.GetComponentsInChildren<MeshRenderer>(true))
                {
                    Material m = Resolve(mr.gameObject);
                    if (m != null) { mr.sharedMaterial = m; prefabTotal++; }
                    else Debug.LogWarning($"[FixURP] 未匹配: {GetPath(mr.transform)}");
                }
            }
            Debug.Log($"[FixURP] ✓ Prefab: {name}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── 第二步：修复当前已加载场景中的预制体实例 ─────────────────────
        int sceneTotal = 0;
        for (int si = 0; si < SceneManager.sceneCount; si++)
        {
            Scene scene = SceneManager.GetSceneAt(si);
            if (!scene.isLoaded) continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                // 找所有预制体实例根节点
                foreach (GameObject go in GetAllChildren(root, includeSelf: true))
                {
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(go)) continue;

                    // 判断是否是我们关心的预制体
                    GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(go);
                    if (prefabAsset == null) continue;
                    string prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
                    bool isTarget = false;
                    foreach (string n in prefabNames)
                        if (prefabPath.Contains(n)) { isTarget = true; break; }
                    if (!isTarget) continue;

                    // 移除所有 m_Materials 相关的 PropertyModification
                    // 这样实例就会回退到使用 Prefab 上已经修好的材质
                    var mods = PrefabUtility.GetPropertyModifications(go);
                    if (mods == null) continue;

                    var cleanMods = new List<PropertyModification>();
                    foreach (var mod in mods)
                    {
                        // 保留非材质的覆盖（位置、旋转、名称等）
                        if (mod.propertyPath.Contains("m_Materials"))
                        {
                            sceneTotal++;
                            continue; // 丢弃旧材质覆盖
                        }
                        cleanMods.Add(mod);
                    }
                    PrefabUtility.SetPropertyModifications(go, cleanMods.ToArray());
                    EditorUtility.SetDirty(go);
                    Debug.Log($"[FixURP] ✓ Scene instance: {go.name}");
                }
            }

            // 标记场景已修改，保存
            EditorSceneManager.MarkSceneDirty(scene);
        }

        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string msg = $"✅ 修复完成！\n• Prefab MeshRenderer 更新：{prefabTotal} 个\n• 场景实例材质覆盖清除：{sceneTotal} 条\n材质资产保存在：{MAT_DIR}";
        Debug.Log("[FixURP] " + msg);
        EditorUtility.DisplayDialog("Physics Lab - URP材质修复", msg, "确定");
    }

    // ── 递归获取所有子物体（含自身） ─────────────────────────────────────
    static IEnumerable<GameObject> GetAllChildren(GameObject go, bool includeSelf)
    {
        if (includeSelf) yield return go;
        foreach (Transform child in go.transform)
            foreach (GameObject c in GetAllChildren(child.gameObject, includeSelf: true))
                yield return c;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  核心匹配逻辑
    // ─────────────────────────────────────────────────────────────────────
    static Material Resolve(GameObject go)
    {
        string n = go.name;   // 保留原始大小写用于精确比较
        string nl = n.ToLower();

        // ── 1. 接线柱 孔 ────────────────────────────────────────────────
        if (nl == "hole")
            return Mat("Hole", C_HOLE, 0.1f, 0f);

        // ── 2. 接线柱子节点 Base / Post / Cap ───────────────────────────
        //    颜色由祖先节点名决定
        if (nl == "base" || nl == "post" || nl == "cap")
        {
            Color c = TerminalColor(go.transform);
            string key = (c == C_RED) ? "RedTerminal" :
                         (c == C_BLACK) ? "BlackTerminal" : "GreenPlastic";
            return Mat(key, c, 0.55f, 0.3f);
        }

        // ── 3. 电阻体底座（大盒）& 圆角端盖 ────────────────────────────
        //    名字含 base（但上面 base 已经处理过接线柱子节点的 Base，
        //    这里处理不在 Terminal 层级下的 Base / CapLeft / CapRight）
        //    —— 实际上已经被上面 case 2 处理，因为它们也叫 "Base"。
        //    所以对于底座大盒/端盖，名字不是 base：
        if (nl == "capleft" || nl == "capright")
            return Mat("GreenPlastic", C_GREEN, 0.35f, 0.1f);

        // ── 4. 色环 ─────────────────────────────────────────────────────
        if (n == "Band_0") return Mat("Band_Brown",  C_BAND_BROWN,  0.25f, 0.05f);
        if (n == "Band_1") return Mat("Band_Black",  C_BAND_BLACK,  0.25f, 0.05f);
        if (n == "Band_2") return Mat("Band_Orange", C_BAND_ORANGE, 0.25f, 0.05f);

        // ── 5. 电阻体 ───────────────────────────────────────────────────
        if (nl == "resistorelement")
            return Mat("ResistorBody", C_RESISTOR, 0.3f, 0.1f);

        // ── 6. 线圈体 & 线圈环 ──────────────────────────────────────────
        if (nl == "coilbody")
            return Mat("Coil", C_COIL, 0.85f, 0.8f);
        if (nl.StartsWith("ring_"))
            return Mat("Ring", C_RING, 0.9f, 0.85f);

        // ── 7. 导线 / 引脚 ──────────────────────────────────────────────
        if (nl.StartsWith("lead") || nl == "contactpin")
            return Mat("Metal", C_METAL, 0.92f, 0.88f);

        // ── 8. 滑块 ─────────────────────────────────────────────────────
        if (nl == "sliderbody" || nl == "slidertopbar" || nl == "sliderbottom")
            return Mat("SliderGreen", C_GREEN_SLIDER, 0.45f, 0.15f);

        // ── 9. 金属件 ───────────────────────────────────────────────────
        if (nl == "rail" || nl == "clipbase" || nl.StartsWith("clipwing")
            || nl.StartsWith("clipbend") || nl == "springcontact"
            || nl == "leverarm")
            return Mat("Metal", C_METAL, 0.92f, 0.88f);

        // ── 10. 转轴 ────────────────────────────────────────────────────
        if (nl == "pivot")
            return Mat("DarkMetal", C_METAL_DARK, 0.7f, 0.5f);

        // ── 11. 拨杆旋钮 / 标志 ─────────────────────────────────────────
        if (nl == "leverknob" || nl == "trianglemark")
            return Mat("KnobGreen", C_GREEN_KNOB, 0.4f, 0.1f);

        // ── 12. 支架竖柱 / 横梁 ─────────────────────────────────────────
        if (nl == "upright" || nl == "railclamp")
            return Mat("GreenPlastic", C_GREEN, 0.35f, 0.1f);

        // ── 13. 接线柱父节点（无 MeshRenderer，不会到这里，保险起见） ──
        if (nl.Contains("terminal"))
            return null;

        // ── 14. 兜底：绿色底座、端盖等 ──────────────────────────────────
        //    这里会覆盖 Base（大底座）、CapLeft/CapRight 等剩余命名
        return Mat("GreenPlastic", C_GREEN, 0.35f, 0.1f);
    }

    /// <summary>向上遍历找到含 terminal 字样的节点，判断正负极</summary>
    static Color TerminalColor(Transform child)
    {
        Transform t = child.parent;
        while (t != null)
        {
            string pn = t.gameObject.name.ToLower();
            if (pn.Contains("terminal"))
            {
                // neg / _b → 黑色
                if (pn.Contains("neg") || pn.EndsWith("_b"))
                    return C_BLACK;
                // pos / _a → 红色
                if (pn.Contains("pos") || pn.EndsWith("_a"))
                    return C_RED;
                // 兜底红色（定值电阻 terminal_pos）
                return C_RED;
            }
            t = t.parent;
        }
        // 不在接线柱层级下（例如真正的底座 Base），用绿色
        return C_GREEN;
    }

    // ─────────────────────────────────────────────────────────────────────
    //  材质创建/复用
    // ─────────────────────────────────────────────────────────────────────
    static Material Mat(string id, Color color, float smooth, float metallic)
    {
        if (s_cache.TryGetValue(id, out Material cached)) return cached;

        string path = $"{MAT_DIR}/{id}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find(URP_LIT)) { name = id };
            AssetDatabase.CreateAsset(mat, path);
        }

        // URP Lit 属性
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smooth);
        mat.SetFloat("_Metallic", metallic);
        // 兼容旧版访问方式
        mat.color = color;

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
