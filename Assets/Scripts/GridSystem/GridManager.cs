using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GridSystem
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("References")]
        public Transform toolbarContainer;
        public Transform gridContainer;
        public GameObject toolUIPrefab;
        public Canvas mainCanvas;

        [Header("Data")]
        public List<GridToolData> availableTools;

        public Dictionary<GridSlotUI, GridToolUI> placedTools = new Dictionary<GridSlotUI, GridToolUI>();
        public List<GridSlotUI> AllSlots { get; private set; } = new List<GridSlotUI>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            InitializeToolbar();
            InitializeGridSlots();
        }

        private void InitializeGridSlots()
        {
            if (gridContainer != null)
            {
                AllSlots.Clear();
                AllSlots.AddRange(gridContainer.GetComponentsInChildren<GridSlotUI>());
            }
        }

        public GridSlotUI GetClosestValidSlot(Vector2 screenPosition, float snapRadius)
        {
            GridSlotUI closestSlot = null;
            float closestDistance = float.MaxValue;

            foreach (var slot in AllSlots)
            {
                if (slot.IsOccupied) continue;

                // Get slot's screen position
                RectTransform slotRect = slot.GetComponent<RectTransform>();
                Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(null, slotRect.position);

                float distance = Vector2.Distance(screenPosition, slotScreenPos);

                if (distance < closestDistance && distance <= snapRadius)
                {
                    closestDistance = distance;
                    closestSlot = slot;
                }
            }

            return closestSlot;
        }

        private void InitializeToolbar()
        {
            foreach (var toolData in availableTools)
            {
                var toolObj = Instantiate(toolUIPrefab, toolbarContainer);
                var toolUI = toolObj.GetComponent<GridToolUI>();
                toolUI.Setup(toolData, mainCanvas);

                // Attach drag scripts dynamically if necessary, or handled on prefab
            }
        }

        public void RegisterPlacedTool(GridSlotUI slot, GridToolUI tool)
        {
            if (!placedTools.ContainsKey(slot))
            {
                placedTools.Add(slot, tool);

                // Clone the tool back into the toolbar so the user can drag another
                if (tool.isFromToolbar)
                {
                    tool.isFromToolbar = false;
                    var toolObj = Instantiate(toolUIPrefab, toolbarContainer);
                    var newToolUI = toolObj.GetComponent<GridToolUI>();
                    newToolUI.Setup(tool.Data, mainCanvas);
                }
            }
        }

        public void UnregisterPlacedTool(GridSlotUI slot)
        {
            if (placedTools.ContainsKey(slot))
            {
                var tool = placedTools[slot];
                placedTools.Remove(slot);
                if (tool != null) Destroy(tool.gameObject);
            }
        }

        public void ClearGrid()
        {
            foreach (var slot in placedTools.Keys)
            {
                slot.ClearSlot();
            }
            placedTools.Clear();
            GridWireManager.Instance.ClearAllWires();
        }
    }
}
