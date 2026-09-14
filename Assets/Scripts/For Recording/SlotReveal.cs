using UnityEngine;

/// TEMPORARY - for recording only. Blinks a customer slot's blockout square a
/// set number of times, then cross-fades to the character sprite. One script
/// rather than two takes, so the handoff is seamless and repeatable.
/// Delete once the customer prefabs from 2-4 exist.
public class SlotReveal : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("The blockout square. Defaults to this object's own renderer.")]
    [SerializeField] private SpriteRenderer blockout;

    [Tooltip("A child object holding the character sprite. Start it at alpha 0 " +
             "so nothing shows until the reveal.")]
    [SerializeField] private SpriteRenderer character;

    [Header("Blink")]
    [SerializeField] private float blinksPerSecond = 1.5f;
    [SerializeField] private int blinkCount = 3;
    [SerializeField] private float minAlpha = 0.2f;

    [Tooltip("Off gives a hard on/off, which suits pixel art. On fades.")]
    [SerializeField] private bool smoothBlink = false;

    [Header("Reveal")]
    [SerializeField] private float holdBeforeReveal = 0.3f;
    [SerializeField] private float fadeDuration = 0.8f;

    [Tooltip("On: the blockout fades out as the character fades in. " +
             "Off: blockout stays and the character fades in over it.")]
    [SerializeField] private bool fadeOutBlockout = true;

    private float timer;
    private bool revealing;

    private void Awake()
    {
        if (blockout == null) blockout = GetComponent<SpriteRenderer>();
        if (character != null) SetAlpha(character, 0f);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float blinkPhase = blinkCount / blinksPerSecond;

        if (!revealing && timer < blinkPhase)
        {
            Blink();
            return;
        }

        if (!revealing)
        {
            // Blinking is done. Snap the blockout back to full so the fade
            // starts from a known state rather than mid-blink.
            SetAlpha(blockout, 1f);
            revealing = true;
            timer = 0f;
            return;
        }

        Reveal();
    }

    private void Blink()
    {
        float phase = timer * blinksPerSecond;

        float t = smoothBlink
            ? (Mathf.Sin(phase * Mathf.PI * 2f) + 1f) * 0.5f
            : (Mathf.Repeat(phase, 1f) < 0.5f ? 1f : 0f);

        SetAlpha(blockout, Mathf.Lerp(minAlpha, 1f, t));
    }

    private void Reveal()
    {
        // Beat of stillness before the fade - without it the reveal reads as
        // part of the blink rather than as a separate moment.
        if (timer < holdBeforeReveal) return;

        float t = Mathf.Clamp01((timer - holdBeforeReveal) / fadeDuration);

        // SmoothStep so the fade eases rather than arriving at full speed.
        float p = Mathf.SmoothStep(0f, 1f, t);

        if (character != null) SetAlpha(character, p);
        if (fadeOutBlockout) SetAlpha(blockout, 1f - p);

        if (t >= 1f) enabled = false;   // done; stop running
    }

    private static void SetAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}
