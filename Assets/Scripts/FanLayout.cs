using UnityEngine;

/// Arranges hand cards along a shallow arc. Replaces HorizontalLayoutGroup -
/// you cannot run both, so disable or remove that component on HandPanel.
[ExecuteAlways]
public class FanLayout : MonoBehaviour
{
    [SerializeField] private float spacing = 82f;   // 72px slot + 10px gutter
    [SerializeField] private float arcHeight = 10f; // peak lift at the centre
    [SerializeField] private float tiltDegrees = 0f; // leave 0 for pixel art

    private void OnEnable() => Arrange();
    private void OnTransformChildrenChanged() => Arrange();

    public void Arrange()
    {
        int count = transform.childCount;
        if (count == 0) return;

        // Centre the row on the panel: with 6 cards, offsets run -2.5 .. +2.5
        float half = (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            var rect = transform.GetChild(i) as RectTransform;
            if (rect == null) continue;

            float offset = i - half;              // -2.5 .. +2.5 for six cards
            float normalised = half > 0f ? offset / half : 0f;   // -1 .. +1

            // A parabola peaking at the centre. Squaring the normalised
            // position means the outermost cards drop by the full arcHeight
            // and the middle ones barely move.
            float lift = arcHeight * (1f - normalised * normalised);

            rect.anchoredPosition = new Vector2(
             Mathf.Round(offset * spacing),
             Mathf.Round(lift)
);

            // Rotation is the thing that breaks pixel-perfect rendering. At 0 this is a
            // no-op and you get the arc silhouette with no sampling cost (option A).
            // Above about 8 degrees the artefacts stop being subtle (option B).
            rect.localRotation = Quaternion.Euler(0f, 0f, -normalised * tiltDegrees);
        }
    }
}
