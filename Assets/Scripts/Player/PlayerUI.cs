using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    public class PlayerUI : MonoBehaviour
    {


        #region Public Properties


        [Tooltip("UI Text to display Player's Name")]
        public Text PlayerNameText;


        [Tooltip("UI Slider to display Player's Health")]
        public Slider PlayerHealthSlider;

        [Tooltip("Pixel offset from the player target")]
        public Vector3 ScreenOffset = new Vector3(0f, 30f, 0f);

        public float LocalPlayerHealthbarWidth = 150;
        public Vector3 LocalPlayerUIOffset = Vector3.zero;
        #endregion


        #region Private Properties

        PlayerManager _target;
        float _characterHeight = 0f;
        Vector3 _targetPosition;
        Camera camToFace;
        bool putToSide = false;

        #endregion


        #region MonoBehaviour Messages

        void Awake()
        {
            if (Camera.main != null)
            {
                camToFace = Camera.main;
            } else
            {
                Debug.LogWarning("Player UI could not find Main Camera. So it will not face screen.");
            }
        }

        void Update()
        {
            // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            // Reflect the Player Health
            if (PlayerHealthSlider != null)
            {
                PlayerHealthSlider.value = _target.Health;
            }
        }

        void LateUpdate()
        {
            // #Critical
            // Follow the Target GameObject on screen.
            if (_target!= null && !putToSide)
            {
                _targetPosition = _target.transform.position;
                _targetPosition.y += _characterHeight;
                transform.position = _targetPosition + ScreenOffset;
            }

            if (camToFace != null && !putToSide)
            {
                transform.rotation = Quaternion.LookRotation(camToFace.transform.forward);
            }
        }

        #endregion


        #region Public Methods

        public void SetTarget(PlayerManager target)
        {
            if (target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            putToSide = false;

            // Cache references for efficiency
            _target = target;

            if (PlayerNameText != null)
            {
                PlayerNameText.text = _target.photonView.owner.name;

                Transform child = PlayerNameText.transform.GetChild(0);
                if (child != null)
                {
                    Text highlight = child.GetComponent<Text>();
                    if (highlight != null)
                    {
                        highlight.text = PlayerNameText.text;
                    }
                }
            }

            CapsuleCollider _characterCollider = _target.GetComponent<CapsuleCollider>();
            // Get data from the Player that won't change during the lifetime of this Component
            if (_characterCollider != null)
            {
                _characterHeight = _characterCollider.height;
            }
        }

        public void PutToTheSide(PlayerManager target)
        {
            GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            putToSide = true;

            if (target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            // Cache references for efficiency
            _target = target;


            if (PlayerNameText != null)
            {
                PlayerNameText.text = _target.photonView.owner.name;

                Transform child = PlayerNameText.transform.GetChild(0);
                if (child != null)
                {
                    Text highlight = child.GetComponent<Text>();
                    if (highlight != null)
                    {
                        highlight.text = PlayerNameText.text;
                    }
                }
                if (PlayerHealthSlider != null)
                {
                    Vector3 healthPos = PlayerHealthSlider.transform.localPosition;
                    Vector3 namePos = PlayerNameText.transform.localPosition;
                    float temp = healthPos.y;
                    healthPos.y = namePos.y;
                    namePos.y = temp;

                    PlayerHealthSlider.transform.localPosition = healthPos + LocalPlayerUIOffset + new Vector3(0, Screen.height / 2 + 10);
                    PlayerHealthSlider.transform.localScale *= 2;

                    PlayerNameText.transform.localPosition = namePos + LocalPlayerUIOffset + new Vector3(0, Screen.height / 2 ) ;

                    for (int i = 0; i < PlayerHealthSlider.transform.childCount; i++)
                    {
                        RectTransform rect = PlayerHealthSlider.transform.GetChild(i).GetComponent<RectTransform>();
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, LocalPlayerHealthbarWidth + rect.rect.width);
                    }
                }
            }
            
        }
        #endregion


    }
}