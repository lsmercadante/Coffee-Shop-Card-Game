using System.Collections.Generic;
using UnityEngine;

/// Holds the cups so the chalkboard can ask about all of them.
public class CupRow : MonoBehaviour
{
    // TODO: a serialized List<CupSlot>, and a public IReadOnlyList<CupSlot>
    // property exposing it. That is the whole class.
    [SerializeField] public List<CupSlot> cupSlots;
    public IReadOnlyList<CupSlot> Cupslots => cupSlots ; 
}
