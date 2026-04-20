using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using GridSystem;
using System.IO;

public class GridSystemExampleGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Grid System Examples")]
    public static void GenerateExamples()
    {
        // 1. Create folders
        CreateFolderIfNeeded("Assets", "Prefabs");
        CreateFolderIfNeeded("Assets/Resources", "GridTools");

        // 2. Create WirePrefab
        string wirePath = "Assets/Prefabs/WirePrefab.prefab";
        if (!File.Exists(wirePath))
        {
            GameObject wireObj = new GameObject("WirePrefab");
            Image wireImage = wireObj.AddComponent<Image>();
            wireImage.color = Color.black;

            RectTransform rt = wireObj.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(100, 5); // Default size

            PrefabUtility.SaveAsPrefabAsset(wireObj, wirePath);
            DestroyImmediate(wireObj);
            Debug.Log("Created WirePrefab at " + wirePath);
        }

        // 3. Create ToolUIPrefab
        string toolPath = "Assets/Prefabs/ToolUIPrefab.prefab";
        if (!File.Exists(toolPath))
        {
            GameObject toolObj = new GameObject("ToolUIPrefab");
            Image toolImage = toolObj.AddComponent<Image>();
            toolImage.color = new Color(0.8f, 0.8f, 0.8f);

            RectTransform rt = toolObj.GetComponent<RectTransform>();
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(80, 80);

            toolObj.AddComponent<CanvasGroup>();
            toolObj.AddComponent<GridToolUI>();

            // Add text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(toolObj.transform, false);
            Text text = textObj.AddComponent<Text>();
            text.text = "Tool";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;

            RectTransform textRt = textObj.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textRt.anchoredPosition = Vector2.zero;

            PrefabUtility.SaveAsPrefabAsset(toolObj, toolPath);
            DestroyImmediate(toolObj);
            Debug.Log("Created ToolUIPrefab at " + toolPath);
        }

        // 4. Create GridToolData ScriptableObjects
        CreateGridToolData("RW1", 4);
        CreateGridToolData("RW2", 4);
        CreateGridToolData("RW3", 4);
        CreateGridToolData("电阻", 2);

        // 5. Assign to managers if present
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            GameObject toolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(toolPath);
            if (toolPrefab != null)
            {
                gridManager.toolUIPrefab = toolPrefab;
            }

            // Load all tool data
            GridToolData[] allTools = Resources.LoadAll<GridToolData>("GridTools");
            gridManager.availableTools = new System.Collections.Generic.List<GridToolData>(allTools);

            EditorUtility.SetDirty(gridManager);
            Debug.Log("Assigned tools and prefab to GridManager");
        }

        GridWireManager wireManager = FindObjectOfType<GridWireManager>();
        if (wireManager != null)
        {
            GameObject wirePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(wirePath);
            if (wirePrefab != null)
            {
                wireManager.wirePrefab = wirePrefab;
            }

            EditorUtility.SetDirty(wireManager);
            Debug.Log("Assigned wire prefab to GridWireManager");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Grid System Examples Generation Complete!");
    }

    private static void CreateFolderIfNeeded(string parentFolder, string newFolder)
    {
        string fullPath = parentFolder + "/" + newFolder;
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            // Ensure parent exists first
            if (parentFolder.Contains("/"))
            {
                string[] parts = parentFolder.Split('/');
                string current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = parts[i];
                    if (!AssetDatabase.IsValidFolder(current + "/" + next))
                    {
                        AssetDatabase.CreateFolder(current, next);
                    }
                    current += "/" + next;
                }
            }

            AssetDatabase.CreateFolder(parentFolder, newFolder);
        }
    }

    private static void CreateGridToolData(string name, int pinCount)
    {
        string path = $"Assets/Resources/GridTools/{name}.asset";
        if (!File.Exists(path))
        {
            GridToolData data = ScriptableObject.CreateInstance<GridToolData>();
            data.id = name;
            data.toolName = name;
            data.pinCount = pinCount;

            AssetDatabase.CreateAsset(data, path);
            Debug.Log($"Created GridToolData for {name} at {path}");
        }
    }
}