using UnityEngine;
using UnityEngine.InputSystem;   // <- new

/// Temporary diagnostic for the screen -> world -> collider path.
/// Delete once CardDragHandler does this for real.
public class DropProbe : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask dropTargetMask;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        // Mouse.current is null if there's no mouse attached (or in some
        // editor states), so guard it rather than letting it throw.
        if (Mouse.current == null) return;

        // wasPressedThisFrame is the new-system equivalent of
        // Input.GetMouseButtonDown(0) - true only on the frame of the press.
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        // ReadValue() returns a Vector2 in screen pixels, same space as
        // the old Input.mousePosition and same space as PointerEventData.position.
        Vector2 screenPos = Mouse.current.position.ReadValue();

        Vector3 world3 = cam.ScreenToWorldPoint(screenPos);
        Vector2 world = new Vector2(world3.x, world3.y);

        Collider2D hit = Physics2D.OverlapPoint(world, dropTargetMask);

        Debug.Log($"screen ({screenPos.x:F0}, {screenPos.y:F0})  ->  " +
                  $"world ({world.x:F2}, {world.y:F2})  ->  " +
                  $"hit: {(hit != null ? hit.name : "nothing")}");
    }
}