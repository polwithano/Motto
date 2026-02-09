using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Managers;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace UI
{
    public class UIDeckViewer : MonoBehaviour
    {
        [SerializeField] private bool sortAlphabetically; 
        
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI drawPileLabel;
        [SerializeField] private TextMeshProUGUI discardPileLabel;
        [SerializeField] private Transform drawPileGrid;
        [SerializeField] private Transform discardPileGrid;
        [SerializeField] private TileView tileViewPrefab;
        
        [Header("Layout")]
        [SerializeField] private float fadeDuration = 0.33f;
        [SerializeField] private float gridCellHeight = 100f;
        [SerializeField] private ScrollRect scrollContent; 
        
        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        public void Open()
        {
            Construct();
            FadeWindow(true);
        }

        public void Close()
        {
            FadeWindow(false);
            ClearGrid(drawPileGrid);
            ClearGrid(discardPileGrid);
        }

        private void FadeWindow(bool fadeIn)
        {
            canvasGroup.DOFade(fadeIn ? 1 : 0, fadeDuration)
                .SetEase(Ease.OutExpo);
            canvasGroup.interactable = fadeIn;
            canvasGroup.blocksRaycasts = fadeIn;
        }

        private void Construct()
        {
            var deck = GameManager.Instance.Deck;

            // Update labels
            drawPileLabel.text = $"Draw pile ({deck.DrawPile.Count})";
            discardPileLabel.text = $"Discard pile ({deck.DiscardPile.Count})";

            // Build UI tiles
            BuildGrid(drawPileGrid, deck.DrawPile);
            BuildGrid(discardPileGrid, deck.DiscardPile);

            // Adjust scroll content height
            scrollContent.normalizedPosition = Vector2.up;
        }
        
        private void BuildGrid(Transform parent, IReadOnlyList<Tile> tiles)
        {
            ClearGrid(parent);
            IEnumerable<Tile> displayList = tiles;

            if (sortAlphabetically)
            {
                displayList = tiles
                    .OrderBy(t => t.IsBlank)                                 // blanks are last
                    .ThenBy(t => t.Character.ToString().ToUpperInvariant()); // alphabetically
            }

            foreach (var tile in displayList)
            {
                var view = Instantiate(tileViewPrefab, parent);
                view.Populate(tile);

                // Shitty Fix
                // Destroy TileView to prevent in-game interactions
                Destroy(view); 
            }
        }

        private void ClearGrid(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }
    }
}
