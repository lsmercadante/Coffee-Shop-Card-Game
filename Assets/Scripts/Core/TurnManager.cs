
using UnityEngine;

/// Owns the shift: which turn it is, how much energy is left, and how many
/// coins. Calls into the line, the deck and the hand; none of them call back.
///
/// Coins live here for now because nothing else owns them. Phase 3 adds
/// income and may want a separate scoring class; until then a second class
/// holding one int is ceremony.
public class TurnManager : MonoBehaviour
{
    /// Scene singleton, same reasoning as PlayController: there is one shift
    /// and it must not be duplicated. CardVisual needs it to know what it can
    /// afford, and cards are spawned at runtime so they cannot be wired by hand.
    public static TurnManager Instance { get; private set; }

    [SerializeField] private CustomerLine line;
    [SerializeField] private DeckManager deck;
    [SerializeField] private HandManager hand;

    [Tooltip("Serialized rather than const, per 2-15 - the small " +
             "configuration is still useful as a tutorial.")]
    [SerializeField] private int energyPerTurn = 6;

    public int Turn { get; private set; }
    public int Energy { get; private set; }
    public int Coins { get; private set; }
    public int EnergyPerTurn => energyPerTurn;

    /// Fired whenever Energy changes. EnergyDisplay listens; card affordability
    /// dimming can too.
    public event System.Action EnergyChanged;

    private void Awake() => Instance = this;

    private void Start()
    {
        Turn = 1;
        line.OpenShift(Turn);
        deck.DealTo(hand);
        SetEnergy(energyPerTurn);
    }

    /// The End Turn button calls this. Everything below CLOSES turn N.
    public void EndTurn()
    {
        // Patience ticks last thing in the turn, so a customer who arrived
        // this turn has had the whole of it before losing anything.
        Coins -= line.TickAll();

        // The entire hand goes, played or not (2-9). Before the next draw,
        // or DealTo finds a full hand and deals nothing.
        hand.DiscardAllTo(deck);

        BeginTurn();
    }

    /// Everything below OPENS turn N+1.
    private void BeginTurn()
    {
        Turn++;

        line.FillToCapacity(Turn);   // Arrivals - the ONLY place seats fill
        deck.DealTo(hand);           // Draw
        SetEnergy(energyPerTurn);

        Debug.Log($"Turn {Turn}: {line.Describe()}   energy {Energy}   coins {Coins}");
    }

    public bool CanAfford(int cost) => cost <= Energy;

    /// Returns false and spends nothing when it can't be afforded, so the
    /// caller can just bail. Never let a caller decrement Energy directly -
    /// EnergyChanged would not fire and the display would silently drift.
    public bool TrySpend(int cost)
    {
        if (!CanAfford(cost)) return false;

        SetEnergy(Energy - cost);
        return true;
    }

    private void SetEnergy(int value)
    {
        Energy = value;
        EnergyChanged?.Invoke();
    }
    [SerializeField] private int discardCost = 1;

    public bool CanDiscard(CardVisual card) => card != null && CanAfford(discardCost);

    /// The card must already be deselected - RemoveCard destroys it, and
    /// PlayController.ClearSelection would then be touching a dead object.
    public bool TryDiscardAndRedraw(CardVisual card)
    {
        if (card == null) return false;
        if (!TrySpend(discardCost)) return false;

        // Discard, not Spend: the card keeps its uses and comes back around.
        // Only pouring consumes a dose.
        deck.Discard(card.Instance);
        hand.RemoveCard(card);
        deck.DealTo(hand);          // refills to the LIMIT, not by one

        return true;
    }
}