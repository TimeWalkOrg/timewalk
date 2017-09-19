using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Derived from Glen Rhodes' tutorial here: https://www.youtube.com/watch?v=h5GFoI38DOg

namespace TimeWalk.Platform
{
    //<summary>Controls night and day skyboxes and sun/moon position</summary>
    public class TWNightAndDay : MonoBehaviour
    {
        // Sun color from min to max intensity
        public Gradient nightDayColor;

        public float sunrise = 7.5f;

		public float maxIntensity = 3f;
		public float minIntensity = 0f;
        // The point at which the sun is below the horizon and effectively "off".
        // 1 is directly above, 0 is directly at the horizon, -1 is directly below
        public float intensityCutOff = -0.2f;

		public float maxAmbient = 1f;
		public float minAmbient = 0f;
        // The point at which the sun is below the horizon and ambient light should be "off".
        public float ambientCutOff = -0.2f;
        
		public Gradient nightDayFogColor;
		public AnimationCurve fogDensityCurve;
		public float fogScale = 1f;

		public float dayAtmosphereThickness = 0.4f;
		public float nightAtmosphereThickness = 0.87f;

		public Vector3 dayRotateSpeed;
		public Vector3 nightRotateSpeed;

		float skySpeed = 1;

        TWRange twentyFour;
        TWRange fullRotation;

		Light mainLight;
		Material skyMat;

        // Use this for initialization
        void Start()
        {
            skyMat = RenderSettings.skybox;
            mainLight = GetComponent<Light>();
            skySpeed = TWGameManager.timeSpeedUpHours / 3600;

            twentyFour = new TWRange(0f, 24f);
            fullRotation = new TWRange(0f, 360f);

            RotateSun();
            TWGameManager.instance.TWNightDayChanged += RotateSun;
        }

        // Update is called once per frame
        void Update()
        {
            // We use the direction of the main light as the basis for updating lighting 
            // values (eg. the closer the light is to overhead the more intense it should be).
            // mainLight.transform.forward.y gives us how much the light is pointing 
            // up (1 is straight up, -1 is straight down). The dot product with Vector3.down 
            // gives us the inverse (1 is down, -1 is up)
            float down = Vector3.Dot(mainLight.transform.forward, Vector3.down);

			// Update sun intensity
			float tRange = 1 - intensityCutOff;
            float dot = Mathf.Clamp01((down - intensityCutOff) / tRange);
			float i = ((maxIntensity - minIntensity) * dot) + minIntensity;
			mainLight.intensity = i;

			// Update ambient intensity
			tRange = 1 - ambientCutOff;
            dot = Mathf.Clamp01((down - ambientCutOff) / tRange);
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

            // Update main light rotation
            RotateSun();
		}

        private void RotateSun()
        {
            // Rotate sun to new position based on time of day.
            float hours = TWGameManager.instance.CurrentTimeHours;
            // Convert hours to angle, but add time of sunrise to account for sunrise being = 180 degrees.
            float angle = twentyFour.Translate((hours + sunrise) % 24, fullRotation);

            //Debug.Log(String.Format("{0} hours = {1} angle ", hours, angle));
            transform.rotation = Quaternion.Euler(angle * -1f, 0, 0);
        }
    }
}
