using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Com.Shuttler.Widdards
{
    public class Scroll : AlphaInheritance
    {
        public int spellIndex = -1;
        public GameObject spellUI;

        [HideInInspector]
        public Magic magicScript;

        public GameObject overloadBanner;
        
        void LateUpdate()
        {
           
            if (spellIndex >= 0 || magicScript != null)
            {
                float scale = magicScript.SpellOverloads[spellIndex] / magicScript.ActiveSpells[spellIndex].OverloadThreshold;
                overloadBanner.transform.localScale = new Vector3(1, scale, 1);
            }
        }

        public void SetSpell(int index, CyclicList<GameObject> UIlist)

        {
            if ( spellUI != null)
            {
                Destroy(RemoveChild(spellUI));
            }
            spellIndex = UIlist.GetActualIndex(index);
            spellUI = Instantiate(UIlist[index]);
            AddChild(spellUI);
            spellUI.transform.localPosition = Vector3.zero;
        }
    }
}