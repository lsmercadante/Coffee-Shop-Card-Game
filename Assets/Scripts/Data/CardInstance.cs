/// One physical card in this shift. The ScriptableObject is the shared
/// definition; this is the copy in hand with its own remaining doses.
[System.Serializable]
public class CardInstance
{
    public CardData data;
    public int usesRemaining;

    public CardInstance(CardData data)
    {
        this.data = data;
        usesRemaining = data.maxUses;
    }

    public bool IsExhausted => usesRemaining <= 0;

    /// Spend one dose. Returns true if the card is now exhausted, which is
    /// 2-10's cue to remove it from the game entirely rather than discarding it.
    public bool Spend()
    {
        usesRemaining--;
        return IsExhausted;
    }
}