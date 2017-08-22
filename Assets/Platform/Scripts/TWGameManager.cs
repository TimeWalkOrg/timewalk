using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform {
    using System;
    using UnityEngine.UI;
    using UnityEngine.SceneManagement;

    public class TWGameManager : MonoBehaviour
    {
        public static TWGameManager instance = null;

		public event Action TWLocationInfoChanged;
		public event Action TWLevelsChanged;
		public event Action TWLevelChanged;
        public event Action TWNightDayChanged;

		public static float startTimeHours = 12.0f; // noon
		public static float timeSpeedUp = 12.0f; // 1 hour = 12 hours

        private static float secondsToHours = 1f / 3600f;
		private TWLocationInfo timeWalkLocationInfo = null;
        private List<TWLevel> timeWalkLevels = null;
        private TWLevel currentLevel = null;
        private float currentTimeHours = 0.0f;
        private Boolean isDayTime = true;
        private Boolean isPaused = false;


		public List<TWLevel> TimeWalkLevels
		{
			get
			{
				return timeWalkLevels;
			}
		}

        public TWLevel CurrentLevel
        {
            get
            {
                return currentLevel;
            }
        }

        //<summary>Current time in hours</summary>
		public float CurrentTimeHours
		{
			get
			{
				return currentTimeHours;
			}
		}

        public Boolean IsDayTime
        {
            get
            {
                return isDayTime;    
            }
        }

        public Boolean IsPaused
        {
            get
            {
                return isPaused;
            }
        }

		public TWLocationInfo TimeWalkLocationInfo
		{
			get
			{
				return timeWalkLocationInfo;
			}
		}

        public Boolean DataReady()
        {
            return (timeWalkLevels != null &&
                    currentLevel != null &&
                    timeWalkLocationInfo != null);
        }

        public TWLevel GetLevelByYear(int year)
        {
            TWLevel level = null;
            timeWalkLevels.ForEach(delegate (TWLevel l)
            {
                if(l.year == year) {
                    level = l;
                }
            });
            return level;
        }
        
        public void OnLocationInfoChanged(TWLocationInfo locationInfo)
        {
            if (locationInfo == null) return;

            if(TWLocationInfoChanged != null)
            {
                timeWalkLocationInfo = locationInfo;
                startTimeHours = locationInfo.startTimeHours;
                timeSpeedUp = locationInfo.timeSpeedUp;
                TWLocationInfoChanged();
            }
        }

        public void OnTimeWalkLevelsChanged(List<TWLevel> levels)
        {
            if (levels == null) return;

			timeWalkLevels = levels;

            timeWalkLevels.Sort();

			if (TWLevelsChanged != null)
			{
				TWLevelsChanged();
			}

			SetCurrentLevel();
        }

        public void OnTimeWalkLevelChanged(TWLevel newLevel)
		{
            if (newLevel == null) return;

			if (TWLevelChanged != null)
            {
				currentLevel = newLevel;
				TWLevelChanged();
			}
		}

		public void OnTimeWalkLevelChanged()
		{
            int newLevelIdx = -1;
            for (int i = 0; i < timeWalkLevels.Count; i++)
            {
                if(timeWalkLevels[i].year == currentLevel.year)
                {
                    newLevelIdx = (i == 0) ? 
                        timeWalkLevels.Count - 1 : i - 1;
                    break;
                }
            }

            if (newLevelIdx > -1)
            {
                OnTimeWalkLevelChanged(timeWalkLevels[newLevelIdx]);
            }
		}

        public void OnNightDayToggled()
        {
            // day == 6am-8pm
            if (isDayTime) {
                // Switch to night (8pm)
                currentTimeHours = 20f + (currentTimeHours % 1);
            }
            else
            {
                // Switch to day (10am)
				currentTimeHours = 10f + (currentTimeHours % 1);
            }
        }

        public void OnRestart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

		public void OnQuit()
		{
			Application.Quit();
		}

        public void OnPause(Boolean pause)
        {
            Time.timeScale = pause ? 0 : 1;
            isPaused = pause;
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

            // Set start time from location info
            currentTimeHours = startTimeHours;
        }

        void Update()
        {
            currentTimeHours += Time.deltaTime * timeSpeedUp * secondsToHours;
            if (currentTimeHours > 24.0f)
            {
                currentTimeHours %= 24.0f;
            }

            Boolean oldIsDayTime = isDayTime;

            if(currentTimeHours >= 6f && currentTimeHours < 20f)
            {
                isDayTime = true;
            } 
            else
            {
                isDayTime = false;
            }

            if(oldIsDayTime != isDayTime) {
                TWNightDayChanged();
            }
        }

        private void SetCurrentLevel()
        {
			// Set current if null
			if (currentLevel == null)
			{
				timeWalkLevels.ForEach(delegate (TWLevel l)
				{
					if (l.isDefault)
					{
						currentLevel = l;
					}
				});

				// Just pick first level if no defaults
				if (currentLevel == null)
				{
					currentLevel = timeWalkLevels[0];
				}

                TWLevelChanged();
			}
        }
    }

    [System.Serializable]
    public class TWLevel : IComparable
    {
        public int year;
        public string label;
        public Boolean isDefault;

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            TWLevel otherLevel = obj as TWLevel;
			if (otherLevel != null)
				return -1 * this.year.CompareTo(otherLevel.year);
			else
				throw new ArgumentException("Object is not a TWLevel");
        }
    }

    [System.Serializable]
    public class TWLocationInfo
    {
        // Will need to rethink city and state for international locations
        public string city;
        public string state;
        public Boolean isNight = false;
        public Boolean isColor = true;
        public float startTimeHours = TWGameManager.startTimeHours;
        public float timeSpeedUp = TWGameManager.timeSpeedUp;
    }
}

