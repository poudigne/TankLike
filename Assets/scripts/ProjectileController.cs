using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	[SerializeField] private bool isFired = false;
	[SerializeField] private float firePower = 0.0f;
	[SerializeField] private float angleRad;

	[SerializeField] private float baseDamage = 50.0f;
	[SerializeField] private float radius = 5.0f;

  public GameObject explosionAnim;

	// Physics manipulation : Frame-Rate independant
	void FixedUpdate() {
		if (!isFired)
			return;

		var newX = transform.position.x + Mathf.Cos(angleRad) * (firePower/4);
		var newY = (transform.position.y + Mathf.Sin(angleRad) * (firePower/4));
		var newPos = new Vector2(newX,newY);

		transform.position = newPos;
	}

	// Sets the flags and value to launch the projectile
	public void FireProjectile(float power, float launchAngle)
	{
		isFired = true;
		firePower = power;
    angleRad = (launchAngle+90) * Mathf.PI/180 ;
	}

  void OnCollisionEnter2D(Collision2D other)
  {
    Debug.Log("bullet Collision with " + other.gameObject.name);
    GameObject explosion = Instantiate(explosionAnim, gameObject.transform.position, Quaternion.identity) as GameObject;
    Destroy(gameObject);
  }
}
