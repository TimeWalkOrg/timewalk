using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform
{
    //<summary>Represents range of float values<summary>
    public class TWRange
    {
        public TWRange(float min, float max)
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
        public float Translate(float x, TWRange targetRange)
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