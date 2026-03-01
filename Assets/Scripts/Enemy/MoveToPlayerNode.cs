using UnityEngine;

public class MoveToPlayerNode : BTNode
{
    private EnemyBlackboard blackboard;
    private Transform enemyTransform;

    public MoveToPlayerNode(EnemyBlackboard bb, Transform enemy)
    {
        blackboard = bb;
        enemyTransform = enemy;
    }

    public override State Evaluate()
    {
        if (blackboard.Player == null)
            return State.Failure;

        float distance = Vector3.Distance(enemyTransform.position, blackboard.Player.position);

        if (distance <= blackboard.AttackRange)
            return State.Success;

        Vector3 dir = (blackboard.Player.position - enemyTransform.position).normalized;
        enemyTransform.position += dir * blackboard.MoveSpeed * Time.deltaTime;

        return State.Running;
    }
}