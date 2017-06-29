using UnityEngine;
using System.Collections;
using System;

namespace Com.Shuttler.Widdards
{
    public class Launcher : Photon.PunBehaviour
    {
        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        public byte MaxPlayersPerRoom = 4;
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        public GameObject progressLabel;

        public GameObject settingsPanel;
        
        bool isConnecting;

        string _gameVersion = "1.1";
        // Use this for initialization
        void Awake()
        {
            PhotonNetwork.logLevel = Loglevel;
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
        }
        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            progressLabel.SetActive(false);
            DeactivateSettings();
        }

        public void Connect()
        {
            isConnecting = true;

            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            if (PhotonNetwork.connected)
            {
                PhotonNetwork.JoinRandomRoom();
            }else
            {
                PhotonNetwork.ConnectUsingSettings(_gameVersion);
            }
        }

        public override void OnJoinedRoom()
        {
            if (PhotonNetwork.room.playerCount == 1)
            {
                Debug.Log("We load the 'Room for 1' ");


                // #Critical
                // Load the Room Level. 
                PhotonNetwork.LoadLevel("Room for 1");
            }
        }
        public override void OnConnectedToMaster()
        {
            Debug.Log("DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN");
            if (isConnecting)
            {
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
                PhotonNetwork.JoinRandomRoom();
            }
        }
        
        public override void OnDisconnectedFromPhoton()
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void ActivateSettings()
        {
            settingsPanel.SetActive(true);
            controlPanel.SetActive(false);
        }

        public void DeactivateSettings()
        {
            settingsPanel.SetActive(false);
            controlPanel.SetActive(true);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingsPanel.GetActive())
                {
                    DeactivateSettings();
                }
                else
                {
                    Quit();
                }
            }
        }
    }
}
