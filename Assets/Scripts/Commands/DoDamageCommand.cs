using System.Collections.Generic;
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
                   && context.hit.collider.TryGetComponent(out IDamageable damageable)
                   && damageable.Owner != context.commandable.Owner;
        }

        public override void Handle(CommandContext context)
        {
            base.Handle(context);
            IDamageable damageable = context.hit.collider.GetComponent<IDamageable>();

            damageable.TakeDamage(10);
        }

        public override bool IsLocked(CommandContext context) => false;
    }
}