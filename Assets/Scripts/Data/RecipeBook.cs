using System.Collections.Generic;
using UnityEngine;

/// The rules of what can become what. Pure logic over card lists - it knows
/// nothing about cups, drag, or the scene, which is what lets CupSlot, the
/// chalkboard and the tooltip all ask it the same questions.
public class RecipeBook : MonoBehaviour
{
    [SerializeField] private List<RecipeData> recipes = new List<RecipeData>();

    public IReadOnlyList<RecipeData> Recipes => recipes;

    /// Exact match: same ingredients, same counts. Null when the cup is
    /// between recipes or has gone somewhere no recipe goes.
    public RecipeData MatchedRecipe(IReadOnlyList<CardData> contents)
    {
        if (contents.Count == 0) return null;

        foreach (RecipeData recipe in recipes)
            if (SameMultiset(contents, recipe.ingredients))     // checks if the cup contents match a recipe
                return recipe;

        return null;
    }

    /// Every recipe this cup could still become, including one it already
    /// matches - a Latte is still "reachable" as a Mocha AND as a Latte,
    /// which is exactly why 3-4 refuses to auto-complete.
    public List<RecipeData> StillReachable(IReadOnlyList<CardData> contents)
    {
        var result = new List<RecipeData>();

        foreach (RecipeData recipe in recipes)
            if (IsSubsetOf(contents, recipe.ingredients))
                result.Add(recipe);     // adds this recipe to the list of reachable recipes

        return result;
    }

    /// Pour legality. Adding the card must leave the cup inside at least one
    /// recipe. This is also what caps the cup at three without a capacity
    /// rule: a fourth ingredient cannot be contained by a three-item recipe.
    public bool CanAccept(IReadOnlyList<CardData> contents, CardData card)
    {
        if (card == null) return false;     // no card attempted to pour

        var probe = new List<CardData>(contents) { card };
        return StillReachable(probe).Count > 0;     // only allows the play if a recipe is still reachable
    }

    /// Ingredients in the recipe this cup does not have yet, counting
    /// duplicates. Used by both the chalkboard and the 3-5b tooltip, so it
    /// lives here rather than being written twice.
    public List<CardData> Missing(IReadOnlyList<CardData> contents,
                                  RecipeData recipe)
    {
        var remaining = new List<CardData>(recipe.ingredients);

        // Remove ONE occurrence per held card, not all of them. None of the
        // six recipes have duplicates today, so this is insurance - but the
        // all-occurrences version would fail silently the day one does.
        foreach (CardData c in contents)
            remaining.Remove(c);

        return remaining;
    }

    // ---- multiset helpers ----

    /// How many of each card. Keys are CardData ASSETS, compared by
    /// reference - two separate Espresso assets would count as different
    /// ingredients, which is baffling to debug and impossible to create
    /// by accident.
    private static Dictionary<CardData, int> Counts(IReadOnlyList<CardData> cards)
    {
        var counts = new Dictionary<CardData, int>();           // a dictionary of cards and an int for how many of that card

        foreach (CardData c in cards)
            counts[c] = counts.TryGetValue(c, out int n) ? n + 1 : 1;       // how many that card there are

        return counts;
    }

    /// Every ingredient in 'inner' appears in 'outer' at least as many times.
    private static bool IsSubsetOf(IReadOnlyList<CardData> inner,
                                   IReadOnlyList<CardData> outer)
    {
        // Cheap reject first: a bigger list cannot fit inside a smaller one.
        if (inner.Count > outer.Count) return false;

        Dictionary<CardData, int> have = Counts(outer);

        foreach (var kv in Counts(inner))
            if (!have.TryGetValue(kv.Key, out int n) || n < kv.Value)       // for it to be a subset, the number of that type of card must be less than the n
                return false;

        return true;
    }

    /// Same size AND contained, which for multisets means equal.
    private static bool SameMultiset(IReadOnlyList<CardData> a,
                                     IReadOnlyList<CardData> b)
        => a.Count == b.Count && IsSubsetOf(a, b);          // must have same number of cards in a and b, 
                                                            //and a must be a subset of b (so they are the same set of cards)
}
