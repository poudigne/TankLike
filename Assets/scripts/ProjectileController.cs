using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	[SerializeField] private bool isFired = false;
	[SerializeField] private float firePower = 0.0f;
	[SerializeField] private Vector2 direction;
	[SerializeField] private float angle;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Physics manipulation : Frame-Rate independant
	void FixedUpdate() {
		if (!isFired)
			return;

		var newX = transform.position.x + Mathf.Cos(angle+90)* (firePower/300);
		var newY = transform.position.y + Mathf.Sin(angle+90)* (firePower/300);
		var newPos = new Vector2(newX,newY);
		transform.position = newPos;

		// var time = Time.deltaTime;
		// float DEG2RAD = Mathf.PI/180;
		// float ang = angle * DEG2RAD;
		// float v0x = firePower * Mathf.Cos(ang); // initial velocity in x
		// float v0y = firePower * Mathf.Sin(ang); // initial velocity in y

		// float x = (v0x * time);
		// double y = (v0y * time) + (0.5 * g * (float)Math.Pow(time, 2));
		// float y = (0.5f * 0.9f * time + v0y) * time;
	}

	// Sets the flags and value to launch the projectile
	public void FireProjectile(float power, float launchAngle)
	{
		Debug.Log("power : " + power + " ; Direction : "+ launchAngle );

		isFired = true;
		angle = -launchAngle;
		firePower = power;
		transform.rigidbody2D.isKinematic = false;

	}
}
