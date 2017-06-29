using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    public class FunkyRotation : MonoBehaviour
    {

        public float rotationSpeed = 0.0f;
        public float rotationClamps = 180f;
        public Vector3 rotationAxis = Vector3.forward;

        private Quaternion initRotation;
        private float counter = 0.8f;

        void Start()
        {
            initRotation = transform.localRotation;
        }
        // Update is called once per frame
        void Update()
        {
            counter += Time.deltaTime * rotationSpeed;
            transform.localRotation = initRotation * Quaternion.AngleAxis(Mathf.Sin(counter) * rotationClamps, rotationAxis);
        }
    }
}
