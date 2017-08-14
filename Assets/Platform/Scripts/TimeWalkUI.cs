﻿﻿﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform
{
    using UnityEngine.UI;

    public class TimeWalkUI : MonoBehaviour
    {
        public GameObject levelTextPrefab;
        public GameObject commandMenuItemTextPrefab;

        private Text locationText;
        private Text dateText;
        private Text timeText;
        private Text helpHintText;
        private Text fullLevelText;
        private GameObject slider;
        private GameObject track;
        private GameObject menu;
        private float lastclockUpdate;


        // TODO Hide full help text after time period
        // TODO Show help hint on start up for x seconds

        private List<UIMenuCommand> commands;

        void Awake()
        {
            locationText = GameObject.Find("Location").GetComponent<Text>();
            dateText = GameObject.Find("Date").GetComponent<Text>();
            timeText = GameObject.Find("Time").GetComponent<Text>();
            helpHintText = GameObject.Find("HelpHint").GetComponent<Text>();
            menu = GameObject.Find("Menu");
            slider = GameObject.Find("Slider");
            track = GameObject.Find("Track");
            fullLevelText = GameObject.Find("FullLevelText").GetComponent<Text>();

			// Add commands
			commands = new List<UIMenuCommand> {
				new UIMenuCommand(KeyCode.Y, "Change Year", ChangeYear),
                new UIMenuCommand(KeyCode.N, "Night/Day", ToggleNightDay),
                new UIMenuCommand(KeyCode.B, "Black & White", ToggleColor),
                new UIMenuCommand(KeyCode.C, "See Comments", ToggleComments),
                new UIMenuCommand(KeyCode.H, "Help", ToggleHelp),
                new UIMenuCommand(KeyCode.R, "Restart", Restart),
                new UIMenuCommand(KeyCode.Q, "Quit", Quit)
			};

            commands.ForEach(delegate (UIMenuCommand c)
            {
                UIMenuCommand command = c;
                Debug.Log("processing " + command.description);

                // Create menu item
                Button item = Instantiate(commandMenuItemTextPrefab, menu.transform).GetComponent<Button>();

                item.GetComponent<Text>().text = command.description + " (" + command.keyCode.ToString() + ")";

                // TODO Add support for menu clicks and/or menu modal (see menu example in SampleScenes)
                //item.onClick.AddListener(delegate {
                //    UIMenuCommand thisCommand = command;
                //    Debug.Log("Command: " + thisCommand.keyCode.ToString() + ", " + thisCommand.description);
                //    thisCommand.commandCallback(); 
                //});
            });
        }

        void Start()
        {
            // Update UI (if ready)
            /*
            if(TWGameManager.instance.timeWalkLocationInfo != null && 
               TWGameManager.instance.timeWalkLevels != null)
            {
				
			}
			*/
			HandleLocationInfo();
			HandleNewLevels();
			HandleNewLevel();


            // Subscribe to changes
            TWGameManager.instance.TWLocationInfoChanged += HandleLocationInfo;
            TWGameManager.instance.TWLevelsChanged += HandleNewLevels;
            TWGameManager.instance.TWLevelChanged += HandleNewLevel;
        }

        // Update is called once per frame
        void Update()
        {
            // Update time if it has been greater than 60s
            if (Time.timeSinceLevelLoad - lastclockUpdate > 60f)
            {
                // TODO use static start time in location info
                timeText.text = System.DateTime.Now.ToString("h:mm tt");
                lastclockUpdate = Time.timeSinceLevelLoad;
            }

            // Invoke any commands the user pressed
            commands.ForEach(delegate (UIMenuCommand c)
            {
                UIMenuCommand command = c;
                if(Input.GetKeyDown(command.keyCode)){
                    Debug.Log("Command: " + command.keyCode.ToString() + ", " + command.description);
                    command.commandCallback();
                }
            });
        }

        private void HandleLocationInfo()
        {
            TWLocationInfo locationInfo = TWGameManager.instance.timeWalkLocationInfo;
            locationText.text = locationInfo.city + ", " + locationInfo.state;
            DateTime now = System.DateTime.Now;
            // TODO use satic start month and day in location info
            dateText.text = now.ToString("MMMM, d ") + TWGameManager.instance.currentLevel.year;
        }

        private void HandleNewLevels()
        {
            List<TWLevel> levels = TWGameManager.instance.timeWalkLevels;
            Button currentLevel = null;
            Button lastItem = null;

            levels.ForEach(delegate (TWLevel l)
            {
				// Create menu item
				Button item = Instantiate(levelTextPrefab, track.transform).GetComponent<Button>();
                item.GetComponent<Text>().text = l.year.ToString();

                if(TWGameManager.instance.currentLevel.year == l.year)
                {
                    currentLevel = item;    
                }

                // Set position
                if(lastItem != null)
                {
                    Vector3 pos = item.transform.position;
                    pos.y = lastItem.transform.position.y + 
                        lastItem.GetComponent<RectTransform>().rect.height;
                    item.transform.position = pos;    
                }

                lastItem = item;
            });

            UpdateHandleUI();
		}

        private void HandleNewLevel(){
            UpdateHandleUI();
		}

        private void UpdateHandleUI()
        {
            TWLevel current = TWGameManager.instance.currentLevel;

			// TODO Move slider handle position

			// Update slider level text
			fullLevelText.text = current.year + ": " + current.label;
        }
                         
        // Slider handler
        private void ChangeYear(int year){
            // TODO
        }

        // Menu
        private void ChangeYear(){
            // TODO
        }

        private void ToggleNightDay(){
            // TODO
        }

        private void ToggleColor(){
            // TODO
        }

        private void ToggleComments(){
            // TODO
        }

        private void ToggleHelp(){
            // TODO
        }

        private void Restart(){
            // TODO
        }

        private void Quit(){
            // TODO
        }
    }

    public class UIMenuCommand {

        public UIMenuCommand(KeyCode keyCode, string description, HandleCommand commandCallback) {
            this.description = description;
            this.keyCode = keyCode;
            this.commandCallback = commandCallback;
        }

        public string description;
        public KeyCode keyCode;

        public delegate void HandleCommand();

        public HandleCommand commandCallback;

    }
}
