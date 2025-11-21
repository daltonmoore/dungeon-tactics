using System;
using DG.Tweening;
using EventBus;
using Events;
using Units;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GridCell : MonoBehaviour, IPointerEnterHandler
    {
        private BoxCollider2D _boxCollider2D;
        private GridConfig _gridConfig;
        
        private void Awake()
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _boxCollider2D.isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("Grid");
        }

        public void Configure(GridConfig gridConfig)
        {
            _gridConfig = gridConfig;
        }

        public void OnPointerEnter(PointerEventData _)
        {
            transform.DOScale(Vector3.one * _gridConfig.HighlightScale, _gridConfig.HighlightScaleLerpDuration)
                .onComplete += () => transform.DOScale(Vector3.one * .75f, _gridConfig.HighlightScaleLerpDuration);
            Bus<GridCellHighlighted>.Raise(Owner.Player1, new GridCellHighlighted(this));
        }
    }
}
