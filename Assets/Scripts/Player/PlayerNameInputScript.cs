using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Com.Shuttler.Widdards
{
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputScript : MonoBehaviour
    {
        static string playerNamePrefKey = "PlayerName";

        void Start()
        {
            string defaultName = "";
            InputField _inputField = this.GetComponent<InputField>();
            if (_inputField!=null)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    _inputField.text = defaultName;
                }
            }

            PhotonNetwork.playerName = defaultName;
        }

        public void SetPlayerName(string value)
        {
            PhotonNetwork.playerName = value + " ";

            PlayerPrefs.SetString(playerNamePrefKey, value);
        }
    }
}
