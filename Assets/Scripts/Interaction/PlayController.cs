using UnityEngine;

/// The single code path every card play goes through, whether the player
/// dragged or click-selected. This dictates the rules, CardDragHandler controls the gesture.
///
/// A scene singleton because there is one set of rules and they do not need to be duplicated
/// across two input styles
public class PlayController : MonoBehaviour
{
    public static PlayController Instance { get; private set; }

    [SerializeField] private Camera worldCamera;
    [SerializeField] private LayerMask dropTargetMask;

    [Tooltip("The HandManager on HandPanel. Removal routes through it rather " +
             "than letting cards destroy themselves.")]
    [SerializeField] private HandManager hand;
    [SerializeField] private DeckManager deck;
    [SerializeField] private TurnManager turns;

    private CardVisual selected;  // for assigning which card is selected 
    private DropTarget[] allTargets;        // needs to be aware of all drop targets

    public CardVisual Selected => selected;  // so that other scripts can read what is selected

    private void Awake()
    {
        Instance = this;
        if (worldCamera == null) worldCamera = Camera.main;
        RefreshTargets();
    }

    /// Phase 2 spawns customers at runtime, so this needs calling on arrival.
    public void RefreshTargets()
    {
        allTargets = FindObjectsByType<DropTarget>(FindObjectsSortMode.None);       // finds all targets on the DropTarget Layer
    }

    /// Screen point -> world point -> collider. The path SpaceProbe proved.
    public DropTarget TargetUnderScreenPoint(Vector2 screenPos)
    {
        Vector3 world3 = worldCamera.ScreenToWorldPoint(screenPos);
        Vector2 world = new Vector2(world3.x, world3.y);

        Collider2D hit = Physics2D.OverlapPoint(world, dropTargetMask); // checks if world point and dropTarget overlap
        return hit != null ? hit.GetComponent<DropTarget>() : null;  // if there is a hit we return the drop target
    }

    public void SelectCard(CardVisual card)
    {
        // Clicking the selected card again deselects it.
        if (selected == card) { ClearSelection(); return; }

        if (selected != null) selected.SetSelected(false);

        selected = card;
        if (selected != null) selected.SetSelected(true);

        RefreshHighlights();
    }

    public void ClearSelection()
    {
        if (selected != null) selected.SetSelected(false);
        selected = null;
        RefreshHighlights();
    }

    /// The ONE place a card is ever played. Returns false if illegal, which
    /// is the drag handler's cue to send the card home.
    public bool TryPlay(CardVisual card, DropTarget target)
    {
        if (card == null || target == null) return false; // card and target must both exist
        if (!target.IsActive) return false;                 //target must be active
        if (!target.CanAccept(card.Data)) return false;    // the play must be valid

        if (!turns.TrySpend(card.Data.energyCost)) return false;

        target.Receive(card.Instance);                      // target recieves an instance of the card

        deck.Discard(card.Instance);     // discards, or drops it if Spend took the last use
        hand.RemoveCard(card);

        selected = null;      // the card is gone; do not call SetSelected on it
        RefreshHighlights();
        return true;
    }

    /// Light every target that would accept the card in play. The set is tiny
    /// (3 cups + up to 6 customers), so brute force is fine - and this is most
    /// of what makes the interaction feel responsive.
    public void RefreshHighlights()
    {
        CardData card = selected != null ? selected.Data : null;

        foreach (DropTarget t in allTargets)
        {
            bool valid = card != null && t.IsActive && t.CanAccept(card);
            t.SetHighlight(valid);
        }
    }

    /// Called by CardDragHandler on pickup so targets light during a drag too.
    public void BeginDragHighlight(CardVisual card)
    {
        selected = card;
        RefreshHighlights();
    }
}
