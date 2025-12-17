using System.Collections.Generic;
using Units;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace Commands
{
    public struct CommandContext
    {
        public AbstractCommandable commandable { get; private set; }
        public RaycastHit2D hit { get; private set; }
        public MouseButton mouseButton { get; private set; }
        public Owner owner { get; private set; }
        public List<PathNodeHex> Path { get; private set; }
        
        public CommandContext(
            AbstractCommandable commandable,
            RaycastHit2D hit,
            List<PathNodeHex> path = null,
            MouseButton mouseButton = MouseButton.Left)
        {
            this.commandable = commandable;
            this.hit = hit;
            this.mouseButton = mouseButton;
            Path = path;
            owner = Owner.Player1;
        }
    }
}