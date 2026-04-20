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
            // Create small square images to represent pins, spaced along the bottom edge for now
            if (Data == null || Data.pinCount <= 0) return;

            float width = _rectTransform.rect.width;
            float spacing = width / (Data.pinCount + 1);

            for (int i = 0; i < Data.pinCount; i++)
            {
                GameObject pinObj = new GameObject($"Pin_{i}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(GridToolPinUI));
                pinObj.transform.SetParent(transform, false);

                RectTransform pinRT = pinObj.GetComponent<RectTransform>();
                pinRT.sizeDelta = new Vector2(20f, 20f); // Size of the pin visual

                // Position pins along the bottom edge
                float xPos = -width / 2f + spacing * (i + 1);
                pinRT.anchoredPosition = new Vector2(xPos, -_rectTransform.rect.height / 2f);

                Image pinImage = pinObj.GetComponent<Image>();
                pinImage.color = Color.white; // Button handles coloring via ColorBlock
                pinImage.raycastTarget = true;

                Button pinButton = pinObj.GetComponent<Button>();
                ColorBlock colors = pinButton.colors;
                colors.normalColor = Color.red;
                colors.highlightedColor = new Color(1f, 0.5f, 0.5f); // Lighter red
                colors.pressedColor = new Color(0.5f, 0f, 0f); // Dark red
                colors.selectedColor = Color.red;
                pinButton.colors = colors;

                GridToolPinUI pinUI = pinObj.GetComponent<GridToolPinUI>();
                pinUI.Setup(this, i);
                Pins.Add(pinUI);
            }
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            _originalPosition = _rectTransform.anchoredPosition;
            _originalParent = transform.parent;

            // Move to root to draw over everything while dragging
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

            // Handled by GridSlotUI or GridManager, fallback below
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
                CurrentSlot?.ClearSlot(); // This automatically unregisters and destroys the tool
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
