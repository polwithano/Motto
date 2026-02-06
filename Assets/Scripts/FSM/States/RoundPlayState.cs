using System;
using System.Collections.Generic;
using System.Linq;
using Animation;
using Events;
using Events.Core;
using Events.Game;
using Events.Inputs;
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

        public string CurrentWord { get; private set; }
        public List<Tile> CurrentTiles { get; private set; } = new(); 

        public override void Enter()
        {
            Bus<BoardUpdatedEvent>.OnEvent += HandleOnBoardUpdated;
            Bus<TileSelectedEvent>.OnEvent += HandleOnTileSelected;
            Bus<TileRedrawEvent>.OnEvent += HandleOnTileRedraw; 
            GameEvents.OnWordScored += HandleOnWordScored; 
            GameEvents.OnScoreSequenceCompleted += HandleOnScoreSequenceCompleted;
        }

        public override void Exit()
        {
            Bus<BoardUpdatedEvent>.OnEvent -= HandleOnBoardUpdated; 
            Bus<TileSelectedEvent>.OnEvent -= HandleOnTileSelected;
            Bus<TileRedrawEvent>.OnEvent -= HandleOnTileRedraw; 
            GameEvents.OnWordScored -= HandleOnWordScored;
            GameEvents.OnScoreSequenceCompleted -= HandleOnScoreSequenceCompleted;
        }
        
        #region Event Handlers
        private void HandleOnTileRedraw(TileRedrawEvent evt)
        {
            if (!Game.Run.Round.AllowDraw()) return; 
            
            Game.Hand.RemoveTile(evt.Model);
            Game.Deck.Discard(evt.Model);
            
            var newTile = Game.Deck.Draw(1).First();
            Game.Hand.TryAddTile(newTile);
            Game.Run.Round.RemoveDraw();
            
            GameEvents.RaiseOnTileRedrawPerformed(evt.Model, newTile);
        }

        private void HandleOnTileSelected(TileSelectedEvent evt)
        {
            var position = evt.View.IsInHand ? GamePosition.Board : GamePosition.Hand;
            if (position == GamePosition.Hand)
            {
                var emptySlot = BoardManager.Instance.GetFirstEmptySlot();
                if (!emptySlot)
                {
                    Debug.LogError($"No Empty Slot found on the board, tile {evt.View.gameObject.name} cannot be moved.");
                    return;
                }
            }
            Bus<TilePositionUpdatedEvent>.Raise(new TilePositionUpdatedEvent(position, evt.View));
        }
        
        private async void HandleOnBoardUpdated(BoardUpdatedEvent evt)
        {
            try
            {
                CurrentWord = evt.Word;
                CurrentTiles = evt.Tiles;
            
                var isLegit = GameManager.Instance.DisableWordCheck || await evt.Word.CheckWordWithBlanksAsync();
            
                OnWordChecked(evt.Word, isLegit);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
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