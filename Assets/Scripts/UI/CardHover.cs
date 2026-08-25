using UnityEngine;
using UnityEngine.EventSystems;

/// Lifts a card when the pointer is over it. Moves CardContent rather than
/// the card root, because the Horizontal Layout Group owns the root's
/// position and will overwrite it on the next layout rebuild.
public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform content;

    [Tooltip("Pixels to lift. Whole numbers only - a card resting at y = 7.3 " +
             "samples its sprites off the pixel grid.")]
    [SerializeField] private float liftPixels = 8f;

    [Tooltip("0 snaps instantly. Around 0.08 reads as responsive without " +
             "feeling floaty.")]
    [SerializeField] private float duration = 0.08f;

    private Vector2 restPosition;
    private float t;        // 0 = resting, 1 = fully lifted
    private bool hovered;

    private void Awake()
    {
        if (content == null) content = transform.GetChild(0) as RectTransform;
        restPosition = content.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData e) => hovered = true;
    public void OnPointerExit(PointerEventData e) => hovered = false;

    private void Update()
    {
        // Move t toward its target rather than tracking elapsed time, so
        // entering and leaving mid-animation reverses smoothly instead of
        // snapping back to the start.
        float target = hovered ? 1f : 0f;

        if (Mathf.Approximately(t, target)) return;

        t = duration <= 0f
            ? target
            : Mathf.MoveTowards(t, target, Time.deltaTime / duration);

        // Round the final position so the card always lands on whole pixels.
        content.anchoredPosition = new Vector2(
            restPosition.x,
            Mathf.Round(restPosition.y + liftPixels * t)
        );
    }
}
