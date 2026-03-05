using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewComboSequence", menuName = "Player/Combo Sequence")]
public class ComboSequence : ScriptableObject
{
    public List<AttackData> attacks; // Kéo đòn 1, 2, 3 vào đây
}