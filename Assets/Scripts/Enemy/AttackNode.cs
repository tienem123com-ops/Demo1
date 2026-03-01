using UnityEngine;

public class AttackNode : BTNode
{
    private EnemyBlackboard blackboard;
    private EnemyAttack attack;

    public AttackNode(EnemyBlackboard bb, EnemyAttack attack)
    {
        blackboard = bb;
        this.attack = attack;
    }

    public override State Evaluate()
    {
        if (blackboard.Player == null)
            return State.Failure;

        attack.Detection_NA(blackboard.Player.gameObject);

        return State.Success;
    }
}