
using System.Collections.Generic;
using UnityEngine;

/// One drink: what goes in, what comes out, what it pays.
[CreateAssetMenu(fileName = "Recipe_", menuName = "Last Call/Recipe Data")]
public class RecipeData : ScriptableObject
{
    [Header("Identity")]
    public string drinkName;

    [Tooltip("A cup is a bag of poured ingredients and becomes " +
             "whichever recipe it exactly matches.")]
    public List<CardData> ingredients = new List<CardData>();

    [Tooltip("The finished-drink CardData this produces.")]
    public CardData resultDrink;

    [Header("Payout")]
    [Tooltip("Paid when served to a customer who prefers this drink.")]
    public int preferredPayout;

    [Tooltip("Paid when served to a customer who accepts but does not prefer it. " +
             "Always half the preferred price, rounded down.")]
    public int acceptedPayout;

    /// Total energy to build and serve one of these. Useful for sanity-checking
    /// prices, and Phase 3's UI can show it.
    public int TotalEnergyCost
    {
        get
        {
            int sum = 1;   // the serve action itself
            foreach (CardData ing in ingredients) sum += ing.energyCost;
            return sum;
        }
    }
}