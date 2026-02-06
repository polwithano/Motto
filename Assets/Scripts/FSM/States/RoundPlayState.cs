using System;
using System.Collections.Generic;
using Events;
using Events.Core;
using Events.Game;
using Events.Rounds;
using Events.Score;
using Managers;
using Misc;
using Models;
using UI;
using UnityEngine;
using Views;

namespace FSM.States
{
    [System.Serializable]
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
            var position = tileView.IsInHand ? TilePosition.Board : TilePosition.Hand;
            if (position == TilePosition.Hand)
            {
                var emptySlot = BoardManager.Instance.GetFirstEmptySlot();
                if (!emptySlot)
                {
                    Debug.LogError($"No Empty Slot found on the board, tile {tileView.gameObject.name} cannot be moved.");
                    return;
                }
            }
            Bus<TilePositionUpdatedEvent>.Raise(new TilePositionUpdatedEvent(position, tileView));
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
            try
            {
                await UIGame.Instance.RoundContext.PlayScoreSequenceAsync(log); 

                CheckForExitConditions();
            
                Game.Hand.RemoveTiles(log.Tiles);
                Game.Deck.DiscardRange(log.Tiles);
            
                await BoardManager.Instance.ClearSlotsAsync();
            
                Game.Hand.FillFromDeck(Game.Deck);
            
                HandView.Instance.InstantiateHand(Game.Hand);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        #endregion
        
        #region Helpers
        private void OnWordChecked(string word, bool isLegit)
        {
            var validationStatus = isLegit ? WordValidationStatus.Validated : WordValidationStatus.Invalidated; 
            Bus<WordValidationEvent>.Raise(new WordValidationEvent(validationStatus, word));
        }

        private void CheckForExitConditions()
        {
            switch (Game.Run.Round.IsCompleted)
            {
                case true:
                {
                    var status = RoundEndedStatus.Success;
                    Bus<RoundEndedEvent>.Raise(new RoundEndedEvent(status, Game.Run.Round));
                    StateMachine.ChangeState(new RoundOverState(StateMachine, status));
                    break;
                }
                case false:
                {
                    if (Game.Run.Round.WordsRemaining <= 0)
                    {
                        var status = RoundEndedStatus.Failure;
                        Bus<RoundEndedEvent>.Raise(new RoundEndedEvent(status, Game.Run.Round));
                        StateMachine.ChangeState(new RoundOverState(StateMachine, status));
                    }

                    break;
                }
            }
        }
        #endregion
    }
}