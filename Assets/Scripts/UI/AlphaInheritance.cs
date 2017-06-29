using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphaInheritance : MonoBehaviour {

    public List<float> originalOpacities;
    public Image myImage;

	// Use this for initialization
	void Awake () {
        myImage = GetComponent<Image>();

        originalOpacities = new List<float>();

        for (int i = transform.childCount-1; i >= 0; i-- )
        { 
            Image im = transform.GetChild(i).GetComponent<Image>();
            if (im != null)
            {
                
                originalOpacities.Add(im.color.a);
                Color c = im.color;
                c.a = originalOpacities[i] * myImage.color.a;
                transform.GetChild(i).GetComponent<Image>().color = c;
            } else
            {
                originalOpacities.Add(-1);
            }
        }
	}

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Image im = transform.GetChild(i).GetComponent<Image>();
            if (im != null)
            {
                Color c = im.color;
                c.a = originalOpacities[i] * myImage.color.a;
                transform.GetChild(i).GetComponent<Image>().color = c;
            }
        }
    }

    public void AddChild(GameObject child)
    {
        Image im = child.GetComponent<Image>();
        if (im != null)
        {
            originalOpacities.Add(im.color.a);
            Color c = im.color;
            c.a = im.color.a * myImage.color.a;
            child.GetComponent<Image>().color = c;
        } else
        {
            originalOpacities.Add(-1);
        }

        child.transform.SetParent(transform);
        child.transform.SetAsLastSibling();
    }

    public void InsertChild(int index, GameObject child)
    {
        Image im = child.GetComponent<Image>();
        if (im != null)
        {
            originalOpacities.Insert(index,im.color.a);
            Color c = im.color;
            c.a = im.color.a * myImage.color.a;
            child.GetComponent<Image>().color = c;
        }
        else
        {
            originalOpacities.Insert(index, -1);
        }

        child.transform.SetParent(transform);
        child.transform.SetSiblingIndex(index);
    }
    public GameObject RemoveChild(GameObject child)
    {
        int i = child.transform.GetSiblingIndex();
        return RemoveChildAt(i);
    }

        public GameObject RemoveChildAt(int index)
    {
        originalOpacities.RemoveAt(index);

        Transform t = transform.GetChild(index);
        t.SetParent(null);
        return t.gameObject;
    }
}
