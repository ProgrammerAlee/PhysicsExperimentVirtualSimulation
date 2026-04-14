using UnityEngine;
using UnityEditor;

public class CreateResistorModel : EditorWindow
{
    [MenuItem("Tools/Create Resistor Model")]
    public static void CreateModel()
    {
        // Create root object
        GameObject root = new GameObject("ResistorModel");

        // Create the flat piece (body)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Quad);
        body.name = "ResistorBody";
        body.transform.SetParent(root.transform);

        // Scale the body appropriately (the image is roughly rectangular)
        body.transform.localScale = new Vector3(1f, 1.5f, 1f);

        // Load the texture
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/片.jpeg");

        // Create a material for the body
        Material mat = new Material(Shader.Find("Standard"));
        if (tex != null)
        {
            mat.mainTexture = tex;
            // Optionally set color or other properties to make it look good
            mat.SetFloat("_Glossiness", 0.1f);
        }
        else
        {
            Debug.LogWarning("Texture 'Assets/片.jpeg' not found.");
        }

        // Apply material
        MeshRenderer renderer = body.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = mat;
        }

        // Save material to project so it persists
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        AssetDatabase.CreateAsset(mat, "Assets/Materials/ResistorBodyMaterial.mat");

        // Create Left Wire
        GameObject leftWire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftWire.name = "LeftWire";
        leftWire.transform.SetParent(root.transform);
        leftWire.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
        leftWire.transform.localRotation = Quaternion.Euler(0, 0, 90);
        leftWire.transform.localPosition = new Vector3(-1.0f, 0, 0);

        // Create Right Wire
        GameObject rightWire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightWire.name = "RightWire";
        rightWire.transform.SetParent(root.transform);
        rightWire.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
        rightWire.transform.localRotation = Quaternion.Euler(0, 0, 90);
        rightWire.transform.localPosition = new Vector3(1.0f, 0, 0);

        // Create wire material
        Material wireMat = new Material(Shader.Find("Standard"));
        wireMat.color = new Color(0.7f, 0.7f, 0.7f); // Silver/gray
        wireMat.SetFloat("_Metallic", 0.8f);
        wireMat.SetFloat("_Glossiness", 0.6f);
        AssetDatabase.CreateAsset(wireMat, "Assets/Materials/ResistorWireMaterial.mat");

        leftWire.GetComponent<MeshRenderer>().sharedMaterial = wireMat;
        rightWire.GetComponent<MeshRenderer>().sharedMaterial = wireMat;

        // Select the created object
        Selection.activeGameObject = root;

        Debug.Log("Resistor Model created successfully!");
    }
}
