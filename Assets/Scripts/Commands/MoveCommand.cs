using TacticsCore.Commands;
using TacticsCore.HexGrid;
using TacticsCore.Interfaces;
using TacticsCore.Units;
using Units;
using UnityEngine;

namespace Commands
{
    [CreateAssetMenu(fileName = "Move Action", menuName = "Units/Commands/Move")]
    public class MoveCommand : BaseCommand
    {
        
        public override bool CanHandle(CommandContext context)
        {
            return context.commandable is LeaderUnit
                && context.hit.collider != null
                && context.Path != null;
        }

        public override void Handle(CommandContext context)
        {
            Debug.Log("Move Command");
            LeaderUnit unit = (LeaderUnit)context.commandable;

            if (unit.Path is not null && unit.Path.Count > 0 && unit.Path[^1] == context.Path[^1])
            {
                if (unit.BattleNode != null)
                {
                    Debug.Log("ATTACK");
                    unit.Attack(unit.EnemyUnit);
                }
                else
                {
                    unit.MoveTo(context.Path);
                }
            }
            else
            {
                unit.ShowPath(context.Path, out PathNodeHex battleNode, out LeaderUnit enemyUnit);
                unit.BattleNode = battleNode;
                unit.EnemyUnit = enemyUnit;
            }
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}