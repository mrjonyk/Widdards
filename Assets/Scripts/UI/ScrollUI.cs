using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Shuttler.Widdards
{
    public class ScrollUI : MonoBehaviour
    {

        public float LerpSpeed;
        public int ScrollCount;
        public CyclicList<Scroll> scrolls;
        public Text spellNameText;

        [HideInInspector]
        public Magic magicScript;
        
        private CyclicList<GameObject> spellUIs;
        private Vector3[] data;
        private float counter = 0.0f;

        void Start()
        {
            scrolls = new CyclicList<Scroll>();
            spellUIs = new CyclicList<GameObject>();

            for (int i=0; i < ScrollCount; i++)
            {
                scrolls.Add(transform.GetChild(i).GetComponent<Scroll>());
            }
            data = new Vector3[ScrollCount];

            for (int i = 0; i < ScrollCount; i++)
            {
                Vector3 dataVector = scrolls[i].transform.position;
                dataVector.z = scrolls[i].myImage.color.a;
                data[i] = dataVector;
            }
        }

        void BindToMagicScript()
        {
            magicScript = Magic.LocalMagicInstance;
            for (int i = 0; i < magicScript.ActiveSpells.Count; i++)
            {
                spellUIs.Add(magicScript.ActiveSpells[i].SpellUI);
            }
            magicScript.GetComponent<PlayerManager>().SelectedSpell = spellUIs.Modulo(2);
            

            for (int i = 0; i < ScrollCount; i++)
            {
                scrolls[i].magicScript = magicScript;
                scrolls[i ].SetSpell(spellUIs.Modulo(i ), spellUIs);
            }
            spellNameText.text = scrolls[2].spellUI.name.Split("(".ToCharArray())[0];
        }

        // Update is called once per frame
        void Update()
        {
            if (magicScript == null)
            {
                BindToMagicScript();
            }
            if (counter == 0)
            {
                return;
            }

            counter -= Time.deltaTime;

            if (counter <= 0)
            {
                counter = 0.0f;
                for (int i = 0; i < data.Length; i++)
                {
                    SetData(i);
                }
            }

            for (int i = 0; i < data.Length; i++)
            {
                scrolls[i].transform.position = Vector3.Lerp(scrolls[i].transform.position, new Vector3(data[i].x, data[i].y, scrolls[i].transform.position.z), LerpSpeed * Time.deltaTime);
                Color c = scrolls[i].myImage.color;
                c.a = Mathf.Lerp(c.a, data[i].z, LerpSpeed * Time.deltaTime);
                scrolls[i].myImage.color = c;
            }
        }

        public void ScrollDown()
        {
            scrolls.Offset -= 1;
            spellUIs.Offset -= 1;
            magicScript.GetComponent<PlayerManager>().SelectedSpell = spellUIs.Modulo(magicScript.GetComponent<PlayerManager>().SelectedSpell - 1);

            scrolls[0].SetSpell(0, spellUIs);

            SetData(0);

            counter = 2.0f;
        }

        public void ScrollUp()
        {
            scrolls.Offset += 1;
            spellUIs.Offset += 1;

            magicScript.GetComponent<PlayerManager>().SelectedSpell = spellUIs.Modulo(magicScript.GetComponent<PlayerManager>().SelectedSpell + 1);

            scrolls[-1].SetSpell(4, spellUIs);
            SetData(scrolls.Count - 1);

            counter = 2.0f;
        }
        
        private void SetData(int index)
        {

            scrolls[index].transform.position = new Vector3(data[index].x, data[index].y, scrolls[index].transform.position.z);
            Color c = scrolls[index].myImage.color;
            c.a = data[index].z;
            scrolls[index].myImage.color = c;

            spellNameText.text = scrolls[2].spellUI.name.Split("(".ToCharArray())[0];
        }
    }
}