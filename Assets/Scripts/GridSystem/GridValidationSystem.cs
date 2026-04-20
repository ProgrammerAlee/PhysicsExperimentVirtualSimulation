using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GridSystem
{
    public class RequiredConnection
    {
        public string SourceToolName;
        public int SourcePin;
        public string TargetToolName;
        public int? TargetPin;

        public bool Matches(string t1Name, int p1, string t2Name, int p2)
        {
            bool matchA = (SourceToolName == t1Name && SourcePin == p1 && TargetToolName == t2Name && (!TargetPin.HasValue || TargetPin.Value == p2));
            bool matchB = (SourceToolName == t2Name && (!TargetPin.HasValue || TargetPin.Value == p1) && TargetToolName == t1Name && SourcePin == p2);
            return matchA || matchB;
        }

        public override string ToString()
        {
            string targetPinStr = TargetPin.HasValue ? $"Pin {TargetPin.Value}" : "Any Pin";
            return $"{SourceToolName}(Pin {SourcePin}) <-> {TargetToolName}({targetPinStr})";
        }
    }

    public class GridValidationSystem : MonoBehaviour
    {
        public static GridValidationSystem Instance { get; private set; }

        public string csvFilePath = "Config/工具接线端连接表"; // Path inside Resources

        private List<RequiredConnection> _requiredConnections = new List<RequiredConnection>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            LoadCSV();
        }

        public void LoadCSV()
        {
            _requiredConnections.Clear();
            TextAsset csvData = Resources.Load<TextAsset>(csvFilePath);

            if (csvData == null)
            {
                Debug.LogError($"Could not find CSV at Resources/{csvFilePath}");
                return;
            }

            // Read GBK encoded CSV
            // Note: Unity's TextAsset.text might mess up GBK. Better to read bytes.
            Encoding gbk = null;
            try
            {
                // Try to register CodePagesEncodingProvider if available at runtime.
                // Avoid a compile-time dependency on System.Text.Encoding.CodePages
                var providerType = Type.GetType("System.Text.CodePagesEncodingProvider, System.Text.Encoding.CodePages")
                                   ?? Type.GetType("System.Text.CodePagesEncodingProvider");
                if (providerType != null)
                {
                    var instanceProp = providerType.GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    var providerInstance = instanceProp?.GetValue(null);
                    if (providerInstance != null)
                    {
                        var registerMethod = typeof(Encoding).GetMethod("RegisterProvider", new Type[] { typeof(EncodingProvider) });
                        registerMethod?.Invoke(null, new[] { providerInstance });
                    }
                }

                gbk = Encoding.GetEncoding("GBK");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to get GBK encoding. Falling back to default/UTF8. Error: {ex.Message}");
                gbk = Encoding.Default;
            }

            string text = gbk.GetString(csvData.bytes);

            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) return;

            // Start from 1 to skip header
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] cols = line.Split(',');

                string sourceToolName = cols[0].Trim();

                // Columns 1, 2, 3, 4 map to Pin 0, 1, 2, 3
                for (int pinIndex = 0; pinIndex < 4; pinIndex++)
                {
                    int colIndex = pinIndex + 1;
                    if (colIndex < cols.Length && !string.IsNullOrWhiteSpace(cols[colIndex]))
                    {
                        string targetToolName = cols[colIndex].Trim();

                        // Check if we already have a reverse connection
                        bool foundExisting = false;
                        foreach (var existingReq in _requiredConnections)
                        {
                            if (existingReq.TargetToolName == sourceToolName && existingReq.SourceToolName == targetToolName)
                            {
                                // We found a reverse connection. Update its TargetPin
                                // e.g., we already had B -> A, and now we see A(pinX) -> B.
                                // We can update B -> A to specify TargetPin = pinX
                                // This works because B's SourcePin is already set.
                                // It effectively becomes B(existingSourcePin) <-> A(pinX)
                                if (!existingReq.TargetPin.HasValue)
                                {
                                    existingReq.TargetPin = pinIndex;
                                    foundExisting = true;
                                    break;
                                }
                            }
                        }

                        if (!foundExisting)
                        {
                            var req = new RequiredConnection
                            {
                                SourceToolName = sourceToolName,
                                SourcePin = pinIndex,
                                TargetToolName = targetToolName,
                                TargetPin = null
                            };
                            _requiredConnections.Add(req);
                        }
                    }
                }
            }
            Debug.Log($"Loaded {_requiredConnections.Count} required connections from CSV.");
        }

        public bool ValidateCircuit()
        {
            List<GridWire> actualWires = GridWireManager.Instance.activeWires;

            if (actualWires.Count != _requiredConnections.Count)
            {
                Debug.Log($"Validation Failed: Expected {_requiredConnections.Count} wires, but found {actualWires.Count}.");
                return false;
            }

            List<RequiredConnection> unmatchedReqs = new List<RequiredConnection>(_requiredConnections);

            foreach (var wire in actualWires)
            {
                if (wire.StartPin == null || wire.EndPin == null)
                {
                    Debug.Log($"Validation Failed: Wire connected to an invalid pin.");
                    return false;
                }

                string t1Name = wire.StartPin.ParentTool.Data.toolName;
                int p1 = wire.StartPin.PinIndex;
                string t2Name = wire.EndPin.ParentTool.Data.toolName;
                int p2 = wire.EndPin.PinIndex;

                bool matched = false;
                for (int i = 0; i < unmatchedReqs.Count; i++)
                {
                    if (unmatchedReqs[i].Matches(t1Name, p1, t2Name, p2))
                    {
                        unmatchedReqs.RemoveAt(i);
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    Debug.Log($"Validation Failed: Incorrect connection {t1Name}(Pin {p1}) <-> {t2Name}(Pin {p2})");
                    return false;
                }
            }

            // Check for extra unconnected tools (tools that exist but aren't in any required connection)
            var placedTools = GridManager.Instance.placedTools.Values;
            foreach (var tool in placedTools)
            {
                bool toolIsInConnection = false;
                foreach (var req in _requiredConnections)
                {
                    if (req.SourceToolName == tool.Data.toolName || req.TargetToolName == tool.Data.toolName)
                    {
                        toolIsInConnection = true;
                        break;
                    }
                }

                if (!toolIsInConnection)
                {
                    Debug.Log($"Validation Failed: Extra tool found {tool.Data.toolName}");
                    return false;
                }
            }

            Debug.Log("Validation Passed! Circuit is correct.");
            return true;
        }
    }
}
