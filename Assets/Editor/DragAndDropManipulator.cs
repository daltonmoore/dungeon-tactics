using System;
using System.Collections.Generic;
using Units;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class DragAndDropManipulator : PointerManipulator
    {
        public bool beganDrag { get; set; }
        private Vector2 targetStartPosition { get; set; }
        private Vector3 pointerStartPosition { get; set; }
        private bool enabled { get; set; }
        private VisualElement root { get; }

        public DragAndDropManipulator(VisualElement target, VisualElement root)
        {
            this.target = target;
            this.root = root;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            // It's a safe bet to register it on the root element to cover all initial layout calculations
            target.RegisterCallback<GeometryChangedEvent>(OnLayoutComplete);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<GeometryChangedEvent>(OnLayoutComplete);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }
        
        private void OnLayoutComplete(GeometryChangedEvent evt)
        {
            SetPositionOfTargetToSpecificSlot(Enum.Parse<BattleUnitPosition>(target.name));
        }

        // this method stores the starting position of target and the pointer,
        // makes target capture the pointer, and denotes that a drag is now in progress
        private void PointerDownHandler(PointerDownEvent evt)
        {
            targetStartPosition = target.transform.position;
            pointerStartPosition = evt.position;
            target.CapturePointer(evt.pointerId);
            enabled = true;
        }
        
        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // if both are true, calculates a new position for target within the bounds of the window.
        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                Vector3 pointerDelta = evt.position - pointerStartPosition;

                target.transform.position = new Vector2(
                    Mathf.Clamp(targetStartPosition.x + pointerDelta.x, 0, target.panel.visualTree.worldBound.width),
                    Mathf.Clamp(targetStartPosition.y + pointerDelta.y, 0, target.panel.visualTree.worldBound.height));
                beganDrag = true;
            }
        }

        // This method checks whether a drag is in progress and whether target has captured the pointer.
        // if both are true, makes target release the pointer.
        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
            {
                target.ReleasePointer(evt.pointerId);
            }
        }

        // This method checks whether a drag is in progress. If true, queries the root
        // of the visual tree to find all slots, decides which slot is the closest one
        // that overlaps target, and sets the position of target so that it rests on top
        // of that slot. Sets the position of target back to its original position
        // if there is no overlapping slot.
        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (enabled)
            {
                VisualElement slotsContainer = root.Q<VisualElement>("slots");
                UQueryBuilder<VisualElement> allSlots = slotsContainer.Query<VisualElement>(className: "slot");
                UQueryBuilder<VisualElement> overlappingSlots = allSlots.Where(OverlapsTarget);
                VisualElement closestOverlappingSlot = FindClosestSlot(overlappingSlots);
                Vector3 closestPos = Vector3.zero;
                if (closestOverlappingSlot != null)
                {
                    BattleUnitPosition targetUnitPosition = Enum.Parse<BattleUnitPosition>(target.name);
                    BattleUnitPosition occupantUnitPosition = Enum.Parse<BattleUnitPosition>(closestOverlappingSlot.name);
                    
                    closestPos = RootSpaceOfSlot(closestOverlappingSlot);
                    closestPos = new Vector2(closestPos.x - 5, closestPos.y - 5);

                    VisualElement slotOccupant = root.Query<VisualElement>(className: "party-unit-button").Where(s => s.name == closestOverlappingSlot.name).ToList()[0];
                    slotOccupant.transform.position = RootSpaceOfSlot(FindSlot(targetUnitPosition));
                    // swap units in slots
                    // for example
                    // [ ]    [ ]                           [ ]    [ ]
                    // [x] -> [o] moving x to o's slot      [o]    [x] x would be the target and o would be the occupant
                    // [ ]    [ ]                           [ ]    [ ]
                    
                    PartyCustomEditor.SwapUnit(targetUnitPosition, occupantUnitPosition);
                    slotOccupant.name = target.name;


                    target.name = closestOverlappingSlot.name;
                }
                
                target.transform.position = closestOverlappingSlot != null ? closestPos : targetStartPosition;
                
                enabled = false;
                beganDrag = false;
            }
        }

        private bool OverlapsTarget(VisualElement slot) => target.worldBound.Overlaps(slot.worldBound);

        private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots)
        {
            List<VisualElement> slotsList = slots.ToList();
            float bestDistanceSq = float.MaxValue;
            VisualElement closest = null;
            foreach (VisualElement slot in slotsList)
            {
                Vector3 displacement = RootSpaceOfSlot(slot) - target.transform.position;
                float distanceSq = displacement.sqrMagnitude;
                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    closest = slot;
                }
            }
            return closest;
        }

        private VisualElement FindSlot(BattleUnitPosition battleUnitPosition)
        {
            VisualElement slotsContainer = root.Q<VisualElement>("slots");
            UQueryBuilder<VisualElement> allSlots = slotsContainer.Query<VisualElement>(className: "slot");
            return allSlots.Where(slot => slot.name == battleUnitPosition.ToString());
        }

        public void SetPositionOfTargetToSpecificSlot(BattleUnitPosition battleUnitPosition)
        {
            target.transform.position = RootSpaceOfSlot(FindSlot(battleUnitPosition));
        }

        private Vector3 RootSpaceOfSlot(VisualElement slot)
        {
            Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
            return root.WorldToLocal(slotWorldSpace);
        }
    }
}