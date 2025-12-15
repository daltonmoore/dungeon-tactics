using EventBus;
using Grid;

namespace Events
{
    public struct HexHighlighted : IEvent
    {
        public PathNodeHex PathNodeHex;

        public HexHighlighted(PathNodeHex pathNodeHex)
        {
            PathNodeHex = pathNodeHex;
        }
    }
}