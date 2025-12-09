using Units;
using UnityEngine;
using UnityEngine.AI;

namespace Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "Units/Commands/Move", order = 100)]
    public class MoveCommand : BaseCommand
    {
        public override bool CanHandle(CommandContext context)
        {
            return context.commandable is AbstractUnit;
        }

        public override void Handle(CommandContext context)
        {
            Debug.Log("Move Command");
            var agent = context.commandable.GetComponent<NavMeshAgent>();
            // agent.Move(context.hit.point);
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}