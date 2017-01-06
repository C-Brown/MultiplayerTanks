using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    public bool m_isOccupied = false;

    void onTriggerEnter (Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            m_isOccupied = true;
        }
    }

    void onTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            m_isOccupied = true;
        }
    }

    void onTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            m_isOccupied = false;
        }
    }
}
