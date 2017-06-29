using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    /// <summary>
    /// 1.  Override this class to implement the dual spell behaviours, 
    ///     one for when the left button was used to instantiate it, and the other for the right button.
    ///     The position of the spell is different based on which button was used.
    /// 
    /// 2.  Attach this to a gameObject then turn it into a prefab.
    /// 
    /// 3.  Add the prefab into the list of spells in Magic Script which you attach to the player prefab.
    /// </summary>
    public abstract class Spell : Photon.PunBehaviour
    {
        public float OverloadThreshold { get { return overloadThreshold; } }

        public float WorldOverloadAmount { get { return worldOverloadAmount; } }

        public float SelfOverloadAmount { get { return selfOverloadAmount; } }
        
        public bool IsWorldSpell
        {
            get { return isWorld; }
        }
        public bool IsSelfSpell
        {
            get { return !isWorld; }
        }
        public int SpellIndex
        {
            get { return spellIndex; }
        }
        public Magic MagicScript
        {
            get { return script; }
        }

        public GameObject SpellUI;

        [SerializeField]
        [Tooltip("If the overload of the spell plus the CastOverloadAmount is over this threshold, then it cannot be cast.")]
        private float overloadThreshold = 1.0f;

        [SerializeField]
        [Tooltip("The amount added to the overload of the spell each time the world spell is cast.")]
        private float worldOverloadAmount = 1.0f;

        [SerializeField]
        [Tooltip("The amount added to the overload of the spell each time the world spell is cast.")]
        private float selfOverloadAmount = 1.0f;
        
        private bool isWorld = true;
        private int spellIndex;
        private Magic script;

        /// <summary>
        /// Don't hide this method behind new. The Magic Script should call this upon initialization.
        /// </summary>
        /// <param name="isWorld"></param>
        public void MagicInitialization(Magic script, bool isWorld, int spellIndex)
        {
            this.script = script;

            photonView.RPC("SetVariables", PhotonTargets.All, isWorld, spellIndex);

            if (isWorld)
            {
                photonView.RPC("WorldSpellInitialization", PhotonTargets.All);
            } else {
                photonView.RPC("SelfSpellInitialization", PhotonTargets.All);
            }
        }

        [PunRPC]
        protected void SetVariables(bool isWorld, int spellIndex)
        {
            this.isWorld = isWorld;
            this.spellIndex = spellIndex;
        }

        /// <summary>
        /// These methods are called before the magic script instantiates a copy of the spell.
        /// 
        /// </summary>
        /// <param name="magicScript">The magic script in question. Has a lot of useful raycasting options.</param>
        /// <param name="screenCoords">The screen coordinates the magic script got from PlayerManager.</param>
        /// <returns></returns>
        public abstract Tuple<Vector3, Quaternion> GetWorldInstantiationPosition(Magic magicScript, Vector3 screenCoords);
        public abstract Tuple<Vector3, Quaternion> GetSelfInstantiationPosition(Magic magicScript, Vector3 screenCoords);
        

        /// <summary>
        /// Override these methods to have the Magic Script call them upon initialization. Not after!!
        /// Use this to subsribe to the magic update list if MagicScript != null (meaning it is the local client) via AddSpellToUpdateList(this);
        /// 
        /// Will be called on all clients.
        /// </summary>
        [PunRPC]
        public abstract void WorldSpellInitialization();
        [PunRPC]
        public abstract void SelfSpellInitialization();


        /// <summary>
        /// Override this method and call AddSpellToUpdateList(this); during SpellInitialization for the Magic Script to call this function every frame.
        /// </summary>
        /// <param name="button"> If the button used to trigger this spell is still pressed. </param>
        /// <param name="hitTransform"> Where a new world spell would be cast. </param>
        public virtual void MagicUpdate(Vector3 screenCoords) { }

        /// <summary>
        /// This is called when the spell is removed from the magic update list.
        /// </summary>
        public virtual void OnStopMagicUpdate() { }

        /// <summary>
        /// Called whenever the Magic Script is disabled, if it is in update list.
        /// </summary>
        public virtual void OnMagicDisable() { }
    }
}
