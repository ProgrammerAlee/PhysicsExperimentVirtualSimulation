using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridSystem
{
    public class GridToolUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public GridToolData Data { get; private set; }
        private Image _iconImage;
        private RectTransform _rectTransform;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        private Vector3 _originalWorldPosition;
        private Transform _originalParent;
        public bool IsPlaced { get; private set; }
        public GridSlotUI CurrentSlot { get; set; }
        public bool isFromToolbar = true;

        public List<GridToolPinUI> Pins { get; private set; } = new List<GridToolPinUI>();

        public void Setup(GridToolData data, Canvas canvas)
        {
            Data = data;
            _canvas = canvas;
            _rectTransform = GetComponent<RectTransform>();
            _iconImage = GetComponent<Image>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (_iconImage != null && data.icon != null)
            {
                _iconImage.sprite = data.icon;
            }

            IsPlaced = false;
            isFromToolbar = true;
            CreatePins();
        }

        private void CreatePins()
        {
            foreach (var pin in Pins)
            {
                if (pin != null) Destroy(pin.gameObject);
            }
            Pins.Clear();

            if (Data == null) return;

            float width = _rectTransform.rect.width;
            float height = _rectTransform.rect.height;

            if (Data.pinDefinitions != null && Data.pinDefinitions.Count > 0)
            {
                for (int i = 0; i < Data.pinDefinitions.Count; i++)
                {
                    var def = Data.pinDefinitions[i];
                    CreatePin(i, def.name, def.normalizedPosition, width, height);
                }
            }
            else if (Data.pinCount > 0)
            {
                float spacing = width / (Data.pinCount + 1);
                for (int i = 0; i < Data.pinCount; i++)
                {
                    float xPos = -width / 2f + spacing * (i + 1);
                    CreatePin(i, ((char)('A' + i)).ToString(), new Vector2(xPos / width, -0.5f), width, height);
                }
            }
        }

        private void CreatePin(int index, string pinName, Vector2 normalizedPos, float width, float height)
        {
            GameObject pinObj = new GameObject($"Pin_{pinName}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(GridToolPinUI));
            pinObj.transform.SetParent(transform, false);

            RectTransform pinRT = pinObj.GetComponent<RectTransform>();
            pinRT.sizeDelta = new Vector2(20f, 20f);
            pinRT.anchoredPosition = new Vector2(normalizedPos.x * width, normalizedPos.y * height);

            Image pinImage = pinObj.GetComponent<Image>();
            pinImage.color = Color.white;
            pinImage.raycastTarget = true;

            Button pinButton = pinObj.GetComponent<Button>();
            ColorBlock colors = pinButton.colors;
            colors.normalColor = Color.red;
            colors.highlightedColor = new Color(1f, 0.5f, 0.5f);
            colors.pressedColor = new Color(0.5f, 0f, 0f);
            colors.selectedColor = Color.red;
            pinButton.colors = colors;

            GridToolPinUI pinUI = pinObj.GetComponent<GridToolPinUI>();
            pinUI.Setup(this, index, pinName);
            Pins.Add(pinUI);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalWorldPosition = _rectTransform.position;
            _originalParent = transform.parent;

            transform.SetParent(_canvas.transform, true);
            transform.SetAsLastSibling();
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 worldPoint;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out worldPoint))
            {
                _rectTransform.position = worldPoint;
            }

            if (GridWireManager.Instance != null)
            {
                GridWireManager.Instance.UpdateWiresForTool(this);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;

            if (GridManager.Instance != null)
            {
                Vector2 toolScreenPosition = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, _rectTransform.position);
                GridSlotUI closestSlot = GridManager.Instance.GetClosestValidSlot(toolScreenPosition, 40f);

                if (closestSlot != null)
                {
                    closestSlot.OccupySlot(this);
                    return;
                }
            }

            if (transform.parent == _canvas.transform)
            {
                ReturnToOriginal();
                if (GridWireManager.Instance != null)
                {
                    GridWireManager.Instance.UpdateWiresForTool(this);
                }
            }
        }

        public void ReturnToOriginal()
        {
            transform.SetParent(_originalParent);
            _rectTransform.position = _originalWorldPosition;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsPlaced && eventData.button == PointerEventData.InputButton.Right)
            {
                if (GridWireManager.Instance != null)
                {
                    GridWireManager.Instance.RemoveWiresConnectedToTool(this);
                }
                CurrentSlot?.ClearSlot(true); // Right click DOES destroy the tool
            }
        }

        public void PlaceOnGrid(GridSlotUI slot)
        {
            // If already placed somewhere else, clear that slot BUT don't destroy this object!
            if (IsPlaced && CurrentSlot != null && CurrentSlot != slot)
            {
                CurrentSlot.ClearSlot(false); 
            }
            
            IsPlaced = true;
            CurrentSlot = slot;

            if (GridManager.Instance != null && GridManager.Instance.toolContainer != null)
            {
                transform.SetParent(GridManager.Instance.toolContainer, true);
            }
            else
            {
                transform.SetParent(slot.transform, false);
            }

            _rectTransform.position = slot.GetComponent<RectTransform>().position;
            
            if (GridWireManager.Instance != null)
            {
                GridWireManager.Instance.UpdateWiresForTool(this);
            }
        }
    }
}
