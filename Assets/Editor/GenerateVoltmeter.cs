using UnityEngine;
using UnityEngine.UI;

public class GenerateVoltmeter
{
    public static void Execute()
    {
        GameObject voltmeter = new GameObject("DigitalVoltmeter");
        
        // Materials
        Material blackMat = new Material(Shader.Find("Standard"));
        blackMat.color = new Color(0.05f, 0.05f, 0.05f);
        blackMat.SetFloat("_Glossiness", 0.3f);
        
        Material redMat = new Material(Shader.Find("Standard"));
        redMat.color = new Color(0.8f, 0.1f, 0.1f);
        
        Material screenMat = new Material(Shader.Find("Standard"));
        screenMat.color = new Color(0.1f, 0.05f, 0.05f); // Dark red background

        Material greenMat = new Material(Shader.Find("Standard"));
        greenMat.color = new Color(0.1f, 0.6f, 0.5f);

        // 1. Base Body
        GameObject baseBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseBody.name = "BaseBody";
        baseBody.transform.SetParent(voltmeter.transform);
        baseBody.transform.localScale = new Vector3(0.14f, 0.04f, 0.08f);
        baseBody.transform.localPosition = new Vector3(0, 0.02f, 0);
        baseBody.GetComponent<MeshRenderer>().material = blackMat;

        // 2. Slanted Body
        GameObject slantBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slantBody.name = "SlantBody";
        slantBody.transform.SetParent(voltmeter.transform);
        slantBody.transform.localScale = new Vector3(0.14f, 0.1f, 0.04f);
        slantBody.transform.localPosition = new Vector3(0, 0.08f, 0.02f);
        slantBody.transform.localRotation = Quaternion.Euler(-15f, 0, 0);
        slantBody.GetComponent<MeshRenderer>().material = blackMat;

        // 3. Terminal Panel (Green)
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Quad);
        panel.name = "TerminalPanel";
        panel.transform.SetParent(baseBody.transform);
        panel.transform.localScale = new Vector3(0.8f, 0.4f, 1f);
        panel.transform.localPosition = new Vector3(0, 0.501f, -0.2f);
        panel.transform.localRotation = Quaternion.Euler(90, 0, 0);
        panel.GetComponent<MeshRenderer>().material = greenMat;

        // 4. Display Screen
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "Screen";
        screen.transform.SetParent(slantBody.transform);
        screen.transform.localScale = new Vector3(0.8f, 0.6f, 1f);
        screen.transform.localPosition = new Vector3(0, 0.1f, -0.501f);
        screen.GetComponent<MeshRenderer>().material = screenMat;

        // 5. Digital Text
        GameObject canvasObj = new GameObject("ScreenCanvas");
        canvasObj.transform.SetParent(screen.transform);
        canvasObj.transform.localPosition = new Vector3(0, 0, -0.001f);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 60);
        
        GameObject textObj = new GameObject("DigitalText");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;
        
        Text text = textObj.AddComponent<Text>();
        text.text = "0.00";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 48;
        text.color = new Color(1f, 0.2f, 0.2f); // Bright red
        text.alignment = TextAnchor.MiddleCenter;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 60);

        // 6. Terminals
        CreateTerminal(voltmeter.transform, "Terminal_Neg", blackMat, new Vector3(-0.04f, 0.04f, -0.015f));
        CreateTerminal(voltmeter.transform, "Terminal_Pos3", redMat, new Vector3(0f, 0.04f, -0.015f));
        CreateTerminal(voltmeter.transform, "Terminal_Pos15", redMat, new Vector3(0.04f, 0.04f, -0.015f));

        // 7. Labels
        CreateLabel(baseBody.transform, "-", new Vector3(-0.3f, 0.502f, 0.3f));
        CreateLabel(baseBody.transform, "3", new Vector3(0f, 0.502f, 0.3f));
        CreateLabel(baseBody.transform, "15", new Vector3(0.3f, 0.502f, 0.3f));

        // Position in front of camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            voltmeter.transform.position = cam.transform.position + cam.transform.forward * 0.5f - cam.transform.up * 0.1f;
            voltmeter.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
        }
        else
        {
            voltmeter.transform.position = new Vector3(0, 1f, 0);
        }
    }

    private static void CreateTerminal(Transform parent, string name, Material mat, Vector3 localPos)
    {
        GameObject terminal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        terminal.name = name;
        terminal.transform.SetParent(parent);
        terminal.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
        terminal.transform.localPosition = localPos;
        terminal.GetComponent<MeshRenderer>().material = mat;
        
        GameObject hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hole.name = "Hole";
        hole.transform.SetParent(terminal.transform);
        hole.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
        hole.transform.localPosition = new Vector3(0, 1f, 0);
        Material blackMat = new Material(Shader.Find("Standard"));
        blackMat.color = Color.black;
        hole.GetComponent<MeshRenderer>().material = blackMat;
    }

    private static void CreateLabel(Transform parent, string textStr, Vector3 localPos)
    {
        GameObject canvasObj = new GameObject("LabelCanvas_" + textStr);
        canvasObj.transform.SetParent(parent);
        canvasObj.transform.localPosition = localPos;
        canvasObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        canvasObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;
        
        Text text = textObj.AddComponent<Text>();
        text.text = textStr;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
    }
}
