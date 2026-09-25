using System.Runtime.CompilerServices;
using UnityEngine;

/// Crosses out what can no longer be made. Recomputes on change, not per
/// frame - pours, serves and exhaustions are the only things that move it.
public class ChalkboardReachability : MonoBehaviour
{
    [SerializeField] private RecipeBook recipes;
    [SerializeField] private CupRow cups;
    [SerializeField] private DeckManager deck;
    [SerializeField] private HandManager hand;
    [SerializeField] private ChalkboardMenu board;

    private void OnEnable()
    {
        // TODO: subscribe to deck.CardExhausted, then Refresh once so the
        // board is correct before anything changes.
        deck.CardExhausted += Refresh;
    }

    private void OnDisable()
    {
        // TODO: unsubscribe.
        deck.CardExhausted -= Refresh;
    }

    /// Called by CupSlot after every pour and by 3-8 after every serve.
    public void Refresh()
    {
        // TODO: for each recipe, tell the board whether to cross it out.
        //
        // You will need SetCrossedOut(RecipeData, bool) on ChalkboardMenu -
        // TMP's <s>text</s> tag is the cheapest strikethrough until 4-7e's
        // chalk art exists.
        
        foreach (RecipeData recipe in recipes.Recipes)
            board.SetCrossedOut(recipe, !IsLive(recipe));
        foreach (CardData card in board.Ingredients)
            board.SetStock(card,deck.DosesRemaining(card) + hand.DosesRemaining(card) );

    }
    private void Start()
    {
        // TODO: Refresh once, so the board is correct before anything
        // changes. It goes HERE rather than in OnEnable because
        // ChalkboardMenu builds its rows in its own Start, and every
        // OnEnable runs before any Start.
        Refresh();
    }

   private bool IsLive(RecipeData recipe)
{
    foreach (CardData ingredient in recipe.ingredients)
    {
        int supply = deck.DosesRemaining(ingredient) + hand.DosesRemaining(ingredient);
        if (supply < Needed(recipe, ingredient))
            return false;
    }

    return true;
}

/// How many of one ingredient this recipe calls for. Usually 1, but a
/// recipe wanting two espressos is dead with one dose left, so the count
/// has to be real rather than assumed.
private static int Needed(RecipeData recipe, CardData ingredient)
{
    int count = 0;
    foreach (CardData item in recipe.ingredients)
        if (item == ingredient)
            count++;
    return count;
}

}
