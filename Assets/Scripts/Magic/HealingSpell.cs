
using System;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    public class HealingSpell : Spell
    {

        public float HealingSpeed = 0.4f;

        void OnTriggerEnter(Collider other)
        {
            PlayerManager p = other.GetComponent<PlayerManager>();
            if (p != null)
            {
                p.Health += HealingSpeed * Time.deltaTime;
            }
        }

        void OnTriggerStay(Collider other)
        {
            PlayerManager p = other.GetComponent<PlayerManager>();
            if (p != null)
            {
                p.Health += HealingSpeed * Time.deltaTime;
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

        [PunRPC]
        public override void WorldSpellInitialization()
        {
            transform.localScale *= 3;
        }

        [PunRPC]
        public override void SelfSpellInitialization()
        {
            GetComponent<TimedDestroyer>().waitTime = 1.0f;
        }

        public override Tuple<Vector3, Quaternion> GetWorldInstantiationPosition(Magic magicScript, Vector3 screenCoords)
        {
            return magicScript.GetDefaultWorldInstantiationPosition(screenCoords);
        }

        public override Tuple<Vector3, Quaternion> GetSelfInstantiationPosition(Magic magicScript, Vector3 screenCoords)
        {
            GameObject g = magicScript.gameObject;
            return new Tuple<Vector3, Quaternion>(g.transform.position + Vector3.up * 0.1f, Quaternion.LookRotation(Vector3.up));
        }
    }
}