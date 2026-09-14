using UnityEngine;
using UnityEngine.EventSystems;

/// Goes on the moving parts - the body sprite and the two icons - unlike the
/// drop collider, which stays on the spot. Hover should follow the eye, so a
/// hover target that walks in with the customer is correct.
[RequireComponent(typeof(Collider2D))]
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string title;
    private string body;

    public void SetText(string title, string body)
    {
        this.title = title;
        this.body = body;
    }

    // Null-checked because the triggers go on the prefab in 2-4, before the
    // panel exists in 2-4b. Without the raycaster these never fire anyway,
    // but a missing panel should not be a null reference if one does.
    public void OnPointerEnter(PointerEventData e)
    {
        if (TooltipPanel.Instance == null) return;
        // No offset: TooltipPanel places itself clear of this point using its
        // own measured size, which a fixed offset here could not do - the
        // panel is a different height for a drink than for a description.
        TooltipPanel.Instance.Show(title, body, transform.position);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (TooltipPanel.Instance == null) return;
        TooltipPanel.Instance.Hide();
    }
}