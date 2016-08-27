using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour
{
#if DEBUG
    public bool isDebug = true;
#else
  public bool isDebug = false;
#endif
    public ParticleSystem explosionAnim;

    [SyncVar]
    public float firePower;
    [SyncVar]
    public float angleDeg;

    private float baseDamage = 150f;
    private float radius = 0.4f;

    private CameraController camera_controller;

    private Vector3 origPos;

    void Awake()
    {
        camera_controller = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        camera_controller.RepositionCamera(transform.position, false);
    }

    //Physics manipulation : Frame-Rate independant
    void FixedUpdate()
    {
        float angleRad = (angleDeg + 90) * Mathf.PI / 180;

        var newX = transform.position.x + Mathf.Cos(angleRad) * (firePower / 2);
        var newY = (transform.position.y + Mathf.Sin(angleRad) * (firePower / 2));
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
                float percentage = 1 - (distanceToMiddle / radius);
                percentage = percentage < 0 ? 0 : percentage;
                if (percentage == 0)
                {
                    //chatController.AddNewLine(string.Empty, attacker_name + " miss ");
                    return;
                }
                ApplyDamage(baseDamage * percentage, tankPlayerInfo);
            }
        }
    }

    private void ApplyDamage(float damageAmount, PlayerInfo defenderInfo)
    {
        //defenderInfo.DoDamage(damageAmount, attacker_name);
    }

    void DrawCircle()
    {
        float theta_scale = 0.5f; //Set lower to add more pointsS
        int size = Mathf.CeilToInt((2 * Mathf.PI) / theta_scale); //Total number of points in circle.

        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetColors(Color.red, Color.red);
        lineRenderer.SetWidth(0.25f, 0.25f);
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
}