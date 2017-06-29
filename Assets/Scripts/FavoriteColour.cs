using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Shuttler.Widdards
{
    public class FavoriteColour : MonoBehaviour
    {
        public ColorPicker colorPicker;
        public Button button;
        public PlayerManager playerManagerPrefab;

        // Use this for initialization
        void Awake()
        {
            Color c = Color.black;
            c.r = PlayerPrefs.GetFloat("FavColour_R");
            c.g = PlayerPrefs.GetFloat("FavColour_G");
            c.b = PlayerPrefs.GetFloat("FavColour_B");

            SetColor(c);
            colorPicker.CurrentColor = c;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnColorButtonClicked()
        {
            colorPicker.gameObject.SetActive(!colorPicker.gameObject.GetActive());
        }

        public void OnColorChanged()
        {

            SetColor(colorPicker.CurrentColor);
            
            PlayerPrefs.SetFloat("FavColour_R", colorPicker.CurrentColor.r);
            PlayerPrefs.SetFloat("FavColour_G", colorPicker.CurrentColor.g);
            PlayerPrefs.SetFloat("FavColour_B", colorPicker.CurrentColor.b);
        }

        private void SetColor(Color color)
        {
            ColorBlock block = button.colors;
            block.normalColor = color;
            block.highlightedColor = color;
            block.pressedColor = color * 0.8f;

            button.colors = block;
            playerManagerPrefab.RobeColor = color;
        }
    }
}
