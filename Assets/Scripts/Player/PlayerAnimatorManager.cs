using UnityEngine;
using System.Collections;


namespace Com.Shuttler.Widdards
{
    public class PlayerAnimatorManager : MonoBehaviour
    {
        
        private Animator animator;
        private float speedDirection = 1f;

        public float BackwardsSpeed = 0.5f;
        public float SpeedDampTime = 0.1f;
        public float DirectionDampTime = .25f;

        #region MONOBEHAVIOUR MESSAGES
        // Use this for initialization
        void Start()
        {
            animator = GetComponent<Animator>();
            if (!animator)
            {
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component", this);
            }

        }
        // Update is called once per frame
        void Update()
        {
            if (!animator)
            {
                return;
            }


            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            float speed = v + Mathf.Abs(h);
            
            ///speedDirection += v;
            
            animator.SetFloat("Speed", speed, SpeedDampTime, Time.deltaTime);
            ///animator.SetFloat("SpeedMult", speedDirection >= 0?1:-BackwardsSpeed);
            animator.SetFloat("Direction", h, DirectionDampTime, Time.deltaTime);

            ///speedDirection *= SpeedDampTime;

            // deal with Jumping
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // only allow jumping if we are running.
            if (stateInfo.IsName("Base Layer.Run") && speedDirection>0)
            {
                // When using trigger parameter
                if (Input.GetButtonDown("Jump"))
                    animator.SetTrigger("Jump");
            }
        }
        #endregion
    }
}