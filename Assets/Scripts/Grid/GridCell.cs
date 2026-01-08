using System;
using DG.Tweening;
using EventBus;
using Events;
using Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Grid
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private GridConfig gridConfig;
        
        private BoxCollider2D _boxCollider2D;

        private void Awake()
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _boxCollider2D.isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("Grid");
        }

        public void Configure(GridConfig gridConfig)
        {
            this.gridConfig = gridConfig;
        }

        private void OnMouseEnter()
        {
            transform.DOScale(Vector3.one * gridConfig.HighlightScale, gridConfig.HighlightScaleLerpDuration)
                .onComplete += () => transform.DOScale(Vector3.one * gridConfig.DefaultScale, gridConfig.HighlightScaleLerpDuration);
            // Bus<GridCellHighlighted>.Raise(Owner.Player1, new GridCellHighlighted(this));
        }
    }
}
