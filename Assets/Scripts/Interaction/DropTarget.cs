using UnityEngine;

/// Base class for anything a card can be dropped onto (cups and customers).
///  Must sit on a GameObject with a Collider2D on the DropTarget
/// layer, because drops resolve through Physics2D.OverlapPoint.
///
/// This is abstract rather than an interface so highlighting can be
/// implemented once here instead of in every target type.
/// 
[RequireComponent(typeof(Collider2D))]      // adds a 2D collider component to the game object if one doesn't exist
// an abstract class cannot be instantiated on its own, it must be inherreted from
// good for defining a system you want multiple types of objects to be able to inheretted from
// in my face, features that are shared by any drop target
public abstract class DropTarget : MonoBehaviour       
{
    [Header("Highlight")]
    [SerializeField] private SpriteRenderer highlightRenderer;      // a sprite render that will control the highlight
    [SerializeField] private Color highlightColour = new Color(1f, 0.95f, 0.6f);        // the highlight color, "new" instantiates this object

    private Color baseColour;           // the base color of the object
    private Collider2D ownCollider;        // the object's own collider

    // protected means it can only be called from within the class 
    // or from classes that inheret from it (derived classes)
    // virtual means child classes are allowed to override it an change its behavior
    // which I want because the customer and cups might need different things in their setup
    protected virtual void Awake()
    {
        ownCollider = GetComponent<Collider2D>();       // assigns the collider component 

        if (highlightRenderer == null)              // if the highlight renderer doesn't exist, just use the basic sprite renderer
            highlightRenderer = GetComponent<SpriteRenderer>();

        if (highlightRenderer != null)                  // if the highlight renderer does exist, set the base color from there
            baseColour = highlightRenderer.color;
    }

    /// A disabled collider means this target is dormant - the sixth customer
    /// spot before the crowd rush opens it. Checked here rather than in each
    /// subclass so no future target type can forget.
    public bool IsActive => ownCollider != null && ownCollider.enabled;

    /// every drop target must define whether it can accept a card
    public abstract bool CanAccept(CardData card);

    ///  every drop target must define a recieve method with this structure
    /// but the method itself may vary so it is not defined here, only required
    public abstract void Receive(CardInstance instance);

    public void SetHighlight(bool on)
    {
        if (highlightRenderer == null) return;      // if no highlight renderer, don't set the highlight
        highlightRenderer.color = on ? highlightColour : baseColour;    // if on = true, we set the highlight
                                                                        // if on = false, we keep the base color
    }
}
