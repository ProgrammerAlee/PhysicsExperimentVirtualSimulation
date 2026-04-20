using UnityEngine;
using UnityEngine.EventSystems;

namespace GridSystem
{
    public class GridToolPinUI : MonoBehaviour, IPointerClickHandler
    {
        public GridToolUI ParentTool { get; private set; }
        public int PinIndex { get; private set; }

        public void Setup(GridToolUI parentTool, int pinIndex)
        {
            ParentTool = parentTool;
            PinIndex = pinIndex;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Forward click to wire manager
            GridWireManager.Instance.OnPinClicked(this);
        }
    }
}
