﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[SelectionBase]
public class TankController : NetworkBehaviour
{
    public float rotationSpeed = 1.0f;
    public float chargingSpeed = 0.01f;

    public bool isMyTurn = true;
    public bool hasFired = false;

    public GameObject projectile;
    public Slider powerSlider;
    public Text powerLabel;
    public Text angleLabel;

    public bool isCharging = false;

    public float firePower; // the fire power of the shoot in %
    public float fireAngleDeg;
    public float nextTurnDelay = 2.0f;
    public GameObject firedProjectile;

    public CameraController cameraController;
    public GameController gameController;
    public Transform spawnBulletLocation;

    public float startAngle = 0.0f;
    public float endAngle = 0.0f;

    [SerializeField]
    Vector2 _velocity = Vector2.zero;

    [SerializeField]
    int _rayCount = 8;

    [SerializeField]
    float MAXIMUM_CLIMB_ANGLE = 30f;

    const float WALK_SPEED = 6f;
    const float GRAVITY = 4f;
    const float TERMINAL_VELOCITY = 12f;

    float GroundCheckDelta
    {
        get { return _bounds.height + 0.5f; }
    }

    const float MARGIN = 0f;

    Transform _spriteTransform;
    BoxCollider2D _collider;
    Rigidbody2D _rigidbody;

    float _xMax = float.MaxValue;
    float _yMax = float.MaxValue; //might not need since there is no vertical movement
    float _yMin = float.MinValue;
    float _xMin = float.MinValue;

    [SerializeField]
    bool _isGrounded;
    [SerializeField]
    bool _isOnSlope;

    readonly List<RaycastHit2D> _raycastHits = new List<RaycastHit2D>();

    Rect _bounds;

    bool IsCanonMoving { get { return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S); } }
    bool IsTankMoving { get { return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D); } }
    bool IsCharging { get { return Input.GetKey(KeyCode.Space); } }
    int GetCanonMovingDirection { get { return (Input.GetKey(KeyCode.W) ? -1 : 1); } }
    Direction TankFacingDirection
    {
        get
        {
            if (transform.localScale.x < 0)
                return Direction.Left;
            else
                return Direction.Right;
        }
    }
    int GetTankMovingDirection
    {
        get
        {
            if (Input.GetKey(KeyCode.A))
                return -1;
            else if (Input.GetKey(KeyCode.D))
                return 1;
            return 1;
        }
    }


    Vector2 TopLeftBoundsPoint
    {
        get
        {
            return _bounds.min + Vector2.up * _bounds.height + MARGIN * (Vector2.right + Vector2.down);
        }
    }

    Vector2 TopRightBoundsPoint
    {
        get
        {
            return _bounds.max + MARGIN * (Vector2.left + Vector2.down);
        }
    }

    Vector2 BottomLeftBoundsPoint
    {
        get
        {
            return _bounds.min + +MARGIN * (Vector2.right + Vector2.up);
        }
    }

    Vector2 BottomRightBoundsPoint
    {
        get
        {
            return _bounds.max + Vector2.down * _bounds.height + MARGIN * (Vector2.left + Vector2.up);
        }
    }

    void Awake()
    {
        gameController = FindObjectOfType<GameController>();

        _collider = GetComponent<BoxCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();

        _spriteTransform = GetComponentsInChildren<Transform>().First(t => t.name == "SpriteGraphics");
    }

    void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        InitilializeUIElement();

        if (isMyTurn)
        {
            UpdateFireAngleUI();
            UpdateFirePowerUI();
        }
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        this._spriteTransform.GetComponent<MeshRenderer>().material.color = Color.blue;
        transform.gameObject.layer = LayerMask.NameToLayer("friendlyTank");
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        if (isMyTurn && !hasFired)
        {
            _bounds.x = _collider.bounds.min.x;
            _bounds.y = _collider.bounds.min.y;
            _bounds.width = _collider.bounds.size.x;
            _bounds.height = _collider.bounds.size.y;

            //_velocity.x = _velocity.y = 0f;
            ApplyMovementInput();

            if (_isGrounded == false)
            {
                ApplyGravity();
            }

            CheckGrounded();

            AlignSpriteToGround();

            if (!hasFired)
                cameraController.RepositionCamera(transform.position);
            // If we rotating the canon; 
            // I dont validate the isMyTurn boolean cause we want to let the player to move the canon around
            if (IsCanonMoving)
                RotateCanon(GetCanonMovingDirection);
            // If we moving the tank
            if (IsTankMoving && !isCharging)
                UpdatePosition();
            // If we're charging and it's my turn
            if (IsCharging)
                ChargingWeapon();
            // If we're done charging and we have a firepower, then we're firing
            if (!IsCharging && firePower > 0.0f)
                FireWeapon();

            if (hasFired && firedProjectile == null)
            {
                isMyTurn = false;
                hasFired = false;
                Invoke("NextTurn", nextTurnDelay);
            }
        }
    }

    void ApplyGravity()
    {
        _velocity.y += -GRAVITY * Time.fixedDeltaTime;
        _velocity.y = Mathf.Clamp(_velocity.y, -TERMINAL_VELOCITY * Time.fixedDeltaTime, float.MaxValue);
    }

    void ApplyMovementInput()
    {
        float input = Input.GetAxisRaw("Horizontal");

        GetRaycastInfo(TopLeftBoundsPoint, TopRightBoundsPoint, Vector2.down, 100f);

        var shortestRay = _raycastHits.Where(hit => hit.collider != null)
            .OrderBy(hit => hit.fraction)
            .FirstOrDefault(hit => Math.Abs(hit.fraction) > 0.0001f);

        float angle = Vector2.Angle(shortestRay.normal, Vector2.up);
        _velocity.x = input * WALK_SPEED * Time.fixedDeltaTime * Mathf.Cos(angle * Mathf.Deg2Rad);
    }

    void UpdatePosition()
    {
        var newPos = new Vector2
        {
            x = _rigidbody.position.x + _velocity.x,
            y = _rigidbody.position.y + _velocity.y
        };
        var canonTransform = GetCanon();
        if (TankFacingDirection == Direction.Left && GetTankMovingDirection == 1)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            transform.rotation = Quaternion.Inverse(transform.rotation);
            fireAngleDeg = canonTransform.eulerAngles.z;
        }
        if (TankFacingDirection == Direction.Right && GetTankMovingDirection == -1)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
            transform.rotation = Quaternion.Inverse(transform.rotation);
            fireAngleDeg = canonTransform.eulerAngles.z;
        }

        _rigidbody.MovePosition(newPos);
    }

    void CheckGrounded()
    {
        GetRaycastInfo(TopLeftBoundsPoint, TopRightBoundsPoint, Vector2.down, _bounds.height + GroundCheckDelta);

        if (_raycastHits.Any(hit => hit.collider != null))
        {
            var shortestRay = _raycastHits.Where(hit => hit.collider != null)
                .OrderBy(hit => hit.fraction)
                .FirstOrDefault(hit => Math.Abs(hit.fraction) > 0.0001f);

            if (shortestRay.distance < GroundCheckDelta)
            {
                _velocity.y = 0;
                _isGrounded = true;

                _isOnSlope = Math.Abs(Vector2.Angle(shortestRay.normal, Vector2.up)) > 0.0001f;

                float targetY = shortestRay.point.y + _bounds.height / 2;

                if (_isOnSlope)
                {
                    var startPoint = TopLeftBoundsPoint + Vector2.right * _bounds.width / 2;
                    var middleRay = Physics2D.Raycast(startPoint, Vector2.down, _bounds.height + 0.3f, LayerMask.GetMask(new string[] { "Ground" }));
                    Debug.DrawLine(_bounds.center, middleRay.point, Color.green);

                    if (middleRay.collider != null)
                    {
                        targetY = middleRay.point.y + _bounds.height / 2;
                    }
                }
                _rigidbody.position = new Vector2(_rigidbody.position.x, targetY);
            }
            else
            {
                _isGrounded = false;
                _isOnSlope = false;
            }
        }
        else
        {
            _isGrounded = false;
            _isOnSlope = false;
        }
    }

    void GetRaycastInfo(Vector2 startPoint, Vector2 endPoint, Vector2 direction, float distance)
    {
        _raycastHits.Clear();

        for (int i = 0; i < _rayCount; i++)
        {
            float lerpAmount = (float)i / (_rayCount - 1);
            var raycastOrigin = Vector2.Lerp(startPoint, endPoint, lerpAmount);

            var hitInfo = Physics2D.Raycast(raycastOrigin, direction, distance, LayerMask.GetMask(new string[] { "Ground" }));

            _raycastHits.Add(hitInfo);
            if (hitInfo.transform != null)
            {
                //Debug.Log(hitInfo.transform.name);
            }
            if (hitInfo.collider != null)
            {
                Debug.DrawLine(raycastOrigin, hitInfo.point, Color.cyan);
            }
        }
    }

    void AlignSpriteToGround()
    {
        //float width = _spriteTransform.localScale.x;
        //float height = _spriteTransform.localScale.y;

        //TODO: get sprite transform width/height?
        Vector2 startPoint = _spriteTransform.position - _spriteTransform.right * 0.5f - _spriteTransform.up * 0.5f;
        startPoint.y += MARGIN;
        startPoint.x += MARGIN;

        Vector2 endPoint = _spriteTransform.position + _spriteTransform.right * 0.5f - _spriteTransform.up * 0.5f;
        endPoint.y += MARGIN;
        endPoint.x -= MARGIN;

        var hitInfoStart = Physics2D.Raycast(startPoint, Vector2.down, 4f, LayerMask.GetMask(new string[] { "Ground" }));
        var hitInfoEnd = Physics2D.Raycast(endPoint, Vector2.down, 4f, LayerMask.GetMask(new string[] { "Ground" }));

        var hitInfos = new[] { hitInfoStart, hitInfoEnd };

        int hitCount = hitInfos.Count(h => h.collider != null);

        //Align to ground
        switch (hitCount)
        {
            case 2:
                var alignAxis = hitInfoEnd.point - hitInfoStart.point;
                _spriteTransform.up = Vector3.Cross(alignAxis, Vector3.back).normalized;
                break;
            case 1:
                var hitInfo = hitInfos.First(h => h.collider != null);
                _spriteTransform.up = hitInfo.normal;
                break;
            default:
                _spriteTransform.up = Vector2.up;
                break;
        }

    }

    // Find the UI element for future reference, this is done programaticaly because tank are Created programmaticaly
    void InitilializeUIElement()
    {
        powerSlider = GameObject.FindObjectOfType<Slider>();
        angleLabel = FindUILabelWithName("angle_value");
        powerLabel = FindUILabelWithName("power_value");
    }

    // We rotate the canon in the direction in the parameter : -1 to the left; 1 to the right
    void RotateCanon(int direction)
    {
        var canonTransform = GetCanon();

        if (direction == -1)
            canonTransform.Rotate(Vector3.back);
        else
            canonTransform.Rotate(Vector3.forward);

        
        RestrictCanonRotation(canonTransform);
        UpdateFireAngleUI();
    }

    // if we're holding space bar, we're charging the attack
    void ChargingWeapon()
    {
        // Charging the power from 0 until it release or firePower = 100
        firePower += chargingSpeed;

        if (firePower > 1)
        {
            firePower = 1;
            FireWeapon();
        }
        UpdateFirePowerUI();
        // this is where we gonna animate and other thing.
    }

    void FireWeapon()
    {
        PlayerInfo attackerInfo = GetComponent<PlayerInfo>();
        CmdFireWeapon(firePower, fireAngleDeg, attackerInfo.playerName);
        firePower = 0.0f;
    }

    // Handle the firing of the projectile
    [Command]
    void CmdFireWeapon(float pfirePower, float pfireAngleDeg, string pattackerInfo)
    {
        firedProjectile = Instantiate(projectile, spawnBulletLocation.position, Quaternion.identity) as GameObject;
        ProjectileController projectileController = firedProjectile.GetComponent<ProjectileController>();
        projectileController.FireProjectile(pfirePower, pfireAngleDeg);
        NetworkServer.Spawn(firedProjectile);
        //hasFired = true;
    }

    // Restrict the canon rotation
    void RestrictCanonRotation(Transform canonTransform)
    {
        canonTransform.eulerAngles = new Vector3(canonTransform.eulerAngles.x, canonTransform.eulerAngles.y, Mathf.Clamp(fireAngleDeg, startAngle, endAngle));
        //if (fireAngleDeg > startAngle && fireAngleDeg < endAngle)
        //    canonTransform.eulerAngles = new Vector3(canonTransform.eulerAngles.x, canonTransform.eulerAngles.y, 90);
        //else if (fireAngleDeg < 270 && fireAngleDeg > 90)
        //    canonTransform.eulerAngles = new Vector3(canonTransform.eulerAngles.x, canonTransform.eulerAngles.y, 270);
    }

    Text FindUILabelWithName(string labelname)
    {
        Text[] labels = FindObjectsOfType<Text>();
        foreach (var label in labels)
        {
            if (label.name == labelname)
                return label;
        }
        return null;
    }
    // Return the Canon transform
    Transform GetCanon()
    {
        Transform canonTransform = transform.FindChild("canon");
        if (canonTransform == null)
            throw new MissingComponentException("The tank is missing a canon. Perhaps you should use the prefabs!");
        return canonTransform;
    }

    // Call the GameController to make next player to play
    void NextTurn()
    {
        Debug.Log("NEXT TURN CALLED");

        gameController.PlayNext();
    }

    // Update the UI element related to power (Slider and Label)
    void UpdateFirePowerUI()
    {
        float firePowerPercent = firePower * 100;
        powerSlider.value = firePowerPercent;
        powerLabel.text = firePowerPercent.ToString();
    }

    // Update the UI element related to angle (Label only for now)
    void UpdateFireAngleUI()
    {
        angleLabel.text = ((int)fireAngleDeg).ToString();
    }

    // Attach the UI ELements to the GameObject
    public void HookUIElements()
    {
        powerSlider = GameObject.FindGameObjectWithTag("power_value_slider").GetComponent<Slider>();
        powerLabel = GameObject.FindGameObjectWithTag("power_value_label").GetComponent<Text>();
        angleLabel = GameObject.FindGameObjectWithTag("angle_value_label").GetComponent<Text>();
    }
}


