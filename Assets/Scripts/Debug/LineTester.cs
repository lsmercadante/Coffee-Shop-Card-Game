using UnityEngine;
using UnityEngine.InputSystem;

/// Space advances a turn. Stands in for TurnManager until 2-12.
public class LineTester : MonoBehaviour
{
    [SerializeField] private CustomerLine line;

    private int turn = 1;

    private void Start()
    {
        line.OpenShift(turn);
        Debug.Log($"Turn {turn}: {line.Describe()}");
    }

    private void Update()
    {
        // Input System package, not UnityEngine.Input - this project has
        // Active Input Handling set to the package, which disables the legacy
        // class entirely. Keyboard.current is null when no keyboard is
        // attached, which is rare on desktop but free to guard.
        if (Keyboard.current == null) return;
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;

        // End of the current turn: patience ticks, anyone at zero leaves and
        // their seat is now empty.
        //
        // The penalty for THIS turn, not a running total. There is no income
        // until 2-12, so a cumulative figure has nothing to be measured
        // against - what needs checking is that a departure charges the right
        // amount, 20 or 35, which a per-turn number shows directly.
        int penalty = line.TickAll();
        string charge = penalty > 0 ? $"   walkout -{penalty}" : "";
        Debug.Log($"End of {turn}: {line.Describe()}{charge}");

        // Start of the next turn: Arrivals, the only place seats ever fill.
        turn++;
        line.FillToCapacity(turn);
        Debug.Log($"Turn {turn}: {line.Describe()}");
    }
}