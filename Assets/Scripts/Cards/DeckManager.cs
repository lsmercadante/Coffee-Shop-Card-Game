using System.Collections.Generic;
using UnityEngine;

/// Owns the draw and discard piles. Does NOT own the hand - HandManager does,
/// and two lists claiming to be the hand is how they drift apart. Cards leave
/// here into the hand and come back as instances, never as copies.
public class DeckManager : MonoBehaviour
{
    [SerializeField] private List<DeckEntry> composition = new List<DeckEntry>();   // defines which cards are in the deck and how many of each

    [Tooltip("0 = random each run. Any other value replays the exact same " +
             "shuffle. Its own seed, separate from CustomerSpawner's - one " +
             "shared stream means reshuffling the deck changes who walks in.")]
    [SerializeField] private int seed = 0;      // want hands to be seeded so they can be tried again during play testing

    private readonly List<CardInstance> drawPile = new List<CardInstance>();        //list of card instances in the draw pile
    private readonly List<CardInstance> discardPile = new List<CardInstance>();     // list of card instances in the discard pile

    private System.Random rng;      // for the seeding

    // these two are used for display also
    public int DrawCount => drawPile.Count;     // how many cards are in the draw pile
    public int DiscardCount => discardPile.Count;           // how many cards are in the discard pile

    /// Cards still in the shift, in either pile. This is the number that
    /// shrinks - DrawCount alone jumps back up on every reshuffle.
    public int CardsRemaining => drawPile.Count + discardPile.Count;

    private void Awake()
    {
        int actualSeed = seed != 0 ? seed : System.Environment.TickCount;       // pulls a seed
        rng = new System.Random(actualSeed);                                    // sets the seed
        Debug.Log($"DeckManager seed {actualSeed}");                            // prints the seed

        Build();                // builds the draw pile and discard pile
    }

    /// One CardInstance per copy. Each carries its own usesRemaining, which is
    /// why two Espressos in hand can show different pips.
    private void Build()
    {
        drawPile.Clear();           // clears the draw pile
        discardPile.Clear();            // clears the discard pile

        foreach (DeckEntry entry in composition)        // for each type of card
        {
            if (entry.card == null)
            {
                Debug.LogError("DeckManager: composition has an empty card slot.", this);
                continue;
            }
            // adds an instance of each card for however many copies are specified
            for (int i = 0; i < entry.copies; i++)
                drawPile.Add(new CardInstance(entry.card));
        }

        Shuffle(drawPile);      // shuffles the draw pile
        Debug.Log($"Deck built: {drawPile.Count} cards.");      // displays the number of cards in the draw pile
    }

    /// Null when the deck is spent. The caller decides what that means -
    /// here it just means a short hand.
    public CardInstance Draw()
    {
        if (drawPile.Count == 0) Recycle();     // recycles the cards if empty
        if (drawPile.Count == 0) return null;       // this would then not fire, unless the draw pile is out of cards completely

        // From the end: RemoveAt(0) shifts every remaining element down one.
        // The pile is shuffled, so which end is the "top" is arbitrary.
        int last = drawPile.Count - 1;      // starting at the end index
        CardInstance card = drawPile[last];     // defininf the card to be the last card
        drawPile.RemoveAt(last);            // removing that card from the draw pile
        return card;                        // returns the card instance
    }

    /// Fill the hand to its size, or until the deck runs dry.
    public void DealTo(HandManager hand)
    {
        while (!hand.IsFull)            // fires when the hand isn't full
        {
            CardInstance card = Draw();         //draws a card and returns a card instance
            if (card == null) return;           // if no card exists

            hand.AddCard(card);                 // adds card to hand through hand manager
        }
    }

    /// Where a card goes when it leaves the hand - played or discarded.
    /// An exhausted card is NOT discarded; it leaves the shift for good,
    /// which is what makes the deck thin rather than just cycle.
    public void Discard(CardInstance card)
    {
        if (card == null) return;       // if no card, return null so no error

        if (card.IsExhausted)           // the condition for when all pips have been used up, this runs through card instance
        {
            Debug.Log($"{card.data.cardName} is spent and leaves the game. " +
                      $"{CardsRemaining} cards left.");
            return;
        }

        discardPile.Add(card);          // if card still has uses, it goes into the discard
    }

    /// Discard becomes the new draw pile. Called only from Draw, when the
    /// draw pile is already empty - never pre-emptively, or the shuffle
    /// happens at an unpredictable moment and a seeded run stops replaying.
    private void Recycle()
    {
        if (discardPile.Count == 0) return;     // if there is nothing to pull from discard, return

        drawPile.AddRange(discardPile);         // adding everything in discard pile into draw pile
        discardPile.Clear();                // clears the discard pile
        Shuffle(drawPile);                  // shuffles the draw pile

        Debug.Log($"Reshuffled: {drawPile.Count} cards back into the draw pile.");
    }

    /// Fisher-Yates. The range shrinks each step, which is what produces
    /// exactly n! outcomes; swapping against the full range every time looks
    /// almost identical and is measurably biased.
    private void Shuffle(IList<CardInstance> list)      // takes in a list of card instances
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    [System.Serializable]
    public class DeckEntry
    {
        public CardData card;

        [Tooltip("Copies control how often you SEE this card. Uses control how " +
                 "many of that drink exist in the whole shift. They are separate " +
                 "levers - milk needs copies because it is in five of six " +
                 "recipes, and a version with fewer milk cards at the same dose " +
                 "count ran dry early and stranded the foam.")]
        public int copies = 1;
    }
}