using System.Collections.Generic;

public class Selector : BTNode
{
    private List<BTNode> nodes;

    public Selector(List<BTNode> nodes)
    {
        this.nodes = nodes;
    }

    public override State Evaluate()
    {
        foreach (var node in nodes)
        {
            State result = node.Evaluate();
            if (result == State.Success || result == State.Running)
                return result;
        }
        return State.Failure;
    }
}