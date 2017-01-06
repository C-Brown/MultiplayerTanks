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

    public ParticleSystem m_misfireEffect;

    public LayerMask m_obstacleMask;

    bool m_canShoot = false;

    // Use this for initialization
    void Start() {

        m_shotsLeft = m_shotsPerBurst;
        m_isReloading = false;

    }

    public void Enable()
    {
        m_canShoot = true;
    }

    public void Disable()
    {
        m_canShoot = false;
    }

	
	// Update is called once per frame
	void Update () {
        
		
	}

    public void Shoot()
    {
        if (m_isReloading || m_bulletPrefab == null || !m_canShoot)
        {
            return;
        }

        RaycastHit hit;

        Vector3 center = new Vector3(transform.position.x, m_bulletSpawn.position.y, transform.position.z);

        Vector3 dir = (m_bulletSpawn.position - center).normalized;

        if (Physics.SphereCast(center, 0.25f, dir, out hit, 2.5f, m_obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if(m_misfireEffect != null)
            {
                ParticleSystem effect = Instantiate(m_misfireEffect, hit.point, Quaternion.identity) as ParticleSystem;
                effect.Stop();
                effect.Play();
                Destroy(effect.gameObject, 3f);
            }
        }
        else
        {

            CmdShoot();

            m_shotsLeft--;

            if (m_shotsLeft <= 0)
            {
                StartCoroutine("Reload");
            }
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
            bullet.m_owner = GetComponent<PlayerManager>();
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
