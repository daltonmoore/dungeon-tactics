using System;
using System.Collections.Generic;
using System.Linq;
using TacticsCore.Commands;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.Events;
using TacticsCore.Units;
using UI.Components;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Containers
{
    public class ActionsUI : MonoBehaviour, IUIElement<AbstractCommandable>
    {
        [SerializeField] private UIActionButton[] actionButtons;
        
        public void EnableFor(AbstractCommandable unit)
        {
            RefreshButtons(unit);
        }

        public void Disable()
        {
            foreach (UIActionButton button in actionButtons)
            {
                button.Disable();
            }
        }
        
        private void RefreshButtons(AbstractCommandable selectedUnit)
        {
            IEnumerable<BaseCommand> availableCommands = selectedUnit != null
                ? selectedUnit.AvailableCommands 
                : Array.Empty<BaseCommand>();

            for (int i = 0; i < actionButtons.Length; i++)
            {
                BaseCommand commandForSlot = availableCommands.FirstOrDefault(action => action.Slot == i);

                if (commandForSlot is not null)
                {
                    actionButtons[i].EnableFor(commandForSlot, selectedUnit, HandleClick(commandForSlot));
                }
                else
                {
                    actionButtons[i].Disable();
                }
            }
        }
        
        private UnityAction HandleClick(BaseCommand action)
        {
            return () => Bus<CommandSelectedEvent>.Raise(Owner.Player1, new CommandSelectedEvent(action));
        }
    }
}