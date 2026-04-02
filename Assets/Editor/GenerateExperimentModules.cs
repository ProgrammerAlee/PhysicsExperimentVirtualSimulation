using UnityEngine;
using UnityEditor;
using System.IO;
using System.Net;

public class GenerateExperimentModules
{
    public static void Execute()
    {
        string[] urls = new string[]
        {
            "https://gqoqjkkptwfbkwyssmnj.supabase.co/storage/v1/object/sign/coplay-prod/generated-images/2026/04/02/5fdb89c1-e520-4e1c-9eaf-925ce8f71981.jpeg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV84OTViMjlkNC00ZDM2LTQwZjItOTQyMC0xOTA0OWZjMDJiYzgiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJjb3BsYXktcHJvZC9nZW5lcmF0ZWQtaW1hZ2VzLzIwMjYvMDQvMDIvNWZkYjg5YzEtZTUyMC00ZTFjLTllYWYtOTI1Y2U4ZjcxOTgxLmpwZWciLCJpYXQiOjE3NzUxMzk4NTAsImV4cCI6MTc5MDY5MTg1MH0.Om7ifqIeGSbgyOYtnKoZgHz5K33qHhpOuuw6FlkGjJs",
            "https://gqoqjkkptwfbkwyssmnj.supabase.co/storage/v1/object/sign/coplay-prod/generated-images/2026/04/02/1c1d8eb8-022d-4187-85e0-7b1247542875.jpeg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV84OTViMjlkNC00ZDM2LTQwZjItOTQyMC0xOTA0OWZjMDJiYzgiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJjb3BsYXktcHJvZC9nZW5lcmF0ZWQtaW1hZ2VzLzIwMjYvMDQvMDIvMWMxZDhlYjgtMDIyZC00MTg3LTg1ZTAtN2IxMjQ3NTQyODc1LmpwZWciLCJpYXQiOjE3NzUxMzk4NTEsImV4cCI6MTc5MDY5MTg1MX0.SD35wWJ-FjCM3PjycVNnhDolG0oRJbVjwu0OhdNdpqY",
            "https://gqoqjkkptwfbkwyssmnj.supabase.co/storage/v1/object/sign/coplay-prod/generated-images/2026/04/02/2a822c74-d98e-43d5-9439-c8458e5287e3.jpeg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV84OTViMjlkNC00ZDM2LTQwZjItOTQyMC0xOTA0OWZjMDJiYzgiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJjb3BsYXktcHJvZC9nZW5lcmF0ZWQtaW1hZ2VzLzIwMjYvMDQvMDIvMmE4MjJjNzQtZDk4ZS00M2Q1LTk0MzktYzg0NThlNTI4N2UzLmpwZWciLCJpYXQiOjE3NzUxMzk4NTIsImV4cCI6MTc5MDY5MTg1Mn0.gFALYwwzXwKZDZxY3a8hFqTMFuyXvIACfdFXXBA4GjM"
        };

        string[] fileNames = new string[]
        {
            "ModuleTex1.jpg",
            "ModuleTex2.jpg",
            "ModuleTex3.jpg"
        };

        if (!Directory.Exists("Assets/Textures"))
        {
            Directory.CreateDirectory("Assets/Textures");
        }
        if (!Directory.Exists("Assets/Materials"))
        {
            Directory.CreateDirectory("Assets/Materials");
        }

        for (int i = 0; i < urls.Length; i++)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(urls[i], "Assets/Textures/" + fileNames[i]);
            }
        }

        AssetDatabase.Refresh();

        Camera cam = Camera.main;
        Vector3 startPos = cam != null ? cam.transform.position + cam.transform.forward * 1.5f : new Vector3(0, 1.5f, 0);
        Quaternion rot = cam != null ? Quaternion.LookRotation(cam.transform.forward) : Quaternion.identity;

        for (int i = 0; i < 3; i++)
        {
            string texPath = "Assets/Textures/" + fileNames[i];
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            
            Material mat = new Material(Shader.Find("Standard"));
            mat.mainTexture = tex;
            AssetDatabase.CreateAsset(mat, "Assets/Materials/ModuleMat" + (i+1) + ".mat");

            GameObject module = new GameObject("ExperimentModule_" + (i+1));
            module.transform.position = startPos + (cam != null ? cam.transform.right : Vector3.right) * (i - 1) * 0.8f;
            module.transform.rotation = rot;
            
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(module.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(0.75f, 1.0f, 0.05f);
            
            Material bodyMat = new Material(Shader.Find("Standard"));
            bodyMat.color = new Color(0.9f, 0.9f, 0.9f);
            body.GetComponent<MeshRenderer>().material = bodyMat;

            GameObject frontFace = GameObject.CreatePrimitive(PrimitiveType.Quad);
            frontFace.name = "FrontFace";
            frontFace.transform.SetParent(module.transform);
            frontFace.transform.localPosition = new Vector3(0, 0, -0.026f);
            frontFace.transform.localRotation = Quaternion.identity;
            frontFace.transform.localScale = new Vector3(0.75f, 1.0f, 1f);
            frontFace.GetComponent<MeshRenderer>().material = mat;
        }
        
        AssetDatabase.SaveAssets();
    }
}
