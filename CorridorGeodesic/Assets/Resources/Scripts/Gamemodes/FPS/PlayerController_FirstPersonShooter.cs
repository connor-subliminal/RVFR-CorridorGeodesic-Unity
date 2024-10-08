//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes: This player controller is based off of the tutorial series by Plai,
//  The Source Engine style step-up method is by Cobertos
// Plai Source: https://www.youtube.com/watch?v=LqnPeqoJRFY
// Cobertos Source: https://cobertos.com/blog/post/how-to-climb-stairs-unity3d
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[CreateAssetMenu(fileName="PlayerController_FirstPersonShooter", menuName="Neverway/ScriptableObjects/Pawns & Gamemodes/Controllers/PlayerController_FirstPersonShooter")]
public class PlayerController_FirstPersonShooter : PawnController
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [SerializeField] public float throwForce=150;


    //=-----------------=
    // Private Variables
    //=-----------------=
    private Vector3 moveDirection;
    private Vector3 slopMoveDirection;
    [HideInInspector] public float yRotation;
    [HideInInspector] public float xRotation;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GameInstance gameInstance;
    private InputActions.FirstPersonShooterActions fpsActions;
    private Rigidbody rigidbody;
    private Camera viewCamera;
    [SerializeField] private GameObject interactionVolume;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public override void PawnAwake(Pawn _pawn)
    {
        // Get references
        gameInstance = FindObjectOfType<GameInstance>();
        rigidbody = _pawn.GetComponent<Rigidbody>();
        viewCamera = _pawn.GetComponentInChildren<Camera>();
        
        // Setup inputs
        fpsActions = new InputActions().FirstPersonShooter;
        fpsActions.Enable();

        // Assign initial values
        rigidbody.mass = _pawn.currentState.gravityMultiplier;
        rigidbody.freezeRotation = true;
        
        // Subscribe to events
        _pawn.OnPawnDeath += () => { OnDeath(_pawn); };
    }
    
    public override void PawnUpdate(Pawn _pawn)
    {
        
        // Check for pause input and set cursor locking accordingly
        UpdatePauseMenu(_pawn);
        if (_pawn.isPaused) return;
        UpdateInteractionUsage(_pawn);
        
        // Debug Respawn
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Destroy(_pawn.gameObject);
        }
        
        UpdateMovement(_pawn);
        UpdateRotation(_pawn);
        UpdateJumping(_pawn);
        
        // Calculate Slope Movement
        slopMoveDirection = Vector3.ProjectOnPlane(moveDirection, _pawn.slopeHit.normal);
    }

    public override void PawnFixedUpdate(Pawn _pawn)
    {
        if (_pawn.isDead)
        {
            return;
        }
        
        MovePlayer(_pawn);
        
        // Set wall-running view tilt
        if (_pawn.GetComponent<Pawn_WallRun>())
        {
            viewCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, _pawn.GetComponent<Pawn_WallRun>().tilt);
        }
        else
        {
            viewCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        }
        _pawn.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private void UpdatePauseMenu(Pawn _pawn)
    {
        // Pause Game
        if (fpsActions.Pause.WasPressedThisFrame()) gameInstance.UI_ShowPause();

        // Lock mouse when unpaused, unlock when paused
        if (_pawn.isPaused)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;    
    }

    private void UpdateInteractionUsage(Pawn _pawn)
    {
        // Item usage
        if (!_pawn.physObjectAttachmentPoint) return;
        if (fpsActions.Secondary.IsPressed())
        {
            if (_pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false))
            {
                _pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false).UseSecondary();
            }
        }
        if (fpsActions.Secondary.WasReleasedThisFrame())
        {
            if (_pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false))
            {
                _pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false).ReleaseSecondary();
            }
        }
        if (fpsActions.Action.WasPressedThisFrame())
        {
            if (_pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false))
            {
                _pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false).UseSpecial();
            }
        }
        if (!_pawn.physObjectAttachmentPoint.heldObject)
        {
            if (fpsActions.Primary.WasPressedThisFrame())
            {
                if (_pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false))
                {
                    _pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false).UsePrimary();
                }
            }
        }
        // Throw object
        else
        {
            if (fpsActions.Primary.WasPressedThisFrame())
            {
                _pawn.physObjectAttachmentPoint.heldObject.
                    GetComponent<Rigidbody>().AddForce(_pawn.physObjectAttachmentPoint.transform.forward*throwForce, ForceMode.Impulse);
                _pawn.physObjectAttachmentPoint.heldObject.GetComponent<Object_Grabbable>().ToggleHeld();
            }
        }
        // Interact
        if (fpsActions.Interact.WasPressedThisFrame())
        {
            var interaction = Instantiate(interactionVolume, viewCamera.transform);
            interaction.transform.GetChild(0).GetComponent<Volume_TriggerInteraction>().targetPawn = _pawn;
            Destroy(interaction, 0.1f);
        }
    }
    
    private void UpdateMovement(Pawn _pawn)
    {
        // Pawn movement
        moveDirection = _pawn.transform.forward * fpsActions.Move.ReadValue<Vector2>().y + _pawn.transform.right * fpsActions.Move.ReadValue<Vector2>().x;
        ControlDrag(_pawn);
        ControlSprinting(_pawn);
    }

    private void UpdateRotation(Pawn _pawn)
    {
        // Pawn looking
        var multiplier = 0.01f;
        var applicationSettings = FindObjectOfType<ApplicationSettings>();
        yRotation += fpsActions.LookAxis.ReadValue<Vector2>().x*(20*applicationSettings.currentSettingsData.horizontalLookSpeed)*multiplier;
        xRotation -= fpsActions.LookAxis.ReadValue<Vector2>().y*(20*applicationSettings.currentSettingsData.verticalLookSpeed)*multiplier;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    private void UpdateJumping(Pawn _pawn)
    {
        //Debug.Log(_pawn.IsGrounded3D());
        if (fpsActions.Jump.WasPressedThisFrame() && _pawn.IsGrounded3D())
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
            rigidbody.AddForce(Vector3.up * _pawn.currentState.jumpForce, ForceMode.Impulse);
        }
    }
    
    private void MovePlayer(Pawn _pawn)
    {
        if (_pawn.IsGrounded3D() && !_pawn.IsGroundSloped3D())
        {
            rigidbody.AddForce(
                moveDirection.normalized * (_pawn.currentState.movementSpeed * _pawn.currentState.movementMultiplier),
                ForceMode.Acceleration);
        }
        else if (_pawn.IsGrounded3D() && _pawn.IsGroundSloped3D())
        {
            rigidbody.AddForce(
                slopMoveDirection.normalized * (_pawn.currentState.movementSpeed * _pawn.currentState.movementMultiplier),
                ForceMode.Acceleration);
        }
        else
        {
            rigidbody.AddForce(
                moveDirection.normalized * (_pawn.currentState.movementSpeed * (_pawn.currentState.movementMultiplier * _pawn.currentState.airMovementMultiplier)),
                ForceMode.Acceleration);
        }
    }

    private void ControlDrag(Pawn _pawn)
    {
        if (_pawn.IsGrounded3D())
        {
            rigidbody.drag = _pawn.currentState.groundDrag;
        }
        else
        {
            rigidbody.drag = _pawn.currentState.airDrag;
        }
    }

    private void ControlSprinting(Pawn _pawn)
    {
        if (fpsActions.Action.IsPressed() && _pawn.IsGrounded3D())
        {
            _pawn.currentState.movementSpeed = Mathf.Lerp(_pawn.currentState.movementSpeed,
                _pawn.defaultState.movementSpeed * _pawn.currentState.sprintSpeedMultiplier,
                _pawn.currentState.sprintAcceleration * Time.deltaTime);
        }
        else 
        {
            _pawn.currentState.movementSpeed = Mathf.Lerp(_pawn.currentState.movementSpeed,
                _pawn.defaultState.movementSpeed,
                _pawn.currentState.sprintAcceleration * Time.deltaTime);
        }
    }

    private void OnDeath(Pawn _pawn)
    {
        var rotation = viewCamera.transform.localPosition;
        
        // Drop held props
        if (_pawn.physObjectAttachmentPoint.heldObject)
        {
            if (_pawn.physObjectAttachmentPoint.heldObject.GetComponent<Object_Grabbable>())
            {
                _pawn.physObjectAttachmentPoint.heldObject.GetComponent<Object_Grabbable>().ToggleHeld();
            }
        }
        
        // Call the function on the georipper to remove all rifts
        // TODO this currently assumes the player is ONLY ever holding the georipper
        // An if statement should be added to check if there were any rifts or something
        _pawn.transform.GetComponentInChildren<Item_Geodesic_Utility>(false).UseSpecial();
        
        // Remove the HUD
        Destroy(GameInstance.GetWidget("WB_HUD"));
        // Add the respawn HUD
        FindObjectOfType<GameInstance>().UI_ShowDeathScreen();
        
        // Play the death animation
        _pawn.GetComponent<Animator>().Play("Death");
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
