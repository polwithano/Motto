using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using Events.Core;
using Events.Game;
using Events.Rounds;
using Events.Score;
using Managers;
using Misc;
using Models;
using UI.Containers;
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
            Bus<TileRedrawEvent>.OnEvent += HandleOnTileRedraw;
            Bus<WordProcessedEvent>.OnEvent += HandleOnWordProcessed;
            Bus<ScoringSequenceEndedEvent>.OnEvent += HandleOnScoringSequenceEnded;
        }

        public override void Exit()
        {
            Bus<BoardUpdatedEvent>.OnEvent -= HandleOnBoardUpdated; 
            Bus<TileRedrawEvent>.OnEvent -= HandleOnTileRedraw; 
            Bus<WordProcessedEvent>.OnEvent -= HandleOnWordProcessed;
            Bus<ScoringSequenceEndedEvent>.OnEvent -= HandleOnScoringSequenceEnded;
        }
        
        #region Event Handlers
        private void HandleOnTileRedraw(TileRedrawEvent evt)
        {
            if (!Game.Run.Round.AllowDraw()) return; 
            
            Game.Hand.RemoveTile(evt.Model);
            Game.Deck.Discard(evt.Model);

            if (Game.Deck.TryDraw(out var newTile))
            {
                Game.Hand.TryAddTile(newTile);
                Game.Run.Round.RemoveDraw();
            
                Bus<TileRedrawCompletedEvent>
                    .Raise(new TileRedrawCompletedEvent(
                        evt.Model, 
                        newTile, 
                        Game.Run.Round.DrawsRemaining, 
                        Game.Deck.DrawPile.Count));   
            }
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

        private void HandleOnWordProcessed(WordProcessedEvent evt)
        {
            if (Game.Run.Round.WordsRemaining <= 0) return;
            
            Game.Run.Round.RemoveAttempt();
            Game.Run.Round.AddWord(CurrentWord, CurrentTiles);
            
            Bus<ScoringSequenceStartedEvent>
                .Raise(new ScoringSequenceStartedEvent(CurrentWord, CurrentTiles));
        }

        private async void HandleOnScoringSequenceEnded(ScoringSequenceEndedEvent evt)
        {
            try
            {
                var log = evt.ScoreLog; 

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
            
            Bus<WordValidationEvent>
                .Raise(new WordValidationEvent(validationStatus, word));
        }

        private void CheckForExitConditions()
        {
            switch (Game.Run.Round.IsCompleted)
            {
                case true:
                {
                    const RoundEndedStatus status = RoundEndedStatus.Success;
                    
                    Bus<RoundEndedEvent>
                        .Raise(new RoundEndedEvent(status, Game.Run.Round));
                    
                    StateMachine.ChangeState(new RoundOverState(StateMachine, status));
                    
                    break;
                }
                case false:
                {
                    if (Game.Run.Round.WordsRemaining <= 0)
                    {
                        const RoundEndedStatus status = RoundEndedStatus.Failure;
                        
                        Bus<RoundEndedEvent>
                            .Raise(new RoundEndedEvent(status, Game.Run.Round));
                        
                        StateMachine.ChangeState(new RoundOverState(StateMachine, status));
                    }

                    break;
                }
            }
        }
        #endregion
    }
}