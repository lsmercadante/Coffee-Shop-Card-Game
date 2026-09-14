using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Renders any CardData onto the card prefab. One prefab, eleven possible
/// sprites - this is the script that makes that true.
public class CardVisual : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private Image background;
    [SerializeField] private Image artwork;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject selectedOutline;

    [Tooltip("Four pip Images, left to right. Created in the prefab and " +
             "shown/hidden - never instantiated at runtime, because a card " +
             "is redrawn every time uses change and allocating per redraw " +
             "would churn the GC during a drag.")]
    [SerializeField] private Image[] pips;

    [Header("Pip colours")]
    [SerializeField] private Color pipFull = Color.white;
    [SerializeField] private Color pipSpent = new Color(1f, 1f, 1f, 0.25f);

    public CardInstance Instance { get; private set; }

    /// Passthrough so PlayController and DropTarget.CanAccept(CardData)
    /// need no changes at all.
    public CardData Data => Instance.data;

    public void Initialize(CardInstance instance)
    {
        Instance = instance;
        CardData data = instance.data;

        nameText.text = data.cardName;
        artwork.sprite = data.artwork;
        background.color = data.identityColor;
        costText.text = data.energyCost.ToString();

        // Actual remaining doses rather than assuming a fresh card - a card
        // drawn mid-shift may already be part-spent.
        SetUses(instance.usesRemaining, data.maxUses);
        SetSelected(false);
    }

    /// Show 'remaining' full pips out of 'max' total. Phase 2 calls this from
    /// CardInstance whenever a dose is spent.
    public void SetUses(int remaining, int max)
    {
        for (int i = 0; i < pips.Length; i++)
        {
            // Hide pips beyond this card's maximum entirely - a 1-use
            // chocolate should show ONE pip, not one full and three empty.
            bool exists = i < max;
            pips[i].gameObject.SetActive(exists);

            if (exists)
                pips[i].color = i < remaining ? pipFull : pipSpent;
            if (max > pips.Length)
                Debug.LogWarning($"{Data.cardName} has {max} uses but only {pips.Length} pips exist");
        }
        //Debug.Log($"{Data.cardName}: SetUses({remaining}, {max})");
    }

    public void SetSelected(bool on)
    {
        if (selectedOutline != null) selectedOutline.SetActive(on);
    }

}
