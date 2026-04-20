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

            lineRT.position = startPos + (dir / 2f);
            lineRT.rotation = Quaternion.Euler(0, 0, angle);
            lineRT.sizeDelta = new Vector2(distance, 5f); // 5 is line thickness
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
                _selectedPin = clickedPin;
                // Optional: visually highlight the selected pin here
            }
            else
            {
                // Second click
                if (_selectedPin != clickedPin && !WireExists(_selectedPin, clickedPin))
                {
                    // Prevent connecting to the same tool
                    if (_selectedPin.ParentTool != clickedPin.ParentTool)
                    {
                        CreateWire(_selectedPin, clickedPin);
                    }
                }

                // Reset selection
                // Optional: remove highlight from _selectedPin here
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
            Image lineImage = wireObj.GetComponent<Image>();

            GridWire newWire = new GridWire(start, end, lineImage);
            activeWires.Add(newWire);
        }

        public void RemoveWire(GridWire wire)
        {
            if (wire.LineImage != null) Destroy(wire.LineImage.gameObject);
            activeWires.Remove(wire);
        }

        public void ClearAllWires()
        {
            foreach (var wire in activeWires)
            {
                if (wire.LineImage != null) Destroy(wire.LineImage.gameObject);
            }
            activeWires.Clear();
            _selectedPin = null;
        }
    }
}
