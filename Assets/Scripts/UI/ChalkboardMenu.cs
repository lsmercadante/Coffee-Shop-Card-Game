using System.Text;
using TMPro;
using UnityEngine;

/// Reads the menu straight off the RecipeData and CardData assets so it
/// can never drift from the actual rules. Phase 3 wires the strikethrough
/// to real reachability; for now it's a static reference panel.
public class ChalkboardMenu : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private RecipeData[] recipes;
    [SerializeField] private CardData[] ingredients;

    [Header("Row prefabs")]
    [Tooltip("A TMP object using the LARGE pixel font.")]
    [SerializeField] private TextMeshProUGUI namePrefab;

    [Tooltip("A TMP object using the SMALL pixel font.")]
    [SerializeField] private TextMeshProUGUI subPrefab;

    [SerializeField] private Transform menuRoot;
    [SerializeField] private Transform stockRoot;

    private void Start() => Rebuild();

    public void Rebuild()
    {
        ClearChildren(menuRoot);
        ClearChildren(stockRoot);

        foreach (RecipeData r in recipes)
        {
            // Drink name. <s> strikes it through - Phase 3 sets this from
            // RecipeBook when no cup can still become this drink.
            var nameRow = Instantiate(namePrefab, menuRoot);
            nameRow.text = r.drinkName;

            // Ingredients beneath, each in its own color. Those colors are
            // the same ones used in the card art, so the board teaches the
            // cards for free.
            var sub = Instantiate(subPrefab, menuRoot);
            sub.text = BuildIngredientLine(r);
        }

        foreach (CardData c in ingredients)
        {
            var row = Instantiate(subPrefab, stockRoot);
            row.text = $"{Hex(c.identityColor)}{c.cardName}</color>  x{RemainingDoses(c)}";
        }
    }

    private string BuildIngredientLine(RecipeData r)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < r.ingredients.Count; i++)
        {
            if (i > 0) sb.Append(" + ");
            CardData ing = r.ingredients[i];
            sb.Append(Hex(ing.identityColor)).Append(Abbrev(ing.cardName)).Append("</color>");
        }

        return sb.ToString();
    }

    /// TMP rich-text color tag.
    private static string Hex(Color c) => $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>";

    /// "Steamed Milk" -> "milk". Keeps Cappuccino's three-ingredient line
    /// inside 130px, which is the whole reason for the abbreviation.
    private static string Abbrev(string name)
    {
        switch (name)
        {
            case "Warm Milk": return "milk";
            case "Ice Cream":    return "ice";
            case "Chocolate":    return "choc";
            case "Espresso":     return "esp";
            case "Foam":         return "foam";
            default:             return name.ToLower();
        }
    }

    /// Phase 2's DeckManager owns the real number. Placeholder for now.
    private int RemainingDoses(CardData c) => c.maxUses;


    // exists for when there the board updates
    private static void ClearChildren(Transform t) 
    {
        for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject);
    }
}