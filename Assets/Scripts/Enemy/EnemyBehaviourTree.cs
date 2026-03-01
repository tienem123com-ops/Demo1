using UnityEngine;
using System.Collections.Generic;

public class EnemyBehaviourTree
{
    private BTNode root;

    public EnemyBehaviourTree(
        EnemyBlackboard blackboard,
        Transform enemyTransform,
        EnemyAttack attack)
    {
        root = new Selector(new List<BTNode>
        {
            new Sequence(new List<BTNode>
            {
                new Detector(blackboard, enemyTransform, 10f),
                new MoveToPlayerNode(blackboard, enemyTransform),
                new AttackNode(blackboard, attack)
            })
        });
    }

    public void Tick()
    {
        root.Evaluate();
    }
}
