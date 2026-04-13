using UnityEngine;
using UnityEditor;

/// <summary>
/// 物理实验室一键生成所有器件
/// </summary>
public class GenerateAllPhysicsComponents
{
    [MenuItem("Physics Lab/Generate ALL Components (生成全部器件)", false, 0)]
    public static void GenerateAll()
    {
        // 清除已有同名物体
        string[] names = { "FixedResistor_定值电阻", "SlidingRheostat_滑动变阻器", "BatterySwitch_电池开关" };
        foreach (string n in names)
        {
            GameObject existing = GameObject.Find(n);
            if (existing != null)
                GameObject.DestroyImmediate(existing);
        }

        GenerateFixedResistor.Execute();
        GenerateSlidingRheostat.Execute();
        GenerateBatterySwitch.Execute();

        // 整理空间排布：水平排开
        RepositionObjects(names, spacing: 0.55f);

        Debug.Log("[Physics Lab] 全部 3 个器件生成完毕！");
        EditorUtility.DisplayDialog("Physics Lab",
            "✅ 所有器件已生成：\n• 定值电阻\n• 滑动变阻器\n• 电池开关\n\n请查看 Hierarchy 面板。", "确定");
    }

    [MenuItem("Physics Lab/Clear ALL Components (清除全部器件)", false, 1)]
    public static void ClearAll()
    {
        string[] names = { "FixedResistor_定值电阻", "SlidingRheostat_滑动变阻器", "BatterySwitch_电池开关" };
        int count = 0;
        foreach (string n in names)
        {
            GameObject existing = GameObject.Find(n);
            if (existing != null)
            {
                GameObject.DestroyImmediate(existing);
                count++;
            }
        }
        Debug.Log($"[Physics Lab] 已清除 {count} 个器件。");
    }

    private static void RepositionObjects(string[] names, float spacing)
    {
        // 以第一个对象为基准，水平排开
        float startX = -(names.Length - 1) * spacing * 0.5f;
        for (int i = 0; i < names.Length; i++)
        {
            GameObject go = GameObject.Find(names[i]);
            if (go != null)
            {
                Vector3 pos = go.transform.position;
                go.transform.position = new Vector3(startX + i * spacing, pos.y, pos.z);
            }
        }
    }
}
