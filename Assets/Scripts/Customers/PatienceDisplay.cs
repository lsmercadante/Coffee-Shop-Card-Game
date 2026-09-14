using UnityEngine;

public class PatienceDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer track;
    [SerializeField] private SpriteRenderer fill;

    [Tooltip("Bar width per turn, in px. 6 keeps Leo's 4-turn bar at 24px, " +
             "inside the 30px the layout budgets, and every state on a whole " +
             "pixel.")]
    [SerializeField] private float pixelsPerTurn = 6f;
    [SerializeField] private float pixelsPerUnit = 32f;

    [SerializeField] private Color high = new Color(0.45f, 0.80f, 0.40f);
    [SerializeField] private Color mid  = new Color(0.95f, 0.75f, 0.30f);
    [SerializeField] private Color low  = new Color(0.90f, 0.35f, 0.35f);

    public void Set(int remaining, int max)
    {
        // Both renderers are left-pivoted, so they grow rightward from a
        // shared origin. Shift that origin left by half the track's width to
        // keep the bar centred under the head whatever max is.
        float left = -(max * pixelsPerTurn) * 0.5f / pixelsPerUnit;
        track.transform.localPosition = new Vector3(left, 0f, 0f);
        fill.transform.localPosition = new Vector3(left, 0f, 0f);

        // The sprite is 1px wide, so at 32 PPU a scale of N makes it N px.
        SetWidth(track, max * pixelsPerTurn);
        SetWidth(fill, Mathf.Max(0, remaining) * pixelsPerTurn);

        // Absolute thresholds, not a ratio of max. Diane at patience 2 opens
        // amber and never shows green - correct, since two turns IS her
        // whole clock, and a ratio would tell her she is fine.
        fill.color = remaining <= 1 ? low : remaining <= 2 ? mid : high;
    }

    private static void SetWidth(SpriteRenderer renderer, float pixels)
    {
        Vector3 scale = renderer.transform.localScale;
        scale.x = pixels;
        renderer.transform.localScale = scale;
    }
}