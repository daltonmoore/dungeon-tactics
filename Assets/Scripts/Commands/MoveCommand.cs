using System.Collections;
using System.Collections.Generic;
using HexGrid;
using Units;
using UnityEngine;
using UnityEngine.AI;

namespace Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "Units/Commands/Move")]
    public class MoveCommand : BaseCommand
    {
        
        public override bool CanHandle(CommandContext context)
        {
            return context.commandable is AbstractUnit
                && context.hit.collider != null
                && context.Path != null
                && !context.Path[^1].IsOccupied;
        }

        public override void Handle(CommandContext context)
        {
            Debug.Log("Move Command");
            AbstractUnit unit = (AbstractUnit)context.commandable;

            if (unit.Path is not null && unit.Path.Count > 0 && unit.Path[^1] == context.Path[^1])
            {
                unit.MoveTo(context.Path);
            }
            else
            {
                unit.ShowPath(context.Path);
            }
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}