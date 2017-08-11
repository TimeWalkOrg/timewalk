using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform
{
    using UnityEngine.UI;

    public class TimeWalkUI : MonoBehaviour
    {
        private Text locationText;
        private Text dateText;
        private Text timeText;
        private Text fullHelpText;
        private Text helpHintText;
        private GameObject slider;
        private float lastclockUpdate;

        void Awake()
        {
            // GameObject.Find("LevelText").GetComponent<Text>();
            locationText = GameObject.Find("Location").GetComponent<Text>();
            dateText = GameObject.Find("Date").GetComponent<Text>();
            timeText = GameObject.Find("Time").GetComponent<Text>();
            fullHelpText = GameObject.Find("FullHelpText").GetComponent<Text>();
            helpHintText = GameObject.Find("HelpHint").GetComponent<Text>();
        }

        void Start()
        {
            // Update UI
            HandleLocationInfo(TWGameManager.instance.timeWalkLocationInfo);
            HandleNewLevels(TWGameManager.instance.timeWalkLevels, TWGameManager.instance.currentLevel);

            // Subscribe to future changes
            TWGameManager.instance.TWLocationInfoChanged += HandleLocationInfo;
            TWGameManager.instance.TWLevelsChanged += HandleNewLevels;
        }
        
        // Update is called once per frame
        void Update()
        {
            // Update time if it has been greater than 1s
            if(Time.timeSinceLevelLoad - lastclockUpdate > 1f)
            {
                timeText.text = System.DateTime.Now.ToString("h:mm:ss tt");
                lastclockUpdate = Time.timeSinceLevelLoad;
            }
        }

        private void HandleLocationInfo(TWLocationInfo locationInfo)
        {
            locationText.text = locationInfo.city + ", " + locationInfo.state;
            DateTime now = System.DateTime.Now;
            dateText.text = now.ToString("MMMM, d ") + TWGameManager.instance.currentLevel.year;
        }

        private void HandleNewLevels(List<TWLevel> levels, TWLevel currentLevel)
        {
            // TODO Update slider
            DateTime now = System.DateTime.Now;
            dateText.text = now.ToString("MMMM, d ") + currentLevel.year;
        }
    }
}
