using UnityEngine;

/// The shared definition of one card type. This is an ASSET, not a scene
/// object - there is one "Espresso" file and the deck holds ten references
/// to it. Never mutate this at runtime: Unity writes changes to disk and
/// they persist between plays. Phase 2's CardInstance holds the mutable state.
[CreateAssetMenu(fileName = "Card_", menuName = "Last Call/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    public string cardName;
    public Sprite artwork;

    [TextArea(2, 3)]
    public string description;

    [Tooltip("The ingredient's colour on the chalkboard and in its card art. " +
             "Reusing one colour in both places means the menu teaches the cards.")]
    public Color identityColor = Color.white;

    [Header("Economy")]
    [Tooltip("Doses per physical card. Copies control how OFTEN you see a card; " +
             "uses control how MANY of that drink exist in the whole shift. " +
             "These are separate levers.")]
    public int maxUses = 1;

    [Tooltip("1 for every ingredient except foam, which is 2.")]
    public int energyCost = 1;

    [Header("Type")]
    [Tooltip("Ingredients go in the deck and get poured. Drinks are produced " +
             "by completing a recipe and get served. Both are cards because " +
             "both are DRAGGED.")]
    public CardKind kind = CardKind.Ingredient;
    
    [Tooltip("Abbreviation for the chalkboard - 'milk', 'esp', 'choc'. Keeps a " +
         "three-ingredient line inside 130px.")]
    public string shortName;
}

public enum CardKind
{
    Ingredient,
    Drink
}
