using System.Collections.Generic;
using UnityEngine;

/// Owns the cards currently in hand: spawning their visuals, tracking them,
/// and clearing them at end of turn.
///
/// Holds CardInstance, not CardData: each physical card carries its own
/// usesRemaining while CardData is the shared definition.
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

    /// End of turn: the ENTIRE hand goes, played or not (2-9). Returns each
    /// instance to the deck rather than destroying it - the visual is disposable,
    /// the instance is not, and a card with 2 uses left must come back around.
    public void DiscardAllTo(DeckManager deck)
    {
        // Backwards: removing from a list while walking it forwards skips
        // elements, which here would leave orphaned card objects on screen.
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            deck.Discard(spawned[i].Instance);
            RemoveCard(spawned[i]);
        }
    }
}