using System.Collections.Generic;
using UnityEngine;

/// Shuffled-bag draw: shuffle all seven, deal them out, reshuffle when empty.
/// Variety without pure randomness - you never get four Dianes running, but two
/// in succession is possible and feels like a rush rather than a bug.
public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private CustomerData[] roster;

    [Tooltip("Minimum other arrivals between two of the SAME no-fallback " +
             "type. Skye and Gabe may stand next to each other - they fail " +
             "on different ingredients, so one drought only costs one of them.")]
    [SerializeField] private int noFallbackSpacing = 4;

    [Tooltip("How many arrivals count as 'the opening'. Five, because three " +
             "seats fill on turn 1 and refill-to-capacity adds two on turn 2.")]
    [SerializeField] private int openingArrivals = 5;

    [Tooltip("0 = random each run. Any other value replays that exact customer " +
             "sequence. The seed actually used is logged either way, so a " +
             "shift that goes wrong can be replayed after the fact.")]
    [SerializeField] private int seed = 0;

    private readonly List<CustomerData> bag = new List<CustomerData>();

    // Arrival number of the last time each no-fallback type was drawn.
    // Per type, not one shared counter - see below.
    private readonly Dictionary<CustomerData, int> lastArrival
        = new Dictionary<CustomerData, int>();

    private int arrivalIndex;
    private System.Random rng;

    private void Awake()
    {
        // A private stream, not UnityEngine.Random. See the note below.
        int actualSeed = seed != 0 ? seed : System.Environment.TickCount;
        rng = new System.Random(actualSeed);
        Debug.Log($"CustomerSpawner seed {actualSeed}");

        Refill();
        PromoteOpeners();
    }

    /// Null means nobody may arrive this turn - the seat stays empty rather
    /// than the shop breaking its own rules to fill it.
    public CustomerData Next(int turn)
    {
        // Two passes. The first scans the current bag; the second scans a
        // fresh one. Without the second, a bag holding only Gabe before turn 7
        // stalls forever - he is skipped every time and never removed.
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 0; i < bag.Count; i++)
            {
                CustomerData candidate = bag[i];
                if (!IsEligible(candidate, turn)) continue;

                bag.RemoveAt(i);
                arrivalIndex++;

                if (!candidate.HasFallback)
                    lastArrival[candidate] = arrivalIndex;

                if (bag.Count == 0) Refill();
                return candidate;
            }

            Refill();
        }

        Debug.LogWarning($"No eligible customer on turn {turn}; seat left empty.");
        return null;
    }

    private bool IsEligible(CustomerData c, int turn)
    {
        if (!c.ArrivesOn(turn)) return false;

        // Per TYPE, not across the no-fallback class. Skye fails on
        // chocolate and Gabe on ice cream, so one drought only ever costs
        // one of them - it takes two Skyes to lose 70 to a single drought.
        // What this really guards is the bag seam: a type dealt last in one
        // bag and first in the next would otherwise arrive twice running.
        if (!c.HasFallback
            && lastArrival.TryGetValue(c, out int last)
            && arrivalIndex - last < noFallbackSpacing)
            return false;

        return true;
    }

    private void Refill()
    {
        bag.Clear();
        bag.AddRange(roster);
        Shuffle(bag);
    }

    /// Fisher-Yates. Not static any more - it draws from this instance's rng.
    /// The range shrinks each step, which is what produces exactly n!
    /// outcomes; swapping against the full range every time looks almost
    /// identical and is measurably biased.
    private void Shuffle(IList<CustomerData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// Deal the openers into the first bag. Runs ONCE, on the opening bag
    /// only - Diane is a regular, not a fixture, so every reshuffle after this
    /// treats her like anyone else.
    private void PromoteOpeners()
    {
        int window = Mathf.Min(openingArrivals, bag.Count);

        for (int i = window; i < bag.Count; i++)
        {
            if (!bag[i].guaranteedOpener) continue;

            int target = rng.Next(0, window);
            (bag[i], bag[target]) = (bag[target], bag[i]);
        }
    }
}