﻿namespace EA4S.Tobogan
{
    public class ToboganPlayState : IGameState
    {
        ToboganGame game;
        
        public ToboganPlayState(ToboganGame game)
        {
            this.game = game;
        }

        public void EnterState()
        {
            game.questionsManager.NewQuestion();
        }

        public void ExitState()
        {
            game.Context.GetPopupWidget().Hide();
        }

        public void Update(float delta)
        {

        }

        public void UpdatePhysics(float delta)
        {

        }
    }
}