using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{

    public Rigidbody m_bulletPrefab;

    public Transform m_bulletSpawn;

    public int m_shotsPerBurst = 2;

    int m_shotsLeft;

    bool m_isReloading;

    public float m_reloadTime = 1f;

	// Use this for initialization
	void Start () {

        m_shotsLeft = m_shotsPerBurst;
        m_isReloading = false;

	}
	
	// Update is called once per frame
	void Update () {
        
		
	}

    public void Shoot()
    {
        if (m_isReloading || m_bulletPrefab == null)
        {
            return;
        }

        CmdShoot();

        m_shotsLeft--;

        if (m_shotsLeft <= 0)
        {
            StartCoroutine("Reload");
        }
    }

    [Command]
    private void CmdShoot()
    {
        Bullet bullet = null;
        // bullet = m_bulletPrefab.GetComponent<Bullet>();

        Rigidbody rbody = Instantiate(m_bulletPrefab, m_bulletSpawn.position, m_bulletSpawn.rotation) as Rigidbody;
        bullet = rbody.gameObject.GetComponent<Bullet>();

        if (rbody != null)
        {
            rbody.velocity = bullet.m_speed * m_bulletSpawn.transform.forward;
            bullet.m_owner = GetComponent<PlayerController>();
            NetworkServer.Spawn(rbody.gameObject);
        }
    }

    public void Reset()
    {
        m_shotsLeft = m_shotsPerBurst;
        m_isReloading = false;
    }

    IEnumerator Reload()
    {
        m_shotsLeft = m_shotsPerBurst;
        m_isReloading = true;
        yield return new WaitForSeconds(m_reloadTime);
        m_isReloading = false;
    }

}
