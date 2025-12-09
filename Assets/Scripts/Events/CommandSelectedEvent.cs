using Commands;
using EventBus;

namespace Events
{
    public struct CommandSelectedEvent : IEvent
    {
        public BaseCommand Command { get; }
        
        public CommandSelectedEvent(BaseCommand command)
        {
            Command = command;
        }
    }
}