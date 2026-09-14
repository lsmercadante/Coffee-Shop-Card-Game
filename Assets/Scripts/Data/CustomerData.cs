using UnityEngine;

/// One customer TYPE, not one arrival. Diane can be in the shop twice at once;
/// both read from this asset and each keeps its own patience on the Customer
/// component. Never mutate this at runtime - Unity writes SO changes to disk.
[CreateAssetMenu(fileName = "Customer_", menuName = "Last Call/Customer")]
public class CustomerData : ScriptableObject
{
    [Header("Identity")]
    public string customerName;

    [TextArea(2, 3)]
    [Tooltip("One line, present tense. 'Just out of school and after a sugar " +
             "rush.' It is the only place the game explains WHY someone is picky.")]
    public string description;

    [Header("Standing sprite")]
    [Tooltip("One still. This is the guide's 'portrait' from 2-1 - there is " +
             "no second image, because a still that can disagree with the " +
             "body art is a bug waiting to happen. Phase 5 replaces this " +
             "with a frame array once the art is final.")]
    public Sprite bodySprite;

    [Header("Order")]
    public RecipeData preferredDrink;

    [Tooltip("LEAVE NULL for Skye and Gabe. Null is the no-fallback flag - " +
             "there is deliberately no separate bool that could disagree with it.")]
    public RecipeData acceptedDrink;

    [Header("Pressure")]
    public int patience = 3;
    public int walkoutPenalty = 20;

    [Header("Arrival window")]
    [Tooltip("0 = from the start. Gabe is 7.")]
    public int earliestTurn = 0;

    [Tooltip("Dealt into the opening arrivals so she is guaranteed to appear " +
             "in the first two turns. NOT a latest turn - she goes back in " +
             "the bag afterwards and can arrive again like anyone else.")]
    public bool guaranteedOpener = false;

    public bool HasFallback => acceptedDrink != null;

    /// The only hard arrival bound in the game. Turn 0 does not exist, so 0
    /// and 1 both mean "from the start".
    public bool ArrivesOn(int turn) => turn >= earliestTurn;
}
