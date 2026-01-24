using System.Collections.Generic;
using Events;
using Managers;
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
            GameEvents.OnRoundStarted += HandleOnRoundStarted;
            GameEvents.OnBoardUpdated += HandleOnBoardUpdated; 
            GameEvents.OnWordInvalidated += HandleWordInvalidated;
            GameEvents.OnWordValidated += HandleWordValidated; 
        }

        private void OnDisable()
        {
            GameEvents.OnRoundStarted -= HandleOnRoundStarted;
            GameEvents.OnBoardUpdated -= HandleOnBoardUpdated;
            GameEvents.OnWordInvalidated -= HandleWordInvalidated;
            GameEvents.OnWordValidated -= HandleWordValidated;
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnRoundStarted(RoundContext context)
        {
            DisableButton();
        }

        private void HandleOnBoardUpdated(string word, List<Tile> tiles)
        {
            DisableButton();
        }
        
        private void HandleWordInvalidated()
        {
            DisableButton();
        }
        
        private void HandleWordValidated(string word)
        {
            EnableButton();
            lettersCount.text = word.Length.ToString();
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