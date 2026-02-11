using Events.Core;
using Events.Game;
using Events.Rounds;
using Events.Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Containers
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
            Bus<BoardUpdatedEvent>.OnEvent += HandleOnBoardUpdated; 
        }

        private void OnDisable()
        {
            Bus<RoundStartedEvent>.OnEvent -= HandleOnRoundStarted; 
            Bus<WordValidationEvent>.OnEvent -= HandleWordValidation;
            Bus<BoardUpdatedEvent>.OnEvent -= HandleOnBoardUpdated; 
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnRoundStarted(RoundStartedEvent evt)
        {
            DisableButton();
        }

        private void HandleOnBoardUpdated(BoardUpdatedEvent evt)
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
            _button.onClick.RemoveAllListeners();
            lettersCount.text = ""; 
        }
        
        private void EnableButton()
        {
            _button.interactable = true;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnButtonValidated);
        }
        
        private void OnButtonValidated()
        {
            Bus<WordProcessedEvent>.Raise(new WordProcessedEvent());
            DisableButton();
        }
    }
}