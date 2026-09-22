
using System.Collections.Generic;
using UnityEngine;

/// One of the three cups. Holds poured ingredients and stacks a small sprite
/// per pour so contents are readable without clicking.
///
/// Phase 3 turns this into the full DrinkInProgress: same class, real recipe
/// logic replacing the CanAccept stub. Do not write a second class for it.
/// 
/// CupSlot inherets from DropTarget because it is a drop target and needs to
/// have all of that functionality
public class CupSlot : DropTarget
{
    [Header("Stacking")]
    [Tooltip("World units between stacked sprites. 0.35 = ~11px at 32 PPU, " +
             "which leaves the top edge of the sprite below still visible.")]
    [SerializeField] private float stackOffsetY = 0.35f;            // how much offset between stacked ingredients

    [Tooltip("Empty child at the cup's base. Pours build upward from here.")]
    [SerializeField] private Transform stackRoot;           // a location for where the stack starts

    [Tooltip("A small world-space object with a SpriteRenderer. Placeholder " +
             "is fine - Phase 5 replaces it.")]
    [SerializeField] private GameObject pouredSpritePrefab;             // the general prefab for a poured sprite

    [SerializeField] private RecipeBook recipes;


    // Insertion order, not a set. The recipe matching is order-free, but
    // 4-6b's liquid bands stack in pour order, so the display needs the
    // sequence even though the rules do not.
    private readonly List<CardData> contents = new List<CardData>();

    /// Read-only view. Anything that mutates the cup goes through Receive.
    public IReadOnlyList<CardData> Contents => contents;      // readonly because we don't want the list object to be swapped for a different one
                                                              // but we do want to add and remove from it
                                                              // IReadOnlyList allows outside scripts to read the contents but not add or remove content

    /// The recipe this cup currently IS, or null.
    public RecipeData Matched => recipes.MatchedRecipe(contents);



    // Awake method is overriding the one from drop target
    protected override void Awake()
    {
        base.Awake();
        if (stackRoot == null) stackRoot = transform;
    }

    public override bool CanAccept(CardData card)
    {
        // TODO: ask the RecipeBook whether adding this card is legal.
        return recipes.CanAccept(contents, card);
    }
    public bool IsEmpty => contents.Count == 0;

    public override void Receive(CardInstance instance)
    {
        // TODO: consume one dose, add the card's DATA (not the instance) to
        // contents, then Refresh.
        //
        // The instance goes on to DeckManager.Discard via PlayController -
        // this method only decides that a dose was used.
        instance.Spend();
        contents.Add(instance.data); // adding the card data to the contents list (so adding an ingredient to the list of things containe din cup)       
        SpawnPouredSprite(instance.data, contents.Count - 1);  // content.Count -1 because we want the index, and count is one too many
        Refresh();
    }

    private void SpawnPouredSprite(CardData card, int indexInStack)
    {
        if (pouredSpritePrefab == null) return;     // if no prefab exists it cannot be spawned

        GameObject go = Instantiate(pouredSpritePrefab, stackRoot);  //instantiates the prefab at the stackroot position
        go.transform.localPosition = new Vector3(0f, indexInStack * stackOffsetY, 0f);  // places the local position (relative to the cupslot)

        var sr = go.GetComponent<SpriteRenderer>();     // pull the sprite renderer from the prefab
        if (sr != null)
        {

            if (card.artwork != null) sr.sprite = card.artwork;         // displays the artowrk
            // Tint from the identity colour so pours are distinguishable before
            // there is any art. Harmless once real sprites exist.
            sr.color = card.identityColor;
            sr.sortingOrder = indexInStack + 1;   // later pours draw in front
        }
    }

    /// Phase 3 wires this to serving. Useful now for testing.
    public void Clear()
    {
        contents.Clear();
        for (int i = stackRoot.childCount - 1; i >= 0; i--)
            Destroy(stackRoot.GetChild(i).gameObject);
        Refresh();
    }
    public List<CardData> DisplayOrder()
    {
        // TODO: split contents into two lists by card.floatsToTop, keeping
        // pour order within each, then concatenate with the floaters LAST -
        // the list is bottom-first, so last means top of the stack.
        List<CardData> floats = new List<CardData>();
        List<CardData> sinks = new List<CardData>();
        foreach (CardData card in contents)
            if (card.floatsToTop)
                floats.Add(card);
            else
                sinks.Add(card);
        sinks.AddRange(floats);

        return sinks;
    }
    public string Describe()
    {
        if (contents.Count == 0)
            return "empty";

        List<string> names = new List<string>();
        foreach (CardData card in contents)
            names.Add(card.cardName);

        string text = string.Join(" + ", names);

        // Read once: Matched runs a RecipeBook search every time it's accessed.
        RecipeData match = Matched;
        if (match == null)
            return text;

        return text + " = " + match.drinkName;
    }

    private void Refresh()
    {
        // TODO: rebuild the tooltip text (see 3-5b) and log Describe() while
        // you are still testing. 4-6b's liquid bands hang off here too.
        Debug.Log($"{Describe()}");
    }
    
}
