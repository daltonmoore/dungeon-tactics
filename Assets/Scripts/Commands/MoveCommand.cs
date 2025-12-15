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
        private List<PathNodeHex> _path;
        
        public override bool CanHandle(CommandContext context)
        {
            return context.commandable is AbstractUnit
                && context.hit.collider != null
                && HasPath(context.commandable.Transform.position, context.hit.collider.transform.position)
                && !_path[^1].IsOccupied;
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

            if (unit.Path != null && unit.Path.Count > 0 && unit.Path[^1] == _path[^1])
            {
                unit.MoveTo(_path);
            }
            else
            {
                unit.ShowPath(_path);
            }
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}