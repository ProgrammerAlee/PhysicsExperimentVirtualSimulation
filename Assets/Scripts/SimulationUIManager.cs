using UnityEngine;
using UnityEngine.UI;

public class SimulationUIManager : MonoBehaviour
{
    public GameObject mainPageUI;
    public GameObject toolSelectionUI;
    public GameObject inventoryPanel;

    private bool isInventoryOpen = false;

    void Start()
    {
        ShowMainPage();
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    public void ShowMainPage()
    {
        mainPageUI.SetActive(true);
        toolSelectionUI.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        isInventoryOpen = false;
        
        SetCursorState(true);
    }

    public void StartSimulation()
    {
        mainPageUI.SetActive(false);
        toolSelectionUI.SetActive(true);
        
        SetCursorState(false);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mainPageUI.activeSelf)
            {
                StartSimulation();
            }
            else
            {
                ShowMainPage();
            }
        }
        
        // Tab key to toggle inventory panel
        if (!mainPageUI.activeSelf && Input.GetKeyDown(KeyCode.Tab))
        {
            isInventoryOpen = !isInventoryOpen;
            if (inventoryPanel != null) inventoryPanel.SetActive(isInventoryOpen);
            SetCursorState(isInventoryOpen || Input.GetKey(KeyCode.LeftAlt));
        }

        // Toggle cursor lock when holding Alt
        if (!mainPageUI.activeSelf && !isInventoryOpen)
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                SetCursorState(true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                SetCursorState(false);
            }
        }
    }

    private void SetCursorState(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
        
        // Disable/Enable FirstPersonController scripts
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();
        foreach(var script in scripts)
        {
            if (script.GetType().Name.Contains("FirstPersonController"))
            {
                script.enabled = !show;
            }
        }
    }
}
