﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
using Prototype.NetworkLobby;

public class GameManager : NetworkBehaviour {

    /* singleton pattern - allows global access from any other component. 
     * goes against OOP obviously..
     * Doesnt suggest using singleton often at all.  but this is the place to use it if needed.
     * GameManager.Instance is how to refer to it. Use all public methos and properties from anywhere.
     * Only permits one component of GameManager to exist per Scene (thats why its called a singleton)
     * 
     * This also controls the main game loop 
     */

    public Text m_messageText;
    
    // network lobby does this
    //int m_minPlayers = 2;
    //int m_maxPlayers = 4;

    //[SyncVar]
    //public int m_playerCount = 0;

    // public Color[] m_playerColors = { Color.red, Color.blue, Color.green, Color.magenta };

    static GameManager instance;

    public static List<PlayerManager> m_allPlayers = new List<PlayerManager>();

    public List<Text> m_playerNameText;
    public List<Text> m_playerScoreText;

    public int m_maxScore = 3;

    [SyncVar]
    bool m_gameOver = false;

    PlayerManager m_winner;

    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = GameObject.FindObjectOfType<GameManager>();
                if(instance == null)
                {
                    instance = new GameObject().AddComponent<GameManager>();
                }
            }

            return instance;
        }
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Server]
    void Start()
    {

            StartCoroutine("GameLoopRoutine");

    }

    IEnumerator GameLoopRoutine()
    {
        LobbyManager lobbyManager = LobbyManager.s_Singleton;

        if(lobbyManager != null)
        {
            while(m_allPlayers.Count < lobbyManager._playerNumber)
            {
                yield return null;
            }

            yield return new WaitForSeconds(2f);

            yield return StartCoroutine("StartGame");
            yield return StartCoroutine("PlayGame");
            yield return StartCoroutine("EndGame");
            StartCoroutine("GameLoopRoutine");
        }
        else
        {
            Debug.LogWarning("========== GAMEMANAGER WARNING! Launch game from Lobby scene only! ==========");
        }
        
    }

    IEnumerator StartGame()
    {
        Reset();
        RpcStartGame();
        UpdateScoreboard();
        yield return new WaitForSeconds(3f);

    }

    [ClientRpc]
    void RpcStartGame()
    {
        UpdateMessage("FIGHT");
        DisablePlayers();
    }

    IEnumerator PlayGame()
    {

        yield return new WaitForSeconds(1f);

        RpcPlayGame();

        while (m_gameOver == false)
        {
            CheckScores();
            yield return null;
        }

    }

    [ClientRpc]
    void RpcPlayGame()
    {
        EnablePlayers();


        UpdateMessage("");
    }

    IEnumerator EndGame()
    {
        RpcEndGame();
        RpcUpdateMessage("GAME OVER.\n" + m_winner.m_pSetup.m_name + " wins!");
        yield return new WaitForSeconds(3f);

        Reset();

        LobbyManager.s_Singleton._playerNumber = 0;
        LobbyManager.s_Singleton.SendReturnToLobby();

    }

    [ClientRpc]
    void RpcEndGame()
    {
        DisablePlayers();
        
    }

    void EnablePlayers()
    {
        for(int i = 0; i < m_allPlayers.Count; i++)
        {
            if (m_allPlayers[i] != null)
            {
                m_allPlayers[i].EnableControls();
            }
        }
    }

    void DisablePlayers()
    {
        for (int i = 0; i < m_allPlayers.Count; i++)
        {
            if (m_allPlayers[i] != null)
            {
                m_allPlayers[i].DisableControls();
            }
        }
    }

    [ClientRpc]
    void RpcUpdateScoreboard(string[] playerNames, int[] playerScores)
    {
        for (int i = 0; i < m_allPlayers.Count; i++)
        {
            if (playerNames[i] != null)
            {
                m_playerNameText[i].text = playerNames[i];
            }

            if (playerScores[i] != null)
            {
                m_playerScoreText[i].text = playerScores[i].ToString();
            }
        }
    }

    [Server]
    public void UpdateScoreboard()
    {
        string[] pNames = new string[m_allPlayers.Count];
        int[] pScores = new int[m_allPlayers.Count];

        for(int i = 0; i < m_allPlayers.Count; i++)
        {
            if(m_allPlayers[i] != null)
            {
                pNames[i] = m_allPlayers[i].GetComponent<PlayerSetup>().m_name;
                pScores[i] = m_allPlayers[i].m_score;
            }
        }
        RpcUpdateScoreboard(pNames, pScores);
    }

    public void CheckScores()
    {
        m_winner = GetWinner();

        if (m_winner != null)
        {
            m_gameOver = true;
        }

    }

    [ClientRpc]
    void RpcUpdateMessage(string msg)
    {
        UpdateMessage(msg);
    }

    public void UpdateMessage(string msg)
    {
        if (m_messageText != null)
        {
            m_messageText.gameObject.SetActive(true);
            m_messageText.text = msg;
        }
    }

    PlayerManager GetWinner()
    {

        for (int i = 0; i < m_allPlayers.Count; i++)
        {
            if (m_allPlayers[i].m_score >= m_maxScore)
            {
                return m_allPlayers[i];
            }
        }

        return null;
    }

    void Reset()
    {
        for(int i = 0; i < m_allPlayers.Count; i++)
        {
            PlayerHealth pHealth = m_allPlayers[i].GetComponent<PlayerHealth>();
            pHealth.Reset();

            m_allPlayers[i].m_score = 0;
        }
    }

}
