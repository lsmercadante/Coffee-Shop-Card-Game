using System.Collections.Generic;
using UnityEngine;

/// A fixed-depth lookahead between the bag and the chairs. Capacity rules do
/// not change at all - seats still refill to capacity every turn, and the
/// queue is always topped back to full - so this adds information without
/// adding or removing pressure. The next-piece preview from a falling-block
/// game, not a waiting room.
public class CustomerQueue : MonoBehaviour
{
    [Tooltip("Front of the queue first. Length IS the lookahead depth.")]
    [SerializeField] private Transform[] positions;

    [SerializeField] private CustomerSpawner spawner;
    [SerializeField] private Customer customerPrefab;

    [Tooltip("Off until the Phase 3 read on whether the deck carries its " +
             "weight. The queue runs either way - this only hides it.")]
    [SerializeField] private bool visible = false;

    private readonly List<Customer> waiting = new List<Customer>();

    public bool IsEmpty => waiting.Count == 0;

    /// Draw until full. Called after seating, never before.
    public void TopUp(int turn)
    {
        if (positions == null || positions.Length == 0)
        {
            Debug.LogError($"{name}: CustomerQueue has no positions assigned - " +
                           "nobody can be drawn.", this);
            return;
        }
        while (waiting.Count < positions.Length)
        {
            CustomerData data = spawner.Next(turn);
            if (data == null) break;          // nobody eligible; queue runs short

            Customer customer = Instantiate(customerPrefab);
            customer.Initialize(data);
            customer.SetWaiting(true);
            waiting.Add(customer);
        }

        Reposition();


    }

    /// Front of the line takes the chair. Null when the queue is dry, which
    /// leaves the seat empty rather than conjuring someone.
    public Customer Dequeue()
    {
        if (waiting.Count == 0) return null;

        Customer front = waiting[0];
        waiting.RemoveAt(0);

        front.gameObject.SetActive(true);
        front.SetWaiting(false);

        Reposition();
        return front;
    }

    private void Reposition()
    {
        for (int i = 0; i < waiting.Count; i++)
        {
            waiting[i].transform.SetParent(positions[i], false);
            waiting[i].transform.localPosition = Vector3.zero;
            waiting[i].gameObject.SetActive(visible);
        }
    }
}
