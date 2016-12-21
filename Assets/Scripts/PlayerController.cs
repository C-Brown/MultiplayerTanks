﻿using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerShoot))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerSetup))]
public class PlayerController : NetworkBehaviour
{

    PlayerHealth m_pHealth;
    PlayerMotor m_pMotor;
    public PlayerSetup m_pSetup;
    PlayerShoot m_pShoot;

    Vector3 m_originalPosition;
    NetworkStartPosition[] m_spawnPoints;

    public GameObject m_spawnFx;

    public int m_score;

    // Use this for initialization
    void Start()
    {
        m_pHealth = GetComponent<PlayerHealth>();
        m_pMotor = GetComponent<PlayerMotor>();
        m_pSetup = GetComponent<PlayerSetup>();
        m_pShoot = GetComponent<PlayerShoot>();

    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        m_spawnPoints = GameObject.FindObjectsOfType<NetworkStartPosition>();
        m_originalPosition = transform.position;
    }

    Vector3 GetInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        return new Vector3(h, 0, v);
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer || m_pHealth.m_isDead)
        {
            return;
        }

        Vector3 inputDirection = GetInput();
        m_pMotor.MovePlayer(inputDirection);   
    }

    void Update()
    {
        if(!isLocalPlayer)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            m_pShoot.Shoot();
        }

        Vector3 inputDirection = GetInput();
        if (inputDirection.sqrMagnitude > 0.25f)
        {
            m_pMotor.RotateChassis(inputDirection);
        }

        Vector3 turretDir = Utility.GetWorldPointFromScreenPoint(Input.mousePosition, m_pMotor.m_turret.position.y) - m_pMotor.m_turret.position;
        m_pMotor.RotateTurret(turretDir);
    }

    void Disable()
    {
        StartCoroutine("RespawnRoutine");
    }

    IEnumerator RespawnRoutine()
    {
        transform.position = GetRandomSpawnPosition();
        m_pMotor.m_rigidbody.velocity = Vector3.zero;

        yield return new WaitForSeconds(3f);
        m_pHealth.Reset();
        m_pShoot.Reset();

        if(m_spawnFx != null)
        {
            GameObject spawnFx = Instantiate(m_spawnFx, transform.position + Vector3.up * 0.5f, Quaternion.identity) as GameObject;
            Destroy(spawnFx, 3f);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        if(m_spawnPoints != null)
        {
            if (m_spawnPoints.Length > 0)
            {
                NetworkStartPosition startPos = m_spawnPoints[Random.Range(0, m_spawnPoints.Length)];
                return startPos.transform.position;
            }
        }
        return m_originalPosition;
    }

}