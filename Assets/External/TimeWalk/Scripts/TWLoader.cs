using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform
{
    public class TWLoader : MonoBehaviour
    {

        public GameObject twGameManager; // TWGameManager prefab
        // TODO
        // public GameObject twSoundManager; // TWSoundManager prefab

        public TWLocationInfo timeWalkLocationInfo = null;
        public List<TWLevel> timeWalkLevels = null;

        void Awake()
        {
            if (timeWalkLocationInfo == null)
            {
                timeWalkLocationInfo = new TWLocationInfo();
            }

            if (timeWalkLevels == null)
            {
                timeWalkLevels = new List<TWLevel>();
            }

            if (TWGameManager.instance == null)
            {
                //Instantiate the prefab
                Instantiate(twGameManager);
            }
        }

        void Start()
        {
            // Load initial data
            TWGameManager.instance.OnLocationInfoChanged(timeWalkLocationInfo);
            TWGameManager.instance.OnTimeWalkLevelsChanged(timeWalkLevels);
        }
    }
}

