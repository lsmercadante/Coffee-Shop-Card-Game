using UnityEngine;

/// One arrival. Owns the runtime state a CustomerData asset must never hold:
/// how much patience THIS instance has left.
public class Customer : MonoBehaviour
{ [SerializeField] private SpriteRenderer body;

    [Tooltip("Stand-in for the Phase 5 impatient animation. SpriteRenderer." +
             "color MULTIPLIES, so a saturated red zeroes the green and blue " +
             "channels and the customer goes near-black - keep the wash pale.")]
    [SerializeField] private Color impatientTint = new Color(1f, 0.65f, 0.65f);
    [SerializeField] private Color normalTint = Color.white;
    [SerializeField] private DrinkIcons icons;      // preffered and accepted drink icons
    [SerializeField] private PatienceDisplay patienceDisplay;       // how much patience they have
    [SerializeField] private TooltipTrigger bodyTooltip;            // hover over the body to reveal info about the customer

    public CustomerData Data { get; private set; }
    public int PatienceRemaining { get; private set; }

    public void Initialize(CustomerData data)
    {
        Data = data;            // assigns data to the specific instance's customer data
        PatienceRemaining = data.patience;      // pulls out the patience value from customer data

        body.sprite = data.bodySprite;
        body.color = normalTint;
        if (data.bodySprite == null)
            Debug.LogWarning($"{data.customerName}: no body sprite assigned.", data);
        icons.Show(data.preferredDrink, data.acceptedDrink);            // showing icons of the preferred and accepted drinks
        patienceDisplay.Set(PatienceRemaining, data.patience);          //showing the patience remaining
        bodyTooltip.SetText(data.customerName, data.description);       // adding the customer name and description to the tooltip
    }

    /// Called once per turn by 2-16, at the close of the turn, for every
    /// slot occupant. Never called in the turn a customer arrives: Arrivals
    /// opens a turn and PatienceTick closes it, so patience always buys the
    /// full number of turns it says. Tick on arrival and a 2-patience
    /// customer gets one turn.
    ///
    /// Returns true when patience has run out - the caller's cue to remove
    /// them and charge walkoutPenalty.
    public bool Tick()
    {
        PatienceRemaining--;        // de-increments the patience remaining by one
        patienceDisplay.Set(PatienceRemaining, Data.patience);     // displays the new patience value
        
        // Last turn: wash them red. The pips say HOW MANY turns; this says
        // HOW WORRIED, which is the signal you get while reading your hand.
        body.color = PatienceRemaining == 1 ? impatientTint : normalTint;

        return PatienceRemaining <= 0;              // keeping track of when the patience remaining hits zero
    }



    /// Waiting customers show the person and nothing else. The pips and icons
    /// appearing is the signal that they are now on your clock - a cleaner tell
    /// than position alone, and it means a queued customer cannot be misread as
    /// one whose patience you have already spent.
    public void SetWaiting(bool waiting)
    {
        icons.gameObject.SetActive(!waiting);
        patienceDisplay.gameObject.SetActive(!waiting);
    }
}

