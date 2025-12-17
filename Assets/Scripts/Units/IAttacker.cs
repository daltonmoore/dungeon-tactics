using System.Collections.Generic;

namespace Units
{
    public interface IAttacker
    {
        // stored path to compare when we click on a hex to show the path, then click again to move.
        public List<PathNodeHex> Path { get; set; }
        
        public void Attack(IAttackable attackable);
        void ShowPath(List<PathNodeHex> contextPath);
    }
}