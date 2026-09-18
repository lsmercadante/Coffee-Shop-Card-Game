
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// The bottom-right panel: energy readout, Discard button, End Turn button.
/// Presentation only - the discard ACTION lives on TurnManager.
public class ControlCluster : MonoBehaviour
{
    [SerializeField] private TurnManager turns;

    [Header("Energy")]
    [SerializeField] private TextMeshProUGUI energyLabel;
    [SerializeField] private Color normal = Color.white;
    [SerializeField] private Color low = new Color(0.95f, 0.75f, 0.30f);
    [SerializeField] private Color critical = new Color(0.90f, 0.35f, 0.35f);

    [Header("Buttons")]
    [SerializeField] private Button discardButton;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Image endTurnBackground;
    [SerializeField] private Color endTurnIdle = Color.white;
    [SerializeField] private Color endTurnPrompt = new Color(0.45f, 0.80f, 0.40f);

    private void OnEnable()
    {
        turns.EnergyChanged += Refresh;
        Refresh();
    }

    // Always unsubscribe. A destroyed listener still holding a subscription
    // keeps the whole object alive and throws on the next invoke.
    private void OnDisable() => turns.EnergyChanged -= Refresh;

    // Selection has no event to listen to, so the Discard button's enabled
    // state is polled. Two comparisons a frame; not worth an event on
    // PlayController for it.
    private void Update()
        => discardButton.interactable = turns.CanDiscard(PlayController.Instance.Selected);

    /// Wired to the Discard button's OnClick.
    public void OnDiscardPressed()
    {
        CardVisual card = PlayController.Instance.Selected;     // which card is selected
        if (!turns.CanDiscard(card)) return;                // figures out if card can be discarded

        // Deselect BEFORE the card is destroyed, or ClearSelection calls
        // SetSelected on a dead object.
        PlayController.Instance.ClearSelection();
        turns.TryDiscardAndRedraw(card);
    }

    private void Refresh()
    {
        energyLabel.text = $"{turns.Energy}/{turns.EnergyPerTurn}";

        energyLabel.color = turns.Energy >= 3 ? normal
                          : turns.Energy == 2 ? low
                          : critical;

        // At 0 energy nothing can be played, so the only move left is ending
        // the turn. Highlight it rather than ending automatically.
        endTurnBackground.color = turns.Energy == 0 ? endTurnPrompt : endTurnIdle;
    }
}