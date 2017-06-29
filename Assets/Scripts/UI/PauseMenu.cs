using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject PausePanel;
        public bool ShouldPausePanelClose = false;

        void Start()
        {
            PausePanel.SetActive(false);
        }

        public void TriggerPauseMenu()
        {
            if (ShouldPausePanelClose)
            {
                PausePanel.SetActive(false);
                ShouldPausePanelClose = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (!PausePanel.activeInHierarchy)
            {
                PausePanel.SetActive(true);
                ShouldPausePanelClose = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}