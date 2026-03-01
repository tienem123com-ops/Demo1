using System.Collections.Generic;

public class Sequence : BTNode
{
    private List<BTNode> nodes;

    public Sequence(List<BTNode> nodes)
    {
        this.nodes = nodes;
    }

    public override State Evaluate()
    {
        foreach (var node in nodes)
        {
            State result = node.Evaluate();
            if (result != State.Success)
                return result;
        }
        return State.Success;
    }
}