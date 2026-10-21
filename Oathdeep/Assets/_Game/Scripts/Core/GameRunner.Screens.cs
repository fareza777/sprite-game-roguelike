using System.Collections.Generic;
using Oathdeep.Dungeon;
using Oathdeep.Player;
using UnityEngine;

namespace Oathdeep.Core
{
    /// <summary>Screens that stop the run clock: pause, inventory, encounter dialogs and the shop.</summary>
    public sealed partial class GameRunner
    {
        public void Pause()
        {
            if (phase != Phase.Playing) return;
            phase = Phase.Paused;
            Time.timeScale = 0f;
            frontEnd.ShowPause();
        }

        public void Resume()
        {
            if (phase != Phase.Paused) return;
            phase = Phase.Playing;
            Time.timeScale = 1f;
            frontEnd.HidePause();
        }

        /// <summary>Stops the clock under a screen without showing the pause menu.</summary>
        bool HoldForScreen()
        {
            if (phase != Phase.Playing) return phase == Phase.Paused;
            phase = Phase.Paused;
            Time.timeScale = 0f;
            return true;
        }

        /// <summary>From play (HUD bag) the game pauses under the page and resumes on close; from the pause menu it returns there.</summary>
        void OpenInventory()
        {
            if (player == null) return;
            var inventory = player.GetComponent<PlayerInventory>();
            if (phase == Phase.Playing)
            {
                HoldForScreen();
                frontEnd.ShowInventory(inventory, Resume);
            }
            else if (phase == Phase.Paused)
                frontEnd.ShowInventory(inventory, null);
        }

        void ShowEncounterDialog(EncounterDialog dialog)
        {
            if (player == null || !HoldForScreen()) return;
            frontEnd.ShowDialog(dialog, Resume);
        }

        void OpenShop(string title, Sprite portrait, List<ShopOffer> stock)
        {
            if (player == null || !HoldForScreen()) return;
            frontEnd.ShowShop(title, player.GetComponent<PlayerInventory>(), stock, Resume);
        }
    }
}
