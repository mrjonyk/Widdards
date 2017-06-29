using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    public class GustSpell : Spell
    {
        public float SelfSpellOverloadPerSecond = 1;
        public float GustIntensity = 10;

        public override Tuple<Vector3, Quaternion> GetSelfInstantiationPosition(Magic magicScript, Vector3 screenCoords)
        {
            GameObject g = magicScript.gameObject;
            return new Tuple<Vector3, Quaternion>(g.transform.position + Vector3.up * 0.1f, Quaternion.LookRotation(Vector3.up));
        }

        public override Tuple<Vector3, Quaternion> GetWorldInstantiationPosition(Magic magicScript, Vector3 screenCoords)
        {
            return magicScript.GetDefaultWorldInstantiationPosition(screenCoords);
        }

        [PunRPC]
        public override void SelfSpellInitialization()
        {
            GetComponent<TimedDestroyer>().enabled = false;
            transform.localScale *= 2f/ 3f;
            if (MagicScript != null)
            {
                MagicScript.AddSpellToUpdateList(this);
            }
            GetComponent<Collider>().enabled = false;
        }

        [PunRPC]
        public override void WorldSpellInitialization()
        {
            
        }

        void OnTriggerStay(Collider other)
        {
            PhotonView pView = other.GetComponent<PhotonView>();
            if (pView.isMine)
            {
                Rigidbody entity = other.GetComponent<Rigidbody>();
                if (entity != null)
                {
                    entity.AddForce(transform.forward * GustIntensity, ForceMode.Force);
                    entity.AddForce(-Physics.gravity * 1.9f);
                }
            }
        }

        void Update()
        {
            if (IsSelfSpell)
            {
                GameObject g = (GameObject)photonView.owner.TagObject;
                transform.position = g.transform.position + Vector3.up * 0.1f;
                transform.rotation = Quaternion.LookRotation(Vector3.up);

            }
        }

        public override void MagicUpdate(Vector3 screenCoords)
        {
            Rigidbody body = MagicScript.GetComponent<Rigidbody>();

            if (body != null)
            {
                body.AddForce(-Physics.gravity * 1.9f);
            }
            MagicScript.AddOverload(SpellIndex, SelfSpellOverloadPerSecond * Time.deltaTime);

            if (MagicScript.SpellOverloads[SpellIndex] > OverloadThreshold)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }

        public override void OnStopMagicUpdate()
        {
            if (gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}