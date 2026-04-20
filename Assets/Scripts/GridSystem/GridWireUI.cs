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
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (GridWireManager.Instance != null && wireReference != null)
                {
                    GridWireManager.Instance.RemoveWire(wireReference);
                }
            }
        }
    }
}
