using UnityEngine;
using System.Collections;


namespace Com.Shuttler.Widdards
{
    /// <summary>
    /// Camera work. Follow a target
    /// </summary>
    public class CameraWork : MonoBehaviour
    {


        #region Public Properties


        [Tooltip("The distance in the local x-z plane to the target")]
        public float distance = 7.0f;


        [Tooltip("The height we want the camera to be above the target")]
        public float height = 3.0f;

        [Tooltip("The initial pitch of camera.")]
        public float pitch = 0.0f;
        
        [Tooltip("The Smooth time lag for the height of the camera.")]
        public float heightSmoothLag = 0.3f;


        [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
        public Vector3 centerOffset = Vector3.zero;


        [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
        public bool followOnStart = false;

        [Tooltip("Set this as true if you want the camera to follow the mouse movements and not the characters'")]
        public bool followMouse = false;

        [Tooltip("The higher this is, the more the camera moves along x axis with the mouse, if the above bool is checked.")]
        public float mouseSensitivityX = 1.0f;
        [Tooltip("The higher this is, the more the camera moves along y axis with the mouse, if the above bool is checked.")]
        public float mouseSensitivityY = 1.0f;

        [Tooltip("This limits how much the pitch can deviate from 0.")]
        public float pitchClamp= 10.0f;

        [Tooltip("How high the camera should hover over the ground if there is ground.")]
        public float hoverHeight = 0.5f;

        #endregion


        #region Private Properties


        // cached transform of the target
        Transform cameraTransform;


        // maintain a flag internally to reconnect if target is lost or camera is switched
        bool isFollowing;


        // Represents the current velocity, this value is modified by SmoothDamp() every time you call it.
        private float heightVelocity = 0.0f;


        // Represents the position we are trying to reach using SmoothDamp()
        private float targetHeight = 100000.0f;

        //The current rotation of Camera orbit relative to character.
        private Quaternion pivotRotation = Quaternion.Euler(0, 1, 0);
        


        #endregion


        #region MonoBehaviour Messages


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase
        /// </summary>
        void Start()
        {
            // Start following the target if wanted.
            if (followOnStart)
            {
                OnStartFollowing();
            }
        }


        /// <summary>
        /// MonoBehaviour method called after all Update functions have been called. This is useful to order script execution. For example a follow camera should always be implemented in LateUpdate because it tracks objects that might have moved inside Update.
        /// </summary>
        void LateUpdate()
        {
            // The transform target may not destroy on level load, 
            // so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
            if (cameraTransform == null && isFollowing)
            {
                OnStartFollowing();
            }


            // only follow is explicitly declared
            if (isFollowing)
            {
                Apply();
            }
        }


        #endregion


        #region Public Methods


        /// <summary>
        /// Raises the start following event. 
        /// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
        /// </summary>
        public void OnStartFollowing()
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: No main camera found. Camera works only with a Camera tagged \"MainCamera\".", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }
            isFollowing = true;
            
            // we don't smooth anything, we go straight to the right camera shot
            Cut();
        }


        #endregion


        #region Private Methods


        /// <summary>
        /// Follow the target smoothly
        /// </summary>
        void Apply()
        {
            Vector3 targetCenter = transform.position + centerOffset;
            if (!followMouse)
            {

                // Calculate the current & target rotation angles
                float originalTargetAngle = transform.eulerAngles.y;
                float currentAngle = cameraTransform.eulerAngles.y;


                // Adjust real target angle when camera is locked
                float targetAngle = originalTargetAngle;


                currentAngle = targetAngle;

                // Convert the angle into a rotation, by which we then reposition the camera
                pivotRotation = Quaternion.Euler(0, currentAngle, 0);
            } else if (Cursor.lockState == CursorLockMode.Locked)
            {
                Quaternion mx = Quaternion.AngleAxis(Input.GetAxis("Mouse X")*mouseSensitivityX,new Vector3(0,1,0));
                pivotRotation = pivotRotation * mx;
                pitch += Input.GetAxis("Mouse Y") * mouseSensitivityY;
                pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);
            }



            targetHeight = targetCenter.y + height;
            
            
            
            // Damp the height
            float currentHeight = cameraTransform.position.y;
            currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothLag);
            
            // Set the position of the camera on the x-z plane to:
            // distance meters behind the target
            cameraTransform.position = new Vector3(targetCenter.x, currentHeight, targetCenter.z);
            
            cameraTransform.position += pivotRotation * Vector3.back * distance;

            // Set the height of the camera


            // Always look at the target    
            SetUpRotation(targetCenter);
        }


        /// <summary>
        /// Directly position the camera to a the specified Target and center.
        /// </summary>
        void Cut()
        {
            float oldHeightSmooth = heightSmoothLag;
            heightSmoothLag = 0.001f;

            bool oldFollowMouse = followMouse;
            followMouse = false;

            Apply();

            followMouse = oldFollowMouse;
            heightSmoothLag = oldHeightSmooth;
        }


        /// <summary>
        /// Sets up the rotation of the camera to always be behind the target
        /// </summary>
        /// <param name="centerPos">Center position.</param>
        void SetUpRotation(Vector3 centerPos)
        {
            Vector3 cameraPos = cameraTransform.position;
            Vector3 offsetToCenter = centerPos - cameraPos;


            // Generate base rotation only around y-axis
            Quaternion yRotation;
            if (distance > 0)
            {
                yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, (offsetToCenter.y + 2), offsetToCenter.z));
            }
            else
            {
                yRotation = Quaternion.LookRotation(new Vector3(-offsetToCenter.x, 0, -offsetToCenter.z));
            }
            Quaternion xRotation = Quaternion.AngleAxis(pitch, new Vector3(1, 0, 0));

            cameraTransform.rotation = yRotation * xRotation * Quaternion.Euler(0, 1, 0);
        }


        #endregion
    }
}
