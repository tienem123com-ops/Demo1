using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Player/Attack Data")]
public class AttackData : ScriptableObject 
{
    public string animationName;
    public List<ActionWindow> windows;
}

