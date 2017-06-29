using System.Collections;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    public class PlayerManager : Photon.PunBehaviour, IPunObservable
    {

        [Tooltip("The Player's UI GameObject Prefab that follows all players.")]
        public GameObject PlayerUiPrefab;

        [Tooltip("The UI Canvas GameObject Prefab that covers the screen of the local player.")]
        public GameObject UICanvasPrefab;

        public float DeathHeight = -100f;
        public Renderer RobeMeshRenderer;
        public Color RobeColor;
        
        public float Health
        {
            get { return health; }
            set { if (value > maxHealth)
                {
                    health = maxHealth;
                }
                else
                {
                    health = value;
                }
            }
        }

        [Tooltip("The lock state of the cursor at the initialization of the player.")]
        public CursorLockMode initialLockMode = CursorLockMode.Locked;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        public int SelectedSpell = 0;

        public Magic magicScript;

        private PauseMenu pauseMenu;

        private ScrollUI scrollUI;
        private CameraWork cameraWork;
        private Vector2 originalCameraOffset;
        private bool isThirdPerson = true;
        private bool isAlreadyZooming = false;
        private float maxHealth = 1;
        [Tooltip("The current Health of our player")]
        private float health = 1f;

        void Awake()
        {
            maxHealth = Health;

            if (photonView.isMine)
            {
                LocalPlayerInstance = gameObject;
                ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add("RobeColor_RGB", new Vector3(RobeColor.r, RobeColor.g, RobeColor.b));
                PhotonNetwork.player.SetCustomProperties(hash);
            } else
            {
                ThirdPersonUserControlPun control = GetComponent<ThirdPersonUserControlPun>();
                ThirdPersonCharacter movement = GetComponent<ThirdPersonCharacter>();
                control.enabled = false;
                movement.enabled = false;
            }
            photonView.owner.TagObject = gameObject;
            DontDestroyOnLoad(gameObject);
        }

        // Use this for initialization
        void Start()
        {
            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(PlayerUiPrefab) as GameObject;
                string method;
                if (photonView.isMine)
                {
                    method = "PutToTheSide";
                }
                else
                {
                    method = "SetTarget";
                }
                _uiGo.SendMessage(method, this, SendMessageOptions.RequireReceiver);
                
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

            if (photonView.isMine)
            {
                if (UICanvasPrefab != null)
                {
                    GameObject _uiCanvas = Instantiate(UICanvasPrefab) as GameObject;
                    pauseMenu = _uiCanvas.GetComponentInChildren<PauseMenu>();
                    scrollUI = _uiCanvas.GetComponentInChildren<ScrollUI>();
                }
                magicScript.isWorldCaster = false;
                Cursor.lockState = initialLockMode;
                cameraWork = GetComponent<CameraWork>();
                originalCameraOffset = new Vector2(cameraWork.distance, cameraWork.height);


                if (cameraWork != null)
                {
                    cameraWork.OnStartFollowing();
                }
                else
                {
                    Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
                }

            } else
            {
                magicScript.enabled = false;
            }

            MaterialPropertyBlock matBlock = new MaterialPropertyBlock();
            Vector3 rgb = (Vector3)photonView.owner.customProperties["RobeColor_RGB"];
            matBlock.SetColor("_Color", new Color(rgb.x, rgb.y, rgb.z));
            RobeMeshRenderer.SetPropertyBlock(matBlock);
        }

        // Update is called once per frame
        void Update()
        {
            if (!photonView.isMine)
            {
                return;
            }
            
            if (Health <= 0f)
            {
                GameManager.Instance.LeaveRoom();
            }
            if (transform.position.y < DeathHeight)
            {
                Health = 0f;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && pauseMenu != null)
            {
                pauseMenu.TriggerPauseMenu();
            }
            if (scrollUI != null)
            {
                float delta = Input.GetAxis("Mouse ScrollWheel");

                if (Input.GetKeyDown(KeyCode.Q) || delta < 0)
                {
                    scrollUI.ScrollDown();
                }
                else if (Input.GetKeyDown(KeyCode.E) || delta > 0)
                {
                    scrollUI.ScrollUp();
                }
            }
            if (Input.GetKeyDown(KeyCode.V) && !isAlreadyZooming)
            {
                if (isThirdPerson)
                {
                    StartCoroutine(LerpCameraDistance(originalCameraOffset, new Vector2(0.4f, 1.5f), 1.3f, new Vector2(-0.2f, 1.5f)));
                } else
                {
                    StartCoroutine(LerpCameraDistance(new Vector2(0.2f, 1.5f), originalCameraOffset, 2, originalCameraOffset));
                }
            }
            
            if (Input.GetButtonDown("Fire3"))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Input.GetButtonUp("Fire3"))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            Vector3 mousePos = new Vector3(float.NaN, float.NaN);
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                mousePos = Input.mousePosition;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                magicScript.isWorldCaster = true;
                magicScript.CastSpell(SelectedSpell, mousePos);
            }
            if (Input.GetButtonDown("Fire2"))
            {
                magicScript.isWorldCaster = false;
                magicScript.CastSpell(SelectedSpell, mousePos);
            }

            if (Input.GetButtonUp("Fire1") || Input.GetButtonUp("Fire2"))
            {
                magicScript.RemoveAllSpellsFromUpdateListOfTypeSpellIndex(SelectedSpell);
            }
            magicScript.MagicUpdate(mousePos);
        }

        public void DealDamageFromAnyClient(float dmg)
        {
            photonView.RPC("DealLocalDamage", PhotonTargets.All, dmg);
        }
        [PunRPC]
        private void DealLocalDamage(float dmg)
        {
            if (photonView.isMine)
            {
                Health -= dmg;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (photonView.isMine)
            {
                float damage = collision.impulse.sqrMagnitude * 0.0003f;
                if (damage > 0.05f)
                {
                    Health -= damage;
                }
            }
        }
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(Health);
            }
            else
            {
                // Network player, receive data
                Health = (float)stream.ReceiveNext();
            }
        }

        private IEnumerator LerpCameraDistance(Vector2 from, Vector2 to, float time, Vector2 teleportValue)
        {
            float counter = 0;
            isAlreadyZooming = true;

            while (counter <= time)
            {
                counter += Time.deltaTime;

                Vector2 v = Vector2.Lerp(from, to, counter/time);
                cameraWork.distance = v.x;
                cameraWork.height = v.y;
                yield return null;
            }
            cameraWork.distance = teleportValue.x;
            cameraWork.height = teleportValue.y;
            isThirdPerson = !isThirdPerson;
            isAlreadyZooming = false;
        }
    }
}
