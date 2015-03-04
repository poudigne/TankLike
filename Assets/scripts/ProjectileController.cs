using System;
using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
  [SerializeField] private bool isFired;
  [SerializeField] private float firePower;
  [SerializeField] private float angleRad;

  [SerializeField] private float baseDamage = 150f;
  [SerializeField] private float radius = 0.5f;

  public GameObject explosionAnim;

  #region Unity Engine
  // Physics manipulation : Frame-Rate independant
  void FixedUpdate()
  {
    if (!isFired)
      return;

    var newX = transform.position.x + Mathf.Cos(angleRad) * (firePower / 4);
    var newY = (transform.position.y + Mathf.Sin(angleRad) * (firePower / 4));
    var newPos = new Vector2(newX, newY);

    transform.position = newPos;
  }
  void OnCollisionEnter2D(Collision2D other)
  {
    GameObject explosion = Instantiate(explosionAnim, gameObject.transform.position, Quaternion.identity) as GameObject;
    ApplyDamage();
    //Destroy(gameObject);
  }
  #endregion
  // Sets the flags and value to launch the projectile
  public void FireProjectile(float power, float launchAngle)
  {
    isFired = true;
    firePower = power;
    angleRad = (launchAngle + 90) * Mathf.PI / 180;
  }

  private void ApplyDamage()
  {
    Vector2 pos = new Vector2(transform.position.x, transform.position.y);
    var hitTank = Physics2D.OverlapCircleAll(pos, radius);

    DrawCircle();

    foreach (var hit in hitTank)
    {
      var distanceToMiddle = Vector3.Distance(transform.position, hit.transform.position);
      if (hit.tag == "Player")
      {
        Debug.Log(hit);
        var tank = hit.transform.gameObject.GetComponent<PlayerInfo>();
        Debug.Log("Ho boboy player " + tank.playerName + " has been hit " + distanceToMiddle);
      }
    }
  }

  void DrawCircle()
  {
    float theta_scale = 0.1f;             //Set lower to add more pointsS
    int size = Mathf.CeilToInt((2 * Mathf.PI) / theta_scale); //Total number of points in circle.

    LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
    lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
    lineRenderer.SetColors(Color.red, Color.red);
    lineRenderer.SetWidth(0.05f, 0.05f);
    lineRenderer.SetVertexCount(size);
    var r = 0.3f;
    int i = 0;
    for (float theta = 0; theta < 2 * Mathf.PI; theta += 0.1f)
    {
      var x = r * Mathf.Cos(theta) + transform.position.x;
      var y = r * Mathf.Sin(theta) + transform.position.y;

      Vector3 pos = new Vector3(x, y, 1);
      lineRenderer.SetPosition(i, pos);
      i++;
    }
  }
}
