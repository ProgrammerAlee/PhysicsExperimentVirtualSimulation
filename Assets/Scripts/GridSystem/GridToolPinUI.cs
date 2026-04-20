using UnityEngine;
using UnityEngine.UI;

namespace GridSystem
{
    [RequireComponent(typeof(Button))]
    public class GridToolPinUI : MonoBehaviour
    {
        public GridToolUI ParentTool { get; private set; }
        public int PinIndex { get; private set; }

        private Button _button;

        public void Setup(GridToolUI parentTool, int pinIndex)
        {
            ParentTool = parentTool;
            PinIndex = pinIndex;

            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnPinClicked);
        }

        public void OnPinClicked()
        {
            Debug.Log($"Pin Clicked: {PinIndex} on {ParentTool.gameObject.name}");
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
