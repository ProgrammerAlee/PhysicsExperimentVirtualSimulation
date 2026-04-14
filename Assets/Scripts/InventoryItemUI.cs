using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    private PropData propData;
    private InventoryView inventoryView;

    public void Setup(PropData data, InventoryView view)
    {
        propData = data;
        inventoryView = view;
        iconImage.sprite = data.icon;
        nameText.text = data.propName;
    }

    public void OnClick()
    {
        inventoryView.SelectItem(propData);
    }
}
