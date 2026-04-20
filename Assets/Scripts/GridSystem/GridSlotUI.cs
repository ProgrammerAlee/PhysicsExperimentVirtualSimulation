using UnityEngine;
using UnityEngine.EventSystems;

namespace GridSystem
{
    public class GridSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        public bool IsOccupied { get; private set; }
        public GridToolUI OccupyingTool { get; private set; }

        private void Awake()
        {
            IsOccupied = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (IsOccupied) return;

            GameObject droppedObj = eventData.pointerDrag;
            if (droppedObj != null)
            {
                GridToolUI toolUI = droppedObj.GetComponent<GridToolUI>();
                if (toolUI != null)
                {
                    OccupySlot(toolUI);
                }
            }
        }

        public void OccupySlot(GridToolUI toolUI)
        {
            IsOccupied = true;
            OccupyingTool = toolUI;
            toolUI.CurrentSlot = this;
            toolUI.PlaceOnGrid(transform);
            GridManager.Instance.RegisterPlacedTool(this, toolUI);
        }

        public void ClearSlot()
        {
            IsOccupied = false;
            OccupyingTool = null;
            GridManager.Instance.UnregisterPlacedTool(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Removed slot clicking as connections are now pin-to-pin
            // Optional: you can handle slot selection if needed
        }
    }
}
