using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Commands
{
    [CreateAssetMenu(fileName = "Attack Action", menuName = "Units/Commands/Attack")]
    public class AttackCommand : BaseCommand
    {
        
        public override bool CanHandle(CommandContext context)
        {
            return context is 
                   { 
                       commandable: IAttacker, 
                       hit:
                       {
                           collider: not null
                       }, 
                       Path: not null
                   }
                   && context.hit.collider.TryGetComponent(out IAttackable _);
        }

        public override void Handle(CommandContext context)
        {
            Debug.Log("Attack Command: Collider Name " 
                      + context.hit.collider.name 
                      + " Path Final Node World Pos " 
                      + context.Path[^1].worldPosition 
                      + " Hit point " + context.hit.point);
            AbstractUnit unit = (AbstractUnit)context.commandable;
            IAttacker attacker = context.commandable as IAttacker;
            IAttackable attackable = context.hit.collider.GetComponent<IAttackable>();

            if (unit.Path is not null && unit.Path.Count > 0 && unit.Path[^1] == context.Path[^1])
            {
                attacker.Attack(attackable);
            }
            else
            {
                attacker.ShowPath(context.Path, attackable, out PathNodeHex battleNode);
                unit.BattleNode = battleNode;
            }
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}