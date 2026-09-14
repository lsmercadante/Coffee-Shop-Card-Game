using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// One panel, reused. Lives on the Canvas so it draws above the world and
/// above the cards, and so it inherits the pixel scale.
public class TooltipPanel : MonoBehaviour
{
    public static TooltipPanel Instance { get; private set; }

    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private RectTransform canvasRect;

    [Tooltip("Clearance in canvas px between the hovered thing and the panel.")]
    [SerializeField] private float gap = 4f;

    private void Awake()
    {
        Instance = this;
        if (worldCamera == null) worldCamera = Camera.main;
        panel.gameObject.SetActive(false);
    }

    public void Show(string title, string body, Vector3 worldAnchor)
    {
        if (string.IsNullOrEmpty(title)) return;

        titleText.text = title;
        bodyText.text = body;
        bodyText.gameObject.SetActive(!string.IsNullOrEmpty(body));
        panel.gameObject.SetActive(true);

        // Force the layout now: the clamp below needs the panel's real size,
        // and TMP will not have resized it until the next layout pass.
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        Vector2 screen = worldCamera.WorldToScreenPoint(worldAnchor);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screen, worldCamera, out Vector2 local);

        panel.anchoredPosition = Place(local);
    }

    public void Hide() => panel.gameObject.SetActive(false);

    /// Sit fully clear of the hovered point and fully on screen.
    private Vector2 Place(Vector2 anchor)
    {
        Vector2 size = panel.rect.size;
        Vector2 half = canvasRect.rect.size * 0.5f;

        // Above, by half the panel's own height plus a gap. Measured rather
        // than fixed, because a one-line drink tooltip and a two-line
        // description need different clearances - which is why TooltipTrigger
        // no longer carries an offset of its own.
        float y = anchor.y + size.y * 0.5f + gap;

        // Not enough headroom: flip underneath. Clamping instead would slide
        // the panel back down onto the thing it is describing.
        if (y + size.y * 0.5f > half.y)
            y = anchor.y - size.y * 0.5f - gap;

        float x = Mathf.Clamp(anchor.x, -half.x + size.x * 0.5f,
                                         half.x - size.x * 0.5f);
        y = Mathf.Clamp(y, -half.y + size.y * 0.5f, half.y - size.y * 0.5f);

        // Pixel-snap. A fractional anchoredPosition puts glyph edges between
        // screen pixels, which reads as a blurry tooltip next to crisp
        // everything-else.
        return new Vector2(Mathf.Round(x), Mathf.Round(y));
    }
}