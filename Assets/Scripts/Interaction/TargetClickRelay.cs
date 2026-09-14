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
        if (pc.Selected == null) return;   // nothing selected, nothing to place

        Vector2 screenPos = Mouse.current.position.ReadValue();
        DropTarget target = pc.TargetUnderScreenPoint(screenPos);

        if (target == null) return;        // clicked empty space; keep selection

        pc.TryPlay(pc.Selected, target);
    }
}
