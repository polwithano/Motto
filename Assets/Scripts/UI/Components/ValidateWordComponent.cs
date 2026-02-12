using Events.Core;
using Events.Game;
using Events.Rounds;
using Events.Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UI.Components.Core;

namespace UI.Components
{
    [RequireComponent(typeof(Button))]
    public class ValidateWordComponent : MultiEventListenerComponent
    {
        [SerializeField] private TextMeshProUGUI lettersCount;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        protected override void RegisterEvents()
        {
            ListenTo<RoundStartedEvent>();
            ListenTo<WordValidationEvent>();
            ListenTo<BoardUpdatedEvent>();
        }

        protected override void OnAnyEvent(IEvent evt)
        {
            switch (evt)
            {
                case RoundStartedEvent:
                case BoardUpdatedEvent:
                    DisableButton();
                    break;

                case WordValidationEvent validationEvent:
                    HandleValidation(validationEvent);
                    break;
            }
        }

        private void HandleValidation(WordValidationEvent evt)
        {
            if (evt.Status == WordValidationStatus.Validated)
            {
                EnableButton();
                lettersCount.text = evt.Word.Length.ToString();
                InvokeSuccess();
            }
            else
            {
                DisableButton();
                InvokeFailed();
            }
        }

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
