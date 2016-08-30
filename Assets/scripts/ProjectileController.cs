using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ProjectileController : NetworkBehaviour
{
    public ParticleSystem explosionAnim;

    [SyncVar]
    public float firePower;
    [SyncVar]
    public float angleDeg;
    [SyncVar]
    public Transform shooter;

    private float baseDamage = 150f;
    [SerializeField]
    private float radius = 0.4f;

    private CameraController camera_controller;


    //private Vector3 /*origPos*/;

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

        Vector3 moveDirection = gameObject.transform.position /*- origPos*/;
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
        if (!isServer)
            return;
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        Collider2D[] hitTank = Physics2D.OverlapCircleAll(pos, radius);

        foreach (var hit in hitTank)
        {
            if (hit.tag == "Player")
            {
                Vector2 hitPos = new Vector2(hit.transform.position.x, hit.transform.position.y);
                
                
                float halfWidth = hit.transform.FindChild("SpriteGraphics").GetComponent<MeshRenderer>().bounds.size.x / 2;
                Debug.Log("half width = " + halfWidth);
                float distanceToMiddle = Vector2.Distance(transform.position, hitPos) - halfWidth;
                Debug.Log("radius " + radius + " / distance = " + distanceToMiddle + " = " +  radius / distanceToMiddle);
                Debug.Log("distance = " + distanceToMiddle + " / radius " + radius + " = " + distanceToMiddle / radius);

                float percentage = 1 - (distanceToMiddle / radius);
                percentage = percentage < 0 ? 0 : percentage;

                Debug.Log("BaseDamage = " + baseDamage + " Percentage = " + percentage);
                PlayerInfo tankPlayerInfo = hit.transform.gameObject.GetComponent<PlayerInfo>();
                ApplyDamage(baseDamage * percentage, tankPlayerInfo);
            }
        }
        Destroy(gameObject);
    }

    private void ApplyDamage(float damageAmount, PlayerInfo defenderInfo)
    {
        if (!isServer)
            return;

        // Notify
        defenderInfo.DoDamage(damageAmount);
        Transform tank = transform.parent;
    }
}