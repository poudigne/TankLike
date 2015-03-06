using System;
using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
#if DEBUG
  public bool isDebug = false;
#else
  public bool isDebug = true;
#endif
  public ParticleSystem explosionAnim;

  [SerializeField] private bool isFired;

  [SerializeField] private float firePower;
  [SerializeField] private float angleRad;
  [SerializeField] private float baseDamage = 150f;
  [SerializeField] private float radius = 0.4f;

  [SerializeField] private PlayerInfo attackerInfo;

  private CameraController cameraController;
  private ChatController chatController;

  private Vector3 origPos;

  #region Unity Engine
  void Awake()
  {
    cameraController = Camera.main.GetComponent<CameraController>();
    chatController = FindObjectOfType<ChatController>();
  }

  void Update()
  {
    cameraController.RepositionCamera(transform.position, false);
  }

  // Physics manipulation : Frame-Rate independant
  void FixedUpdate()
  {
    if (!isFired)
      return;

    var newX = transform.position.x + Mathf.Cos(angleRad) * (firePower / 4);
    var newY = (transform.position.y + Mathf.Sin(angleRad) * (firePower / 4));
    var newPos = new Vector2(newX, newY);

    Vector3 moveDirection = gameObject.transform.position - origPos;
    if (moveDirection != Vector3.zero)
    {
      float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
      transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    transform.position = newPos;
  }
  void OnCollisionEnter2D(Collision2D other)
  {
    GameObject explosion = Instantiate(explosionAnim, gameObject.transform.position, Quaternion.identity) as GameObject;
    ParticleSystem explosionPFX = explosion.GetComponent<ParticleSystem>();
    Destroy(explosion, explosionPFX.duration);

    CalculateDamage();
    Destroy(gameObject);
  }
  #endregion
  // Sets the flags and value to launch the projectile
  public void FireProjectile(float power, float launchAngle, PlayerInfo pAttackerInfo)
  {
    Debug.Log(pAttackerInfo);
    isFired = true;
    firePower = power;
    angleRad = (launchAngle + 90) * Mathf.PI / 180;
    attackerInfo = pAttackerInfo;
    origPos = transform.position;
  }

  private void CalculateDamage()
  {
    Vector2 pos = new Vector2(transform.position.x, transform.position.y);
    Collider2D[] hitTank = Physics2D.OverlapCircleAll(pos, radius);

    if (isDebug) DrawCircle();

    foreach (var hit in hitTank)
    {
      if (hit.tag == "Player")
      {
        Vector2 hitPos = new Vector2(hit.transform.position.x, hit.transform.position.y);
        float distanceToMiddle = Vector2.Distance(transform.position, (hitPos + hit.offset));
        PlayerInfo tankPlayerInfo = hit.transform.gameObject.GetComponent<PlayerInfo>();
        float percentage = 1 - (distanceToMiddle/radius);
        percentage = percentage < 0 ? 0 : percentage;
        if (percentage == 0)
        {
          chatController.AddNewLine(string.Empty, attackerInfo.playerName + " miss ");
          return;
        }
        ApplyDamage(baseDamage * percentage, tankPlayerInfo);
      }
    }
  }

  private void ApplyDamage(float damageAmount, PlayerInfo defenderInfo)
  {
    defenderInfo.DoDamage(damageAmount, attackerInfo);
  }

  #region Debug function
  void DrawCircle()
  {
    float theta_scale = 0.1f;             //Set lower to add more pointsS
    int size = Mathf.CeilToInt((2 * Mathf.PI) / theta_scale); //Total number of points in circle.

    LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
    lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
    lineRenderer.SetColors(Color.red, Color.red);
    lineRenderer.SetWidth(0.025f, 0.025f);
    lineRenderer.SetVertexCount(size);
    var r = radius;
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
  #endregion
}
