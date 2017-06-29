
using System.Collections.Generic;
using UnityEngine;

namespace Com.Shuttler.Widdards {
    public class DrainLifeSpell : Spell {

        public float SpellDamage = 0.3f;
        public float SelfSpellOverloadPerSecond = 1.0f;
        public float MaxRayDistance = 10f;
        public float WorldSpellSizeMultiplier = 6;
        public float LerpSpeed = 0.3f;

        public LineRenderer Ray;

        private Dictionary<PlayerManager, float> playersToDamage;
        
        void OnTriggerStay(Collider other)
        {
            PlayerManager player = other.GetComponent<PlayerManager>();
            if (player != null)
            {
                player.Health -= SpellDamage * Time.deltaTime;
            }
        }

        public override Tuple<Vector3, Quaternion> GetWorldInstantiationPosition(Magic magicScript, Vector3 screenCoords)
        {
            return magicScript.GetDefaultWorldInstantiationPosition(screenCoords);
        }

        public override Tuple<Vector3, Quaternion> GetSelfInstantiationPosition(Magic magicScript, Vector3 screenCoords)
        {
            return magicScript.GetDefaultSelfInstantiationPosition(screenCoords);
        }

        [PunRPC]
        public override void WorldSpellInitialization()
        {
            transform.localScale *= WorldSpellSizeMultiplier;
            Ray.enabled = false;
        }

        [PunRPC]
        public override void SelfSpellInitialization()
        {
            GetComponent<TimedDestroyer>().enabled = false;
            GetComponent<SphereCollider>().enabled = false;
            if (MagicScript != null)
            {
                MagicScript.AddSpellToUpdateList(this);
            }
            Ray.SetPosition(0, transform.position);
            Ray.SetPosition(1, transform.position + transform.forward);
        }

        public override void MagicUpdate(Vector3 screenCoords)
        {
            Tuple<Vector3, Quaternion> newPos;

            if (IsSelfSpell) {
                newPos = GetSelfInstantiationPosition(MagicScript, screenCoords);

                if (newPos == null)
                {
                    PhotonNetwork.Destroy(gameObject);
                    return;
                }
                transform.position = Vector3.Lerp(transform.position, newPos.First, LerpSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, newPos.Second, LerpSpeed);

                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hit = new RaycastHit();
                hit.point = ray.GetPoint(MaxRayDistance);

                if (Physics.Raycast(ray, out hit, MaxRayDistance, LayerMask.GetMask("Default", "Player"), QueryTriggerInteraction.Ignore))
                {
                    PlayerManager player = hit.collider.GetComponent<PlayerManager>();
                    if (player != null)
                    {
                        player.DealDamageFromAnyClient(SpellDamage * Time.deltaTime);///photonView.RPC("DamagePlayer", PhotonTargets.Others, p.owner);
                    }
                }
                photonView.RPC("SetRay", PhotonTargets.All, hit.point);

                MagicScript.AddOverload(SpellIndex, SelfSpellOverloadPerSecond * Time.deltaTime);

                if (MagicScript.SpellOverloads[SpellIndex] > OverloadThreshold)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        [PunRPC]
        private void SetRay(Vector3 endPoint)
        {
            Ray.SetPosition(0, transform.position);
            Ray.SetPosition(1, endPoint);
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
