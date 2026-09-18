
using UnityEngine;
using UnityEngine.InputSystem;

/// D deals a hand, F discards it. Stands in for the draw and EndTurn phases
/// until TurnManager arrives in 2-12.
public class DeckTester : MonoBehaviour
{
    [SerializeField] private DeckManager deck;
    [SerializeField] private HandManager hand;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            deck.DealTo(hand);
            Report("dealt");
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            hand.DiscardAllTo(deck);
            Report("discarded");
        }
    }

    private void Report(string what)
        => Debug.Log($"{what}: hand {hand.Count}, draw {deck.DrawCount}, " +
                     $"discard {deck.DiscardCount}, in play {deck.CardsRemaining}");
}