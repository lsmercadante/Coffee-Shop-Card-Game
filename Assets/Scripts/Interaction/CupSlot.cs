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

    // The cup's contents. Phase 3's RecipeBook reads this to decide what the
    // cup has become and what it can still become.
    private readonly List<CardInstance> contents = new List<CardInstance>();        // readonly because we don't want the list object to be swapped for a different one
                                                                                    // but we do want to add and remove from it
    public IReadOnlyList<CardInstance> Contents => contents;        // IReadOnlyList allows outside scripts to read the contents but not add or remove content

    // Awake method is overriding the one from drop target
    protected override void Awake()
    {
        base.Awake();
        if (stackRoot == null) stackRoot = transform;
    }

    /// PHASE 1 STUB. Phase 3 replaces with:
    ///   RecipeBook.StillReachable(contents + card).Count > 0
    /// which is what makes a foam pour lock the cup to Cappuccino.
    public override bool CanAccept(CardData card) => true;

    public override void Receive(CardInstance instance)
    {
        contents.Add(instance);         // adding the instance to the contents list (so adding an ingredient to the list of things containe din cup)
        SpawnPouredSprite(instance.data, contents.Count - 1);  // content.Count -1 because we want the index, and count is one too many
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
    }
}