using UnityEngine;
using UnityEditor;

/// <summary>
/// 生成滑动变阻器 3D 模型
/// 外观参考：左右两个绿色扇形支架，中央金属导轨，导轨上缠绕线圈，
///           绿色滑块（H形）可在导轨上滑动，两端底部各有黑红两个接线柱
/// </summary>
public class GenerateSlidingRheostat
{
    [MenuItem("Physics Lab/Generate Sliding Rheostat (滑动变阻器)")]
    public static void Execute()
    {
        GameObject root = new GameObject("SlidingRheostat_滑动变阻器");

        // ---- 材质 ----
        Material greenMat   = CreateMat("GreenPlastic",  new Color(0.45f, 0.82f, 0.15f), 0.35f, 0.1f);
        Material blackMat   = CreateMat("BlackTerminal", new Color(0.08f, 0.08f, 0.08f), 0.55f, 0.3f);
        Material redMat     = CreateMat("RedTerminal",   new Color(0.85f, 0.15f, 0.10f), 0.55f, 0.3f);
        Material metalMat   = CreateMat("Metal",         new Color(0.80f, 0.80f, 0.82f), 0.95f, 0.9f);
        Material coilMat    = CreateMat("Coil",          new Color(0.72f, 0.72f, 0.72f), 0.85f, 0.8f);
        Material sliderMat  = CreateMat("SliderGreen",   new Color(0.40f, 0.78f, 0.12f), 0.45f, 0.15f);

        float halfLen = 0.22f; // 变阻器半长

        // ============ 左支架 ============
        CreateBracket(root.transform, "Bracket_Left", greenMat, metalMat, blackMat, redMat,
            new Vector3(-halfLen, 0f, 0f), true);

        // ============ 右支架 ============
        CreateBracket(root.transform, "Bracket_Right", greenMat, metalMat, redMat, blackMat,
            new Vector3(halfLen, 0f, 0f), false);

        // ============ 金属导轨（上方细圆杆）============
        CreateCylinder("Rail", root.transform,
            new Vector3(0f, 0.065f, 0f),
            Quaternion.Euler(0, 0, 90),
            new Vector3(0.008f, halfLen, 0.008f),
            metalMat);

        // ============ 电阻线圈体（沿导轨分布的螺旋线圈近似：密集圆环） ============
        // 用细长圆柱近似螺旋管
        GameObject coilBody = CreateCylinder("CoilBody", root.transform,
            new Vector3(0f, 0.038f, 0f),
            Quaternion.Euler(0, 0, 90),
            new Vector3(0.030f, halfLen * 0.88f, 0.030f),
            coilMat);

        // 叠加螺旋纹（用等间距薄圆柱模拟）
        int coilCount = 28;
        for (int i = 0; i < coilCount; i++)
        {
            float t = (float)i / (coilCount - 1);
            float x = Mathf.Lerp(-halfLen * 0.85f, halfLen * 0.85f, t);
            Material cm = CreateMat("Ring" + i, new Color(0.55f, 0.55f, 0.55f), 0.9f, 0.85f);
            CreateCylinder("Ring_" + i, root.transform,
                new Vector3(x, 0.038f, 0f),
                Quaternion.Euler(0, 0, 90),
                new Vector3(0.033f, 0.003f, 0.033f),
                cm);
        }

        // ============ 滑块（绿色 H 形） ============
        GameObject slider = new GameObject("Slider");
        slider.transform.SetParent(root.transform);
        slider.transform.localPosition = new Vector3(0f, 0f, 0f);
        slider.transform.localRotation = Quaternion.identity;

        // 滑块主体
        CreateBox("SliderBody", slider.transform,
            new Vector3(0f, 0.055f, 0f), Quaternion.identity,
            new Vector3(0.060f, 0.055f, 0.055f), sliderMat);

        // 顶部横梁
        CreateBox("SliderTopBar", slider.transform,
            new Vector3(0f, 0.090f, 0f), Quaternion.identity,
            new Vector3(0.075f, 0.018f, 0.028f), sliderMat);

        // 底部滑槽
        CreateBox("SliderBottom", slider.transform,
            new Vector3(0f, 0.020f, 0f), Quaternion.identity,
            new Vector3(0.050f, 0.012f, 0.025f), sliderMat);

        // 导电触点（金属小块）
        CreateBox("ContactPin", slider.transform,
            new Vector3(0f, 0.038f, -0.016f), Quaternion.identity,
            new Vector3(0.008f, 0.008f, 0.008f), metalMat);

        // ---- 放置 ----
        PlaceInFrontOfCamera(root.transform);
        Selection.activeGameObject = root;
        Debug.Log("[Physics Lab] 滑动变阻器 (SlidingRheostat) 创建完成！");
    }

    // ======================== 支架 ========================
    /// <summary>
    /// 创建一侧支架组件：扇形底座 + 竖柱 + 接线柱
    /// </summary>
    static void CreateBracket(Transform parent, string name, Material greenMat,
        Material metalMat, Material termColorA, Material termColorB, Vector3 localPos, bool isLeft)
    {
        GameObject bracket = new GameObject(name);
        bracket.transform.SetParent(parent);
        bracket.transform.localPosition = localPos;
        bracket.transform.localRotation = Quaternion.identity;

        // 扇形底座（用扁平圆柱近似）
        CreateCylinder("Base", bracket.transform,
            new Vector3(0f, 0.010f, 0f), Quaternion.identity,
            new Vector3(0.065f, 0.010f, 0.060f), greenMat);

        // 竖立支柱（绿色厚板）
        CreateBox("Upright", bracket.transform,
            new Vector3(0f, 0.055f, 0.005f), Quaternion.identity,
            new Vector3(0.012f, 0.090f, 0.050f), greenMat);

        // 横向顶端圆柱（夹持导轨）
        CreateCylinder("RailClamp", bracket.transform,
            new Vector3(0f, 0.065f, 0.005f),
            Quaternion.Euler(0, 0, 90),
            new Vector3(0.014f, 0.040f, 0.014f), greenMat);

        // 底部接线柱 A（前）
        float sign = isLeft ? 1f : -1f;
        CreateTerminalPost(bracket.transform, "Terminal_A", termColorA, metalMat,
            new Vector3(sign * 0.008f, 0.020f, -0.016f));

        // 底部接线柱 B（后）
        CreateTerminalPost(bracket.transform, "Terminal_B", termColorB, metalMat,
            new Vector3(-sign * 0.008f, 0.020f, 0.014f));
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
            new Vector3(0, 0.030f, 0), Quaternion.identity,
            new Vector3(0.016f, 0.018f, 0.016f), color);
        CreateSphere("Cap", terminal.transform,
            new Vector3(0, 0.047f, 0), Quaternion.identity,
            new Vector3(0.020f, 0.010f, 0.020f), color);

        Material holeMat = CreateMat("Hole_" + name, new Color(0.02f, 0.02f, 0.02f), 0.1f, 0f);
        CreateCylinder("Hole", terminal.transform,
            new Vector3(0, 0.052f, 0), Quaternion.identity,
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
                + cam.transform.forward * 0.8f
                + cam.transform.up * -0.1f;
            t.rotation = Quaternion.LookRotation(cam.transform.forward);
        }
        else
        {
            t.position = new Vector3(0.5f, 1.2f, 0);
        }
    }
}
