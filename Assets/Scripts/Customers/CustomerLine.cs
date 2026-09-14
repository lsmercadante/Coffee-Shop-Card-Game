using System.Collections.Generic;
using UnityEngine;

/// Owns the six slots and decides who stands where. The spawner decides WHO
/// arrives; this decides WHETHER anyone can.
public class CustomerLine : MonoBehaviour
{
    [Tooltip("All six spots, left to right. The sixth has a disabled collider " +
             "until the CROWD rush opens it in Phase 4.")]
    [SerializeField] private CustomerSlot[] slots;

    [SerializeField] private CustomerQueue queue;

    [Tooltip("Serialized rather than const, per 2-15 - the small configuration " +
             "is still useful as a tutorial.")]
    [SerializeField] private int startingOccupied = 3;

    [SerializeField] private float departureLinger = 0.4f;

    /// Turn 1: seed the shop partly full so there is something to do
    /// immediately and still room for the queue to build.
    public void OpenShift(int turn)
    {
        queue.TopUp(turn);

        int placed = 0;

        foreach (CustomerSlot slot in slots)
        {
            if (placed >= startingOccupied) break;
            if (!slot.IsActive || !slot.IsEmpty) continue;
            if (TryFill(slot)) placed++;
        }

        queue.TopUp(turn);
    }

    /// 2-15: every empty ACTIVE seat takes a new customer, every turn. A full,
    /// ticking shop is what makes stalling pointless - holding a finished drink
    /// shelters you from nothing when no seat is safe.
    ///
    /// ONLY the Arrivals phase may call this, once per turn. Calling it in
    /// response to a seat emptying would mean serving someone conjures their
    /// replacement mid-turn: the line would never drain and clearing a
    /// customer would cost you rather than pay you.
    public int FillToCapacity(int turn)
    {
        int arrivals = 0;

        foreach (CustomerSlot slot in slots)
        {
            if (!slot.IsActive || !slot.IsEmpty) continue;
            if (TryFill(slot)) arrivals++;
        }

        // Top up AFTER seating, so anyone drawn now is visible for a full turn
        // before they can take a chair. Topping up first would let a customer
        // be drawn and seated in the same phase, which is the whole thing the
        // queue exists to prevent.
        queue.TopUp(turn);

        return arrivals;
    }

    /// No turn argument: the queue already resolved eligibility when it drew
    /// this person. Seating is now just moving someone who is already here.
    private bool TryFill(CustomerSlot slot)
    {
        Customer customer = queue.Dequeue();
        if (customer == null) return false;   // queue dry; seat stays empty

        slot.Place(customer);
        return true;
    }

    /// Called on EndTurn by 2-16. Returns the total walkout penalty so the
    /// caller can subtract it - this class does not know about coins.
    public int TickAll()
    {
        int penalty = 0;

        foreach (CustomerSlot slot in slots)
        {
            if (slot.IsEmpty) continue;
            if (!slot.Occupant.Tick()) continue;

            penalty += slot.Occupant.Data.walkoutPenalty;
            slot.Vacate(departureLinger);
        }

        return penalty;
    }

    public string Describe()
    {
        var parts = new List<string>();

        foreach (CustomerSlot slot in slots)
        {
            if (!slot.IsActive) parts.Add("[closed]");
            else if (slot.IsEmpty) parts.Add("[--]");
            else parts.Add($"[{slot.Occupant.Data.customerName} {slot.Occupant.PatienceRemaining}]");
        }

        return string.Join(" ", parts);
    }
}
