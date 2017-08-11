using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform {
    using System;
    using UnityEngine.UI;

    public class TWGameManager : MonoBehaviour
    {
        public static TWGameManager instance = null;
        
        public TWLocationInfo timeWalkLocationInfo = null;

        public List<TWLevel> timeWalkLevels = null;

        public TWLevel currentLevel = null; 

        public DateTime levelStartTime;


        public event Action<TWLocationInfo> TWLocationInfoChanged;
        
        public void OnLocationInfoChanged(TWLocationInfo locationInfo)
        {
            if(TWLocationInfoChanged != null)
            {
                timeWalkLocationInfo = locationInfo;
                TWLocationInfoChanged(locationInfo);
            }
        }

        public event Action<List<TWLevel>, TWLevel> TWLevelsChanged;

        public void OnTimeWalkLevelsChanged(List<TWLevel> levels, TWLevel current)
        {
            if (TWLevelsChanged != null)
            {
                levelStartTime = DateTime.Now;
                timeWalkLevels = levels;
                currentLevel = current;
                TWLevelsChanged(timeWalkLevels, currentLevel);
            }
        }

        void Awake()
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
    }

    [System.Serializable]
    public class TWLevel
    {
        public int year;
        public string label;
    }

    [System.Serializable]
    public class TWLocationInfo
    {
        // Will need to rethink city and state for international locations
        public string city;
        public string state;
        public Boolean isNight;
        public Boolean isColor;
    }
}

