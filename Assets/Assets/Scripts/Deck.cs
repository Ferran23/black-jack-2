using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Button startGameButton;
    public Button stopGameButton;
    public Button confirmCreditButton;
    public InputField creditInputField;
    public Text finalMessage;
    public Text probMessage;
    public Text playerPointsText;
    public Text dealerPointsText;
    public Text creditText;
    public Text playerLabel;
    public Text dealerLabel;
    public Text textProb;

    public int[] values = new int[52];
    int cardIndex = 0;
    private List<int> deckIndices = new List<int>();
    private bool cartaVolteada = false;
    private int credit = 0;
    private int currentBet = 0;
    private int totalWinnings = 0;

    private int dealerHiddenValue;
    private GameObject cartaOcultaDealer;
    private bool creditConfirmado = false;

    private void Awake()
    {
        InitCardValues();
    }

    private void InitCardValues()
    {
        deckIndices.Clear();
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
        deckIndices.Clear();
        for (int i = 0; i < 52; i++)
        {
            deckIndices.Add(i);
        }

        for (int i = 0; i < deckIndices.Count; i++)
        {
            int randomIndex = Random.Range(0, deckIndices.Count);
            int temp = deckIndices[i];
            deckIndices[i] = deckIndices[randomIndex];
            deckIndices[randomIndex] = temp;
        }
        cardIndex = 0;
    }

    private void UpdateAdviceText(float probSafe, float probBust)
    {
        string advice = "";
        if (probSafe > 0.7f)
        {
            advice = "Buena idea pedir carta.";
        }
        else if (probBust > 0.6f)
        {
            advice = "Mejor quedarse.";
        }
        else
        {
            advice = "Está ajustado, tú decides.";
        }

        probMessage.text += $"\n{advice}";
    }

    private void CalculateProbabilities()
    {
        if (player == null || dealer == null) return;

        int playerTotal = player.GetComponent<CardHand>().points;
        int dealerVisiblePoints = dealer.GetComponent<CardHand>().points;
        if (!cartaVolteada)
        {
            dealerVisiblePoints -= dealerHiddenValue;
        }

        int favorableDealer = 0;
        int favorableSafe = 0;
        int favorableBust = 0;
        int remainingCards = 52 - cardIndex;

        for (int i = cardIndex; i < 52; i++)
        {
            int nextCardValue = values[deckIndices[i]];

            // Simulación de posibles escenarios del dealer
            int dealerSim = dealerVisiblePoints + values[deckIndices[i]];
            while (dealerSim < 17 && i + 1 < 52)
            {
                dealerSim += values[deckIndices[i + 1]];
            }
            if (dealerSim > playerTotal)
            {
                favorableDealer++;
            }

            // Cálculo para jugador
            if (playerTotal + nextCardValue >= 17 && playerTotal + nextCardValue <= 21) favorableSafe++;
            if (playerTotal + nextCardValue > 21) favorableBust++;
        }

        float probDealerWin = (float)favorableDealer / remainingCards;
        float probPlayerSafe = (float)favorableSafe / remainingCards;
        float probPlayerBust = (float)favorableBust / remainingCards;

        probMessage.text = $"Probabilidades:\nDealer > Player: {probDealerWin:F4}\n17 <= X <= 21: {probPlayerSafe:F2}\nX > 21: {probPlayerBust:F2}";

        UpdateAdviceText(probPlayerSafe, probPlayerBust);
    }


    private void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[deckIndices[cardIndex]], values[deckIndices[cardIndex]]);
        cardIndex++;
        CalculateProbabilities();
    }

    private void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[deckIndices[cardIndex]], values[deckIndices[cardIndex]]);
        cardIndex++;
        CalculateProbabilities();
    }

        private void Start()
    {
        ShuffleCards();
        DisableGameButtons();
        UpdateUI();
    }

    public void SetInitialCredit()
    {
        if (int.TryParse(creditInputField.text, out credit) && credit > 0)
        {
            creditText.text = "Crédito: " + credit + "€";
            creditInputField.interactable = false;
            confirmCreditButton.gameObject.SetActive(false);
            startGameButton.interactable = true;
            finalMessage.text = "";
            creditConfirmado = true;
            totalWinnings = 0;
        }
        else
        {
            finalMessage.text = "Introduce un crédito válido para empezar.";
        }
    }

    public void OnStartGame()
    {
        if (!creditConfirmado)
        {
            finalMessage.text = "Primero debes confirmar tu crédito inicial.";
            return;
        }

        currentBet = credit;

        if (currentBet > 0)
        {
            credit -= currentBet;
            finalMessage.text = "";
            probMessage.text = "";
            textProb.text = "";
            player.GetComponent<CardHand>().Clear();
            dealer.GetComponent<CardHand>().Clear();
            ShuffleCards();
            cartaOcultaDealer = null;
            cartaVolteada = false;
            UpdateUI();
            StartGame();
            EnableGameButtons();
            startGameButton.GetComponentInChildren<Text>().text = "Continue";
            UpdateUI();
        }
        else
        {
            finalMessage.text = "No tienes crédito para apostar.";
        }
    }

    public void OnStopGame()
    {
        finalMessage.text = "Te retiraste. Ganancias totales: " + credit + "€";
        DisableGameButtons();
        creditConfirmado = false;
    }

    private void EnableGameButtons()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
    }

    private void DisableGameButtons()
    {
        if (hitButton != null) hitButton.interactable = false;
        if (stickButton != null) stickButton.interactable = false;
    }

    private void StartGame()
    {
        cartaVolteada = false;

        PushPlayer();

        dealerHiddenValue = values[deckIndices[cardIndex]];
        dealer.GetComponent<CardHand>().Push(faces[deckIndices[cardIndex]], dealerHiddenValue);
        cartaOcultaDealer = dealer.GetComponent<CardHand>().cards[^1];
        cartaOcultaDealer.GetComponent<CardModel>().ToggleFace(false);
        cardIndex++;

        PushPlayer();
        PushDealer();

        UpdateUI();

        int playerPoints = player.GetComponent<CardHand>().points;
        if (playerPoints == 21)
        {
            cartaOcultaDealer.GetComponent<CardModel>().ToggleFace(true);
            cartaVolteada = true;
            UpdateUI();

            finalMessage.text = "¡Blackjack! Has ganado " + (currentBet * 2) + "€";
            totalWinnings += currentBet * 2;
            credit += currentBet * 2;
            DisableGameButtons();
            creditInputField.interactable = false;
        }
    }

    public void Hit()
    {
        StartCoroutine(HitWithDelay());
    }

    IEnumerator HitWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        PushPlayer();
        UpdateUI();

        int playerPoints = player.GetComponent<CardHand>().points;

        if (playerPoints > 21)
        {
            RevealDealerCard();
            finalMessage.text = "Has perdido. Te pasaste de 21.";
            DisableGameButtons();
        }
        else if (playerPoints == 21)
        {
            finalMessage.text = "¡Blackjack! Turno del dealer.";
            DisableGameButtons();
            Stand();
        }
    }

    public void Stand()
    {
        RevealDealerCard();
        StartCoroutine(DealerTurnWithDelay());
    }

    IEnumerator DealerTurnWithDelay()
    {
        while (dealer.GetComponent<CardHand>().points < 17)
        {
            yield return new WaitForSeconds(0.7f);
            PushDealer();
        }

        int dealerTotal = dealer.GetComponent<CardHand>().points;
        int playerTotal = player.GetComponent<CardHand>().points;

        if (dealerTotal > 21 || playerTotal > dealerTotal)
        {
            finalMessage.text = "¡Has ganado! +" + (currentBet * 2) + "€";
            totalWinnings += currentBet * 2;
            credit += currentBet * 2;
        }
        else if (playerTotal < dealerTotal)
        {
            finalMessage.text = "Has perdido, el dealer ganó.";
        }
        else
        {
            finalMessage.text = "Empate. Recuperas tu crédito.";
            credit += currentBet;
        }

        DisableGameButtons();
        UpdateUI();
    }

    public void PlayAgain()
    {
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();

        cartaOcultaDealer = null;
        cartaVolteada = false;

        if (creditText != null)
            creditText.text = "Crédito: " + credit + "€";

        if (finalMessage != null)
            finalMessage.text = "";

        if (probMessage != null)
            probMessage.text = "";

        if (textProb != null)
            textProb.text = "";

        if (playerPointsText != null)
            playerPointsText.text = "";

        if (dealerPointsText != null)
            dealerPointsText.text = "";

        if (playerLabel != null)
            playerLabel.text = "Jugador:";

        if (dealerLabel != null)
            dealerLabel.text = "Dealer:";

        if (credit <= 0 || !creditConfirmado)
        {
            if (creditInputField != null) creditInputField.text = "";
            if (creditInputField != null) creditInputField.interactable = true;
            if (confirmCreditButton != null) confirmCreditButton.gameObject.SetActive(true);
            if (startGameButton != null) startGameButton.GetComponentInChildren<Text>().text = "Play";
            if (startGameButton != null) startGameButton.interactable = false;
            creditConfirmado = false;
        }
        else
        {
            if (startGameButton != null)
            {
                startGameButton.GetComponentInChildren<Text>().text = "Continue";
                startGameButton.interactable = true;
            }
        }

        DisableGameButtons();
        UpdateUI();
    }

    private void RevealDealerCard()
    {
        if (!cartaVolteada && cartaOcultaDealer != null)
        {
            cartaOcultaDealer.GetComponent<CardModel>().ToggleFace(true);
            cartaVolteada = true;
            UpdateUI();
            CalculateProbabilities();
        }
    }

    private void UpdateUI()
    {
        if (playerPointsText != null)
            playerPointsText.text = "Puntos: " + player.GetComponent<CardHand>().points;

        if (dealerPointsText != null)
            dealerPointsText.text = cartaVolteada ? "Puntos: " + dealer.GetComponent<CardHand>().points : "Puntos: ?";

        if (creditText != null)
            creditText.text = "Crédito: " + credit + "€";
    }
} 

