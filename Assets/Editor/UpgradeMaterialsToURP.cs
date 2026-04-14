using UnityEngine;
using UnityEditor;

public class UpgradeMaterialsToURP : EditorWindow
{
    [MenuItem("Tools/Upgrade All Materials To URP")]
    public static void UpgradeAll()
    {
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        
        if (urpShader == null)
        {
            Debug.LogError("Could not find URP Lit shader.");
            return;
        }

        int count = 0;
        foreach (string guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Skip materials in Packages/ or internal Unity stuff
            if (!path.StartsWith("Assets/")) continue;
            
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && mat.shader != null && mat.shader.name != urpShader.name)
            {
                // Only upgrade standard or hidden/error shaders
                if (mat.shader.name.Contains("Standard") || 
                    mat.shader.name.Contains("Error") || 
                    mat.shader.name.Contains("Hidden") || 
                    mat.shader.name.Contains("Internal"))
                {
                    mat.shader = urpShader;
                    EditorUtility.SetDirty(mat);
                    count++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Upgraded {count} materials to URP Lit shader.");
    }
}
