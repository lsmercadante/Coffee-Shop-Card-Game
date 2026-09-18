using TMPro;
using UnityEngine;

/// Shows the two pile counts. Its own component rather than a general
/// HudDisplay, because the HUD ends up carrying four unrelated things - deck
/// counts from here, coins and quota from scoring, the clock from
/// TurnManager, the rush countdown from the CROWD event. One class holding
/// all four references would be the only object in the game that knows about
/// everything; split by data source and each component talks to one system.
public class DeckCounter : MonoBehaviour
{
    [SerializeField] private DeckManager deck;
    [SerializeField] private TextMeshProUGUI drawLabel;
    [SerializeField] private TextMeshProUGUI discardLabel;

    private void Update()
    {
        drawLabel.text = deck.DrawCount.ToString();
        discardLabel.text = deck.DiscardCount.ToString();
    }
}
