using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Combat/Combo Chain")]
public class SO_ComboChain : ScriptableObject
{
    public List<ComboStep> comboSteps;
    public float comboResetTime = 1.0f;
}