using System.Collections.Generic;
using UnityEngine;

/// Owns the cards currently in hand: spawning their visuals, tracking them,
/// and clearing them at end of turn.
///
/// Holds CardInstance, not CardData: each physical card carries its own
/// usesRemaining while CardData is the shared definition. Phase 2's
/// DeckManager becomes the supplier; nothing else changes.
public class HandManager : MonoBehaviour
{
    [SerializeField] private CardVisual cardPrefab;

    [Tooltip("Where cards are parented. Usually this object - the one with " +
             "the Horizontal Layout Group.")]
    [SerializeField] private Transform cardParent;

    [Tooltip("Serialized rather than const so the small configuration stays " +
             "usable as a tutorial, per 2-15.")]
    [SerializeField] private int handSize = 6;

    // The live card objects, in left-to-right order. HandManager owns these -
    // nothing else should Destroy them directly.
    private readonly List<CardVisual> spawned = new List<CardVisual>();

    public int Count => spawned.Count;
    public int HandSize => handSize;
    public bool IsFull => spawned.Count >= handSize;

    private void Awake()
    {
        if (cardParent == null) cardParent = transform;
    }

    /// Spawn one card and add it to the right-hand end of the hand.
    /// Returns the visual so the caller can wire it up if needed.
    public CardVisual AddCard(CardInstance instance)
    {
        if (IsFull)
        {
            Debug.LogWarning($"Hand is full ({handSize}); refusing {instance.data.cardName}");
            return null;
        }

        CardVisual card = Instantiate(cardPrefab, cardParent);
        card.Initialize(instance);

        // The Horizontal Layout Group positions it; we only decide the order.
        spawned.Add(card);
        return card;
    }

    /// Called by PlayController after a card is played, and by the end-of-turn
    /// discard. Removes from tracking AND destroys the object, so the two can
    /// never drift apart.
    public void RemoveCard(CardVisual card)
    {
        if (!spawned.Remove(card)) return;   // not ours; do nothing
        Destroy(card.gameObject);
    }

    /// End of turn: the ENTIRE hand is discarded, played or not (2-9).
    /// This is what makes the deck cycle roughly every four turns.
    public void ClearHand()
    {
        // Iterate backwards - removing from a list while walking it forwards
        // skips elements, which here would leave orphaned card objects.
        for (int i = spawned.Count - 1; i >= 0; i--)
            RemoveCard(spawned[i]);
    }

    /// Fill to handSize from a supplier. Phase 2's DeckManager becomes the
    /// supplier; for now 1-15 wraps a fixed test list.
    public void DealUpTo(IList<CardInstance> source)
    {
        int i = 0;
        while (!IsFull && i < source.Count)
            AddCard(source[i++]);
    }

    [Header("Phase 1 testing only")]
    [SerializeField] private bool dealTestHandOnStart = true;

    [Tooltip("Six cards to deal on Start so the layout can be checked before " +
             "DeckManager exists. Delete this and the Start method in Phase 2.")]
    [SerializeField] private List<CardData> testHand = new List<CardData>();

    private void Start()
    {
        if (!dealTestHandOnStart) return;

        for (int i = 0; i < testHand.Count; i++)
        {
            if (IsFull) break;

            CardInstance instance = new CardInstance(testHand[i]);

            // TEMPORARY: part-spend the first card so the pips can be checked
            // against a case that occurs constantly in play. Delete once verified.
            if (i == 1) instance.usesRemaining = 1;

            AddCard(instance);
        }
    }
}
