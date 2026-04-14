using UnityEngine;

[CreateAssetMenu(fileName = "NewPropData", menuName = "Inventory/Prop Data")]
public class PropData : ScriptableObject
{
    public string propName;
    public GameObject prefab;
    public Sprite icon;
}
