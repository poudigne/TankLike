using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	[SerializeField] private bool isFired = false;
	[SerializeField] private float firePower = 0.0f;
	[SerializeField] private float angleRad;
	[SerializeField] private float windFactor;

	[SerializeField] private float baseDamage = 50.0f;
	[SerializeField] private float radius = 5.0f;

	// Physics manipulation : Frame-Rate independant
	void FixedUpdate() {
		if (!isFired)
			return;

		windFactor = 0.5f;
		var newX = transform.position.x + Mathf.Cos(angleRad) * (firePower/4);
		var newY = (transform.position.y + Mathf.Sin(angleRad) * (firePower/4));
		var newPos = new Vector2(newX,newY);

		transform.position = newPos;
	}

	// Sets the flags and value to launch the projectile
	public void FireProjectile(float power, float launchAngle, float windDragFactor)
	{
		isFired = true;
		angleRad = (launchAngle+90) * Mathf.PI/180 ;
		firePower = power;
		windFactor = windDragFactor; 

	}
}
