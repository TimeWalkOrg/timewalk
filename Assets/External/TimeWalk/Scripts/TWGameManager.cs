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
        public event Action TWPauseToggled;

		public static float startTimeHours = 12.0f; // noon
		public static float timeSpeedUp = 12.0f; // 1 hour = 12 hours

        private static float secondsToHours = 1f / 3600f;
		private TWLocationInfo timeWalkLocationInfo = null;
        private List<TWLevel> timeWalkLevels = null;
        private TWLevel previousLevel = null;
        private Boolean previousLevelUnloading = false;
        private TWLevel currentLevel = null;
        private Boolean currentLevelLoading = false;
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

            timeWalkLocationInfo = locationInfo;
            startTimeHours = locationInfo.startTimeHours;
            timeSpeedUp = locationInfo.timeSpeedUp;

            if (TWLocationInfoChanged != null)
            {   
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

            ResetNonCurrentLevels();
        }

        public void OnTimeWalkLevelChanged(TWLevel newLevel)
		{
            if (newLevel == null) return;

            SetCurrentLevel(newLevel);
		}

		public void OnTimeWalkLevelChanged()
		{
            if (timeWalkLevels == null) return;

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
                // Switch to night (8:xx pm)
                currentTimeHours = 20f + (currentTimeHours % 1);
            }
            else
            {
                // Switch to day (10:xx am)
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
            if(TWPauseToggled != null)
            {
                TWPauseToggled();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ProcessLevelChange();
        }

        void OnSceneUnLoaded(Scene scene)
        {
            ProcessLevelChange();
        }


		void Awake()
        {
            // TODO Probably don't need singleton if we have a master scene

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

        private void OnEnable()
        {
			SceneManager.sceneLoaded += OnSceneLoaded;
			SceneManager.sceneUnloaded += OnSceneUnLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnLoaded;
        }

        private void SetCurrentLevel(TWLevel newLevel)
        {
            if (currentLevel == newLevel) return;

            previousLevel = currentLevel;
            currentLevel = newLevel;

			Debug.Log(String.Format("Level change from {0} to {1}",
								previousLevel != null ? previousLevel.levelSceneName : "null",
								currentLevel != null ? currentLevel.levelSceneName : "null"));
            ProcessLevelChange();
        }

        private void SetCurrentLevel()
        {
            if (timeWalkLevels == null) return;

            TWLevel newLevel = null;

            timeWalkLevels.ForEach(delegate (TWLevel l)
			{
				if (l.isDefault)
				{
					newLevel = l;
				}
			});

			// Just pick first level if no defaults
			if (newLevel == null)
			{
				newLevel = timeWalkLevels[0];
			}

            SetCurrentLevel(newLevel);
        }

        private void ProcessLevelChange()
        {
            // If current exists and isn't already loading or loaded, load it
            if(currentLevel != null)
            {
                Scene currentLevelScene = SceneManager.GetSceneByName(currentLevel.levelSceneName);
                if(currentLevelLoading) 
                {
                    // Make sure it is valid
                    if(currentLevelScene.IsValid()) {
                        if(currentLevelScene.isLoaded) {
                            
                            currentLevelLoading = false;
							
                            // New scene ready
							if (TWLevelChanged != null)
							{
								TWLevelChanged();
							}
                        } 
                    } 
                    else 
                    {
                        Debug.LogError("Unable to load " + currentLevelScene.name);    
                    }
                }
                else
                if(!currentLevelScene.isLoaded)
                {
					// Load new scene
					SceneManager.LoadSceneAsync(currentLevel.levelSceneName, LoadSceneMode.Additive);
                    currentLevelLoading = true;
                }
            }

			// If previous exists and is loaded, unload it
			if (previousLevel != null)
			{
                Scene previousLevelScene = SceneManager.GetSceneByName(previousLevel.levelSceneName);
				if (previousLevelUnloading)
				{
                    if(!previousLevelScene.isLoaded)
					{
                        // Old scene unloaded
                        previousLevelUnloading = false;
						previousLevel = null;
					}
				}
				else
				{
					SceneManager.UnloadSceneAsync(previousLevel.levelSceneName);
                    previousLevelUnloading = true;
				}
			}
        }

		private void ResetNonCurrentLevels()
		{
			if (timeWalkLevels == null) return;

			timeWalkLevels.ForEach(delegate (TWLevel l)
			{
                if(l.levelSceneName != currentLevel.levelSceneName)
                {
                    Scene levelScene = SceneManager.GetSceneByName(l.levelSceneName);
                    if (levelScene.isLoaded)
                    {
                        SceneManager.UnloadSceneAsync(levelScene.name);
                    }
                }
			});
		}
    }



    [System.Serializable]
    public class TWLevel : IComparable
    {
        public int year;
        public string label;
        public Boolean isDefault;
		public string levelSceneName;

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

