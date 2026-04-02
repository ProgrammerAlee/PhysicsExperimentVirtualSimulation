using UnityEngine;
using UnityEngine.UI;

public class InventoryItemToggle : MonoBehaviour
{
    public GameObject targetObject;
    public Toggle toggle;

    void Start()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
            
            // Initialize toggle state based on object's active state
            if (targetObject != null)
            {
                toggle.isOn = targetObject.activeSelf;
            }
        }
    }

    void OnToggleChanged(bool isOn)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(isOn);
        }
    }
}
