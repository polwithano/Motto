using System.Collections.Generic;
using Events;
using Events.Core;
using Events.Rounds;
using Events.Score;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class UIValidateWordButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI lettersCount; 
        
        private Button _button;
        
        #region Mono
        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            Bus<RoundStartedEvent>.OnEvent += HandleOnRoundStarted;
            Bus<WordValidationEvent>.OnEvent += HandleWordValidation; 
            GameEvents.OnBoardUpdated += HandleOnBoardUpdated; 
        }

        private void OnDisable()
        {
            Bus<RoundStartedEvent>.OnEvent -= HandleOnRoundStarted; 
            GameEvents.OnBoardUpdated -= HandleOnBoardUpdated;
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnRoundStarted(RoundStartedEvent evt)
        {
            DisableButton();
        }

        private void HandleOnBoardUpdated(string word, List<Tile> tiles)
        {
            DisableButton();
        }
        
        private void HandleWordValidation(WordValidationEvent evt)
        {
            if (evt.Status == WordValidationStatus.Validated)
            {
                EnableButton();
                lettersCount.text = evt.Word.Length.ToString();
            }
            else
                DisableButton();
        }
        #endregion
        
        private void DisableButton()
        {
            _button.interactable = false;
            lettersCount.text = ""; 
            _button.onClick.RemoveAllListeners();
        }
        
        private void EnableButton()
        {
            _button.interactable = true;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnButtonValidated);
        }
        
        private void OnButtonValidated()
        {
            GameEvents.RaiseOnWordScored();
            DisableButton();
        }
    }
}