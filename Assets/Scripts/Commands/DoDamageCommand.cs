using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Commands
{
    [CreateAssetMenu(fileName = "Do Damage Action", menuName = "Units/Commands/Do Damage")]
    public class DoDamageCommand : BaseCommand
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
                   && context.hit.collider.TryGetComponent(out IDamageable damageable)
                   && damageable.Owner != Owner.Player1;
        }

        public override void Handle(CommandContext context)
        {
            AbstractUnit unit = (AbstractUnit)context.commandable;
            IDamageable damageable = context.hit.collider.GetComponent<IDamageable>();

            damageable.TakeDamage(10);
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}