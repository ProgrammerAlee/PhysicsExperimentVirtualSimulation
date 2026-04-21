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

        private Vector2 _originalPosition;
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
            // Clear existing pins if any
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
                // Fallback to legacy bottom-edge positioning
                float spacing = width / (Data.pinCount + 1);
                for (int i = 0; i < Data.pinCount; i++)
                {
                    float xPos = -width / 2f + spacing * (i + 1);
                    Vector2 pos = new Vector2(xPos, -height / 2f);
                    // Convert to normalized for internal consistency if needed, but here we just pass it
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
            
            // normalizedPos is relative to center, so (-0.5, -0.5) is bottom-left
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
            _originalPosition = _rectTransform.anchoredPosition;
            _originalParent = transform.parent;

            transform.SetParent(_canvas.transform, true);
            transform.SetAsLastSibling();
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
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
            _rectTransform.anchoredPosition = _originalPosition;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsPlaced && eventData.button == PointerEventData.InputButton.Right)
            {
                if (GridWireManager.Instance != null)
                {
                    GridWireManager.Instance.RemoveWiresConnectedToTool(this);
                }
                CurrentSlot?.ClearSlot();
            }
        }

        public void PlaceOnGrid(Transform parent)
        {
            if (IsPlaced && CurrentSlot != null && CurrentSlot.transform != parent)
            {
                CurrentSlot.ClearSlot();
            }
            IsPlaced = true;
            transform.SetParent(parent, false);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;

            if (GridWireManager.Instance != null)
            {
                GridWireManager.Instance.UpdateWiresForTool(this);
            }
        }
    }
}
