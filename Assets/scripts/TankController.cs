using UnityEngine;
using System.Collections;

public class TankController : MonoBehaviour
{
	public GameObject projectile;

	public float movementSpeed = 1.0f;
	public float rotationSpeed = 3.0f;
	public float chargingSpeed = 1.0f;

	public bool isMyTurn = true;

	[SerializeField] private bool isCharging = false;
	[SerializeField] private float firePower = 0.0f; // the fire power of the shoot in %

	// Use this for initialization
	void Start ()
	{
		if (projectile == null)
			throw new MissingComponentException("Object is missing a projectile. Please drag a projectile with the correct layer!");
	}
	
	// Update is called once per frame
	void Update ()
	{
		// If we rotating the canon; 
		// I dont validate the isMyTurn boolean cause we want to let the player to move the canon around
		if (IsCanonMoving)
			RotateCanon (GetCanonMovingDirection);
		// If we moving the tank
		if (IsTankMoving && isMyTurn)
			MoveTank (GetTankMovingDirection);
		// If we're charging and it's my turn
		if (IsCharging && isMyTurn)
			ChargingWeapon ();
		// If we're done charging and we have a firepower, then we're firing
		if (!IsCharging && firePower > 0.0f && isMyTurn)
			FireWeapon ();
	}

	// We rotate the canon in the direction in the parameter : -1 to the left; 1 to the right
	void RotateCanon (int direction)
	{
		var canonTransform = GetCanon ();

		var modifier = rotationSpeed * direction;
		canonTransform.Rotate (Vector3.forward * modifier);
		//float newQuaternion = transform.rotation. + modifier * Time.deltaTime;
		//canonTransform.rotation = newQuaternion;
	}

	// We move the tank in the direction in the parameter : -1 to the left; 1 to the right
	void MoveTank (int direction)
	{
		var modifier = movementSpeed * direction;
		float newPos = transform.position.x + modifier * Time.deltaTime;
		transform.position = new Vector3 (newPos, transform.position.y, transform.position.z);
	}

	// if we're holding space bar, we're charging the attack
	void ChargingWeapon ()
	{
		// Charging the power from 0 until it release or firePower = 100
		firePower += chargingSpeed;
		if (firePower > 100)
		{
			firePower = 100;
			FireWeapon();
		}

		// this is where we gonna animate and other thing.
		// Debug.Log ("Charging... " + firePower);
	}
	// Handle the firing of the projectile
	void FireWeapon ()
	{
		isMyTurn = false;
		var canon = GetCanon();
		var firedProjectile = Instantiate(projectile, transform.position, Quaternion.identity) as GameObject;

		var projectileController = firedProjectile.GetComponent<ProjectileController>();
		projectileController.FireProjectile(firePower, canon.eulerAngles.z);

		Debug.Log(" Angles : " + canon.eulerAngles.z);
		Debug.Log ("Fire for a power of : " + firePower);
	}
	// Return the Canon transform
	Transform GetCanon ()
	{
		var canonTransform = transform.GetChild (0);
		if (canonTransform == null)
			throw new MissingComponentException ("The tank is missing a canon. Perhaps you should use the prefabs!");
		return canonTransform;
	}

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


}