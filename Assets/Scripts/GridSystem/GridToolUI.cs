using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridSystem
{
    public class GridToolUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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
                GameObject pinObj = new GameObject($"Pin_{i}", typeof(RectTransform), typeof(Image), typeof(GridToolPinUI));
                pinObj.transform.SetParent(transform, false);

                RectTransform pinRT = pinObj.GetComponent<RectTransform>();
                pinRT.sizeDelta = new Vector2(10f, 10f); // Size of the pin visual

                // Position pins along the bottom edge
                float xPos = -width / 2f + spacing * (i + 1);
                pinRT.anchoredPosition = new Vector2(xPos, -_rectTransform.rect.height / 2f);

                Image pinImage = pinObj.GetComponent<Image>();
                pinImage.color = Color.red; // Distinct color for pins

                GridToolPinUI pinUI = pinObj.GetComponent<GridToolPinUI>();
                pinUI.Setup(this, i);
                Pins.Add(pinUI);
            }
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsPlaced)
            {
                CurrentSlot?.ClearSlot();
                CurrentSlot = null;
                IsPlaced = false;
            }

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
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = true;

            // Handled by GridSlotUI or GridManager, fallback below
            if (!IsPlaced)
            {
                ReturnToOriginal();
            }
        }

        public void ReturnToOriginal()
        {
            transform.SetParent(_originalParent);
            _rectTransform.anchoredPosition = _originalPosition;
        }

        public void PlaceOnGrid(Transform parent)
        {
            IsPlaced = true;
            transform.SetParent(parent, false);
            _rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
