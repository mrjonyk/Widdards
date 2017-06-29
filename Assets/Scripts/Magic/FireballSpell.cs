using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    public class FireballSpell : Spell
    {
        public GameObject FireballPrefab;
        public float fireballOffset = 0.2f;
        public float fireballHeight = 10f;

        private Vector3 localPos;

        public override Tuple<Vector3, Quaternion> GetSelfInstantiationPosition(Magic magicScript, Vector3 screenCoords)
        {
            return magicScript.GetDefaultSelfInstantiationPosition(screenCoords);
        }

        public override Tuple<Vector3, Quaternion> GetWorldInstantiationPosition(Magic magicScript, Vector3 screenCoords)
        {
            return magicScript.GetDefaultWorldInstantiationPosition(screenCoords);
        }

        [PunRPC]
        public override void SelfSpellInitialization()
        {
            if (photonView.isMine)
            {
                PhotonNetwork.Instantiate(FireballPrefab.name, transform.position + transform.forward*fireballOffset, transform.rotation, 0);
            }
            localPos = transform.position - ((GameObject)photonView.owner.TagObject).transform.position;
        }

        [PunRPC]
        public override void WorldSpellInitialization()
        {
            transform.localScale *= 3;
            GetComponent<TimedDestroyer>().waitTime = 1;

            if (photonView.isMine)
            {
                Vector3 fireballPosition = transform.position + Vector3.up * fireballHeight + transform.forward*5;
                GameObject fireball = PhotonNetwork.Instantiate(FireballPrefab.name, fireballPosition, Quaternion.LookRotation(transform.position - fireballPosition), 0);
                
                Explosion boom = fireball.GetComponent<Explosion>();
                boom.SetVariablesOnAllClients(boom.speed * 2, boom.expansionFactor *2f/3f, 1, fireball.transform.localScale.x * 3);
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (IsSelfSpell)
            {
                transform.position = ((GameObject)photonView.owner.TagObject).transform.position + localPos;
            }
        }
    }
}
