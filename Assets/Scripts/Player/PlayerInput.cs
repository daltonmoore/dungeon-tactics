using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using Commands;
using Drawing;
using EventBus;
using Events;
using HexGrid;
using NUnit.Framework;
using UI;
using Units;
using Unity.Cinemachine;
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
        [SerializeField] private GameObject fakeCursor;
        [SerializeField] private LayerMask selectableUnitsLayers;
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private LayerMask floorLayers;
        
        private BaseCommand _activeCommand;
        private ISelectable _selectedUnit;
        private GameObject _visualPrefabInstance;
        private CinemachineBrain _cinemachineBrain;
        private CinemachineCamera _cinemachineCamera;
        private BoxCollider2D _cameraTargetCollider;
        private bool _isDragging;
        private Vector2 _dragOrigin;
        private Vector3 _lastMousePos;

        private void Awake()
        {
            // Cursor.visible = false;
            // Cursor.lockState = CursorLockMode.Confined;
            Bus<HexHighlighted>.OnEvent[Owner.Player1] += HandleHexHighlighted; 
            Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] += HandleCommandSelected;
            _cinemachineBrain = GetComponent<CinemachineBrain>();
        }

        private void Start()
        {
            if (cameraTarget.TryGetComponent(out BoxCollider2D boxCollider2D))
            {
                boxCollider2D.size = new Vector2(camera.orthographicSize * 2 * camera.aspect, camera.orthographicSize * 2);
            }
            
            
            StartCoroutine(WaitUntilACameraIsActive());
            _cameraTargetCollider = cameraTarget.GetComponent<BoxCollider2D>();
        }

        private void OnDestroy()
        {
            Bus<CommandSelectedEvent>.OnEvent[Owner.Player1] -= HandleCommandSelected;
        }

        private void Update()
        {
            // Start dragging
            if (Input.GetMouseButtonDown(0)) // Use 0 for left click, 1 for right, 2 for middle
            {
                _isDragging = true;
                // Capture the starting position in world space
                _dragOrigin = camera.ScreenToWorldPoint(Input.mousePosition);

                using (Draw.ingame.WithDuration(4f))
                {
                    Draw.ingame.Circle(new Vector3(_dragOrigin.x, _dragOrigin.y, 0), Vector3.forward, 0.5f, Color.red);
                }
            }

            // Stop dragging
            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (!_isDragging)
            {
                var cursorWorldPoint = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                cursorWorldPoint.z = 0;
                fakeCursor.transform.position = cursorWorldPoint;
                HandlePanning();
            }
            
            HandleScroll();
            HandleSelect();
            HandleRightClick();
            HandleCommandVisualPrefab();
        }

        private void FixedUpdate()
        {
            if (_isDragging)
            {
                fakeCursor.transform.position = _dragOrigin;
                Vector2 delta = Mouse.current.delta.ReadValue();
                if (delta.sqrMagnitude > 0)
                {
                    Vector2 awayFromDragOrigin = -delta - _dragOrigin;
                    Vector2 force = awayFromDragOrigin * cameraConfig.MousePanSpeed -
                                    cameraTarget.linearVelocity * cameraConfig.PanDamping;

                    cameraTarget.AddForce(force, ForceMode2D.Force);
                    cameraTarget.linearVelocity = Vector2.ClampMagnitude(cameraTarget.linearVelocity, 1f);
                }
                else
                {
                    cameraTarget.linearVelocity = Vector2.zero;
                }
            }
        }

        private IEnumerator WaitUntilACameraIsActive()
        {
            yield return new WaitUntil(() =>
            {
                _cinemachineCamera = _cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
                return _cinemachineCamera != null;
            });
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

        private void HandleScroll()
        {
            var scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0 && _cinemachineCamera != null)
            {
                float newOrthographicSize = _cinemachineCamera.Lens.OrthographicSize - scroll * cameraConfig.ZoomSpeed;

                _cinemachineCamera.Lens.OrthographicSize =
                    Mathf.Clamp(newOrthographicSize, cameraConfig.MinZoomDistance, cameraConfig.MaxZoomDistance);
                
                _cameraTargetCollider.size = new Vector2(camera.orthographicSize * 2 * camera.aspect, camera.orthographicSize * 2);
            }
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
            if (!Mouse.current.rightButton.wasReleasedThisFrame) return;
            
            Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, 
                interactableLayers | floorLayers | selectableUnitsLayers);

            if (hits.Length == 0) { return; }

            var hitsOrderedByLayerThenByIsUnit = hits.OrderByDescending(hit =>
                hit.transform.gameObject.TryGetComponent(out AbstractUnit _) ? 1 : 0);
                
            RaycastHit2D hit = hitsOrderedByLayerThenByIsUnit.First();
            
            Debug.Log($"Right Click Hit: {hit.collider.gameObject.name}");
            
            if (hit.collider != null
                && hit.collider.gameObject.layer != LayerMask.NameToLayer("FogOfWar"))
            {
                CommandContext context;
                if (BattleManager.Instance.BattleInProgress)
                {
                    context = new CommandContext(BattleManager.Instance.CurrentBattler, hit);
                }
                else
                {
                    Pathfinder.Instance.FindPath(_selectedUnit.Transform.position, hit.point, out List<PathNodeHex> path);
                    context = new CommandContext(_selectedUnit as AbstractCommandable, hit, path, MouseButton.Right);
                }
                
                foreach (ICommand command in GetAvailableCommands(context.commandable))
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
            if (!_isDragging && cursor != null)
            {
                cursor.transform.position = args.PathNodeHex.worldPosition;
            }
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
        
        private List<BaseCommand> GetAvailableCommands(AbstractCommandable unit)
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
