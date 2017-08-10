using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform {
    using System;
    using UnityEngine.UI;

    public class TWGameManager : MonoBehaviour
    {
        public static TWGameManager instance = null;
        
        // timewalk levels are years
        public int year; 
        
        // Will need to rethink city and state for international locations
        public string city = "Twin Peaks";
        public string state = "WA";
        public Boolean isNight = false;
        public Boolean isColor = true;

        private DateTime startTime = DateTime.Now;
        private GameObject timeWalkUI = null;

        // Use this for initialization
        void Start()
        {
            // Singleton
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                // There can only be one...
                Destroy(gameObject);
            }

            // Don't destroy this object when new scene is loaded 
            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

        }

        // init for new scene
        void InitScene()
        {
            UpdateUI();
        }

        // Update UI when info changes
        void UpdateUI()
        {
            if (timeWalkUI == null) return;

        }

        // Makes sure only objects associated with year are enabled in scene
        void FilterObjectsByYear()
        {
            // TODO
        }

        public void ChangeYear(int year)
        {
            // TODO
        }
        
        // TODO ChangeLocation
    }
}

