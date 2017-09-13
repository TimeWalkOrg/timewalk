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
        private GameObject helpHint;
        private GameObject slider;
        private GameObject track;
        private GameObject handle;
        private GameObject helpMenu;
        private float lastClockUpdate = 0f;
        private Range levelYearRange; //x=min,y=max
        private Range slidePixelRange; //x=min,y=max
        private float showHelpTime;
        private float showHelpDuration = 10f;
        private float levelLoadTime;
        private float fullLevelTextDuration = 10f;



        // TODO Hide full help text after time period
        // TODO Show help hint on start up for x seconds

        private List<UIMenuCommand> commands;

        void Awake()
        {
            locationText = GameObject.Find("Location").GetComponent<Text>();
            dateText = GameObject.Find("Date").GetComponent<Text>();
            timeText = GameObject.Find("Time").GetComponent<Text>();
            helpHint = GameObject.Find("HelpHint");
            helpHintText = helpHint.GetComponent<Text>();
            helpMenu = GameObject.Find("Menu");
            slider = GameObject.Find("Slider");
            track = GameObject.Find("Track");
            handle = GameObject.Find("Handle");
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

                // Create menu item
                Button item = Instantiate(commandMenuItemTextPrefab, helpMenu.transform).GetComponent<Button>();

                item.GetComponent<Text>().text = command.description + " (" + command.keyCode.ToString() + ")";

                // TODO Add support for menu clicks and/or menu modal (see menu example in SampleScenes)
                //item.onClick.AddListener(delegate {
                //    UIMenuCommand thisCommand = command;
                //    Debug.Log("Command: " + thisCommand.keyCode.ToString() + ", " + thisCommand.description);
                //    thisCommand.commandCallback(); 
                //});
            });

            // TODO Snap slider to levels
            // Disable slider for now
            slider.GetComponent<Slider>().enabled = false;
        }

        private void OnDisable()
        {
			// Unsubscribe to changes
			TWGameManager.instance.TWLocationInfoChanged -= HandleLocationInfo;
			TWGameManager.instance.TWLevelsChanged -= HandleNewLevels;
			TWGameManager.instance.TWLevelChanged -= HandleNewLevel;
        }

        void Start()
        {
			// Subscribe to changes
			TWGameManager.instance.TWLocationInfoChanged += HandleLocationInfo;
			TWGameManager.instance.TWLevelsChanged += HandleNewLevels;
			TWGameManager.instance.TWLevelChanged += HandleNewLevel;

            showHelpTime = levelLoadTime = Time.timeSinceLevelLoad;

            // Try to load what we can
            HandleLocationInfo();
            HandleNewLevels();
            HandleNewLevel();
        }

        // Update is called once per frame
        void Update()
        {
            // Update time every 1s
            if (Time.timeSinceLevelLoad - lastClockUpdate > 1.0f)
            {
                UpdateTime();
                lastClockUpdate = Time.timeSinceLevelLoad;
            }

            // Hide help after duration
            if ((Time.timeSinceLevelLoad - showHelpTime) > showHelpDuration &&
                    !TWGameManager.instance.IsPaused)
            {
                helpMenu.SetActive(false);
                helpHint.SetActive(false);
            }

            // Truncate level text after duration
            if ((Time.timeSinceLevelLoad - levelLoadTime) > fullLevelTextDuration)
            {
                TruncateLevelText();
            }

            // Invoke any commands from the user
            commands.ForEach(delegate (UIMenuCommand c)
            {
                UIMenuCommand command = c;
                if (Input.GetKeyDown(command.keyCode))
                {
                    Debug.Log("Command: " + command.keyCode.ToString() + ", " + command.description);
                    command.commandCallback();
                }
            });
        }

        private void UpdateTime()
        {
            if (timeText == null) return;
            TimeSpan t = TimeSpan.FromHours(TWGameManager.instance.CurrentTimeHours);
            timeText.text = String.Format("{0}:{1}" + (t.Hours >= 12 ? "pm" : "am"),
                                          t.Hours > 12 ? t.Hours % 12 : t.Hours,
                                          t.Minutes.ToString("D2"));
        }

        private void HandleLocationInfo()
        {
            TWLocationInfo locationInfo = TWGameManager.instance.TimeWalkLocationInfo;
            if (locationText == null || locationInfo == null) return;

            TWLevel currentLevel = TWGameManager.instance.CurrentLevel;
            locationText.text = locationInfo.city + ", " + locationInfo.state;
            UpdateTime();
        }

        private void HandleNewLevels()
        {
            List<TWLevel> levels = TWGameManager.instance.TimeWalkLevels;
            if (levels == null || levelTextPrefab == null || slider == null) return;

            if (levels.Count <= 1)
            {
                slider.gameObject.SetActive(false);
                return;
            }

            slider.gameObject.SetActive(true);

            RectTransform lastItemRectTransform = null;

            Slider sliderComp = slider.GetComponent<Slider>();
            Rect sliderRect = slider.GetComponent<RectTransform>().rect;
            float levelItemHeight = levelTextPrefab.GetComponent<RectTransform>().rect.height;

            int maxYear = levels[0].year;
            int minYear = levels[levels.Count - 1].year;

            levelYearRange = new Range(minYear, maxYear);
            // Figured out the appropriate pixel range through trial and error...
            // There MUST be a better way.
            slidePixelRange = new Range(levelItemHeight * 2, sliderRect.height + levelItemHeight + 10);

            ClearLevels();

            sliderComp.maxValue = maxYear;
            sliderComp.minValue = minYear;

            levels.ForEach(delegate (TWLevel l)
            {
                // Create menu item
                Button item = Instantiate(levelTextPrefab, track.transform).GetComponent<Button>();
                item.GetComponent<Text>().text = l.year.ToString();
                item.name = "LevelTextItem" + l.year.ToString();

                // TODO Listen for button clicks

                RectTransform itemRect = item.GetComponent<RectTransform>();

                Vector3 pos = itemRect.position;

                // Convert current year to corresponding pixel in y range
                float yearInPixels = levelYearRange.Translate(l.year, slidePixelRange);

                // Place level on slide as close as possible to year in pixels
                pos.y = yearInPixels;

                itemRect.position = pos;

                lastItemRectTransform = itemRect;
            });
        }
		

        private void HandleNewLevel()
        {
            TWLevel currentLevel = TWGameManager.instance.CurrentLevel;
            if (currentLevel == null || 
                dateText == null || fullLevelText == null ||
                slider == null || handle == null) return;
            
            DateTime now = System.DateTime.Now;
			
            // Set Date
            // TODO use satic start month and day in location info
			dateText.text = now.ToString("MMMM, d ") + currentLevel.year;

			// Update slider level text
			fullLevelText.text = currentLevel.year + ": " + currentLevel.label;

            // Move slider
            slider.GetComponent<Slider>().value = currentLevel.year;

            // Make sure slider handle is last sibling (for z order)
            handle.GetComponent<Transform>().SetAsLastSibling();

            levelLoadTime = Time.timeSinceLevelLoad;
		}

        private void TruncateLevelText()
        {
            TWLevel currentLevel = TWGameManager.instance.CurrentLevel;
            fullLevelText.text = currentLevel.year.ToString();
        }
                         
        // Slider handler
        private void ChangeYear(int year)
        {
            TWLevel newLevel = TWGameManager.instance.GetLevelByYear(year);
            TWGameManager.instance.OnTimeWalkLevelChanged(newLevel);
        }

        // Menu
        private void ChangeYear()
        {   
            TWGameManager.instance.OnTimeWalkLevelChanged();
        }

        private void ToggleNightDay()
        {
            TWGameManager.instance.OnNightDayToggled();
        }

        private void ToggleColor()
        {
			// TODO Need the latest Colorful FX: 
            // https://www.assetstore.unity3d.com/en/#!/content/44845

        }

        private void ToggleComments()
        {
            // TODO
        }

        private void ToggleHelp()
        {
            if (!helpMenu.activeSelf)
            {
                showHelpTime = Time.timeSinceLevelLoad;
                TWGameManager.instance.OnPause(true);
                helpMenu.SetActive(true);
            }
            else
            {
                TWGameManager.instance.OnPause(false);
                helpMenu.SetActive(false);
            }
        }

        private void Restart()
        {
            TWGameManager.instance.OnRestart();
        }

        private void Quit()
        {
            TWGameManager.instance.OnQuit();
        }

		private void ClearLevels()
		{
            GameObject[] levels = GameObject.FindGameObjectsWithTag("TimeWalkUILevelText");
            for (int i = 0; i < levels.Length; i++) 
            {
                Destroy(levels[i]);    
            }
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

	//<summary>Represents range of float values<summary>
	public class Range
	{
		public Range(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

        private float min;
        private float max;

        public float Min 
        { 
            get
            {
                return min;    
            } 
        }

		public float Max
		{
			get
			{
				return max;
			}
		}

        public float Delta()
        {
            return this.max - this.min;
        }

		//<summary>Translates value from Range to target Range</summary>
		//<param name=x>Value from the current Range to translate</param>
		//<param name=targetRange>The range to which x should be translated</param>
		public float Translate(float x, Range targetRange)
		{
            if (this.Delta() <= 0f)
			{
				throw new ArgumentException(String.Format("current range does not have a positive delta"));
			}

            if (targetRange.Delta() <= 0f)
            {
				throw new ArgumentException(String.Format("target does not have a positive delta"));
            }

			if (x > max || x < min)
			{
                throw new ArgumentException(String.Format("x ({0}) is out of current range", x));
			}

            float ratio = targetRange.Delta() / Delta();
            float converted = ratio * (max - x);
            float translated = targetRange.max - converted;
            // Debug.Log(String.Format("{0} in range ({4},{5}) translated to range ({1},{2}) is {3}", x, targetRange.min, targetRange.max, translated, min, max));
            return translated;
		}
	}
}
