using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Combo Sequence")]
public class ComboSequenceSO : ScriptableObject
{
    public List<AttackData> attacks = new List<AttackData>();
}


