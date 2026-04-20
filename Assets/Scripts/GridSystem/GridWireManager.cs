using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GridSystem
{
    public class GridWire
    {
        public GridToolPinUI StartPin;
        public GridToolPinUI EndPin;
        public Image LineImage;

        public GridWire(GridToolPinUI start, GridToolPinUI end, Image line)
        {
            StartPin = start;
            EndPin = end;
            LineImage = line;
            UpdateLine();
        }

        public void UpdateLine()
        {
            if (StartPin == null || EndPin == null || LineImage == null) return;

            RectTransform startRT = StartPin.GetComponent<RectTransform>();
            RectTransform endRT = EndPin.GetComponent<RectTransform>();
            RectTransform lineRT = LineImage.GetComponent<RectTransform>();

            Vector2 startPos = startRT.position;
            Vector2 endPos = endRT.position;
            Vector2 dir = endPos - startPos;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Convert world distance to local canvas space
            float localDistance = distance;
            if (GridManager.Instance != null && GridManager.Instance.mainCanvas != null)
            {
                localDistance = distance / GridManager.Instance.mainCanvas.scaleFactor;
            }

            // Shorten the wire so it starts and ends at the edge of the 20x20 pin (10px radius on each end = 20px total)
            float adjustedDistance = Mathf.Max(0, localDistance - 20f);

            lineRT.position = startPos + (dir / 2f);
            lineRT.rotation = Quaternion.Euler(0, 0, angle);
            lineRT.sizeDelta = new Vector2(adjustedDistance, 4f); // 4 is the new thinner line thickness
        }

        public bool Contains(GridToolPinUI pin)
        {
            return StartPin == pin || EndPin == pin;
        }

        public bool SameAs(GridToolPinUI p1, GridToolPinUI p2)
        {
            return (StartPin == p1 && EndPin == p2) || (StartPin == p2 && EndPin == p1);
        }
    }

    public class GridWireManager : MonoBehaviour
    {
        public static GridWireManager Instance { get; private set; }

        public Transform wireContainer;
        public GameObject wirePrefab; // Simple Image with a RectTransform centered at 0,0

        private GridToolPinUI _selectedPin;
        public List<GridWire> activeWires = new List<GridWire>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void OnPinClicked(GridToolPinUI clickedPin)
        {
            if (_selectedPin == null)
            {
                // First click
                SelectPin(clickedPin);
            }
            else if (_selectedPin == clickedPin)
            {
                // Second click on SAME pin: Deselect
                DeselectPin();
            }
            else if (_selectedPin.ParentTool == clickedPin.ParentTool)
            {
                // Second click on DIFFERENT pin but SAME tool: Deselect old, Select new
                DeselectPin();
                SelectPin(clickedPin);
            }
            else
            {
                // Second click on DIFFERENT tool: Connect
                if (!WireExists(_selectedPin, clickedPin))
                {
                    CreateWire(_selectedPin, clickedPin);
                }
                DeselectPin();
            }
        }

        private void SelectPin(GridToolPinUI pin)
        {
            _selectedPin = pin;
            pin.SetSelected(true);
        }

        private void DeselectPin()
        {
            if (_selectedPin != null)
            {
                _selectedPin.SetSelected(false);
                _selectedPin = null;
            }
        }

        private bool WireExists(GridToolPinUI p1, GridToolPinUI p2)
        {
            foreach (var wire in activeWires)
            {
                if (wire.SameAs(p1, p2)) return true;
            }
            return false;
        }

        private void CreateWire(GridToolPinUI start, GridToolPinUI end)
        {
            GameObject wireObj = Instantiate(wirePrefab, wireContainer);
            GridWireUI wireUI = wireObj.AddComponent<GridWireUI>();
            Image lineImage = wireObj.GetComponent<Image>();
            lineImage.raycastTarget = true; // Ensure it's clickable

            GridWire newWire = new GridWire(start, end, lineImage);
            wireUI.wireReference = newWire;
            activeWires.Add(newWire);
        }

        public void RemoveWiresConnectedToTool(GridToolUI tool)
        {
            List<GridWire> wiresToRemove = new List<GridWire>();
            foreach (var wire in activeWires)
            {
                if (tool.Pins.Contains(wire.StartPin) || tool.Pins.Contains(wire.EndPin))
                {
                    wiresToRemove.Add(wire);
                }
            }
            foreach (var wire in wiresToRemove)
            {
                RemoveWire(wire);
            }
        }

        public void RemoveWire(GridWire wire)
        {
            if (wire.LineImage != null) Destroy(wire.LineImage.gameObject);
            activeWires.Remove(wire);
        }

        public void UpdateWiresForTool(GridToolUI tool)
        {
            foreach (var wire in activeWires)
            {
                if (tool.Pins.Contains(wire.StartPin) || tool.Pins.Contains(wire.EndPin))
                {
                    wire.UpdateLine();
                }
            }
        }

        public void ClearAllWires()
        {
            foreach (var wire in activeWires)
            {
                if (wire.LineImage != null) Destroy(wire.LineImage.gameObject);
            }
            activeWires.Clear();
            DeselectPin();
        }
    }
}
