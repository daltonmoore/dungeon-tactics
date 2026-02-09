using System.Collections.Generic;
using TacticsCore.Commands;
using TacticsCore.Data;
using TacticsCore.Interfaces;
using TacticsCore.Units;
using Units;
using UnityEngine;

namespace Commands
{
    [CreateAssetMenu(fileName = "Do Damage Action", menuName = "Units/Commands/Do Damage")]
    public class DoDamageCommand : TurnBasedCommand
    {
        
        public override bool CanHandle(CommandContext context)
        {
            return base.CanHandle(context)
                   && context.commandable.TryGetComponent(out BattleUnit _)
                   && context.hit.collider.TryGetComponent(out IDamageable damageable)
                   && damageable.Owner != context.commandable.Owner;
        }

        public override void Handle(CommandContext context)
        {
            base.Handle(context);
            IDamageable damageable = context.hit.collider.GetComponent<IDamageable>();
            BattleUnit battler = context.commandable.GetComponent<BattleUnit>();

            damageable.TakeDamage(battler.RollDamage());
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}