#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using GridSystem;

namespace GridSystem.EditorScripts
{
    public class ToolDataGenerator : EditorWindow
    {
        [MenuItem("Tools/Generate Specialized Tool Data")]
        public static void GenerateSpecializedTools()
        {
            // Ensure Resources/GridTools folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/GridTools"))
                AssetDatabase.CreateFolder("Assets/Resources", "GridTools");

            CreateResistorData();
            CreateICData();
            CreateRWData();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=cyan><b>Specialized Tool Data Generated!</b></color>");
        }

        private static void CreateResistorData()
        {
            GridToolData data = CreateOrGetToolData("Resistor_R");
            data.toolName = "电阻 (R)";
            data.pinDefinitions = new List<PinDefinition>
            {
                new PinDefinition { name = "A", normalizedPosition = new Vector2(-0.5f, 0f) }, // Left
                new PinDefinition { name = "B", normalizedPosition = new Vector2(0.5f, 0f) }   // Right
            };
            data.pinCount = data.pinDefinitions.Count;
            EditorUtility.SetDirty(data);
        }

        private static void CreateICData()
        {
            GridToolData data = CreateOrGetToolData("IC_Module");
            data.toolName = "集成电路 (IC)";
            data.pinDefinitions = new List<PinDefinition>
            {
                new PinDefinition { name = "A", normalizedPosition = new Vector2(-0.5f, 0.25f) },  // Left Top
                new PinDefinition { name = "B", normalizedPosition = new Vector2(0.5f, 0f) },       // Right Center
                new PinDefinition { name = "C", normalizedPosition = new Vector2(-0.5f, -0.25f) }  // Left Bottom
            };
            data.pinCount = data.pinDefinitions.Count;
            EditorUtility.SetDirty(data);
        }

        private static void CreateRWData()
        {
            GridToolData data = CreateOrGetToolData("Potentiometer_RW");
            data.toolName = "滑动变阻器 (RW)";
            data.pinDefinitions = new List<PinDefinition>
            {
                new PinDefinition { name = "A", normalizedPosition = new Vector2(-0.5f, 0f) }, // Left
                new PinDefinition { name = "B", normalizedPosition = new Vector2(0f, 0.5f) },  // Top
                new PinDefinition { name = "C", normalizedPosition = new Vector2(0.5f, 0f) }   // Right
            };
            data.pinCount = data.pinDefinitions.Count;
            EditorUtility.SetDirty(data);
        }

        private static GridToolData CreateOrGetToolData(string id)
        {
            string path = $"Assets/Resources/GridTools/{id}.asset";
            GridToolData data = AssetDatabase.LoadAssetAtPath<GridToolData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<GridToolData>();
                data.id = id;
                AssetDatabase.CreateAsset(data, path);
            }
            return data;
        }
    }
}
#endif
