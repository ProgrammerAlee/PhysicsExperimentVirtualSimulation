#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using GridSystem;

namespace GridSystem.EditorScripts
{
    public class GridSystemUIBuilder : EditorWindow
    {
        [MenuItem("Tools/Build Dot Matrix Grid UI")]
        public static void BuildUI()
        {
            // 1. Create Main Canvas
            GameObject canvasGO = new GameObject("GridSystemCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();

            // 2. Create Managers Container
            GameObject managersGO = new GameObject("GridSystemManagers");
            managersGO.transform.SetParent(canvasGO.transform, false);

            GridManager gridManager = managersGO.AddComponent<GridManager>();
            GridWireManager wireManager = managersGO.AddComponent<GridWireManager>();
            GridValidationSystem validationSystem = managersGO.AddComponent<GridValidationSystem>();

            gridManager.mainCanvas = canvas;

            // 3. Create White Background
            GameObject bgGO = new GameObject("WhiteBackground");
            bgGO.transform.SetParent(canvasGO.transform, false);
            RectTransform bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = Color.white; // Pure white background

            // 4. Create Toolbar Panel
            GameObject toolbarPanelGO = new GameObject("ToolbarPanel");
            toolbarPanelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform toolbarRect = toolbarPanelGO.AddComponent<RectTransform>();
            toolbarRect.anchorMin = new Vector2(0f, 0f);
            toolbarRect.anchorMax = new Vector2(1f, 0.15f); // Bottom 15%
            toolbarRect.offsetMin = Vector2.zero;
            toolbarRect.offsetMax = Vector2.zero;

            Image toolbarBg = toolbarPanelGO.AddComponent<Image>();
            toolbarBg.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray toolbar

            HorizontalLayoutGroup toolbarLayout = toolbarPanelGO.AddComponent<HorizontalLayoutGroup>();
            toolbarLayout.childAlignment = TextAnchor.MiddleCenter;
            toolbarLayout.childControlHeight = false;
            toolbarLayout.childControlWidth = false;
            toolbarLayout.spacing = 30;

            gridManager.toolbarContainer = toolbarPanelGO.transform;

            // 4.5 Create Submit Button
            GameObject submitBtnGO = new GameObject("SubmitButton", typeof(RectTransform), typeof(Image), typeof(Button));
            submitBtnGO.transform.SetParent(toolbarPanelGO.transform, false);
            RectTransform submitRect = submitBtnGO.GetComponent<RectTransform>();
            submitRect.sizeDelta = new Vector2(150, 60);

            Image submitImage = submitBtnGO.GetComponent<Image>();
            submitImage.color = new Color(0.2f, 0.6f, 0.2f); // Greenish

            Button submitButton = submitBtnGO.GetComponent<Button>();

            GameObject submitTextGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            submitTextGO.transform.SetParent(submitBtnGO.transform, false);
            Text submitText = submitTextGO.GetComponent<Text>();
            submitText.text = "提交 / 验证";
            submitText.alignment = TextAnchor.MiddleCenter;
            submitText.color = Color.white;
            submitText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            submitText.fontSize = 24;

            // 4.6 Create Result Text on Canvas
            GameObject resultTextGO = new GameObject("ResultText", typeof(RectTransform), typeof(Text));
            resultTextGO.transform.SetParent(canvasGO.transform, false);
            RectTransform resultRect = resultTextGO.GetComponent<RectTransform>();
            resultRect.anchorMin = new Vector2(0.5f, 1f);
            resultRect.anchorMax = new Vector2(0.5f, 1f);
            resultRect.pivot = new Vector2(0.5f, 1f);
            resultRect.anchoredPosition = new Vector2(0, -50); // Top center
            resultRect.sizeDelta = new Vector2(800, 100);

            Text resText = resultTextGO.GetComponent<Text>();
            resText.text = "等待提交...";
            resText.alignment = TextAnchor.MiddleCenter;
            resText.color = Color.black;
            resText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            resText.fontSize = 36;

            validationSystem.resultText = resText;

            // Link Button to Validation System using script
            UnityEditor.Events.UnityEventTools.AddPersistentListener(submitButton.onClick, validationSystem.ValidateCircuit);

            // 5. Create Wire Container (behind the grid dots so dots stay visible)
            GameObject wireContainerGO = new GameObject("WireContainer");
            wireContainerGO.transform.SetParent(canvasGO.transform, false);
            RectTransform wireRect = wireContainerGO.AddComponent<RectTransform>();
            wireRect.anchorMin = Vector2.zero;
            wireRect.anchorMax = Vector2.one;
            wireRect.offsetMin = Vector2.zero;
            wireRect.offsetMax = Vector2.zero;
            wireManager.wireContainer = wireContainerGO.transform;

            // 6. Create Grid Panel
            GameObject gridPanelGO = new GameObject("DotMatrixGrid");
            gridPanelGO.transform.SetParent(canvasGO.transform, false);
            RectTransform gridPanelRect = gridPanelGO.AddComponent<RectTransform>();
            gridPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridPanelRect.pivot = new Vector2(0.5f, 0.5f);
            gridPanelRect.sizeDelta = new Vector2(1200, 800); // 24x16 dots

            // Transparent background for grid panel
            Image gridBg = gridPanelGO.AddComponent<Image>();
            gridBg.color = new Color(0, 0, 0, 0);

            GridLayoutGroup gridLayout = gridPanelGO.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(50, 50);
            gridLayout.spacing = new Vector2(0, 0); // No spacing, slots touch each other
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 24; // 24 columns

            gridManager.gridContainer = gridPanelGO.transform;

            // 7. Generate Invisible Grid Slots with Center Dots
            int rowCount = 16;
            int colCount = 24;
            for (int i = 0; i < rowCount * colCount; i++)
            {
                // The main layout cell (50x50)
                GameObject slotGO = new GameObject($"DotSlot_{i}");
                slotGO.AddComponent<RectTransform>();
                slotGO.transform.SetParent(gridPanelGO.transform, false);

                // Add GridSlotUI directly to the main layout cell
                // We no longer need an Image or Raycast target here for dropping!
                slotGO.AddComponent<GridSlotUI>();

                // The visual dot inside the slot
                GameObject dotVisualGO = new GameObject("VisualDot");
                dotVisualGO.transform.SetParent(slotGO.transform, false);
                RectTransform dotRect = dotVisualGO.AddComponent<RectTransform>();
                dotRect.anchorMin = new Vector2(0.5f, 0.5f);
                dotRect.anchorMax = new Vector2(0.5f, 0.5f);
                dotRect.sizeDelta = new Vector2(8, 8); // Dot size
                Image dotImage = dotVisualGO.AddComponent<Image>();
                dotImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray dot
                // Disable raycast on visual dot so it doesn't block slot clicks
                dotImage.raycastTarget = false;
            }

            // Select the created canvas
            Selection.activeGameObject = canvasGO;

            Debug.Log("<color=green><b>Dot Matrix Grid System UI successfully built!</b></color>\n" +
                      "Next steps:\n" +
                      "1. Create a Tool UI Prefab and Wire Prefab.\n" +
                      "2. Select the 'GridSystemManagers' object.\n" +
                      "3. Assign your prefabs in the Inspector.\n" +
                      "4. Add GridToolData to the Available Tools list.");
        }
    }
}
#endif
