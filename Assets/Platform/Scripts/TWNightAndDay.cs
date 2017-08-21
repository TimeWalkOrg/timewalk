using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform
{
    //<summary>Controls night and day skyboxes and sun/moon position</summary>
    public class TWNightAndDay : MonoBehaviour
    {

        public Material skyDay;
        public Material skyNight;
		public Color colorMoon = Color.blue;
        public Color colorSun = Color.white;
        public float dayIntensity = 0.4f;
        public float nightIntensity = 0.1f;

        // Use this for initialization
        void Start()
        {
            UpdateNightAndDay();
            TWGameManager.instance.TWNightDayChanged += UpdateNightAndDay;
        }

        // Update is called once per frame
        void Update()
        {
            // Rotate sunmoon
        }

        private void UpdateNightAndDay()
        {
            // TODO set flare
            if(TWGameManager.instance.IsDayTime)
            {
                RenderSettings.skybox = skyDay;
				RenderSettings.ambientIntensity = dayIntensity;
				GetComponent<Light>().color = colorSun;
			}
            else
            {
                RenderSettings.skybox = skyNight;
                GetComponent<Light>().color = colorMoon;
				RenderSettings.ambientIntensity = nightIntensity;
			}
        }
    }
}
