using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Commands
{
    public class TurnBasedCommand : BaseCommand
    {
        
        public override bool CanHandle(CommandContext context)
        {
            return context is
                   {
                       hit:
                       {
                           collider: not null
                       }
                   }
                   && context.commandable.TryGetComponent(out BattleUnit battler)
                   && battler.IsMyTurn;
        }

        public override void Handle(CommandContext context)
        {
            AbstractUnit unit = (AbstractUnit)context.commandable;
            BattleUnit battler = unit.GetComponent<BattleUnit>();

            battler.IsMyTurn = false;
            battler.EndedTurn = true;
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}