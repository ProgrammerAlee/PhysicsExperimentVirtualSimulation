using UnityEngine;

namespace GridSystem
{
    [CreateAssetMenu(fileName = "NewGridTool", menuName = "GridSystem/GridToolData")]
    public class GridToolData : ScriptableObject
    {
        public string id;
        public string toolName;
        public Sprite icon;
        public int pinCount = 2; // Number of connection pins this tool has
    }
}
