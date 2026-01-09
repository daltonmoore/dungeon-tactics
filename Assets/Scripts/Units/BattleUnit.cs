using System;
using UnityEngine;

namespace Units
{
    public class BattleUnit : AbstractUnit
    {
        [SerializeField] public GameObject currentTurnHighlight;
        
        public bool IsMyTurn { get; set; }
        public bool EndedTurn { get; set; }
        
        public enum EActionType
        {
            Attack
        }
        
        

        public void HighlightForCurrentTurn()
        {
            if (currentTurnHighlight != null)
            {
                currentTurnHighlight.SetActive(true);
            }
        }

        public void ResetHighlightForCurrentTurn()
        {
            if (currentTurnHighlight != null)
            {
                currentTurnHighlight.SetActive(false);
            }
        }

        public void HighlightForHover()
        {
            if (decal != null)
            {
                decal.gameObject.SetActive(true);
            }
        }

        public void ResetHighlightForHover()
        {
            if (decal != null)
            {
                decal.gameObject.SetActive(false);
            }
        }

        public void DoAction()
        {
            throw new NotImplementedException();
        }

    }
}