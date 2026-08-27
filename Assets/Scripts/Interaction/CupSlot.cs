using System.Collections.Generic;
using UnityEngine;

/// One of the three cups. Holds poured ingredients and stacks a small sprite
/// per pour so contents are readable without clicking.
///
/// Phase 3 turns this into the full DrinkInProgress: same class, real recipe
/// logic replacing the CanAccept stub. Do not write a second class for it.
public class CupSlot : DropTarget
{
    [Header("Stacking")]
    [Tooltip("World units between stacked sprites. 0.35 = ~11px at 32 PPU, " +
             "which leaves the top edge of the sprite below still visible.")]
    [SerializeField] private float stackOffsetY = 0.35f;

    [Tooltip("Empty child at the cup's base. Pours build upward from here.")]
    [SerializeField] private Transform stackRoot;

    [Tooltip("A small world-space object with a SpriteRenderer. Placeholder " +
             "is fine - Phase 5 replaces it.")]
    [SerializeField] private GameObject pouredSpritePrefab;

    // The cup's contents. Phase 3's RecipeBook reads this to decide what the
    // cup has become and what it can still become.
    private readonly List<CardInstance> contents = new List<CardInstance>();
    public IReadOnlyList<CardInstance> Contents => contents;

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
        contents.Add(instance);
        SpawnPouredSprite(instance.data, contents.Count - 1);
    }

    private void SpawnPouredSprite(CardData card, int indexInStack)
    {
        if (pouredSpritePrefab == null) return;

        GameObject go = Instantiate(pouredSpritePrefab, stackRoot);
        go.transform.localPosition = new Vector3(0f, indexInStack * stackOffsetY, 0f);

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = card.artwork;
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