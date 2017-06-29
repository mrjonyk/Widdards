using UnityEngine;
using System.Collections;

namespace Com.Shuttler.Widdards
{
    public class Explosion : Photon.PunBehaviour
    {

        public float speed = 1.0f;
        public float expansionFactor = 15f;
        public float maxAirTime = 1f;
        
        private Vector3 size;
        private ArrayList mask;

        void Start()
        {
            GetComponent<Rigidbody>().velocity = transform.forward * speed;
            size = transform.localScale;

            mask = new ArrayList();
            StartCoroutine(MaxAirTime(maxAirTime));
        }

        void OnTriggerEnter(Collider other)
        {
            var hitPlayer = other.GetComponent<PlayerManager>();

            if (hitPlayer != null && !mask.Contains(hitPlayer))
            {
                hitPlayer.Health -= (expansionFactor * size.x) / (10f * (hitPlayer.transform.position - transform.position).sqrMagnitude);
                mask.Add(hitPlayer);
            }
        }

        void OnCollisionEnter(Collision bang)
        {
            
            if (!GetComponent<Collider>().isTrigger && photonView.isMine)
            {
                photonView.RPC("Boom", PhotonTargets.All, transform.position);
            }
        }

        public void SetVariablesOnAllClients(float Speed, float ExpansionFactor, float MaxAirTime, float scale)
        {
            photonView.RPC("SetVariables", PhotonTargets.All, Speed, ExpansionFactor, MaxAirTime, scale);
        }
        [PunRPC]
        private void SetVariables(float speed, float expansionFactor, float maxAirTime, float scale)
        {
            this.speed = speed;
            this.expansionFactor = expansionFactor;
            this.maxAirTime = maxAirTime;
            transform.localScale = new Vector3(scale, scale, scale);
        }

        [PunRPC]
        private void Boom(Vector3 position)
        {
            transform.position = position;
            StartCoroutine(BoomBaby(0.2f));
        }
        
        IEnumerator MaxAirTime(float time)
        {
            yield return new WaitForSeconds(time);
            yield return BoomBaby(0.2f);
        }

        IEnumerator BoomBaby(float explosionTime)
        {
            float timer = 0f;
            size = transform.localScale;

            GetComponent<Collider>().isTrigger = true;
            GetComponent<Rigidbody>().isKinematic = true;

            while (timer <= explosionTime)
            {
                transform.localScale = Vector3.Lerp(size, size * expansionFactor, timer / explosionTime);

                timer += Time.deltaTime;
                yield return null;
            }
            if (photonView.isMine && gameObject != null)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            yield return null;
        }
    }
}
