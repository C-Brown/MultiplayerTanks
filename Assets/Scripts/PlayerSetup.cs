using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerSetup : NetworkBehaviour
{
    [SyncVar(hook="UpdateColor")]
    public Color m_playerColor;

    [SyncVar(hook = "UpdateName")]
    public string m_name = "PLAYER";

    // public int m_playerNum = 1;
    public Text m_playerNameText;

    void Start()
    {

        UpdateName(m_name);
        UpdateColor(m_playerColor);

    }

    public override void OnStartClient()
    {
        base.OnStartClient();

    }

    // TODO WEIRD THEY SKIPPED THIS STEP - came back to it later in video
    private void UpdateName(string name)
    {
        if (m_playerNameText != null)
        {
            m_playerNameText.enabled = true;
            m_playerNameText.text = m_name; //+ pNum.ToString();
        }
    }

    private void UpdateColor(Color pColor)
    {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in meshes)
        {
            r.material.color = pColor;
        }
    }

    public override void OnStartLocalPlayer()
    {

        base.OnStartLocalPlayer();

        CmdSetupPlayer();

        //UpdateColor(); call from SyncVar hook, update from GameManager

        //UpdateName(); call from SyncVar hook, update from GameManager

    }

    [Command]
    void CmdSetupPlayer()
    {
        GameManager.Instance.AddPlayer(this);
        GameManager.Instance.m_playerCount++;
    }

    
}
