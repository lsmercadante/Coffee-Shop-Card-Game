using UnityEngine;

public class DrinkIcons : MonoBehaviour
{
    [SerializeField] private SpriteRenderer preferredIcon;
    [SerializeField] private SpriteRenderer acceptedIcon;
    [SerializeField] private TooltipTrigger preferredTooltip;
    [SerializeField] private TooltipTrigger acceptedTooltip;

    [Range(0f, 1f)]
    [SerializeField] private float acceptedAlpha = 0.8f;

    public void Show(RecipeData preferred, RecipeData accepted)
    {
        bool hasFallback = accepted != null;

        preferredIcon.sprite = preferred != null ? preferred.icon : null;
        preferredTooltip.SetText(preferred != null ? preferred.drinkName : "", "Wants this");

        // A missing icon renders nothing, which looks exactly like a
        // no-fallback customer rather than like missing art. Say so.
        if (preferred != null && preferred.icon == null)
            Debug.LogWarning($"{preferred.drinkName} has no icon assigned.", preferred);

        // The column is top-aligned and the positions are fixed in the
        // prefab, so hiding the accepted icon leaves a visible empty slot
        // underneath. That gap is the tell for a no-fallback customer -
        // centring a lone icon instead would read as "has one drink" rather
        // than "has no fallback", which is the wrong thing to learn at a
        // glance about Skye and Gabe.
        acceptedIcon.gameObject.SetActive(hasFallback);

        if (hasFallback)
        {
            acceptedIcon.sprite = accepted.icon;

            // Dimmed rather than smaller: a 16px icon scaled to 12 resamples
            // off the pixel grid, the same problem the fan tilt has.
            Color c = acceptedIcon.color;
            c.a = acceptedAlpha;
            acceptedIcon.color = c;

            acceptedTooltip.SetText(accepted.drinkName, "Will settle for this");
        }
    }
}
