using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Derived from Glen Rhodes' tutorial here: https://www.youtube.com/watch?v=h5GFoI38DOg

namespace TimeWalk.Platform
{
    //<summary>Controls night and day skyboxes and sun/moon position</summary>
    public class TWNightAndDay : MonoBehaviour
    {
		public Gradient nightDayColor;

		public float maxIntensity = 3f;
		public float minIntensity = 0f;
		public float minPoint = -0.2f;

		public float maxAmbient = 1f;
		public float minAmbient = 0f;
		public float minAmbientPoint = -0.2f;


		public Gradient nightDayFogColor;
		public AnimationCurve fogDensityCurve;
		public float fogScale = 1f;

		public float dayAtmosphereThickness = 0.4f;
		public float nightAtmosphereThickness = 0.87f;

		public Vector3 dayRotateSpeed;
		public Vector3 nightRotateSpeed;

		float skySpeed = 1;

		Light mainLight;
		Material skyMat;

        // Use this for initialization
        void Start()
        {
            skyMat = RenderSettings.skybox;
            mainLight = GetComponent<Light>();

            //UpdateNightAndDay();
            //TWGameManager.instance.TWNightDayChanged += UpdateNightAndDay;
        }

        // Update is called once per frame
        void Update()
        {
			// Update sun intensity
			float tRange = 1 - minPoint;
			float dot = Mathf.Clamp01((Vector3.Dot(mainLight.transform.forward, Vector3.down) - minPoint) / tRange);
			float i = ((maxIntensity - minIntensity) * dot) + minIntensity;
			mainLight.intensity = i;

			// Update ambient intensity
			tRange = 1 - minAmbientPoint;
			dot = Mathf.Clamp01((Vector3.Dot(mainLight.transform.forward, Vector3.down) - minAmbientPoint) / tRange);
			i = ((maxAmbient - minAmbient) * dot) + minAmbient;
			RenderSettings.ambientIntensity = i;

			// Update light color
			mainLight.color = nightDayColor.Evaluate(dot);
			RenderSettings.ambientLight = mainLight.color;

			// Update fog
			RenderSettings.fogColor = nightDayFogColor.Evaluate(dot);
			RenderSettings.fogDensity = fogDensityCurve.Evaluate(dot) * fogScale;

			// Update atmosphere thickness
			i = ((dayAtmosphereThickness - nightAtmosphereThickness) * dot) + nightAtmosphereThickness;
			skyMat.SetFloat("_AtmosphereThickness", i);

			// Update sun rotation
			if (dot > 0)
				transform.Rotate(dayRotateSpeed * Time.deltaTime * skySpeed);
			else
				transform.Rotate(nightRotateSpeed * Time.deltaTime * skySpeed);

			if (Input.GetKeyDown(KeyCode.Q)) skySpeed *= 0.5f;
			if (Input.GetKeyDown(KeyCode.E)) skySpeed *= 2f;
		}

        /*
        private void UpdateNightAndDay()
        {
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
        */
    }
}
