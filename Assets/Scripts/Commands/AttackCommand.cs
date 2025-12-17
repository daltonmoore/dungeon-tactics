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
                   && context.Path[^1].IsOccupied
                   && context.hit.collider.TryGetComponent(out IAttackable _);
        }

        public override void Handle(CommandContext context)
        {
            AbstractUnit unit = (AbstractUnit)context.commandable;
            IAttacker attacker = context.commandable as IAttacker;

            if (unit.Path is not null && unit.Path.Count > 0 && unit.Path[^1] == context.Path[^1])
            {
                attacker.Attack(context.hit.collider.GetComponent<IAttackable>());
            }
            else
            {
                attacker.ShowPath(context.Path);
            }
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}