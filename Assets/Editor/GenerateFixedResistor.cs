using UnityEngine;
using UnityEditor;

/// <summary>
/// 生成定值电阻 3D 模型
/// 外观参考：绿色长方形底座，左侧黑色接线柱，右侧红色接线柱，中间有矩形电阻体
/// </summary>
public class GenerateFixedResistor
{
    [MenuItem("Physics Lab/Generate Fixed Resistor (定值电阻)")]
    public static void Execute()
    {
        GameObject root = new GameObject("FixedResistor_定值电阻");

        // ---- 材质 ----
        Material greenMat = CreateMat("GreenPlastic", new Color(0.45f, 0.82f, 0.15f), 0.4f, 0.2f);
        Material blackMat = CreateMat("BlackTerminal", new Color(0.08f, 0.08f, 0.08f), 0.6f, 0.4f);
        Material redMat   = CreateMat("RedTerminal",   new Color(0.85f, 0.15f, 0.10f), 0.6f, 0.4f);
        Material resistorBodyMat = CreateMat("ResistorBody", new Color(0.92f, 0.90f, 0.82f), 0.3f, 0.1f);
        Material resistorBandMat = CreateMat("ResistorBand", new Color(0.20f, 0.15f, 0.05f), 0.3f, 0.1f);
        Material metalMat  = CreateMat("Metal", new Color(0.75f, 0.75f, 0.75f), 0.9f, 0.7f);

        // ---- 底座（绿色扁平长方体，圆角通过缩放近似） ----
        // 尺寸参考图片：宽约 0.30m，高约 0.04m，深约 0.09m
        GameObject base1 = CreateBox("Base", root.transform,
            new Vector3(0f, 0.02f, 0f),
            Quaternion.identity,
            new Vector3(0.30f, 0.04f, 0.09f),
            greenMat);

        // 底部圆角模拟 —— 两端半球
        GameObject capL = CreateSphere("CapLeft", root.transform,
            new Vector3(-0.135f, 0.02f, 0f), Quaternion.identity,
            new Vector3(0.04f, 0.04f, 0.09f), greenMat);
        GameObject capR = CreateSphere("CapRight", root.transform,
            new Vector3(0.135f, 0.02f, 0f), Quaternion.identity,
            new Vector3(0.04f, 0.04f, 0.09f), greenMat);

        // ---- 左侧接线柱（黑色）----
        CreateTerminalPost(root.transform, "Terminal_Neg", blackMat, metalMat,
            new Vector3(-0.10f, 0.04f, 0f));

        // ---- 右侧接线柱（红色）----
        CreateTerminalPost(root.transform, "Terminal_Pos", redMat, metalMat,
            new Vector3(0.10f, 0.04f, 0f));

        // ---- 中央电阻元件（小矩形陶瓷体） ----
        // 外壳
        GameObject resBody = CreateBox("ResistorElement", root.transform,
            new Vector3(0f, 0.057f, 0f),
            Quaternion.identity,
            new Vector3(0.055f, 0.025f, 0.030f),
            resistorBodyMat);

        // 色环（3 条）
        float[] bandX = { -0.014f, 0f, 0.014f };
        Color[] bandColors = {
            new Color(0.60f, 0.30f, 0.00f), // 棕
            new Color(0.10f, 0.10f, 0.10f), // 黑
            new Color(1.00f, 0.60f, 0.00f), // 橙
        };
        for (int i = 0; i < 3; i++)
        {
            Material bm = CreateMat("Band" + i, bandColors[i], 0.2f, 0.05f);
            CreateBox("Band_" + i, resBody.transform,
                new Vector3(bandX[i] / 0.055f, 0f, 0f), // localPos in parent space
                Quaternion.identity,
                new Vector3(0.10f, 1.04f, 1.04f),        // local scale relative to parent
                bm);
        }

        // 两侧导线（金属细棒连接到接线柱）
        CreateBox("LeadLeft", root.transform,
            new Vector3(-0.065f, 0.057f, 0f),
            Quaternion.identity,
            new Vector3(0.035f, 0.003f, 0.003f),
            metalMat);
        CreateBox("LeadRight", root.transform,
            new Vector3(0.065f, 0.057f, 0f),
            Quaternion.identity,
            new Vector3(0.035f, 0.003f, 0.003f),
            metalMat);

        // ---- 放置到摄像机前 ----
        PlaceInFrontOfCamera(root.transform);

        Selection.activeGameObject = root;
        Debug.Log("[Physics Lab] 定值电阻 (FixedResistor) 创建完成！");
    }

    // ======================== 辅助方法 ========================

    static Material CreateMat(string name, Color color, float smoothness, float metallic)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;
        mat.SetFloat("_Glossiness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        return mat;
    }

    static GameObject CreateBox(string name, Transform parent, Vector3 localPos,
        Quaternion localRot, Vector3 localScale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale    = localScale;
        go.GetComponent<MeshRenderer>().material = mat;
        return go;
    }

    static GameObject CreateSphere(string name, Transform parent, Vector3 localPos,
        Quaternion localRot, Vector3 localScale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale    = localScale;
        go.GetComponent<MeshRenderer>().material = mat;
        return go;
    }

    static GameObject CreateCylinder(string name, Transform parent, Vector3 localPos,
        Quaternion localRot, Vector3 localScale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.transform.localScale    = localScale;
        go.GetComponent<MeshRenderer>().material = mat;
        return go;
    }

    /// <summary>
    /// 创建接线柱：底座圆柱 + 螺纹柱 + 顶部圆盘
    /// </summary>
    static void CreateTerminalPost(Transform parent, string name, Material color, Material metalMat, Vector3 pos)
    {
        GameObject terminal = new GameObject(name);
        terminal.transform.SetParent(parent);
        terminal.transform.localPosition = pos;
        terminal.transform.localRotation = Quaternion.identity;

        // 底座圆台（较宽的圆柱）
        CreateCylinder("Base", terminal.transform,
            new Vector3(0, 0.013f, 0), Quaternion.identity,
            new Vector3(0.032f, 0.013f, 0.032f), color);

        // 螺纹柱（较细圆柱）
        CreateCylinder("Post", terminal.transform,
            new Vector3(0, 0.038f, 0), Quaternion.identity,
            new Vector3(0.018f, 0.020f, 0.018f), color);

        // 顶部帽
        CreateSphere("Cap", terminal.transform,
            new Vector3(0, 0.058f, 0), Quaternion.identity,
            new Vector3(0.022f, 0.012f, 0.022f), color);

        // 中心孔（小黑圆柱）
        Material holeMat = CreateMat("Hole", new Color(0.02f, 0.02f, 0.02f), 0.1f, 0f);
        CreateCylinder("Hole", terminal.transform,
            new Vector3(0, 0.063f, 0), Quaternion.identity,
            new Vector3(0.008f, 0.008f, 0.008f), holeMat);
    }

    static void PlaceInFrontOfCamera(Transform t)
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            t.position = cam.transform.position
                + cam.transform.forward * 0.6f
                + cam.transform.up * -0.1f;
            t.rotation = Quaternion.LookRotation(cam.transform.forward);
        }
        else
        {
            t.position = new Vector3(0, 1.2f, 0);
        }
    }
}
