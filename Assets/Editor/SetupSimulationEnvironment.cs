using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SetupSimulationEnvironment
{
    public static void Execute()
    {
        // Cleanup previous failed attempts
        GameObject oldPlayer = GameObject.Find("FirstPersonController");
        if (oldPlayer != null) Object.DestroyImmediate(oldPlayer);
        GameObject oldCanvas = GameObject.Find("Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);
        GameObject oldEventSystem = GameObject.Find("EventSystem");
        if (oldEventSystem != null) Object.DestroyImmediate(oldEventSystem);

        // 1. Skybox
        Material skyboxMat = new Material(Shader.Find("Skybox/Procedural"));
        skyboxMat.SetColor("_SkyTint", new Color(0.1f, 0.2f, 0.3f));
        skyboxMat.SetColor("_GroundColor", new Color(0.05f, 0.05f, 0.05f));
        skyboxMat.SetFloat("_SunSize", 0.04f);
        skyboxMat.SetFloat("_AtmosphereThickness", 0.8f);
        
        if (!System.IO.Directory.Exists("Assets/Materials"))
            System.IO.Directory.CreateDirectory("Assets/Materials");
            
        UnityEditor.AssetDatabase.CreateAsset(skyboxMat, "Assets/Materials/SimulationSkybox.mat");
        RenderSettings.skybox = skyboxMat;
        DynamicGI.UpdateEnvironment();

        // 2. First Person Controller
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "FirstPersonController";
        player.transform.position = new Vector3(0, 1.5f, 0);
        
        // Find existing Main Camera or create one
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        
        mainCam.transform.SetParent(player.transform);
        mainCam.transform.localPosition = new Vector3(0, 0.6f, 0);
        mainCam.transform.localRotation = Quaternion.identity;

        FirstPersonController fpc = player.AddComponent<FirstPersonController>();
        
        // 3. UI Setup
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        // Main Page UI
        GameObject mainPage = new GameObject("MainPageUI");
        mainPage.transform.SetParent(canvasObj.transform, false);
        RectTransform mainPageRect = mainPage.AddComponent<RectTransform>();
        mainPageRect.anchorMin = Vector2.zero;
        mainPageRect.anchorMax = Vector2.one;
        mainPageRect.sizeDelta = Vector2.zero;
        
        Image mainPageBg = mainPage.AddComponent<Image>();
        mainPageBg.color = new Color(0, 0, 0, 0.8f);

        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainPage.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "物理虚拟仿真系统";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.8f);
        titleRect.sizeDelta = new Vector2(600, 100);

        GameObject startBtnObj = new GameObject("StartButton");
        startBtnObj.transform.SetParent(mainPage.transform, false);
        Image startBtnImg = startBtnObj.AddComponent<Image>();
        startBtnImg.color = new Color(0.2f, 0.6f, 1f);
        Button startBtn = startBtnObj.AddComponent<Button>();
        RectTransform startBtnRect = startBtnObj.GetComponent<RectTransform>();
        startBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
        startBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
        startBtnRect.sizeDelta = new Vector2(200, 60);
        
        GameObject startBtnTextObj = new GameObject("Text");
        startBtnTextObj.transform.SetParent(startBtnObj.transform, false);
        Text startBtnText = startBtnTextObj.AddComponent<Text>();
        startBtnText.text = "进入仿真";
        startBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        startBtnText.fontSize = 24;
        startBtnText.alignment = TextAnchor.MiddleCenter;
        startBtnText.color = Color.white;
        startBtnTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);

        // Tool Selection UI
        GameObject toolPanel = new GameObject("ToolSelectionUI");
        toolPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform toolPanelRect = toolPanel.AddComponent<RectTransform>();
        toolPanelRect.anchorMin = new Vector2(0, 0);
        toolPanelRect.anchorMax = new Vector2(0, 1);
        toolPanelRect.pivot = new Vector2(0, 0.5f);
        toolPanelRect.sizeDelta = new Vector2(200, 0);
        
        Image toolPanelBg = toolPanel.AddComponent<Image>();
        toolPanelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        VerticalLayoutGroup vlg = toolPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 50, 10);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = false;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;

        string[] tools = { "选择工具", "移动工具", "旋转工具", "添加物体", "重置场景" };
        foreach (string tool in tools)
        {
            GameObject btnObj = new GameObject(tool + "Button");
            btnObj.transform.SetParent(toolPanel.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.3f, 0.3f, 0.3f);
            btnObj.AddComponent<Button>();
            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.minHeight = 40;
            
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            Text txt = txtObj.AddComponent<Text>();
            txt.text = tool;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 18;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
        }

        // Setup UIManager
        SimulationUIManager uiManager = canvasObj.AddComponent<SimulationUIManager>();
        uiManager.mainPageUI = mainPage;
        
        // Hook up start button
        startBtn.onClick.AddListener(uiManager.StartSimulation);
    }
}
