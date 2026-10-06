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

    private Customer occupant;

    public Customer Occupant => occupant;
    public bool IsEmpty => occupant == null;

    [Tooltip("Parented to the SLOT, not the customer, so it outlives the object " +
         "it announces.")]
    [SerializeField] private GameObject departurePuffPrefab;


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
        occupant.StopWalk();

        if (departurePuffPrefab != null)
            Instantiate(departurePuffPrefab,
            occupant.transform.position + Vector3.up * 0.75f,
            Quaternion.identity, transform);


        occupant.transform.SetParent(transform.parent, true);
        Destroy(occupant.gameObject, lingerSeconds);
        occupant = null;
    }

    /// Nothing is servable yet. Drinks do not exist as cards until 3-7, so
    /// there is nothing a customer could take and customers never highlight
    /// during a drag - which is correct, not a placeholder.
    public override bool CanAccept(CardData card)
    { return (!IsEmpty && occupant.Evaluate(card) != ServeResponse.Refused); }

    /// Never called. Serving goes through PlayController.TryServe, which takes
    /// a cup rather than a CardInstance - a drink has no instance behind it.
    /// This exists because DropTarget requires it.
    public override void Receive(CardInstance instance)
        => Debug.LogWarning($"{name}: Receive called on a customer slot", this);

    public void Place(Customer customer)
    {
        occupant = customer;

        // Before the reparent: this is where they are standing right now, which
        // is the queue position they just left.
        Vector3 from = customer.transform.position;

        customer.transform.SetParent(transform, false);
        customer.transform.localPosition = customerOffset;

        customer.WalkIn(from);
    }
}
