using System.Collections.Generic;   // NEW - for Dictionary
using System.Text;
using TMPro;
using UnityEngine;

/// Reads the menu straight off the RecipeData and CardData assets so it
/// can never drift from the actual rules. Pure display: it renders what
/// it is told, and never asks the deck or the cups anything itself.
public class ChalkboardMenu : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private RecipeData[] recipes;
    [SerializeField] private CardData[] ingredients;
    public IReadOnlyList<CardData> Ingredients => ingredients;


    [Header("Row prefabs")]
    [Tooltip("A TMP object using the LARGE pixel font.")]
    [SerializeField] private TextMeshProUGUI namePrefab;

    [Tooltip("A TMP object using the SMALL pixel font.")]
    [SerializeField] private TextMeshProUGUI subPrefab;

    [SerializeField] private Transform menuRoot;
    [SerializeField] private Transform stockRoot;

    // NEW: the rows we built, so we can edit one without rebuilding all of
    // them. Keyed by the asset itself - same reason RecipeBook can use
    // CardData as a dictionary key.
    private readonly Dictionary<RecipeData, TextMeshProUGUI> nameRows =
        new Dictionary<RecipeData, TextMeshProUGUI>();

    private readonly Dictionary<CardData, TextMeshProUGUI> stockRows =
        new Dictionary<CardData, TextMeshProUGUI>();

   private void Awake() => Rebuild();

    public void Rebuild()
    {
        ClearChildren(menuRoot);
        ClearChildren(stockRoot);
        nameRows.Clear();   // NEW - the old entries point at destroyed objects
        stockRows.Clear();   // NEW - the old entries point at destroyed objects

        foreach (RecipeData r in recipes)
        {
            var nameRow = Instantiate(namePrefab, menuRoot);
            nameRow.text = r.drinkName;
            nameRows[r] = nameRow;   // NEW - remember it

            var sub = Instantiate(subPrefab, menuRoot);
            sub.text = BuildIngredientLine(r);
        }

        foreach (CardData c in ingredients)
        {   
            var stockRow = Instantiate(subPrefab, stockRoot);
            stockRow.text = $"{Hex(c.identityColor)}{c.cardName}</color>";
            stockRows[c] = stockRow;   // NEW - remember it
        }
    }

    /// NEW. Called by the reachability script once per recipe, per refresh.
    public void SetCrossedOut(RecipeData recipe, bool crossedOut)
    {
        if (!nameRows.TryGetValue(recipe, out TextMeshProUGUI row))
        {
            Debug.LogWarning($"No chalkboard row for {recipe.drinkName}", this);
            return;
        }

        row.text = crossedOut ? $"<s>{recipe.drinkName}</s>" : recipe.drinkName;
    }

    public void SetStock(CardData card, int doses)
    {
        if (!stockRows.TryGetValue(card, out TextMeshProUGUI row))
            {
                Debug.LogWarning($"No chalkboard row for {card.cardName}", this);
            return;
            }
        row.text = $"{Hex(card.identityColor)}{card.cardName}</color>  x{doses}";
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
            case "Warm Milk":  return "milk";
            case "Ice Cream":  return "ice";
            case "Chocolate":  return "choc";
            case "Espresso":   return "esp";
            case "Foam":       return "foam";
            default:           return name.ToLower();
        }
    }

    private static void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject);
    }
}