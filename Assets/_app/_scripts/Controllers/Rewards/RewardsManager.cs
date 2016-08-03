﻿using UnityEngine;
using System.Collections;
using ModularFramework.Core;

namespace EA4S
{
    [RequireComponent(typeof(RewardsAnimator))]
    public class RewardsManager : MonoBehaviour
    {
        public Antura AnturaController;

        int tutorialIndex = 10;

        void Start()
        {
            AudioManager.I.PlayMusic(Music.Theme4);
            Debug.Log("RewardsManager playsession: " + AppManager.Instance.PlaySession);

            // here we set the Rewards base on current progression level (playsession -1 because Rewards appear when playsession is already incremented)
            if ((AppManager.Instance.PlaySession - 1) == 1) {
                AnturaController.SetPreset(1);
                tutorialIndex = 10;
                LoggerEA4S.Log("app", "Reward", "get_reward", "1");
            } else if ((AppManager.Instance.PlaySession - 1) == 2) {
                AnturaController.SetPreset(2);
                tutorialIndex = 20;
                LoggerEA4S.Log("app", "Reward", "get_reward", "2");
            } else if ((AppManager.Instance.PlaySession - 1) > 2) {
                AnturaController.SetPreset(3);
                tutorialIndex = 30;
                LoggerEA4S.Log("app", "Reward", "get_reward", "3");
            }
            LoggerEA4S.Save();
            SceneTransitioner.Close();
            ShowTutor();
        }

        public void ShowTutor()
        {
            switch (tutorialIndex) {
                case 10:
                    tutorialIndex++;
                    WidgetSubtitles.I.DisplaySentence("game_rewards_intro1", 2, true, ShowTutor);
                    break;
                case 11:
                    tutorialIndex++;
                    WidgetSubtitles.I.DisplaySentence("game_rewards_intro2", 2, true, ShowReward);
                    break;
                case 20:
                    tutorialIndex++;
                    WidgetSubtitles.I.DisplaySentence("game_reward_A1", 2, true, ShowTutor);
                    break;
                case 21:
                    tutorialIndex++;
                    WidgetSubtitles.I.DisplaySentence("game_reward_A2", 2, true, ShowReward);
                    break;
                case 30:
                    tutorialIndex++;
                    WidgetSubtitles.I.DisplaySentence("end_learningblock_A1", 2, true, ShowTutor);
                    break;
                case 31:
                    tutorialIndex++;
                    WidgetSubtitles.I.DisplaySentence("end_learningblock_A2", 2, true, ShowReward);
                    break;
            }
        }

        public void ShowReward()
        {
            StartCoroutine(StartReward());
        }

        IEnumerator StartReward()
        {
            // Wait for animation to complete
            RewardsAnimator animator = this.GetComponent<RewardsAnimator>();
            while (!animator.IsComplete)
                yield return null;

            ContinueScreen.Show(Continue, ContinueScreenMode.Button);
        }


        public void Continue()
        {
            // if we just did Assestment then go mood
            if ((AppManager.Instance.PlaySession) > 3) {
                GameManager.Instance.Modules.SceneModule.LoadSceneWithTransition("app_Mood");
            } else {
                GameManager.Instance.Modules.SceneModule.LoadSceneWithTransition("app_Journey");
            }
        }

    }
}