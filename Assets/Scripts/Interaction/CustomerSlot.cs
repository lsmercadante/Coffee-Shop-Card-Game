using UnityEngine;

/// The second DropTarget. A fixed position in the line that may or may not
/// have someone standing in it. Serving is the same drag verb as pouring, so
/// this needs no new machinery in PlayController at all - which is the test
/// of whether DropTarget was drawn in the right place.
public class CustomerSlot : DropTarget
{
    [Tooltip("Where the Customer prefab root lands, local to this spot. The " +
             "spot sits at y 0.75, so -0.75 puts a bottom-pivot sprite on the " +
             "floor line. Default is correct - do not set this per spot.")]
    [SerializeField] private Vector3 customerOffset = new Vector3(0f, -0.75f, 0f);

    [Tooltip("A short-lived sprite spawned when anyone leaves. Parented to the " +
             "SLOT, not the customer, so it outlives the object it announces.")]
    [SerializeField] private GameObject departurePuffPrefab;

    private Customer occupant;

    public Customer Occupant => occupant;
    public bool IsEmpty => occupant == null;

    public void Place(Customer customer)
    {
        occupant = customer;
        customer.transform.SetParent(transform, false);
        customer.transform.localPosition = customerOffset;
    }

    /// The customer leaves at once - the reference is dropped now, and the
    /// puff and the departing sprite finish on their own time.
    ///
    /// The seat does NOT refill as a result. Vacating and filling are
    /// separate events: only CustomerLine.FillToCapacity places anyone, and
    /// only the Arrivals phase calls it, so a seat emptied during a turn
    /// stays empty until the next turn opens. Nothing here may ever ask for
    /// a replacement - see the note under CustomerLine.
    public void Vacate(float lingerSeconds)
    {
        if (occupant == null) return;

        occupant.transform.SetParent(transform.parent, true);
        Destroy(occupant.gameObject, lingerSeconds);
        occupant = null;
    }

     /// Nothing is servable yet. Drinks do not exist as cards until 3-7, so
    /// there is nothing a customer could take and customers never highlight
    /// during a drag - which is correct, not a placeholder.
    public override bool CanAccept(CardData card) => false;

    /// Unreachable while CanAccept is false. 3-7 writes both together.
    public override void Receive(CardInstance instance) { }
}
