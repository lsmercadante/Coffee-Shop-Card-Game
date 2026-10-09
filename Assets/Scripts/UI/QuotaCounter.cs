using TMPro;
using UnityEngine;

public class QuotaCounter : MonoBehaviour
{
    [SerializeField] private TurnManager turns;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private int placeholderTarget = 400;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        if (TurnManager.Instance == null) return;
        TurnManager.Instance.CoinsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.CoinsChanged -= Refresh;
    }

    private void Refresh()
    {
        label.text = $"{TurnManager.Instance.Coins}/{placeholderTarget}";
    }
}
