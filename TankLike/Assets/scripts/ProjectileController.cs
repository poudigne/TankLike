﻿using System;
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

    //[SyncVar]
    private CameraController camera_controller;
    Rigidbody2D _rigidbody;

    //private Vector3 /*origPos*/;

    void Awake()
    {
        camera_controller = Camera.main.GetComponent<CameraController>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        camera_controller.RepositionCamera(transform.position, false);
    }

    //Physics manipulation : Frame-Rate independant
    void FixedUpdate()
    {
        float angleRad = (angleDeg + 90) * Mathf.PI / 180;
        //_rigidbody.velocity = new Vector2(Mathf.Sin(angleRad) * speed, Mathf.Cos(angleRad) * speed);

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
        //transform.LookAt();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!isServer)
        {
            return;
        }
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

        foreach (var hit in hitTank)
        {
            if (hit.tag == "Player")
            {
                Vector2 hitPos = new Vector2(hit.transform.position.x, hit.transform.position.y);


                float halfWidth = hit.transform.FindChild("SpriteGraphics").GetComponent<MeshRenderer>().bounds.size.x / 2;
                Debug.Log("half width = " + halfWidth);
                float distanceToMiddle = Vector2.Distance(transform.position, hitPos) - halfWidth;
                Debug.Log("radius " + radius + " / distance = " + distanceToMiddle + " = " + radius / distanceToMiddle);
                Debug.Log("distance = " + distanceToMiddle + " / radius " + radius + " = " + distanceToMiddle / radius);

                float percentage = 1 - (distanceToMiddle / radius);
                percentage = percentage < 0 ? 0 : percentage;

                Debug.Log("BaseDamage = " + baseDamage + " Percentage = " + percentage);
                PlayerInfo tankPlayerInfo = hit.transform.gameObject.GetComponent<PlayerInfo>();
                float totalDamage = baseDamage * percentage;
                ApplyDamage(totalDamage, tankPlayerInfo);
                CalculateReward(damage);
            }
        }
        Destroy(gameObject);
    }

    private void CalculateReward(float damage)
    {
        int money = ((int)damage / 3) + UnityEngine.Random.Range(-10, 10);

        Transform tank = transform.parent;
        InvokeGiveMoney(tank, money);
    }

    private void ApplyDamage(float damageAmount, PlayerInfo defenderInfo)
    {
        defenderInfo.DoDamage(damageAmount);

        Transform tank = transform.parent;
        InvokeDamageDone(tank.GetComponent<PlayerInfo>(), damageAmount);
    }

    public delegate void DamageDoneEventHandler(PlayerInfo playerInfo, float damageAmount);
    public event DamageDoneEventHandler DamageDone;

    private void InvokeDamageDone(PlayerInfo playerInfo, float damageAmount)
    {
        if (DamageDone != null)
        {
            DamageDone(playerInfo, damageAmount);
        }
    }
}