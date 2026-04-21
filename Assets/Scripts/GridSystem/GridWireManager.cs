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

            RectTransform lineRT = LineImage.GetComponent<RectTransform>();
            Vector3 startPos = StartPin.GetComponent<RectTransform>().position;
            Vector3 endPos = EndPin.GetComponent<RectTransform>().position;
            
            Vector3 dir = endPos - startPos;
            float distance = dir.magnitude;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            lineRT.position = startPos + (dir / 2f);
            lineRT.rotation = Quaternion.Euler(0, 0, angle);

            float canvasScale = 1f;
            if (GridManager.Instance != null && GridManager.Instance.mainCanvas != null)
                canvasScale = GridManager.Instance.mainCanvas.scaleFactor;
            
            float localDistance = distance / canvasScale;
            float adjustedDistance = Mathf.Max(0, localDistance - 20f);
            lineRT.sizeDelta = new Vector2(adjustedDistance, 8f); // Make wires slightly thicker for easier clicking
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
        public GameObject wirePrefab;

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
                SelectPin(clickedPin);
            }
            else if (_selectedPin == clickedPin)
            {
                DeselectPin();
            }
            else if (_selectedPin.ParentTool == clickedPin.ParentTool)
            {
                DeselectPin();
                SelectPin(clickedPin);
            }
            else
            {
                if (!WireExists(_selectedPin, clickedPin))
                {
                    CreateWire(_selectedPin, clickedPin);
                }
                DeselectPin();
            }
        }

        // NEW: Handle clicking on an existing wire
        public void OnWireClicked(GridWire targetWire)
        {
            if (_selectedPin == null) return;

            // If a pin is selected and we click a wire, connect the pin to one of the wire's endpoints
            // Logically, this makes the pin part of the same electrical node.
            // We connect to the closest endpoint of the wire for visual neatness.
            
            Vector3 pinPos = _selectedPin.GetComponent<RectTransform>().position;
            Vector3 startPos = targetWire.StartPin.GetComponent<RectTransform>().position;
            Vector3 endPos = targetWire.EndPin.GetComponent<RectTransform>().position;

            GridToolPinUI targetPin = (Vector3.Distance(pinPos, startPos) < Vector3.Distance(pinPos, endPos)) 
                                      ? targetWire.StartPin : targetWire.EndPin;

            if (_selectedPin.ParentTool != targetPin.ParentTool && !WireExists(_selectedPin, targetPin))
            {
                CreateWire(_selectedPin, targetPin);
                Debug.Log($"Connected Pin {_selectedPin.PinName} to wire junction via Pin {targetPin.PinName}");
            }
            
            DeselectPin();
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
            lineImage.raycastTarget = true; // IMPORTANT: Allow clicks

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
