using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Com.Shuttler.Widdards
{
    public class TimedDestroyer : Photon.PunBehaviour
    {

        public float waitTime = 1.0f;

        private float counter;

        void Start()
        {
            counter = 0.0f;
        }

        void Update()
        {

            counter += Time.deltaTime;

            if (counter >= waitTime && photonView.isMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
