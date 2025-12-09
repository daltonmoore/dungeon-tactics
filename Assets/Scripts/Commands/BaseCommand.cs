using UnityEngine;

namespace Commands
{
    public abstract class BaseCommand : ScriptableObject, ICommand
    {
        [field: SerializeField] public string Name { get; private set; } = "Command";
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: Range(-1, 8)] [field: SerializeField] public int Slot { get; private set; }
        [field: SerializeField] public bool RequiresClickToActivate { get; private set; } = true;
        [field: SerializeField] public GameObject VisualPrefab { get; private set; }
        
        public abstract bool CanHandle(CommandContext context);

        public abstract void Handle(CommandContext context);
        
        public abstract bool IsLocked(CommandContext context);
        
    }
}