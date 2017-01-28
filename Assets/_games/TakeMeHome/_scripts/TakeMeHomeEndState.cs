﻿namespace EA4S.Minigames.TakeMeHome
{
	public class TakeMeHomeEndState : IState {

		TakeMeHomeGame game;

		float timer = 2;
		public TakeMeHomeEndState(TakeMeHomeGame game)
		{
			this.game = game;
		}

		public void EnterState()
		{
			game.Context.GetAudioManager().PlayMusic(Music.Relax);
		}

		public void ExitState()
		{
		}

		public void Update(float delta)
		{
			timer -= delta;

			if (timer < 0)
			{
				game.EndGame(game.CurrentStars, game.CurrentScore);
			}
		}

		public void UpdatePhysics(float delta)
		{
		}
	}
}
