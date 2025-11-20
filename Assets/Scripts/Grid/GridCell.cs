using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GridCell : MonoBehaviour, IPointerEnterHandler
    {
        private BoxCollider2D _boxCollider2D;
        
        private void Awake()
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _boxCollider2D.isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("Grid");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), .5f).onComplete += () => transform.DOScale(Vector3.one * .75f, .5f);
        }
    }
}
