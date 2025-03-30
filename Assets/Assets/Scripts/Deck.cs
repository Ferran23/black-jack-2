using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;
    public Text playerPointsText;
    public Text dealerPointsText;
    public Text creditText;
    public Text playerLabel;
    public Text dealerLabel;

    public int[] values = new int[52];
    int cardIndex = 0;
    private List<int> deckIndices = new List<int>();
    private bool cartaVolteada = false;
    private int credit = 1000;

    private void Awake()
    {
        InitCardValues();
    }

    private void Start()
    {
        ShuffleCards();
        StartGame();
        UpdateUI();
    }

    private void InitCardValues()
    {
        for (int i = 0; i < 52; i++)
        {
            int valor = (i % 13) + 1;
            if (valor > 10) valor = 10;
            if (valor == 1) valor = 11;
            values[i] = valor;
            deckIndices.Add(i);
        }
    }

    private void ShuffleCards()
    {
        for (int i = 0; i < deckIndices.Count; i++)
        {
            int randomIndex = Random.Range(0, deckIndices.Count);
            int temp = deckIndices[i];
            deckIndices[i] = deckIndices[randomIndex];
            deckIndices[randomIndex] = temp;
        }
        cardIndex = 0;
    }

    void StartGame()
    {
        cartaVolteada = false;
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }
        UpdateUI();

        if (player != null && dealer != null && (player.GetComponent<CardHand>().points == 21 || dealer.GetComponent<CardHand>().points == 21))
        {
            finalMessage.text = "Blackjack! Fin del juego.";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    private void CalculateProbabilities()
    {
        if (player == null || dealer == null) return;
        int playerTotal = player.GetComponent<CardHand>().points;
        int dealerTotal = dealer.GetComponent<CardHand>().points;
        int hiddenCardValue = values[deckIndices[cardIndex]];

        float probDealerWin = 0;
        float probPlayerSafe = 0;
        float probPlayerBust = 0;

        int favorableDealer = 0;
        int favorableSafe = 0;
        int favorableBust = 0;
        int remainingCards = 52 - cardIndex;

        for (int i = cardIndex; i < 52; i++)
        {
            int nextCardValue = values[deckIndices[i]];
            if (dealerTotal + hiddenCardValue > playerTotal) favorableDealer++;
            if (playerTotal + nextCardValue >= 17 && playerTotal + nextCardValue <= 21) favorableSafe++;
            if (playerTotal + nextCardValue > 21) favorableBust++;
        }

        probDealerWin = (float)favorableDealer / remainingCards;
        probPlayerSafe = (float)favorableSafe / remainingCards;
        probPlayerBust = (float)favorableBust / remainingCards;

        probMessage.text = $"Probabilidades:\nDealer > Player: {probDealerWin:F4}\n17 <= X <= 21: {probPlayerSafe:F2}\nX > 21: {probPlayerBust:F2}";
    }

    void PushDealer()
    {
        if (dealer != null)
        {
            dealer.GetComponent<CardHand>().Push(faces[deckIndices[cardIndex]], values[deckIndices[cardIndex]]);
            cardIndex++;
        }
    }

    void PushPlayer()
    {
        if (player != null)
        {
            player.GetComponent<CardHand>().Push(faces[deckIndices[cardIndex]], values[deckIndices[cardIndex]]);
            cardIndex++;
            CalculateProbabilities();
        }
    }

    public void Hit()
    {
        if (player == null) return;

        if (!cartaVolteada)
        {
            dealer.GetComponent<CardHand>().InitialToggle();
            cartaVolteada = true;
        }

        PushPlayer();
        UpdateUI();

        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "Has perdido. Te pasaste de 21.";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stand()
    {
        if (dealer == null || player == null) return;

        if (!cartaVolteada)
        {
            dealer.GetComponent<CardHand>().InitialToggle();
            cartaVolteada = true;
        }

        while (dealer.GetComponent<CardHand>().points < 17)
        {
            PushDealer();
        }

        int dealerTotal = dealer.GetComponent<CardHand>().points;
        int playerTotal = player.GetComponent<CardHand>().points;

        if (dealerTotal > 21 || playerTotal > dealerTotal)
            finalMessage.text = "¡Has ganado!";
        else if (playerTotal < dealerTotal)
            finalMessage.text = "Has perdido, el crupier ganó.";
        else
            finalMessage.text = "Empate.";

        hitButton.interactable = false;
        stickButton.interactable = false;
        UpdateUI();
    }

    public void PlayAgain()
    {
        if (player == null || dealer == null)
        {
            Debug.LogError("Error: Player o Dealer no están asignados en el Inspector.");
            return;
        }

        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        probMessage.text = "";

        if (playerPointsText != null) playerPointsText.text = "";
        if (dealerPointsText != null) dealerPointsText.text = "";
        if (playerLabel != null) playerLabel.text = "Jugador:";
        if (dealerLabel != null) dealerLabel.text = "Dealer:";
        if (creditText != null) creditText.text = "Crédito: " + credit + "€";

        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        ShuffleCards();
        StartGame();
    }

    private void UpdateUI()
    {
        if (playerPointsText != null)
            playerPointsText.text = "Puntos: " + player.GetComponent<CardHand>().points;

        if (dealerPointsText != null)
            dealerPointsText.text = "Puntos: " + dealer.GetComponent<CardHand>().points;

        if (creditText != null)
            creditText.text = "Crédito: " + credit + "€";
    }
}

