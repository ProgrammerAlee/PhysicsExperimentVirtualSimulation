using UnityEngine;
using UnityEditor;

/// <summary>
/// 生成电池开关 3D 模型
/// 外观参考：绿色矩形底座，底座上有弹片夹（金属），
///           金属拨杆（可打开/关闭），底部左红右黑两个接线柱
/// </summary>
public class GenerateBatterySwitch
{
    [MenuItem("Physics Lab/Generate Battery Switch (电池开关)")]
    public static void Execute()
    {
        GameObject root = new GameObject("BatterySwitch_电池开关");

        // ---- 材质 ----
        Material greenMat = CreateMat("GreenBase",      new Color(0.42f, 0.80f, 0.13f), 0.35f, 0.1f);
        Material metalMat = CreateMat("Metal",          new Color(0.80f, 0.82f, 0.84f), 0.92f, 0.88f);
        Material blackMat = CreateMat("BlackTerminal",  new Color(0.08f, 0.08f, 0.08f), 0.55f, 0.3f);
        Material redMat   = CreateMat("RedTerminal",    new Color(0.85f, 0.15f, 0.10f), 0.55f, 0.3f);
        Material pivotMat = CreateMat("DarkMetal",      new Color(0.25f, 0.25f, 0.25f), 0.7f, 0.5f);
        Material knobMat  = CreateMat("KnobGreen",      new Color(0.30f, 0.70f, 0.10f), 0.4f, 0.1f);

        // ============ 底座（绿色矩形，带轻微圆角） ============
        CreateBox("Base", root.transform,
            new Vector3(0f, 0.012f, 0f), Quaternion.identity,
            new Vector3(0.130f, 0.024f, 0.090f), greenMat);

        // 底座圆角端盖
        CreateSphere("CapL", root.transform,
            new Vector3(-0.055f, 0.012f, 0f), Quaternion.identity,
            new Vector3(0.028f, 0.024f, 0.090f), greenMat);
        CreateSphere("CapR", root.transform,
            new Vector3(0.055f, 0.012f, 0f), Quaternion.identity,
            new Vector3(0.028f, 0.024f, 0.090f), greenMat);

        // ============ 弹片夹底托（中央金属矩形板） ============
        CreateBox("ClipBase", root.transform,
            new Vector3(0.018f, 0.026f, 0f), Quaternion.identity,
            new Vector3(0.060f, 0.006f, 0.055f), metalMat);

        // 弹片夹左翼
        CreateBox("ClipWingL", root.transform,
            new Vector3(-0.013f, 0.026f, 0f), Quaternion.identity,
            new Vector3(0.020f, 0.005f, 0.040f), metalMat);

        // 弹片夹右翼
        CreateBox("ClipWingR", root.transform,
            new Vector3(0.048f, 0.026f, 0f), Quaternion.identity,
            new Vector3(0.020f, 0.005f, 0.040f), metalMat);

        // 弹片夹弯曲部（用倾斜方块）
        CreateBox("ClipBendL", root.transform,
            new Vector3(-0.022f, 0.034f, 0f),
            Quaternion.Euler(0, 0, 25f),
            new Vector3(0.008f, 0.028f, 0.036f), metalMat);
        CreateBox("ClipBendR", root.transform,
            new Vector3(0.058f, 0.034f, 0f),
            Quaternion.Euler(0, 0, -25f),
            new Vector3(0.008f, 0.028f, 0.036f), metalMat);

        // ============ 转轴（小圆柱） ============
        CreateCylinder("Pivot", root.transform,
            new Vector3(-0.030f, 0.028f, 0f),
            Quaternion.Euler(0, 0, 90),
            new Vector3(0.010f, 0.055f, 0.010f), pivotMat);

        // ============ 拨杆（打开状态：斜向上） ============
        // 杆身
        GameObject lever = new GameObject("Lever");
        lever.transform.SetParent(root.transform);
        lever.transform.localPosition = new Vector3(-0.030f, 0.028f, 0f);
        lever.transform.localRotation = Quaternion.Euler(0, 0, -50f); // 打开状态约 50°

        CreateBox("LeverArm", lever.transform,
            new Vector3(0f, 0.060f, 0f), Quaternion.identity,
            new Vector3(0.010f, 0.110f, 0.010f), metalMat);

        // 杆顶端绿色旋钮手柄
        CreateSphere("LeverKnob", lever.transform,
            new Vector3(0f, 0.118f, 0f), Quaternion.identity,
            new Vector3(0.024f, 0.024f, 0.024f), knobMat);

        // ============ 底部弹片（连接到下接线柱的金属弹片） ============
        CreateBox("SpringContact", root.transform,
            new Vector3(0.020f, 0.024f, -0.008f),
            Quaternion.Euler(-10f, 0, 0),
            new Vector3(0.018f, 0.004f, 0.040f), metalMat);

        // ============ 接线柱 ============
        // 左侧红色
        CreateTerminalPost(root.transform, "Terminal_Pos", redMat, metalMat,
            new Vector3(-0.038f, 0.024f, 0.022f));

        // 右侧黑色
        CreateTerminalPost(root.transform, "Terminal_Neg", blackMat, metalMat,
            new Vector3(0.038f, 0.024f, 0.022f));

        // ============ 标志三角形（底座上的绿色三角形标志） ============
        // 用小盒子近似
        CreateBox("TriangleMark", root.transform,
            new Vector3(0.038f, 0.025f, -0.018f),
            Quaternion.Euler(0, 0, 0),
            new Vector3(0.014f, 0.002f, 0.016f), knobMat);

        // ---- 放置 ----
        PlaceInFrontOfCamera(root.transform);
        Selection.activeGameObject = root;
        Debug.Log("[Physics Lab] 电池开关 (BatterySwitch) 创建完成！");
    }

    // ======================== 接线柱 ========================
    static void CreateTerminalPost(Transform parent, string name, Material color,
        Material metalMat, Vector3 pos)
    {
        GameObject terminal = new GameObject(name);
        terminal.transform.SetParent(parent);
        terminal.transform.localPosition = pos;
        terminal.transform.localRotation = Quaternion.identity;

        CreateCylinder("Base", terminal.transform,
            new Vector3(0, 0.010f, 0), Quaternion.identity,
            new Vector3(0.026f, 0.010f, 0.026f), color);
        CreateCylinder("Post", terminal.transform,
            new Vector3(0, 0.028f, 0), Quaternion.identity,
            new Vector3(0.016f, 0.016f, 0.016f), color);
        CreateSphere("Cap", terminal.transform,
            new Vector3(0, 0.043f, 0), Quaternion.identity,
            new Vector3(0.020f, 0.010f, 0.020f), color);

        Material holeMat = CreateMat("Hole_" + name, new Color(0.02f, 0.02f, 0.02f), 0.1f, 0f);
        CreateCylinder("Hole", terminal.transform,
            new Vector3(0, 0.048f, 0), Quaternion.identity,
            new Vector3(0.007f, 0.007f, 0.007f), holeMat);
    }

    // ======================== 基础图元 ========================
    static Material CreateMat(string name, Color color, float smooth, float metallic)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;
        mat.SetFloat("_Glossiness", smooth);
        mat.SetFloat("_Metallic", metallic);
        return mat;
    }

    static GameObject CreateBox(string name, Transform parent, Vector3 lp, Quaternion lr, Vector3 ls, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = lp;
        go.transform.localRotation = lr;
        go.transform.localScale    = ls;
        go.GetComponent<MeshRenderer>().material = mat;
        return go;
    }

    static GameObject CreateCylinder(string name, Transform parent, Vector3 lp, Quaternion lr, Vector3 ls, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = lp;
        go.transform.localRotation = lr;
        go.transform.localScale    = ls;
        go.GetComponent<MeshRenderer>().material = mat;
        return go;
    }

    static GameObject CreateSphere(string name, Transform parent, Vector3 lp, Quaternion lr, Vector3 ls, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = lp;
        go.transform.localRotation = lr;
        go.transform.localScale    = ls;
        go.GetComponent<MeshRenderer>().material = mat;
        return go;
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
            t.position = new Vector3(-0.5f, 1.2f, 0);
        }
    }
}
