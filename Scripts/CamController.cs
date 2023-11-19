using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamController : MonoBehaviour
{
    [Min(0f)]
    [Tooltip("General movement speed")]
    public float MovementSpeed = 7.5f;
    [Min(0f)]
    [Tooltip("Rotation speed")]
    public float RotationSpeed = 100.0f;
    [Min(0f)]
    [Tooltip("Vertical movement speed")]
    public float VerticalMovementSpeed = 2.5f;
    [Min(0f)]
    [Tooltip("Duration required for the camera to accelerate and decelerate if easing is enabled")]
    public float EasingTime = 0.2f;
    [Min(0f)]
    [Tooltip("This value indicates how quickly the general movement speed can be changed per second via gamepad input")]
    public float XZSpeedChangeRate = 5.0f;
    [Min(0f)]
    [Tooltip("This value indicates how quickly the vertical movement speed can be changed per second via gamepad input")]
    public float YSpeedChangeRate = 2.0f;
    [Min(0f)]
    [Tooltip("This value indicates how quickly the rotation speed can be changed per second via gamepad input")]
    public float RotationSpeedChangeRate = 20.0f;
    [Min(0f)]
    [Tooltip("This value indicates how quickly the easing time can be changed per second via gamepad input")]
    public float EasingTimeChangeRate = 0.1f;
    [Min(0f)]
    [Tooltip("This value indicates how quickly the FOV (field of view) value can be changed per second via gamepad input")]
    public float FOVChangeRate = 10.0f;
    [Tooltip("Should the camera movement be restricted to the X and Z axes?")]
    public bool RestrictYMovement = false;
    [Tooltip("Should the camera accelerate/decelerate at the start/end of movement input or abruptly start/stop at a constant speed?")]
    public bool EasingEnabled = true;
    [Tooltip("Which layers should be accessible by the fly-to function?")]
    public LayerMask TeleportLayerMask;

    private GameObject _imgCrosshair;
    private GameObject _panelBlackBar;
    private TextMeshProUGUI _txtSpeed;
    private TextMeshProUGUI _txtFOV;
    private TextMeshProUGUI _txtRestrictY;
    private TextMeshProUGUI _txtEasing;
    private float _verticalMovement;
    private float _xRotation = 0f;
    private float _yRotation = 0f;
    private Vector3 _currentVelocity;
    private float _currentVerticalVelocity;
    private Camera _cameraComponent;
    private float _initialFOV;
    private bool _isTeleporting = false;
    private Vector3 _teleportDestination;
    private float _teleportStartTime;
    private int _camUiState = 2;
    private float _lastDpadRightPressTime = 0f;
    private const float _doublePressInterval = 0.5f;

    private void Start()
    {
        _cameraComponent = Camera.main;
        _initialFOV = _cameraComponent.fieldOfView;

        _txtSpeed = GameObject.Find("TxtSpeed")?.GetComponent<TextMeshProUGUI>();
        _txtFOV = GameObject.Find("TxtFOV")?.GetComponent<TextMeshProUGUI>();
        _txtRestrictY = GameObject.Find("TxtRestrictY")?.GetComponent<TextMeshProUGUI>();
        _txtEasing = GameObject.Find("TxtEasing")?.GetComponent<TextMeshProUGUI>();

        _imgCrosshair = GameObject.Find("ImgCrosshair");
        _panelBlackBar = GameObject.Find("BlackBar");

        StartCoroutine(UpdateTextCoroutine());
    }

    void Update()
    {
        // Perform the following calls only if no fly-to is currently in progress
        if (!_isTeleporting)
        {
            HandleMovement();
            HandleRotation();
            HandleVerticalMovement();
            HandleFOVChange();
            RestoreInitialFOV();
            HandleTeleport();
        }
        else
        {
            TeleportToDestination();
        }

        HandleUIState();
        HandleSpeedAdjustments();
        ToggleEasing();
        HandleDpadRightDoublePress();
    }

    /// <summary>
    /// Coroutine - Update the texts every 0.1 seconds.
    /// </summary>
    IEnumerator UpdateTextCoroutine()
    {
        while (true)
        {
            if (_txtSpeed) _txtSpeed.text = $"Speed: {MovementSpeed.ToString("F2")} // {VerticalMovementSpeed.ToString("F2")} // {RotationSpeed.ToString("F2")}";
            if (_txtFOV) _txtFOV.text = $"FOV: {_cameraComponent.fieldOfView.ToString("F2")} // {FOVChangeRate.ToString("F2")}";
            if (_txtRestrictY) _txtRestrictY.text = $"Restrict Y: {(RestrictYMovement ? "On" : "Off")}";
            if (_txtEasing) _txtEasing.text = $"Easing: {(EasingEnabled ? "On" : "Off")} // {EasingTime.ToString("F2")}";

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// Reset FOV to its initial value.
    /// </summary>
    void RestoreInitialFOV()
    {
        if (Gamepad.current.startButton.wasPressedThisFrame)
        {
            _cameraComponent.fieldOfView = _initialFOV;
        }
    }

    /// <summary>
    /// Fire a raycast to check if fly-to should be executed.
    /// </summary>
    void HandleTeleport()
    {
        if (Gamepad.current.leftStickButton.isPressed)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, TeleportLayerMask))
            {
                _isTeleporting = true;
                _teleportDestination = hit.point - 0.5f * transform.forward;
                _teleportStartTime = Time.time;
                _currentVelocity = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// Perform fly-to logic.
    /// </summary>
    void TeleportToDestination()
    {
        float duration = 0.4f;
        float fraction = (Time.time - _teleportStartTime) / duration;

        if (fraction < 1.0f)
        {
            transform.position = Vector3.Lerp(transform.position, _teleportDestination, fraction);
        }
        else
        {
            transform.position = _teleportDestination;
            _isTeleporting = false;
        }
    }

    /// <summary>
    /// Adjust FOV (field of view) of the main camera.
    /// </summary>
    void HandleFOVChange()
    {
        if (Gamepad.current.buttonWest.isPressed)
        {
            _cameraComponent.fieldOfView += FOVChangeRate * Time.deltaTime;
        }
        else if (Gamepad.current.buttonNorth.isPressed)
        {
            _cameraComponent.fieldOfView -= FOVChangeRate * Time.deltaTime;
        }
    }

    /// <summary>
    /// Perform general camera movement.
    /// </summary>
    void HandleMovement()
    {
        float effectiveEasingTime = EasingEnabled ? EasingTime : 0f;

        Vector2 moveInput = Gamepad.current.leftStick.ReadValue();
        Vector3 targetVelocity;

        float rightTriggerValue = Gamepad.current.rightTrigger.ReadValue();
        float leftTriggerValue = Gamepad.current.leftTrigger.ReadValue();
        float speedMultiplier = 1.0f;

        if (rightTriggerValue > 0)
        {
            speedMultiplier = 2.5f;
        }
        else if (leftTriggerValue > 0)
        {
            speedMultiplier = 0.5f;
        }

        if (RestrictYMovement)
        {
            targetVelocity = (transform.right * moveInput.x + transform.forward * moveInput.y) * MovementSpeed * speedMultiplier;
            targetVelocity.y = 0;
        }
        else
        {
            targetVelocity = transform.rotation * new Vector3(moveInput.x, 0, moveInput.y) * MovementSpeed * speedMultiplier;
        }

        _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, Time.deltaTime / effectiveEasingTime);
        transform.Translate(_currentVelocity * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Perform camera rotation.
    /// </summary>
    void HandleRotation()
    {
        Vector2 rotateInput = Gamepad.current.rightStick.ReadValue();
        _xRotation -= rotateInput.y * RotationSpeed * Time.deltaTime;
        _xRotation = Mathf.Clamp(_xRotation, -180f, 180f);

        _yRotation += rotateInput.x * RotationSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(_xRotation, _yRotation, 0);
    }

    /// <summary>
    /// Perform vertical camera movement.
    /// </summary>
    void HandleVerticalMovement()
    {
        float effectiveEasingTime = EasingEnabled ? EasingTime : 0f;

        float rightTriggerValue = Gamepad.current.rightTrigger.ReadValue();
        float leftTriggerValue = Gamepad.current.leftTrigger.ReadValue();
        float speedMultiplier = 1.0f;

        if (rightTriggerValue > 0)
        {
            speedMultiplier = 2.5f;
        }
        else if (leftTriggerValue > 0)
        {
            speedMultiplier = 0.5f;
        }

        if (Gamepad.current.buttonSouth.isPressed)
            _verticalMovement = 1.0f;
        else if (Gamepad.current.buttonEast.isPressed)
            _verticalMovement = -1.0f;
        else
            _verticalMovement = 0;

        float targetVerticalVelocity = _verticalMovement * VerticalMovementSpeed * speedMultiplier;
        _currentVerticalVelocity = Mathf.Lerp(_currentVerticalVelocity, targetVerticalVelocity, Time.deltaTime / effectiveEasingTime);
        transform.Translate(Vector3.up * _currentVerticalVelocity * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Toggle between UI display states.
    /// </summary>
    void HandleUIState()
    {
        if (Gamepad.current.selectButton.wasPressedThisFrame)
        {
            switch (_camUiState)
            {
                case 0:
                    if (_panelBlackBar) _panelBlackBar.SetActive(true);
                    if (_imgCrosshair) _imgCrosshair.SetActive(false);
                    _camUiState = 1;
                    break;
                case 1:
                    if (_panelBlackBar) _panelBlackBar.SetActive(true);
                    if (_imgCrosshair) _imgCrosshair.SetActive(true);
                    _camUiState = 2;
                    break;
                case 2:
                    if (_panelBlackBar) _panelBlackBar.SetActive(false);
                    if (_imgCrosshair) _imgCrosshair.SetActive(false);
                    _camUiState = 0;
                    break;
            }
        }
    }

    /// <summary>
    /// Handle all the available speed settings.
    /// </summary>
    void HandleSpeedAdjustments()
    {
        bool leftShoulder = Gamepad.current.leftShoulder.isPressed;
        bool rightShoulder = Gamepad.current.rightShoulder.isPressed;

        if (leftShoulder || rightShoulder)
        {
            float delta = (rightShoulder ? 1 : -1) * Time.deltaTime;

            if (Gamepad.current.dpad.up.isPressed)
            {
                VerticalMovementSpeed += delta * YSpeedChangeRate;
            }
            else if (Gamepad.current.dpad.right.isPressed)
            {
                RotationSpeed += delta * RotationSpeedChangeRate;
            }
            else if (Gamepad.current.dpad.down.isPressed)
            {
                EasingTime += delta * EasingTimeChangeRate;
            }
            else if (Gamepad.current.dpad.left.isPressed)
            {
                FOVChangeRate += delta * 0.25f;
            }
            else
            {
                MovementSpeed += delta * XZSpeedChangeRate;
            }
        }
    }

    /// <summary>
    /// Enable or disable easing.
    /// </summary>
    void ToggleEasing()
    {
        if (Gamepad.current.rightStickButton.wasPressedThisFrame)
        {
            EasingEnabled = !EasingEnabled;
        }
    }

    /// <summary>
    /// Reduce or allow movement on the Y-axis.
    /// </summary>
    void HandleDpadRightDoublePress()
    {
        if (Gamepad.current.dpad.right.wasPressedThisFrame)
        {
            if (Time.time - _lastDpadRightPressTime < _doublePressInterval)
            {
                RestrictYMovement = !RestrictYMovement;
                _lastDpadRightPressTime = 0f;
            }
            else
            {
                _lastDpadRightPressTime = Time.time;
            }
        }
    }
}