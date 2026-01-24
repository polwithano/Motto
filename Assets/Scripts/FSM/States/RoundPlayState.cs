using System.Collections.Generic;
using Events;
using Managers;
using Misc;
using Models;
using UI;
using UnityEngine;
using Views;

namespace FSM
{
    public class RoundPlayState : GameState
    {
        public RoundPlayState(GameStateMachine machine) : base(machine) { }

        public string CurrentWord;
        public List<Tile> CurrentTiles = new(); 

        public override void Enter()
        {
            GameEvents.OnBoardUpdated += HandleOnBoardUpdated;
            GameEvents.OnTileSelected += HandleOnTileSelected; 
            GameEvents.OnTileRedraw += HandleOnTileRedraw;
            GameEvents.OnWordScored += HandleOnWordScored; 
            GameEvents.OnScoreSequenceCompleted += HandleOnScoreSequenceCompleted;
        }

        public override void Exit()
        {
            GameEvents.OnBoardUpdated -= HandleOnBoardUpdated;
            GameEvents.OnTileSelected -= HandleOnTileSelected; 
            GameEvents.OnTileRedraw -= HandleOnTileRedraw;
            GameEvents.OnWordScored -= HandleOnWordScored;
            GameEvents.OnScoreSequenceCompleted -= HandleOnScoreSequenceCompleted;
        }
        
        #region Event Handlers
        private void HandleOnTileRedraw(Tile tile)
        {
            if (!Game.Run.Round.AllowDraw()) return; 
            
            Game.Hand.RemoveTile(tile);
            Game.Deck.Discard(tile);
            
            var newTile = Game.Deck.Draw(1)[0];
            Game.Hand.TryAddTile(newTile);
            Game.Run.Round.RemoveDraw();
            
            GameEvents.RaiseOnTileRedrawPerformed(tile, newTile);
        }

        private void HandleOnTileSelected(TileView tileView)
        {
            if (tileView.IsInHand)
            {
                var emptySlot = BoardManager.Instance.GetFirstEmptySlot();
                if (emptySlot)
                {
                    GameEvents.RaiseOnTileAddedToBoard(tileView);
                }
            }
            else
            {
                GameEvents.RaiseOnTileRemovedFromBoard(tileView);
            }
        }
        
        private async void HandleOnBoardUpdated(string word, List<Tile> tiles)
        {
            CurrentWord = word;
            CurrentTiles = tiles;
            
            var isLegit = GameManager.Instance.DisableWordCheck || await word.CheckWordWithBlanksAsync();
            
            OnWordChecked(word, isLegit);
        }

        private void HandleOnWordScored()
        {
            if (Game.Run.Round.WordsRemaining <= 0) return;
            
            Game.Run.Round.RemoveAttempt();
            Game.Run.Round.AddWord(CurrentWord, CurrentTiles);
            
            GameEvents.RaiseOnScoringStarted(CurrentWord, CurrentTiles);
        }

        private async void HandleOnScoreSequenceCompleted(ScoreLog log)
        {
            await UIGame.Instance.RoundContext.PlayScoreSequenceAsync(log); 

            CheckForExitConditions();
            
            Game.Hand.RemoveTiles(log.Tiles);
            Game.Deck.DiscardRange(log.Tiles);
            
            BoardManager.Instance.ClearSlots();
            
            Game.Hand.FillFromDeck(Game.Deck);
            
            HandView.Instance.InstantiateHand(Game.Hand);
        }
        #endregion
        
        #region Helpers
        private void OnWordChecked(string word, bool isLegit)
        {
            if (isLegit) GameEvents.RaiseOnWordValidated(word);
            else GameEvents.RaiseOnWordInvalidated();
        }

        private void CheckForExitConditions()
        {
            if (Game.Run.Round.IsCompleted)
            {
                GameEvents.RaiseOnRoundSuccessful();
                StateMachine.ChangeState(new RoundOverState(StateMachine, true));
            }
            else if (!Game.Run.Round.IsCompleted)
            {
                if (Game.Run.Round.WordsRemaining <= 0)
                {
                    GameEvents.RaiseOnRoundFailed();
                    StateMachine.ChangeState(new RoundOverState(StateMachine, false));
                }
            }
        }
        #endregion
    }
}