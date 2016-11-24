﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ModularFramework.Core;
using ModularFramework.Helpers;
using ArabicSupport;
using EA4S;

namespace EA4S.MakeFriends
{
    public class MakeFriendsGame : MiniGame
    {
        public LivingLetterArea leftArea;
        public LivingLetterArea rightArea;
        public LetterPickerController letterPicker;
        public Canvas endGameCanvas;
        public StarFlowers starFlowers;
        public GameObject sceneCamera;
        public static int numberOfRounds = 6;
        public float uiDelay;
        public Vector3 endCameraPosition;
        public Vector3 endCameraRotation;
        public GameObject letterBalloonPrefab;
        public GameObject letterBalloonContainer;
        public GameObject FxParticlesPoof;
        public DropZoneController dropZone;
        public WinCelebrationController winCelebration;
        [Header("Difficulty Override")]
        public bool overrideDifficulty;
        public MakeFriendsVariation difficultySetting;
        new public static MakeFriendsGame Instance;
        [Header("Gameplay Info")]
        new public MakeFriendsGameplayInfo GameplayInfo;

        [HideInInspector]
        public MakeFriendsConfiguration Configuration { get { return MakeFriendsConfiguration.Instance; } }

        private LL_WordData wordData1;
        private List<ILivingLetterData> wordLetters1 = new List<ILivingLetterData>();
        private LL_WordData wordData2;
        private List<ILivingLetterData> wordLetters2 = new List<ILivingLetterData>();
        private List<ILivingLetterData> commonLetters = new List<ILivingLetterData>();
        private List<ILivingLetterData> uncommonLetters = new List<ILivingLetterData>();
        private List<ILivingLetterData> choiceLetters = new List<ILivingLetterData>();
        private List<ILivingLetterData> correctChoices = new List<ILivingLetterData>();
        private List<ILivingLetterData> incorrectChoices = new List<ILivingLetterData>();
        private int currentRound = 0;
        private int _currentScore = 0;
        private bool isTutorialRound;

        public int CurrentScore
        {
            get { return _currentScore; }
            set
            {
                _currentScore = value; 
                if (CurrentScore == STARS_1_THRESHOLD)
                {
                    MinigamesUI.Starbar.GotoStar(0);
                }
                else if (CurrentScore == STARS_2_THRESHOLD)
                {
                    MinigamesUI.Starbar.GotoStar(1);
                }
                else if (CurrentScore == STARS_3_THRESHOLD)
                {
                    MinigamesUI.Starbar.GotoStar(2);
                }
            }
        }

        private IPopupWidget Popup { get { return GetConfiguration().Context.GetPopupWidget(); } }

        private IAudioManager AudioManager { get { return GetConfiguration().Context.GetAudioManager(); } }

        public MakeFriendsIntroductionState IntroductionState { get; private set; }

        public MakeFriendsQuestionState QuestionState { get; private set; }

        public MakeFriendsPlayState PlayState { get; private set; }

        public MakeFriendsResultState ResultState { get; private set; }

        private readonly int STARS_1_THRESHOLD = Mathf.CeilToInt(0.33f * numberOfRounds);
        private readonly int STARS_2_THRESHOLD = Mathf.CeilToInt(0.66f * numberOfRounds);
        private readonly int STARS_3_THRESHOLD = numberOfRounds;

        public int CurrentStars
        {
            get
            {
                if (CurrentScore < STARS_1_THRESHOLD)
                    return 0;
                if (CurrentScore < STARS_2_THRESHOLD)
                    return 1;
                if (CurrentScore < STARS_3_THRESHOLD)
                    return 2;
                return 3;
            }
        }

        protected override void OnInitialize(IGameContext context)
        {
            IntroductionState = new MakeFriendsIntroductionState(this);
            QuestionState = new MakeFriendsQuestionState(this);
            PlayState = new MakeFriendsPlayState(this);
            ResultState = new MakeFriendsResultState(this);
        }

        protected override IGameState GetInitialState()
        {
            return IntroductionState;
        }

        protected override IGameConfiguration GetConfiguration()
        {
            return MakeFriendsConfiguration.Instance;
        }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void Start()
        {
            base.Start();

            //AppManager.Instance.InitDataAI();
            //AppManager.Instance.CurrentGameManagerGO = gameObject;
            //SceneTransitioner.Close();
            //Random.seed = System.DateTime.Now.GetHashCode();
            //LoggerEA4S.Log("minigame", "template", "start", "");
            //LoggerEA4S.Save();

            PlayIdleMusic();
        }

        public void PlayActiveMusic()
        {
            MakeFriendsConfiguration.Instance.Context.GetAudioManager().PlayMusic(Music.Theme6);
        }

        public void PlayIdleMusic()
        {
            MakeFriendsConfiguration.Instance.Context.GetAudioManager().PlayMusic(Music.Relax);
        }

        public void Play()
        {
            currentRound++;
            if (currentRound <= numberOfRounds)
            {
                StartNewRound();
            }
            else
            {
                EndGame();
            }
        }

        public void StartNewRound()
        {
            Reset();
            SetNewWords();
            SetLetterChoices();
            SpawnLivingLetters();
            ShowLetterPicker();
            ShowDropZone();
        }

        public void PlayTutorial()
        {
            Debug.Log("Playing Tutorial");

            isTutorialRound = true;
            PlayIdleMusic();

            Reset();
            SetNewWords();
            SetLetterChoices();
            SpawnLivingLetters();
            ShowLetterPicker();
            ShowDropZone();

            StartCoroutine(ShowTutorialUI_Coroutine());
        }

        private IEnumerator ShowTutorialUI_Coroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(uiDelay);

                foreach (var choice in letterPicker.CorrectLetterChoices)
                {
                    var from = new Vector3(0f, -1f, -1f);
                    var to = new Vector3(0f, 7f, 0f);

                    TutorialUI.DrawLine(from, to, TutorialUI.DrawLineMode.Finger, false, true);
                }
            }
        }

        private void HideTutorialUI()
        {
            TutorialUI.Clear(false);
        }

        private void SetNewWords()
        {
            wordData1 = null;
            wordData2 = null;
            wordLetters1.Clear();
            wordLetters2.Clear();
            commonLetters.Clear();
            uncommonLetters.Clear();

            var question = GetConfiguration().Questions.GetNextQuestion();

            List<ILivingLetterData> words = question.GetQuestions().ToList();
            wordData1 = words[0] as LL_WordData;
            wordData2 = words[1] as LL_WordData;
            commonLetters = question.GetCorrectAnswers().ToList();
            uncommonLetters = question.GetWrongAnswers().ToList();

            Debug.Log("[New Round] Word 1: " + ArabicFixer.Fix(wordData1.Data.Arabic) + ", Word 2: " + ArabicFixer.Fix(wordData2.Data.Arabic)
                + "\nCommon: " + string.Join(" / ", commonLetters.Select(x => x.TextForLivingLetter.ToString()).Reverse().ToArray())
                + ", Uncommon: " + string.Join(" / ", uncommonLetters.Select(x => x.TextForLivingLetter.ToString()).Reverse().ToArray()));
        }

        private void SetLetterChoices()
        {
            choiceLetters.AddRange(commonLetters);
            if (choiceLetters.Count > letterPicker.letterChoices.Length)
            {
                choiceLetters = choiceLetters.GetRange(0, letterPicker.letterChoices.Length);
            }
            //Debug.Log("Added " + choiceLetters.Count + " common letters to choices");
                
            int vacantChoiceLettersCount = letterPicker.letterChoices.Length - choiceLetters.Count;

            // Get other random letters (without repetition)
            for (int i = 0; i < vacantChoiceLettersCount; i++)
            {
                LL_LetterData letter;
                do
                {
                    if (i < uncommonLetters.Count)
                    {
                        letter = uncommonLetters[i] as LL_LetterData;
                        //Debug.Log("Considering as choice: " + letter.TextForLivingLetter);
                        if (choiceLetters.Exists(x => x.Id == letter.Id))
                        {
                            letter = AppManager.Instance.Teacher.GetAllTestLetterDataLL().GetRandomElement();
                            //Debug.Log("Using random choice instead: " + letter);
                        }
                    }
                    else
                    {
                        letter = AppManager.Instance.Teacher.GetAllTestLetterDataLL().GetRandomElement();
                        //Debug.Log("No more word letters, using random: " + letter.TextForLivingLetter);
                    }
                } while (choiceLetters.Exists(x => x.Id == letter.Id));
                choiceLetters.Add(letter);
                //Debug.Log("Added " + letter.TextForLivingLetter + " to choices");
            }
            choiceLetters.Shuffle();

            letterPicker.DisplayLetters(choiceLetters);
            if (isTutorialRound)
            {
                letterPicker.SetCorrectChoices(commonLetters);
            }
        }

        private void SpawnLivingLetters()
        {
            leftArea.SpawnLivingLetter(wordData1);
            rightArea.SpawnLivingLetter(wordData2);

            leftArea.MakeEntrance();
            rightArea.MakeEntrance();
        }

        private void ShowDropZone()
        {
            dropZone.Appear(uiDelay);
        }

        private void HideDropZone()
        {
            dropZone.Disappear();
        }

        private void ShowLetterPicker()
        {
            letterPicker.Block();
            letterPicker.ShowAndUnblockDelayed(uiDelay);
        }

        private void HideLetterPicker()
        {
            letterPicker.Block();
            letterPicker.Hide();
        }

        public void OnRoundResultPressed()
        {
            GetConfiguration().Context.GetAudioManager().PlaySound(Sfx.UIButtonClick);
            Popup.Hide();
            Play();
        }

        public void OnLetterChoiceSelected(LetterChoiceController letterChoice)
        {
            letterPicker.BlockForSeconds(2f);

            if (commonLetters.Exists(x => x.Id == letterChoice.letterData.Id))
            {
                letterChoice.State = LetterChoiceController.ChoiceState.CORRECT;
                //letterChoice.SpawnBalloon(true);
                GetConfiguration().Context.GetAudioManager().PlaySound(Sfx.LetterHappy);
                dropZone.AnimateCorrect();

                if (!correctChoices.Exists(x => x.Id == letterChoice.letterData.Id))
                {
                    correctChoices.Add(letterChoice.letterData);
                }

                if (correctChoices.Count >= commonLetters.Count)
                {
                    EndRound(true);
                }
                else
                {
                    dropZone.ResetLetter(3f);
                }
            }
            else
            {
                letterChoice.State = LetterChoiceController.ChoiceState.WRONG;
                //letterChoice.SpawnBalloon(false);
                GetConfiguration().Context.GetAudioManager().PlaySound(Sfx.LetterSad);
                dropZone.AnimateWrong();
                incorrectChoices.Add(letterChoice.letterData);
                if (!isTutorialRound)
                {
                    leftArea.MoveAwayAngrily();
                    rightArea.MoveAwayAngrily();
                    if (incorrectChoices.Count >= 3)
                    {
                        EndRound(false);
                    }
                }
            }
        }

        private void EndRound(bool win)
        {
            StartCoroutine(EndRound_Coroutine(win));
        }

        private IEnumerator EndRound_Coroutine(bool win)
        {
            var winDelay1 = 2f;
            var winDelay2 = 1.5f;
            var friendlyExitDelay = leftArea.friendlyExitDuration;
            var loseDelay = 1.5f;

            HideLetterPicker();

            if (isTutorialRound) // Ignore score
            {
                if (win)
                {
                    Debug.Log("Cleared tutorial");

                    GetConfiguration().Context.GetAudioManager().PlaySound(Sfx.Win);
                    leftArea.Celebrate();
                    rightArea.Celebrate();
                    winCelebration.Show();

                    yield return new WaitForSeconds(winDelay1);
                    winCelebration.Hide();
                    leftArea.MakeFriendlyExit();
                    rightArea.MakeFriendlyExit();

                    yield return new WaitForSeconds(winDelay2);
                    HideDropZone();

                    isTutorialRound = false;
                    HideTutorialUI();
                    IntroductionState.OnFinishedTutorial();
                }
                else
                {
                    Debug.Log("This is isn't supposed to happen...");
                }
            }
            else if (win)
            {
                Debug.Log("Win");

                GetConfiguration().Context.GetAudioManager().PlaySound(Sfx.Win);
                leftArea.Celebrate();
                rightArea.Celebrate();
                winCelebration.Show();
                CurrentScore++;

                yield return new WaitForSeconds(winDelay1);
                // Go to Friends Zone
                // ...
                winCelebration.Hide();
                leftArea.MakeFriendlyExit();
                rightArea.MakeFriendlyExit();

                yield return new WaitForSeconds(friendlyExitDelay);
                leftArea.GoToFriendsZone(FriendsZonesManager.instance.currentZone);
                rightArea.GoToFriendsZone(FriendsZonesManager.instance.currentZone);
                FriendsZonesManager.instance.IncrementCurrentZone();

                yield return new WaitForSeconds(winDelay2);
                HideDropZone();
                //WidgetPopupWindow.I.ShowSentenceWithMark(OnRoundResultPressed, "comment_welldone", true, null);
                //Popup.Show();
                //Popup.SetButtonCallback(OnRoundResultPressed);
                //Popup.SetTitle(TextID.WELL_DONE);
                //Popup.SetMark(true, true);
                Play();
            }
            else
            {
                Debug.Log("Lose");

                GetConfiguration().Context.GetAudioManager().PlaySound(Sfx.Lose);
                yield return new WaitForSeconds(loseDelay);
                HideDropZone();
                //WidgetPopupWindow.I.ShowSentenceWithMark(OnRoundResultPressed, "game_balloons_commentA", false, null);
                Popup.Show();
                Popup.SetButtonCallback(OnRoundResultPressed);
                //Popup.SetTitle(TextID.GAME_RESULT_RETRY);
                Popup.SetMark(true, false);
            }
        }

        private void Reset()
        {
            commonLetters.Clear();
            choiceLetters.Clear();
            correctChoices.Clear();
            incorrectChoices.Clear();
            wordData1 = null;
            wordData2 = null;
            wordLetters1.Clear();
            wordLetters2.Clear();

            letterPicker.Reset();
            dropZone.Reset();
            leftArea.Reset();
            rightArea.Reset();
        }

        private void EndGame()
        {
            StartCoroutine(EndGame_Coroutine());
        }

        private IEnumerator EndGame_Coroutine()
        {
            var delay1 = 1f;
            yield return new WaitForSeconds(delay1);

            PlayIdleMusic();
            Reset();

            // Zoom out camera
            var fromPosition = sceneCamera.transform.localPosition;
            var toPosition = endCameraPosition;
            var fromRotation = sceneCamera.transform.localRotation.eulerAngles;
            var toRotation = endCameraRotation;
            var interpolant = 0f;
            var lerpProgress = 0f;
            var lerpLength = 2f;

            while (lerpProgress < lerpLength)
            {
                sceneCamera.transform.localPosition = Vector3.Lerp(fromPosition, toPosition, interpolant);
                sceneCamera.transform.localRotation = Quaternion.Euler(Vector3.Lerp(fromRotation, toRotation, interpolant));
                lerpProgress += Time.deltaTime;
                interpolant = lerpProgress / lerpLength;
                interpolant = Mathf.Sin(interpolant * Mathf.PI * 0.5f);
                yield return new WaitForFixedUpdate();
            }
                
//            endGameCanvas.gameObject.SetActive(true);
//
//            int numberOfStars = 0;
//
//            if (friendships <= 0)
//            {
//                numberOfStars = 0;
//                WidgetSubtitles.I.DisplaySentence("game_result_retry");
//            }
//            else if ((float)friendships / numberOfRounds < 0.5f)
//            {
//                numberOfStars = 1;
//                WidgetSubtitles.I.DisplaySentence("game_result_fair");
//            }
//            else if (friendships < numberOfRounds)
//            {
//                numberOfStars = 2;
//                WidgetSubtitles.I.DisplaySentence("game_result_good");
//            }
//            else
//            {
//                numberOfStars = 3;
//                WidgetSubtitles.I.DisplaySentence("game_result_great");
//            }
//            starFlowers.Show(numberOfStars);

            PlayState.OnResult();
        }
    }


    [Serializable]
    public class MakeFriendsGameplayInfo : AnturaGameplayInfo
    {
        [Tooltip("Play session duration in seconds.")]
        public float PlayTime = 0f;
    }
}
