using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

namespace Units
{
    public interface IDamageable
    {
        public Slider HealthBar { get; }
        public Owner Owner { get; set; }
        
        public void TakeDamage(int damage);
    }
}