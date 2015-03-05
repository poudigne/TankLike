using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TankController : MonoBehaviour
{
	public float movementSpeed = 1.0f;
	public float rotationSpeed = 1.0f;
	public float chargingSpeed = 0.01f;

	public bool isMyTurn = true;

	[SerializeField] private GameObject projectile;
	[SerializeField] private Slider powerSlider;
	[SerializeField] private Text powerLabel;
	[SerializeField] private Text angleLabel;

	[SerializeField] private bool isCharging = false;
	[SerializeField] private float firePower; // the fire power of the shoot in %
	[SerializeField] private float fireAngleDeg;


  #region Unity Engine
  // Use this for initialization
	void Start ()
	{
		// projectile = GameObject.FindObjectOfType
		if (projectile == null)
			throw new MissingComponentException("Object is missing a projectile. Please drag a projectile with the correct layer!");

		InitilializeUIElement();

		UpdateFireAngleUI();
		UpdateFirePowerUI();
	}
	
	// Update is called once per frame
	void Update ()
	{
		// If we rotating the canon; 
		// I dont validate the isMyTurn boolean cause we want to let the player to move the canon around
		if (IsCanonMoving)
			RotateCanon (GetCanonMovingDirection);
		// If we moving the tank
		if (IsTankMoving && isMyTurn && !isCharging)
			MoveTank (GetTankMovingDirection);
		// If we're charging and it's my turn
		if (IsCharging && isMyTurn)
			ChargingWeapon ();
		// If we're done charging and we have a firepower, then we're firing
		if (!IsCharging && firePower > 0.0f && isMyTurn)
			FireWeapon ();
	}
  #endregion
  // Find the UI element for future reference, this is done programaticaly because tank are Created programmaticaly
	void InitilializeUIElement ()
	{
		powerSlider = GameObject.FindObjectOfType<Slider>();
		angleLabel = FindUILabelWithName("angle_value");
		powerLabel = FindUILabelWithName("power_value");
	}

	// We rotate the canon in the direction in the parameter : -1 to the left; 1 to the right
	void RotateCanon (int direction)
	{
		var canonTransform = GetCanon ();

		var modifier = rotationSpeed * direction;
		canonTransform.Rotate (Vector3.forward * modifier);
		fireAngleDeg = canonTransform.eulerAngles.z;
		RestrictCanonRotation(canonTransform);

		// if (fireAngleDeg)

		UpdateFireAngleUI ();
		//float newQuaternion = transform.rotation. + modifier * Time.deltaTime;
		//canonTransform.rotation = newQuaternion;
	}

	// We move the tank in the direction in the parameter : -1 to the left; 1 to the right
	void MoveTank (int direction)
	{
		var canonTransform = GetCanon ();
		if (transform.localScale.x != direction){
      gameObject.transform.localScale = new Vector3(direction, transform.localScale.y);
      canonTransform.rotation = Quaternion.Inverse(canonTransform.rotation);
		}

		fireAngleDeg = canonTransform.eulerAngles.z;
		
		UpdateFireAngleUI ();
		transform.GetComponent<Rigidbody2D>().velocity = new Vector2(movementSpeed * direction,transform.GetComponent<Rigidbody2D>().velocity.y);
	}

	// if we're holding space bar, we're charging the attack
	void ChargingWeapon ()
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

	// Handle the firing of the projectile
	void FireWeapon ()
	{
		GameObject firedProjectile = Instantiate(projectile, transform.position, Quaternion.identity) as GameObject;
		ProjectileController projectileController = firedProjectile.GetComponent<ProjectileController>();
	  PlayerInfo attackerInfo = GetComponent<PlayerInfo>();
    projectileController.FireProjectile(firePower, fireAngleDeg, attackerInfo);
    firePower = 0.0f;
    //isMyTurn = false;
	}

	// Restrict the canon rotation
	void RestrictCanonRotation (Transform canonTransform)
	{
		if (fireAngleDeg > 90 && fireAngleDeg < 270) 
			canonTransform.eulerAngles = new Vector3(canonTransform.eulerAngles.x, canonTransform.eulerAngles.y, 90);
		else if (fireAngleDeg < 270 && fireAngleDeg > 90) 
			canonTransform.eulerAngles = new Vector3(canonTransform.eulerAngles.x, canonTransform.eulerAngles.y, 270);
	}
  
  Text FindUILabelWithName (string labelname)
	{
		Text[] labels = FindObjectsOfType<Text>();
		foreach(var label in labels)
		{
			if (label.name == labelname)
				return label;
		}
		return null;
	}
  // Return the Canon transform
  Transform GetCanon()
  {
    Transform canonTransform = transform.GetChild(0);
    if (canonTransform == null)
      throw new MissingComponentException("The tank is missing a canon. Perhaps you should use the prefabs!");
    return canonTransform;
  }

  #region UI Update
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
  #endregion
	#region Input check
	// if we're rotating the canon left or right
	bool IsCanonMoving { get { return Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.S); } }
	// If we're moving tank left or Right
	bool IsTankMoving { get { return Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.D); } }
	// If we're holding space key to charge up the fire
	bool IsCharging { get { return Input.GetKey(KeyCode.Space); } }

	// If we press w we want to rotate to the left so we return -1 otherwise 1
	int GetCanonMovingDirection { get { return (Input.GetKey (KeyCode.W) ? -1 : 1); } }
	// If we press A we want to move to the left so we return -1 otherwise 1
	int GetTankMovingDirection { get { return (Input.GetKey (KeyCode.A) ? -1 : 1); } }
	#endregion
  #region

  
  #endregion
}