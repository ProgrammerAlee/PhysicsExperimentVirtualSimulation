using System;
using System.Collections.Generic;  
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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

        public string csvFilePath = "Config/CircuitValidation"; // Recommended path
        public Text resultText;

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
                Debug.LogWarning($"Could not find CSV at Resources/{csvFilePath}");
                return;
            }

            Encoding gbk = null;
            try
            {
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
            catch
            {
                gbk = Encoding.UTF8;
            }

            string text = gbk.GetString(csvData.bytes);
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1) return;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string[] cols = line.Split(',');

                string sourceToolName = cols[0].Trim();
                for (int pinIndex = 0; pinIndex < 4; pinIndex++)
                {
                    int colIndex = pinIndex + 1;
                    if (colIndex < cols.Length && !string.IsNullOrWhiteSpace(cols[colIndex]))
                    {
                        string targetToolName = cols[colIndex].Trim();
                        bool foundExisting = false;
                        foreach (var existingReq in _requiredConnections)
                        {
                            if (existingReq.TargetToolName == sourceToolName && existingReq.SourceToolName == targetToolName)
                            {
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
                            _requiredConnections.Add(new RequiredConnection
                            {
                                SourceToolName = sourceToolName,
                                SourcePin = pinIndex,
                                TargetToolName = targetToolName,
                                TargetPin = null
                            });
                        }
                    }
                }
            }
        }

        public void ValidateCircuit()
        {
            bool isValid = PerformValidation(out string message);
            if (resultText != null)
            {
                resultText.text = message;
                resultText.color = isValid ? Color.green : Color.red;
            }
            Debug.Log(message);
        }

        private bool PerformValidation(out string message)
        {
            List<GridWire> actualWires = GridWireManager.Instance.activeWires;

            if (actualWires.Count != _requiredConnections.Count)
            {
                message = $"验证失败: 期望 {_requiredConnections.Count} 根导线，实际发现 {actualWires.Count} 根。";
                return false;
            }

            List<RequiredConnection> unmatchedReqs = new List<RequiredConnection>(_requiredConnections);        

            foreach (var wire in actualWires)
            {
                if (wire.StartPin == null || wire.EndPin == null)
                {
                    message = "验证失败: 导线连接到了无效的针脚。";
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
                    message = $"验证失败: 错误的连接 {t1Name}(Pin {p1}) <-> {t2Name}(Pin {p2})";
                    return false;
                }
            }

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
                    message = $"验证失败: 发现多余的元件 {tool.Data.toolName}";
                    return false;
                }
            }

            message = "验证通过! 电路连接正确。";
            return true;
        }
    }
}
