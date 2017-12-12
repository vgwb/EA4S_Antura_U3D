using Antura.LivingLetters;
using Antura.Teacher;

namespace Antura.Minigames.SickLetters
{
    public enum SickLettersVariation
    {
        LetterName = MiniGameCode.SickLetters_lettername,
    }

    public class SickLettersConfiguration : AbstractGameConfiguration
    {
        public SickLettersVariation Variation { get; set; }

        public override void SetMiniGameCode(MiniGameCode code)
        {
            Variation = (SickLettersVariation)code;
        }

        // Singleton Pattern
        static SickLettersConfiguration instance;
        public static SickLettersConfiguration Instance
        {
            get
            {
                if (instance == null)
                    instance = new SickLettersConfiguration();
                return instance;
            }
        }

        private SickLettersConfiguration()
        {
            // Default values
            Context = new MinigamesGameContext(MiniGameCode.SickLetters_lettername, System.DateTime.Now.Ticks.ToString());
            Questions = new SickLettersQuestionProvider();
            TutorialEnabled = true;
            //SickLettersQuestions = new SickLettersQuestionProvider();
            Difficulty = 0.1f;
            ConfigAI.VerboseTeacher = true;
        }

        public override IQuestionBuilder SetupBuilder()
        {
            IQuestionBuilder builder = null;

            int nPacks = 20;
            int nCorrect = 1;
            int nWrong = 0;

            var builderParams = new QuestionBuilderParameters();
            builder = new RandomLettersQuestionBuilder(nPacks, nCorrect, nWrong, parameters: builderParams);

            return builder;
        }

        public override MiniGameLearnRules SetupLearnRules()
        {
            var rules = new MiniGameLearnRules();
            // example: a.minigameVoteSkewOffset = 1f;
            return rules;
        }

    }
}
