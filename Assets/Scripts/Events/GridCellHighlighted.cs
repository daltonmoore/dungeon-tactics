using EventBus;
using Grid;

namespace Events
{
    public struct GridCellHighlighted : IEvent
    {
        public GridCell GridCell;

        public GridCellHighlighted(GridCell gridCell)
        {
            GridCell = gridCell;
        }
    }
}