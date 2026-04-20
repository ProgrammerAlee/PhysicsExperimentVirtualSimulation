# Distance-Based Snapping Implementation Plan

## Overview
Currently, the grid snapping relies on Unity's built-in drag-and-drop event interfaces (`IDropHandler`) combined with an explicit tiny hitbox (`20x20` target inside a `50x50` cell). This requires the mouse to be directly over the tiny center of the cell for a tool drop to succeed.

We will replace this with a forgiving, distance-based mathematical approach. When the tool is dropped, we will calculate the distance from the tool's position to all available slot positions on the canvas, and snap it to the closest valid (unoccupied) slot within a defined radius.

## Steps

### 1. Update `GridManager.cs` to track all slots
Currently, `GridManager` only tracks `placedTools`. It needs to maintain a list of all available `GridSlotUI` objects to calculate distances efficiently.

**Changes:**
- Add a new field: `public List<GridSlotUI> allSlots = new List<GridSlotUI>();`
- In `Start()`, populate `allSlots` by finding all `GridSlotUI` components in the grid container:
  ```csharp
  allSlots.AddRange(gridContainer.GetComponentsInChildren<GridSlotUI>());
  ```
- Add a helper method to find the closest valid slot:
  ```csharp
  public GridSlotUI GetClosestValidSlot(Vector2 screenPosition, float maxDistanceRadius)
  {
      GridSlotUI closestSlot = null;
      float closestDistance = maxDistanceRadius;

      foreach (var slot in allSlots)
      {
          if (slot.IsOccupied) continue;

          // Convert slot world position to screen position for accurate distance checking
          Vector2 slotScreenPos = RectTransformUtility.WorldToScreenPoint(mainCanvas.worldCamera, slot.transform.position);
          float dist = Vector2.Distance(screenPosition, slotScreenPos);

          if (dist < closestDistance)
          {
              closestDistance = dist;
              closestSlot = slot;
          }
      }

      return closestSlot;
  }
  ```

### 2. Update `GridSlotUI.cs`
Remove the Unity UI drop handler logic, as the snapping logic will be driven by the tool being dragged.

**Changes:**
- Remove `IDropHandler` interface from the class declaration.
- Remove the `OnDrop(PointerEventData eventData)` method entirely.

### 3. Update `GridToolUI.cs`
Change the `OnEndDrag` logic to find the closest slot and handle snapping.

**Changes:**
- In `OnEndDrag`, instead of just checking `transform.parent == _canvas.transform` and returning to original, we ask `GridManager` for the closest slot.
  ```csharp
  public void OnEndDrag(PointerEventData eventData)
  {
      _canvasGroup.blocksRaycasts = true;

      // Distance-based snapping logic
      float snapRadius = 40f; // Configurable radius
      GridSlotUI closestSlot = GridManager.Instance.GetClosestValidSlot(eventData.position, snapRadius);

      if (closestSlot != null)
      {
          // Found a slot nearby! Occupy it.
          closestSlot.OccupySlot(this);
      }
      else
      {
          // No slot found in radius, return to origin
          ReturnToOriginal();
      }

      if (GridWireManager.Instance != null)
      {
          GridWireManager.Instance.UpdateWiresForTool(this);
      }
  }
  ```

### 4. Cleanup `GridSystemUIBuilder.cs` (Optional but recommended)
The tiny 20x20 hitboxes are no longer needed since we aren't relying on physics raycasts / UI raycasts for the drop target.

**Changes:**
- The 20x20 `DropTarget` child object logic can be simplified, removing the need for `Image` components with transparent colors just to catch UI raycasts.
- The `GridSlotUI` component can be attached directly to the `DotSlot_{i}` GameObject or kept on the `DropTarget` GameObject, but the rigid size delta won't strictly dictate drop behavior anymore.

## Summary of File Modifications
- **GridManager.cs**: Add list to store all slots and a `GetClosestValidSlot` method.
- **GridSlotUI.cs**: Remove `IDropHandler` and `OnDrop` method.
- **GridToolUI.cs**: Update `OnEndDrag` to call `GetClosestValidSlot` and trigger `OccupySlot`.
