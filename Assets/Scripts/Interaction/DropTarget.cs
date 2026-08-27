using UnityEngine;

/// Base class for anything a card can be dropped onto. Cups now, customers
/// in Phase 2. Must sit on a GameObject with a Collider2D on the DropTarget
/// layer, because drops resolve through Physics2D.OverlapPoint.
///
/// This is abstract rather than an interface so highlighting can be
/// implemented once here instead of in every target type.
[RequireComponent(typeof(Collider2D))]
// an abstract class needs to be overwritten when inheretted from
public abstract class DropTarget : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private SpriteRenderer highlightRenderer;
    [SerializeField] private Color highlightColour = new Color(1f, 0.95f, 0.6f);

    private Color baseColour;
    private Collider2D ownCollider;

    // virtual means it can be overwritten but doesn't need to be
    protected virtual void Awake()
    {
        ownCollider = GetComponent<Collider2D>();

        if (highlightRenderer == null)
            highlightRenderer = GetComponent<SpriteRenderer>();

        if (highlightRenderer != null)
            baseColour = highlightRenderer.color;
    }

    /// A disabled collider means this target is dormant - the sixth customer
    /// spot before the CROWD rush opens it. Checked here rather than in each
    /// subclass so no future target type can forget.
    public bool IsActive => ownCollider != null && ownCollider.enabled;

    /// Would this card be a legal play here? Phase 3 puts real recipe logic
    /// behind this; cups say yes to everything for now.
    public abstract bool CanAccept(CardData card);

    /// Take the card. Only called after CanAccept returned true.
    public abstract void Receive(CardInstance instance);

    public void SetHighlight(bool on)
    {
        if (highlightRenderer == null) return;
        highlightRenderer.color = on ? highlightColour : baseColour;
    }
}
