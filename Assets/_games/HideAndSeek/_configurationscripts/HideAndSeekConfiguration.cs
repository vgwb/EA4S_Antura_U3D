﻿namespace EA4S.HideAndSeek
{
    public class HideAndSeekConfiguration : IGameConfiguration
    {
        // Game configuration
        public IGameContext Context { get; set; }

        public IQuestionProvider Questions { get; set; }

        public float Difficulty { get; set; }

        /////////////////
        // Singleton Pattern
		static HideAndSeekConfiguration instance;
		public static HideAndSeekConfiguration Instance
        {
            get
            {
                if (instance == null)
					instance = new HideAndSeekConfiguration();
                return instance;
            }
        }
        /////////////////

		private HideAndSeekConfiguration()
        {
            // Default values
            // THESE SETTINGS ARE FOR SAMPLE PURPOSES, THESE VALUES MUST BE SET BY GAME CORE
            Context = new SampleGameContext();
            Questions = new SampleQuestionProvider();
            Difficulty = 0.5f;
        }

        public IQuestionBuilder SetupBuilder() {
            IQuestionBuilder builder = null;
            // TODO
            return builder;
        }
    }
}
