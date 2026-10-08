using UnityEngine;
using UnityEngine.InputSystem;

/// The second half of click-select-click. Attach to the same GameObject as
/// PlayController.
///
/// Separate from PlayController because it is an INPUT concern - if you later
/// add gamepad or touch, this is the only file that changes.
public class TargetClickRelay : MonoBehaviour
{
    private void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        
        var pc = PlayController.Instance;
        if (pc.Selected == null && pc.SelectedCup == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        DropTarget target = pc.TargetUnderScreenPoint(screenPos);
        if (target == null) return;

        if (pc.SelectedCup != null)
            pc.TryServe(pc.SelectedCup, target as CustomerSlot);
        else
            pc.TryPlay(pc.Selected, target);



    }
}
