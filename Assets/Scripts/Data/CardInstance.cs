/// One physical card in this shift. The ScriptableObject is the shared
/// definition; this is the copy in hand with its own remaining doses.
[System.Serializable]
public class CardInstance
{
    public CardData data;
    public int usesRemaining;

    public CardInstance(CardData data)          // a constructor that creates a card instance from card data, and assigns uses remaining
    {
        this.data = data;                       // this is basically the c# way of doing start() or awake() 
        usesRemaining = data.maxUses;
    }

    public bool IsExhausted => usesRemaining <= 0;      // defines when a card is exhausted

    /// Spend one dose. Returns true if the card is now exhausted, which is
    /// 2-10's cue to remove it from the game entirely rather than discarding it.
    public bool Spend()
    {
        usesRemaining--;        // uses remaining decreases
        return IsExhausted;         // returns whether or not the card is exhausted
    }
}