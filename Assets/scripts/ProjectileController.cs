using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	[SerializeField] private bool isFired = false;
	[SerializeField] private float firePower = 0.0f;
	[SerializeField] private float angleRad;
	[SerializeField] private float windFactor;

	// Physics manipulation : Frame-Rate independant
	void FixedUpdate() {
		if (!isFired)
			return;

		windFactor = 0.5f;

		//angle *= DEG2RAD;
		var newX = transform.position.x + Mathf.Cos(angleRad)* windFactor * (firePower/4);
		var newY = (transform.position.y + Mathf.Sin(angleRad)* windFactor * (firePower/4));
		var newPos = new Vector2(newX,newY);
		transform.position = newPos;
	}

	// Sets the flags and value to launch the projectile
	public void FireProjectile(float power, float launchAngle, float windDragFactor)
	{
		isFired = true;
		// float DEG2RAD = ;
		angleRad = (launchAngle+45) * Mathf.PI/180 ;
		firePower = power;
		windFactor = windDragFactor; 
		transform.rigidbody2D.isKinematic = false;
		//transform.rigidbody2D.drag = windDragFactor;

	}
}
