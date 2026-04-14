using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<PropData> availableProps;

    [SerializeField] private Transform playerTransform; // Assign in inspector or find dynamically
    [SerializeField] private Camera mainCamera; // Assign in inspector

    [Header("Placement Settings")]
    [SerializeField] private float placementDistance = 2f;
    [SerializeField] private KeyCode placementKey = KeyCode.E;

    private GameObject currentPreviewObject;
    private PropData currentSelectedProp;
    private bool isPlacing = false;

    private void Start()
    {
        // Populate the inventory initially
        PopulateInventory();
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false); // Start closed
        }

        if (mainCamera == null) mainCamera = Camera.main;
        if (playerTransform == null && mainCamera != null) playerTransform = mainCamera.transform;
    }

    private void Update()
    {
        // Handle placement preview and final placement
        if (isPlacing && currentPreviewObject != null)
        {
            UpdatePreviewPosition();

            if (Input.GetKeyDown(placementKey))
            {
                PlaceItem();
            }
        }
        else if (!isPlacing && !inventoryPanel.activeSelf)
        {
            // Handle taking items back into inventory (destroying them)
            if (Input.GetKeyDown(placementKey))
            {
                TryTakeBackItem();
            }
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);

            // If we close the inventory while placing, cancel placement
            if (!inventoryPanel.activeSelf && isPlacing)
            {
                CancelPlacement();
            }
        }
    }

    private void PopulateInventory()
    {
        if (contentContainer == null || itemPrefab == null) return;

        // Clear existing items
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new items based on availableProps list
        foreach (PropData propData in availableProps)
        {
            GameObject itemObj = Instantiate(itemPrefab, contentContainer);
            InventoryItemUI itemUI = itemObj.GetComponent<InventoryItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(propData, this);
            }
            
            // Setup button click
            Button btn = itemObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => SelectItem(propData));
            }
        }
    }

    public void SelectItem(PropData propData)
    {
        currentSelectedProp = propData;
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false); // Close panel on selection
        }
        StartPlacement();
    }

    private void StartPlacement()
    {
        if (currentSelectedProp.prefab == null) return;

        isPlacing = true;

        // Destroy existing preview if any
        if (currentPreviewObject != null)
        {
            Destroy(currentPreviewObject);
        }

        // Instantiate preview object
        currentPreviewObject = Instantiate(currentSelectedProp.prefab);

        // Disable any existing colliders during preview to prevent it interacting while moving
        Collider[] colliders = currentPreviewObject.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        Rigidbody[] rbs = currentPreviewObject.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            rb.isKinematic = true; // Make kinematic during preview
            rb.detectCollisions = false; // Disable collision detection during preview
        }
    }

    private void UpdatePreviewPosition()
    {
        if (currentPreviewObject == null || mainCamera == null) return;

        // Position preview in front of the camera (center of the screen/crosshair)
        Vector3 targetPosition = mainCamera.transform.position + mainCamera.transform.forward * placementDistance;

        // Simple placement logic - just put it in front
        currentPreviewObject.transform.position = targetPosition;

        // Face the same direction as the camera
        currentPreviewObject.transform.rotation = mainCamera.transform.rotation;
    }

    private void PlaceItem()
    {
        if (currentPreviewObject == null) return;

        // Finalize placement

        // Re-enable physics (colliders)
        Collider[] colliders = currentPreviewObject.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
            // Unity 5+ requirement: MeshColliders must be convex if they have a non-kinematic Rigidbody attached
            if (col is MeshCollider meshCollider)
            {
                if (meshCollider.sharedMesh != null && meshCollider.sharedMesh.name == "Quad")
                {
                    // We'll handle replacing Quad meshes immediately below
                }
                else
                {
                    meshCollider.convex = true;
                }
            }
        }

        // Second pass: Replace invalid MeshColliders with BoxColliders
        MeshCollider[] meshColliders = currentPreviewObject.GetComponentsInChildren<MeshCollider>();
        foreach (var mc in meshColliders)
        {
            if (mc.sharedMesh != null && mc.sharedMesh.name == "Quad")
            {
                GameObject obj = mc.gameObject;
                DestroyImmediate(mc); // Must be DestroyImmediate so it's gone before physics update
                obj.AddComponent<BoxCollider>();
            }
        }

        // Restore original Rigidbody state (they were made kinematic during preview)
        // We look at the original prefab to see what the intended state was
        Rigidbody[] prefabRbs = currentSelectedProp.prefab.GetComponentsInChildren<Rigidbody>();
        Rigidbody[] currentRbs = currentPreviewObject.GetComponentsInChildren<Rigidbody>();

        for (int i = 0; i < currentRbs.Length; i++)
        {
            // Reset velocity and angular velocity to prevent explosive physics forces
            // caused by enabling colliders that might be slightly intersecting with other objects
            currentRbs[i].velocity = Vector3.zero;
            currentRbs[i].angularVelocity = Vector3.zero;
            currentRbs[i].detectCollisions = true; // Re-enable collision detection

            if (i < prefabRbs.Length)
            {
                // Restore the original isKinematic state from the prefab
                currentRbs[i].isKinematic = prefabRbs[i].isKinematic;
            }
            else
            {
                currentRbs[i].isKinematic = false;
            }
        }

        // Add PlacedProp component to identify it as a pick-up-able item
        currentPreviewObject.AddComponent<PlacedProp>();

        // IMPORTANT: Let colliders properly enable before removing them from "Ignore Raycast" or whatever layer they might be
        // Let's make sure the object is on the Default layer (or whatever the prefab's original layer is)
        currentPreviewObject.layer = currentSelectedProp.prefab.layer;
        foreach (Transform child in currentPreviewObject.transform)
        {
            child.gameObject.layer = child.gameObject.layer; // Just triggering layer update
        }

        // Reset state
        isPlacing = false;
        currentPreviewObject = null;
        currentSelectedProp = null;
    }

    private void TryTakeBackItem()
    {
        if (mainCamera == null) return;

        // Raycast from center of the screen
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, placementDistance))
        {
            // Check if we hit a placed prop
            PlacedProp prop = hit.collider.GetComponentInParent<PlacedProp>();
            if (prop != null)
            {
                // Destroy the object
                Destroy(prop.gameObject);

                // Optionally: You could also add it back to inventory data here if your inventory had a limited count
                // But right now the inventory seems to just be a permanent list of "availableProps"
            }
        }
    }

    private void CancelPlacement()
    {
        isPlacing = false;
        if (currentPreviewObject != null)
        {
            Destroy(currentPreviewObject);
            currentPreviewObject = null;
        }
        currentSelectedProp = null;
    }
}
