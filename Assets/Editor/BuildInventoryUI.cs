using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class BuildInventoryUI
{
    public static void Execute()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Remove old panel if exists
        Transform oldPanel = canvasObj.transform.Find("InventoryPanel");
        if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);

        // Create Inventory Panel
        GameObject panelObj = new GameObject("InventoryPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.15f, 0.15f);
        panelRect.anchorMax = new Vector2(0.85f, 0.85f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        // Add Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "实验器材库 (按Tab关闭)";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 32;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;

        // Create Grid Container
        GameObject gridObj = new GameObject("GridContainer");
        gridObj.transform.SetParent(panelObj.transform, false);
        RectTransform gridRect = gridObj.AddComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0, 0);
        gridRect.anchorMax = new Vector2(1, 0.9f);
        gridRect.offsetMin = Vector2.zero;
        gridRect.offsetMax = Vector2.zero;

        GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(160, 200);
        grid.spacing = new Vector2(20, 20);
        grid.padding = new RectOffset(40, 40, 40, 40);
        grid.childAlignment = TextAnchor.UpperCenter;

        string[] names = { "电压表", "标准电池", "红色线束", "黑色线束", "实验板1", "实验板2", "实验板3" };
        string[] paths = { "DigitalVoltmeter", "Enviro/电池", "Wire_Positive", "Wire_Negative", "ExperimentModule_1", "ExperimentModule_2", "ExperimentModule_3" };

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        for (int i = 0; i < names.Length; i++)
        {
            GameObject itemObj = new GameObject("Item_" + names[i]);
            itemObj.transform.SetParent(gridObj.transform, false);
            Image itemBg = itemObj.AddComponent<Image>();
            itemBg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            // Image placeholder
            GameObject imgObj = new GameObject("Image");
            imgObj.transform.SetParent(itemObj.transform, false);
            RectTransform imgRect = imgObj.AddComponent<RectTransform>();
            imgRect.anchorMin = new Vector2(0.1f, 0.4f);
            imgRect.anchorMax = new Vector2(0.9f, 0.9f);
            imgRect.offsetMin = Vector2.zero;
            imgRect.offsetMax = Vector2.zero;
            Image img = imgObj.AddComponent<Image>();
            img.color = new Color(0.4f, 0.4f, 0.4f, 1f);

            // Label
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(itemObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.2f);
            textRect.anchorMax = new Vector2(0.9f, 0.4f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            Text text = textObj.AddComponent<Text>();
            text.text = names[i];
            text.font = font;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            // Toggle
            GameObject toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(itemObj.transform, false);
            RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(0.3f, 0.05f);
            toggleRect.anchorMax = new Vector2(0.7f, 0.2f);
            toggleRect.offsetMin = Vector2.zero;
            toggleRect.offsetMax = Vector2.zero;
            Toggle toggle = toggleObj.AddComponent<Toggle>();

            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImg = bgObj.AddComponent<Image>();
            bgImg.color = Color.white;
            toggle.targetGraphic = bgImg;

            GameObject checkObj = new GameObject("Checkmark");
            checkObj.transform.SetParent(bgObj.transform, false);
            RectTransform checkRect = checkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            Image checkImg = checkObj.AddComponent<Image>();
            checkImg.color = Color.black;
            toggle.graphic = checkImg;

            // Script
            InventoryItemToggle iit = itemObj.AddComponent<InventoryItemToggle>();
            iit.toggle = toggle;
            iit.targetObject = GameObject.Find(paths[i]);
        }

        panelObj.SetActive(false);

        SimulationUIManager uiManager = canvasObj.GetComponent<SimulationUIManager>();
        if (uiManager != null)
        {
            uiManager.inventoryPanel = panelObj;
        }
    }
}
