using UnityEngine;
using UnityEditor;
using System.IO;

public class GenerateFloorMaterial
{
    public static void Execute()
    {
        string albedoPath = "Assets/Textures/WoodFloor_Albedo.png";
        string normalPath = "Assets/Textures/WoodFloor_Normal.png";
        string roughnessPath = "Assets/Textures/WoodFloor_Roughness.png";
        string materialPath = "Assets/Materials/WoodFloorMaterial.mat";

        // Ensure directories exist
        if (!Directory.Exists("Assets/Materials"))
        {
            Directory.CreateDirectory("Assets/Materials");
        }

        // Load Albedo Texture
        Texture2D albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);
        if (albedoTex == null)
        {
            Debug.LogError("Albedo texture not found at " + albedoPath);
            return;
        }

        // Make Albedo readable
        MakeTextureReadable(albedoPath);
        albedoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(albedoPath);

        int width = albedoTex.width;
        int height = albedoTex.height;

        // Generate Normal and Roughness Maps
        Texture2D normalTex = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
        Texture2D roughnessTex = new Texture2D(width, height, TextureFormat.RGBA32, true, true);

        Color[] albedoPixels = albedoTex.GetPixels();
        Color[] normalPixels = new Color[albedoPixels.Length];
        Color[] roughnessPixels = new Color[albedoPixels.Length];

        float normalStrength = 3.0f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                Color c = albedoPixels[idx];
                float grayscale = c.grayscale;

                // Roughness: Darker areas (cracks) are rougher (closer to 1), lighter areas are smoother (closer to 0.3)
                float roughness = Mathf.Lerp(0.8f, 0.2f, grayscale);
                roughnessPixels[idx] = new Color(roughness, roughness, roughness, 1f);

                // Normal Map (Sobel filter)
                float xLeft = albedoPixels[y * width + Mathf.Max(0, x - 1)].grayscale;
                float xRight = albedoPixels[y * width + Mathf.Min(width - 1, x + 1)].grayscale;
                float yUp = albedoPixels[Mathf.Min(height - 1, y + 1) * width + x].grayscale;
                float yDown = albedoPixels[Mathf.Max(0, y - 1) * width + x].grayscale;

                float dX = (xRight - xLeft) * normalStrength;
                float dY = (yUp - yDown) * normalStrength;
                float dZ = 1.0f;

                Vector3 normal = new Vector3(-dX, -dY, dZ).normalized;
                
                // Convert normal from [-1, 1] to [0, 1]
                normalPixels[idx] = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f, 1f);
            }
        }

        normalTex.SetPixels(normalPixels);
        normalTex.Apply();
        File.WriteAllBytes(normalPath, normalTex.EncodeToPNG());

        roughnessTex.SetPixels(roughnessPixels);
        roughnessTex.Apply();
        File.WriteAllBytes(roughnessPath, roughnessTex.EncodeToPNG());

        AssetDatabase.Refresh();

        // Set Normal Map texture type
        TextureImporter normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
        if (normalImporter != null)
        {
            normalImporter.textureType = TextureImporterType.NormalMap;
            normalImporter.SaveAndReimport();
        }

        // Load generated textures
        Texture2D loadedNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        Texture2D loadedRoughness = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);

        // Create Material
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetTexture("_MainTex", albedoTex);
        mat.SetTexture("_BumpMap", loadedNormal);
        mat.EnableKeyword("_NORMALMAP");
        
        // Standard shader uses Metallic/Smoothness. We have Roughness.
        // Smoothness = 1 - Roughness. We can use the Albedo alpha or Metallic alpha for smoothness.
        // Let's just set a base smoothness and use the roughness map in the MetallicGlossMap (alpha channel).
        // Actually, Standard shader expects Smoothness in the Alpha of Metallic map.
        Texture2D metallicSmoothnessTex = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
        Color[] msPixels = new Color[albedoPixels.Length];
        for (int i = 0; i < albedoPixels.Length; i++)
        {
            float r = roughnessPixels[i].r;
            float smoothness = 1.0f - r;
            msPixels[i] = new Color(0f, 0f, 0f, smoothness); // 0 metallic, smoothness in alpha
        }
        metallicSmoothnessTex.SetPixels(msPixels);
        metallicSmoothnessTex.Apply();
        string msPath = "Assets/Textures/WoodFloor_MetallicSmoothness.png";
        File.WriteAllBytes(msPath, metallicSmoothnessTex.EncodeToPNG());
        AssetDatabase.Refresh();
        
        Texture2D loadedMS = AssetDatabase.LoadAssetAtPath<Texture2D>(msPath);
        mat.SetTexture("_MetallicGlossMap", loadedMS);
        mat.EnableKeyword("_METALLICGLOSSMAP");
        
        // Tiling
        mat.mainTextureScale = new Vector2(5, 5);

        AssetDatabase.CreateAsset(mat, materialPath);
        AssetDatabase.SaveAssets();

        // Assign to Floor
        GameObject floor = GameObject.Find("Enviro/chemical_synthesis_laboratory/r1.wrl.cleaner.materialmerger.gles/__Room_1/__Room_1_Floor/Object_2923");
        if (floor != null)
        {
            MeshRenderer mr = floor.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = mat;
                Debug.Log("Material assigned successfully.");
            }
            else
            {
                Debug.LogError("MeshRenderer not found on floor object.");
            }
        }
        else
        {
            Debug.LogError("Floor object not found.");
        }
    }

    private static void MakeTextureReadable(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
    }
}
