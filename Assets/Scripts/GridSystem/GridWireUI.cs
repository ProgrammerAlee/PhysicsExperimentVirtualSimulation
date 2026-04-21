using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridSystem
{
    public class GridWireUI : MonoBehaviour, IPointerClickHandler
    {
        public GridWire wireReference;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Left click: Try to connect a selected pin to this wire
                if (GridWireManager.Instance != null && wireReference != null)
                {
                    GridWireManager.Instance.OnWireClicked(wireReference);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Right click: Remove wire
                if (GridWireManager.Instance != null && wireReference != null)
                {
                    GridWireManager.Instance.RemoveWire(wireReference);
                }
            }
        }
    }
}
