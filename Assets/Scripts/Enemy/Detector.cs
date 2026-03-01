using UnityEngine;

public class Detector : BTNode
{
    private EnemyBlackboard blackboard;
    private Transform enemyTransform;
    private float detectRadius;

    public Detector(EnemyBlackboard blackboard, Transform enemy, float radius)
    {
        this.blackboard = blackboard;
        enemyTransform = enemy;
        detectRadius = radius;
    }

    public override State Evaluate()
    {
        Collider[] hits = Physics.OverlapSphere(enemyTransform.position, detectRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                blackboard.Player = hit.transform;
                return State.Success;
            }
        }

        return State.Failure;
    }
}