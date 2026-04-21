using UnityEngine;
using UnityEngine.UI;

namespace GridSystem
{
    [RequireComponent(typeof(Button))]
    public class GridToolPinUI : MonoBehaviour
    {
        public GridToolUI ParentTool { get; private set; }
        public int PinIndex { get; private set; }
        public string PinName { get; private set; }

        private Button _button;

        public void Setup(GridToolUI parentTool, int pinIndex, string pinName)
        {
            ParentTool = parentTool;
            PinIndex = pinIndex;
            PinName = pinName;

            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnPinClicked);
            
            // Optionally add a tooltip or text label for the pin name
            gameObject.name = $"Pin_{pinName}_{pinIndex}";
        }

        public void OnPinClicked()
        {
            if (ParentTool == null || ParentTool.isFromToolbar) return;
            Debug.Log($"Pin Clicked: {PinName} (Index: {PinIndex}) on {ParentTool.gameObject.name}");
            GridWireManager.Instance.OnPinClicked(this);
        }

        public void SetSelected(bool isSelected)
        {
            if (_button != null)
            {
                ColorBlock colors = _button.colors;
                colors.normalColor = isSelected ? Color.yellow : Color.red;
                colors.selectedColor = isSelected ? Color.yellow : Color.red;
                _button.colors = colors;
            }
        }
    }
}
