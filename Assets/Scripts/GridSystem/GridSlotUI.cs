using UnityEngine;
using UnityEngine.EventSystems;

namespace GridSystem
{
    public class GridSlotUI : MonoBehaviour, IPointerClickHandler
    {
        public bool IsOccupied { get; private set; }
        public GridToolUI OccupyingTool { get; private set; }    

        private void Awake()
        {
            IsOccupied = false;
        }

        public void OccupySlot(GridToolUI toolUI)
        {
            IsOccupied = true;
            OccupyingTool = toolUI;
            toolUI.PlaceOnGrid(this);
            GridManager.Instance.RegisterPlacedTool(this, toolUI);
        }

        public void ClearSlot(bool destroyTool = true)
        {
            IsOccupied = false;
            OccupyingTool = null;
            GridManager.Instance.UnregisterPlacedTool(this, destroyTool);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Optional click handling
        }
    }
}
