using System;
using System.Collections.Generic;
using System.Linq;
using Commands;
using Drawing;
using EventBus;
using Events;
using HexGrid;
using NUnit.Framework;
using UI;
using Units;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using MouseButton = UnityEngine.InputSystem.LowLevel.MouseButton;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D cameraTarget;
        [SerializeField] private CameraConfig cameraConfig;
        [SerializeField] private new Camera camera;
        [SerializeField] private GameObject cursor;
        [SerializeField] private LayerMask selectableUnitsLayers;
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private LayerMask floorLayers;
        
        private BaseCommand _activeCommand;
        private ISelectable _selectedUnit;
        private GameObject _visualPrefabInstance;

        private void Awake()
        {
            Bus<HexHighlighted>.OnEvent[Owner.Player1] += HandleHexHighlighted; 
            Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] += HandleCommandSelected;
        }

        private void Start()
        {
            if (cameraTarget.TryGetComponent(out BoxCollider2D boxCollider2D))
            {
                boxCollider2D.size = new Vector2(camera.orthographicSize * 2 * camera.aspect, camera.orthographicSize * 2);
            }
        }

        private void OnDestroy()
        {
            Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] -= HandleCommandSelected;
        }

        private void Update()
        {
            HandlePanning();
            HandleSelect();
            HandleRightClick();
            HandleCommandVisualPrefab();
        }

        private void HandleCommandVisualPrefab()
        {
            if (_visualPrefabInstance == null) return;
            
            if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                Destroy(_visualPrefabInstance);
                _visualPrefabInstance = null;
                _activeCommand = null;
                return;
            }
            
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, floorLayers);
            if (hit.collider != null)
            {
                _visualPrefabInstance.transform.position = hit.point;
                HandleCommandVisualOrientation();
                // bool allRestrictionsPass = _activeCommand.AllRestrictionsPass(hit.point);

                // _ghostRenderer.material.SetColor(TINT,
                //     allRestrictionsPass ? availableToPlaceTintColor : errorTintColor);
                // _ghostRenderer.material.SetColor(FRESNEL,
                //     allRestrictionsPass ? availableToPlaceFresnelColor : errorFresnelColor);
            }
        }

        private void HandleCommandVisualOrientation()
        {
            var unitPos = _selectedUnit.Transform.position;
            var visualPos = _visualPrefabInstance.transform.position;
            var visualPosInt = new Vector3Int(Mathf.RoundToInt(visualPos.x), Mathf.RoundToInt(visualPos.y));
                
            // Calculate the direction vector from the unit to the visual
            var direction = visualPosInt - unitPos;
                
            // Use the unit's right direction vector (not a position in space)
            var unitRight = _selectedUnit.Transform.right;

            var cross = Vector3.Cross(unitRight, direction);
                
            // Debug.Log($"Cross Magnitude: {cross.magnitude}");
            // Debug.Log($"Cross {cross}");

            // Use a small epsilon for float comparison instead of == 0
            if (cross.sqrMagnitude < 0.001f)
            {
                var dot = Vector3.Dot(unitRight, direction);
                if (dot < 0)
                {
                    // Debug.Log("Left / Behind (Opposite to Right)");
                    _visualPrefabInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -180));
                }
                else
                {
                    // Debug.Log("Right / Front (Aligned with Right)");
                    _visualPrefabInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                }
            }
            else
            {
                if (cross.z < 0)
                {
                    // Debug.Log("Below");
                    _visualPrefabInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90));
                }
                else
                {
                    // Debug.Log("Above");
                    _visualPrefabInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                }
            }
                
            Debug.DrawLine(unitPos, unitPos + unitRight, Color.red, 0); // Draw local right
            Debug.DrawLine(unitPos, visualPos, Color.green, 0); // Draw direction to visual
        }

        private void HandleSelect()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleMouseDown();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleMouseUp();
            }
        }

        private void HandleMouseDown()
        {
            
        }
        
        private void HandleMouseUp()
        {
            HandleLeftClick();
            _selectedUnit?.Select();
        }
        
        private void HandleCommandSelected(CommandSelectedEvent evt)
        {
            _activeCommand = evt.Command;

            if (!_activeCommand.RequiresClickToActivate)
            {
                ActivateCommand(new RaycastHit2D());
            }
            else if (_activeCommand.VisualPrefab != null)
            {
                _visualPrefabInstance = Instantiate(_activeCommand.VisualPrefab);
                // _ghostRenderer = _ghostInstance.GetComponentInChildren<MeshRenderer>();
            }
        }

        private void HandlePanning()
        {
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();
            cameraTarget.linearVelocity = new Vector2(moveAmount.x, moveAmount.y);
        }
        
        private Vector2 GetKeyboardMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;
            
            if (Keyboard.current.upArrowKey.isPressed)
            {
                moveAmount.y += cameraConfig.KeyboardPanSpeed;
            }
        
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                moveAmount.x += cameraConfig.KeyboardPanSpeed;
            }
        
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                moveAmount.x -= cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.downArrowKey.isPressed)
            {
                moveAmount.y -= cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;
        }
        
        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;
            
            if (!cameraConfig.EnableEdgePan) { return moveAmount; }
            
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (mousePosition.x <= cameraConfig.EdgePanSize)
            {
                moveAmount.x -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
            {
                moveAmount.x += cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y >= screenHeight - cameraConfig.EdgePanSize)
            {
                moveAmount.y += cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y <= cameraConfig.EdgePanSize)
            {
                moveAmount.y -= cameraConfig.MousePanSpeed;           
            }
            
            return moveAmount;
        }
        
        private void HandleLeftClick()
        {
            if (camera == null) { return; } 
            
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit2D selectableUnitHit = Physics2D.Raycast(
                ray.origin, 
                ray.direction,
                float.MaxValue,
                selectableUnitsLayers);
            RaycastHit2D commandHit = Physics2D.Raycast(
                ray.origin, 
                ray.direction, 
                float.MaxValue,
                selectableUnitsLayers | interactableLayers | floorLayers);
            
            if (_activeCommand is null 
                && !RuntimeUI.IsPointerOverCanvas()
                && selectableUnitHit.collider != null
                && selectableUnitHit.collider.TryGetComponent(out ISelectable selectable)
                && selectableUnitHit.collider.TryGetComponent(out AbstractUnit unit)
                && unit.Owner == Owner.Player1)
            {
                selectable.Select();
                _selectedUnit = selectable;
            }
            else if (_activeCommand is not null 
                     && !RuntimeUI.IsPointerOverCanvas()
                     && commandHit.collider != null)
            {
                ActivateCommand(commandHit);
            }
            
        }

        private void HandleRightClick()
        {
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            AbstractUnit abstractUnit = _selectedUnit as AbstractUnit;
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, 
                interactableLayers | floorLayers);

            if (hits.Length == 0) { return; }
            
            var hitsOrderedByLayer = hits.OrderBy(hit => hit.transform.gameObject.layer);
            RaycastHit2D hit = hitsOrderedByLayer.First();
            
            if (Mouse.current.rightButton.wasReleasedThisFrame
                && hit.collider != null
                && abstractUnit != null)
            {
                Pathfinder.Instance.FindPath(_selectedUnit.Transform.position, hit.point, out List<PathNodeHex> path);
                CommandContext context = new(_selectedUnit as AbstractCommandable, hit, path, MouseButton.Right);
                foreach (ICommand command in GetAvailableCommands(abstractUnit))
                {
                    if (command.CanHandle(context))
                    {
                        command.Handle(context);
                            
                        break;
                    }
                }
            }
        }
        
        private void HandleHexHighlighted(HexHighlighted args)
        {
            cursor.transform.position = args.PathNodeHex.worldPosition;
        }
        
        private void ActivateCommand(RaycastHit2D hit)
        {
            // TODO: Do we need ghosts?
            // if (_ghostInstance != null)
            // {
            //     Destroy(_ghostInstance);
            //     _ghostInstance = null;
            // }
            
            CommandContext context = new(_selectedUnit as AbstractUnit, hit);
            if (_activeCommand.CanHandle(context))
            {
                _activeCommand.Handle(context);
            }
                
            _activeCommand = null;
        }
        
        private List<BaseCommand> GetAvailableCommands(AbstractUnit unit)
        {
            OverrideCommandsCommand[] overrideCommands =
                unit.AvailableCommands
                    .Where(command => command is OverrideCommandsCommand)
                    .Cast<OverrideCommandsCommand>()
                    .ToArray();

            List<BaseCommand> allAvailableCommands = new();
            foreach (OverrideCommandsCommand overrideCommand in overrideCommands)
            {
                allAvailableCommands.AddRange(overrideCommand.Commands.Where(command => command is not OverrideCommandsCommand));
            }
            
            allAvailableCommands.AddRange(unit.AvailableCommands
                .Where(command => command is not OverrideCommandsCommand)
            );

            return allAvailableCommands;
        }
    }
}
