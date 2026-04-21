using UnityEngine;
using System.Collections.Generic;

namespace GridSystem
{
    [System.Serializable]
    public class PinDefinition
    {
        public string name;
        public Vector2 normalizedPosition; // (-0.5, -0.5) to (0.5, 0.5)
    }

    [CreateAssetMenu(fileName = "NewGridTool", menuName = "GridSystem/GridToolData")]
    public class GridToolData : ScriptableObject
    {
        public string id;
        public string toolName;
        public Sprite icon;
        public int pinCount = 2; // Keep for backward compatibility or derivation
        public List<PinDefinition> pinDefinitions = new List<PinDefinition>();
    }
}
