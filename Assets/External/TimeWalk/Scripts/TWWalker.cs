using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TimeWalk.Platform
{
    using UnityStandardAssets.Characters.FirstPerson;

    public class TWWalker : MonoBehaviour
    {
        private FirstPersonController firstPersonController;

        // Use this for initialization
        void Start()
        {
            firstPersonController = GetComponent<FirstPersonController>();

            // Subscribe to changes
            TWGameManager.instance.TWPauseToggled += HandleTogglePause;
        }

        void OnDisable()
        {
            // Unsubscribe to changes
            TWGameManager.instance.TWPauseToggled -= HandleTogglePause;
        }

        void HandleTogglePause()
        {
            firstPersonController.enabled = !TWGameManager.instance.IsPaused;
        }
    }
}
