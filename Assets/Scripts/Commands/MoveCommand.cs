using System.Collections;
using System.Collections.Generic;
using HexGrid;
using Units;
using UnityEngine;
using UnityEngine.AI;

namespace Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "Units/Commands/Move", order = 100)]
    public class MoveCommand : BaseCommand
    {
        private List<Vector3> _path;
        
        public override bool CanHandle(CommandContext context)
        {
            return context.commandable is AbstractUnit
                && context.hit.collider != null
                && HasPath(context.commandable.Transform.position, context.hit.collider.transform.position);
        }

        private bool HasPath(Vector3 startPos, Vector3 endPos)
        {
            Pathfinder.Instance.FindPath(startPos, endPos, out _path);
            
            return _path != null;
        }

        public override void Handle(CommandContext context)
        {
            Debug.Log("Move Command");
            AbstractUnit unit = (AbstractUnit)context.commandable;
            
            unit.ShowPath(_path);
            // unit.MoveTo(_path);
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}