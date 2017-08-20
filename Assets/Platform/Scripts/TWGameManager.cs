using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform {
    using System;
    using UnityEngine.UI;

    public class TWGameManager : MonoBehaviour
    {
        public static TWGameManager instance = null;

		public event Action TWLocationInfoChanged;
		public event Action TWLevelsChanged;
		public event Action TWLevelChanged;
        public DateTime levelStartTime;
        
        private TWLocationInfo timeWalkLocationInfo = null;
        private List<TWLevel> timeWalkLevels = null;
        private TWLevel currentLevel = null;

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
                TWLocationInfoChanged();
            }
        }

        public void OnTimeWalkLevelsChanged(List<TWLevel> levels)
        {
            if (levels == null) return;

			levelStartTime = DateTime.Now;
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
				levelStartTime = DateTime.Now;
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
        public Boolean isNight;
        public Boolean isColor;
        public DateTime startTime; // Time of day to start clock?
    }
}

