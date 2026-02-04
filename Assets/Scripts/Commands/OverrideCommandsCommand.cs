using TacticsCore.Commands;
using Units;
using UnityEngine;

namespace Commands
{
    [CreateAssetMenu(fileName = "Override Commands", menuName = "Units/Commands/Override", order = 110)]
    public class OverrideCommandsCommand : BaseCommand
    {
        [field: SerializeField] public BaseCommand[] Commands { get; private set; }
        
        public override bool CanHandle(CommandContext context)
        {
            return context.commandable != null;
        }

        public override void Handle(CommandContext context)
        {
            context.commandable.SetCommandOverrides(Commands);
        }
     
        public override bool IsLocked(CommandContext context) => false;
    }
}