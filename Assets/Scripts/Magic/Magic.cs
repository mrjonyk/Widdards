using System.Collections.Generic;
using UnityEngine;

namespace Com.Shuttler.Widdards
{
    [RequireComponent(typeof(SphereCollider))]
    public class Magic : Photon.PunBehaviour
    {
        [Tooltip("The local magic instance. Use this to know if the local player is represented in the Scene and has magic.")]
        public static Magic LocalMagicInstance;

        [Tooltip("How much world spells should hover over the raycast hit.")]
        public float SpellHoverHeight = 0.1f;

        [Tooltip("How far self spells are from the player.")]
        public float SelfSpellDistance = 1f;

        public Vector3 SpellCastOffset = Vector3.up;

        [Tooltip("How fast the spells cool off the overload from being casted, in overload per second.")]
        public float SpellCooldownSpeed = 1.0F;

        [Tooltip("True if this script should cast world spells, false if it should cast self spells.")]
        public bool isWorldCaster;

        [Tooltip("Should all spell prefabs be activated? If false, activate spellprefabs using ActivateSpellPrefab(int index), only then can they be used in game.")]
        public bool ActivateAllSpells = true;

        [Tooltip("Add prefabs here that have the Spell class implemented and attached.")]
        public GameObject[] SpellPrefabs;
        
        public List<Spell> ActiveSpells
        {
            get { return new List<Spell>(activeSpells); }
        }
        
        public List<float> SpellOverloads
        {
            get { return new List<float>(spellOverloads); }
        }

        private List<Spell> magicUpdateList;
        private List<Spell> activeSpells;
        private List<float> spellOverloads;

        void Awake()
        {
            activeSpells = new List<Spell>();
            magicUpdateList = new List<Spell>();
            spellOverloads = new List<float>();

            if (photonView.isMine)
            {
                LocalMagicInstance = this;
                Debug.Log("Local Magic Instance: "+ LocalMagicInstance);
                
                if (ActivateAllSpells)
                {
                    for ( int i = 0; i < SpellPrefabs.Length; i++)
                    {
                        ActivateSpellPrefab(i);
                    }
                }
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (!photonView.isMine)
                return;
            for (int i = 0; i < spellOverloads.Count; i++)
            {
                AddOverload(i, -SpellCooldownSpeed * Time.deltaTime);
            }
        }

        void OnDisable()
        {
            foreach (Spell spell in magicUpdateList) {
                spell.OnMagicDisable();
            }
        }

        public void CastSpell(int spellIndex, Vector3 screenCoords)
        {
            if (spellIndex < 0 || spellIndex >= activeSpells.Count)
            {
                Debug.LogError("Your SelectedSpell is not within the bounds of the Spells array as 0 < " + spellIndex + " < " + activeSpells.Count + " is not true.");
                return;
            }

            Spell selectedSpell = activeSpells[spellIndex];

            Tuple<Vector3, Quaternion> hitTransform;

            if (isWorldCaster)
            {
                hitTransform = selectedSpell.GetWorldInstantiationPosition(this, screenCoords);
            } else
            {
                hitTransform = selectedSpell.GetSelfInstantiationPosition(this, screenCoords);
            }

            if (hitTransform != null)
            {
                float overload = isWorldCaster ? selectedSpell.WorldOverloadAmount : selectedSpell.SelfOverloadAmount;
                
                if (spellOverloads[spellIndex] + overload > selectedSpell.OverloadThreshold)
                {
                    return;
                }
                Debug.Log("Spell " + spellIndex + ": " + selectedSpell.name + " was cast as a " + (isWorldCaster ? "world" : "self") + " spell with overload cost: " + overload);
                AddOverload(spellIndex, overload);

                GameObject spellInstance = PhotonNetwork.Instantiate(selectedSpell.gameObject.name, hitTransform.First, hitTransform.Second, 0);
                spellInstance.GetComponent<Spell>().MagicInitialization(this, isWorldCaster, spellIndex);
            }
        }

        public void ActivateSpellPrefab(int prefabIndex)
        {
            if (prefabIndex < 0 || prefabIndex >= SpellPrefabs.Length)
            {
                Debug.LogError("Your index is not within the bounds of the Spell Prefab array as 0 < " + prefabIndex + " < " + SpellPrefabs.Length + " is not true.");
                return;
            }
            Spell spell = SpellPrefabs[prefabIndex].GetComponent<Spell>();
            if (spell != null)
            {
                activeSpells.Add(spell);
                spellOverloads.Add(0.0f);
            } else
            {
                Debug.LogError("Your SpellPrefab at index "+prefabIndex+" does not have a Spell component attached.");
            }
        }

        public void DeactivateSpellPrefab(int prefabIndex)
        {
            if (prefabIndex < 0 || prefabIndex >= SpellPrefabs.Length)
            {
                Debug.LogError("Your index is not within the bounds of the Spell Prefab array as 0 < " + prefabIndex + " < " + SpellPrefabs.Length + " is not true.");
                return;
            }
            Spell spell = SpellPrefabs[prefabIndex].GetComponent<Spell>();
            if (spell != null)
            {
                int i = activeSpells.IndexOf(spell);
                activeSpells.RemoveAt(i);
                spellOverloads.RemoveAt(i);
            }
            else
            {
                Debug.LogError("Your SpellPrefab at index " + prefabIndex + " does not have a Spell component attached.");
            }
        }

        public void AddSpellToUpdateList(Spell spell)
        {
            if (!activeSpells.Contains(spell))
                magicUpdateList.Add(spell);
        }
        public void RemoveSpellFromUpdateList(Spell spell)
        {
            if (spell != null)
            {
                spell.OnStopMagicUpdate();
                magicUpdateList.Remove(spell);
            }
        }
        public void RemoveAllSpellsFromUpdateListOfTypeSpellIndex (int spellIndex)
        {
            for (int i = magicUpdateList.Count - 1; i >= 0; i--)
            {
                if (spellIndex == magicUpdateList[i].SpellIndex && magicUpdateList[i] != null)
                { 
                    magicUpdateList[i].OnStopMagicUpdate();
                    magicUpdateList.RemoveAt(i);
                }
            }
        }

        public void AddOverload(int index, float amount)
        {
            spellOverloads[index] += amount;
            if (spellOverloads[index] < 0)
            {
                spellOverloads[index] = 0.0f;
            }
        }


        public void MagicUpdate(Vector3 screenCoords)
        {
            CheckForTrashInUpdateList();

            if (magicUpdateList.Count > 0)
            {
                foreach (Spell spellInstance in magicUpdateList)
                {
                    spellInstance.MagicUpdate(screenCoords);
                }
            }
        }
        public Tuple<bool, RaycastHit> PhysicsRaycast(Vector3 screenCoords, bool hitOtherPlayers)
        {
            if (float.IsNaN(screenCoords.x))
            {
                screenCoords = new Vector3(Screen.width/2, Screen.height/2);
            }
            
            Ray ray = Camera.main.ScreenPointToRay(screenCoords);
            RaycastHit hitInfo = new RaycastHit();
            hitInfo.point = ray.GetPoint(100);
            hitInfo.normal = ray.direction;

            int prevLayer = gameObject.layer;

            if (photonView.isMine)
            {
                gameObject.layer = LayerMask.GetMask("Ignore Raycast");
            }
            int layerMask;
            if (hitOtherPlayers)
            {
                layerMask = LayerMask.GetMask("Default", "MagicBlock", "Player");
            }
            else
            {
                layerMask = LayerMask.GetMask("Default", "MagicBlock");
            }
            bool rayhit = Physics.Raycast(ray, out hitInfo, 100, layerMask, QueryTriggerInteraction.Ignore);

            if (rayhit)
            {
                RaycastHit cameraHit;
                Debug.DrawLine(hitInfo.point, hitInfo.point + hitInfo.normal);
                if (hitInfo.collider.gameObject.layer == 9)
                {
                    return null;
                }
                if(hitInfo.collider is TerrainCollider && hitInfo.collider.Raycast(new Ray(Camera.main.transform.position,Vector3.up), out cameraHit,10))
                {
                    float diff = 100 - hitInfo.distance;
                    ray.origin = hitInfo.point + hitInfo.normal * 0.1f;
                    hitInfo.point = ray.GetPoint(diff);
                    hitInfo.normal = ray.direction;
                    rayhit = Physics.Raycast(ray, out hitInfo, diff, layerMask, QueryTriggerInteraction.Ignore);
                }
            }

            gameObject.layer = prevLayer;
            return new Tuple<bool, RaycastHit>(rayhit, hitInfo);
        }
        public Tuple<bool, RaycastHit> PhysicsRaycast(Vector3 screenCoords)
        {
            return PhysicsRaycast(screenCoords, true);
        }

        public Tuple<Vector3, Quaternion> GetDefaultSelfInstantiationPosition(Vector3 screenCoords)
        {
            return GetDefaultSelfInstantiationPosition(screenCoords, PhysicsRaycast(screenCoords, true));
        }
        public Tuple<Vector3, Quaternion> GetDefaultSelfInstantiationPosition(Vector3 screenCoords, Tuple<bool, RaycastHit> raycastHit)
        {
            Vector3 dir;
            if (raycastHit == null)
            {
                return null;
            }
            if (float.IsNaN(screenCoords.x))
            {
                dir = Camera.main.transform.forward; ///Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
            }
            else
            {
                dir = raycastHit.Second.point - transform.position - SpellCastOffset;
            }
            Quaternion q = Quaternion.LookRotation(dir);
            dir = transform.position + SpellCastOffset + dir.normalized * SelfSpellDistance;
            return new Tuple<Vector3, Quaternion>(dir, q);
        }
        public Tuple<Vector3, Quaternion> GetDefaultWorldInstantiationPosition(Vector3 screenCoords)
        {
            Tuple<bool, RaycastHit> rayhit = PhysicsRaycast(screenCoords, false);

            if (rayhit != null && rayhit.First)
            {
                return new Tuple<Vector3, Quaternion>(rayhit.Second.point + rayhit.Second.normal * SpellHoverHeight, Quaternion.LookRotation(rayhit.Second.normal));
            } else
            {
                return null;
            }
        }

        private void CheckForTrashInUpdateList()
        {
            for (int i = magicUpdateList.Count-1; i >= 0; i--)
            {
                if (magicUpdateList[i] == null)
                {
                    magicUpdateList.RemoveAt(i);
                }
            }
        }
    }
}

/*
public Tuple<Vector3, Quaternion> SphereRaycast(Vector3 screenCoords)
{
    RaycastHit hitInfo;
    Ray ray = Camera.main.ScreenPointToRay(screenCoords);

    ray.origin = ray.GetPoint(10);
    ray.direction = -ray.direction;
    magicSphere.Raycast(ray, out hitInfo, 10);

    Vector3 offset = transform.TransformPoint(SpellCastOffset);

    Vector3 dir = hitInfo.point - offset;

    Vector3 position = offset + dir.normalized * SelfSpellDistance;
    Quaternion rotation = Quaternion.LookRotation(dir);

    return new Tuple<Vector3, Quaternion>(position, rotation);
}

public Tuple<Vector3, Quaternion> PlaneRaycast(Vector3 screenCoords)
{
    float dist;
    Ray ray = Camera.main.ScreenPointToRay(screenCoords);

    if (magicPlane.Raycast(ray, out dist))
    {
        Vector3 hit = ray.GetPoint(dist) + Vector3.up * SpellHoverHeight;
        Vector3 offset = transform.TransformPoint(SpellCastOffset);
        Debug.DrawLine(offset, hit);
        Vector3 dir = hit - offset;

        if (dir.sqrMagnitude <= magicSphere.radius * magicSphere.radius)
        {
            Vector3 position;

            if (dir == Vector3.zero)
            {
                dir = Vector3.forward;
                position = offset + dir * SelfSpellDistance;
            }
            else
            {
                position = offset + dir.normalized * SelfSpellDistance;
            }
            Quaternion rotation = Quaternion.LookRotation(dir);
            return new Tuple<Vector3, Quaternion>(position, rotation);
        }
    }
    return SphereRaycast(screenCoords);
} 
#endregion
*/