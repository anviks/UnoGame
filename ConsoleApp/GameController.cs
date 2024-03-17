using DAL;
using Domain.Cards;
using Domain.Players;
using UnoEngine;

namespace ConsoleApp;

public class GameController
{
    private readonly Engine _engine;
    private readonly IGameStorage _storage;

    public GameController(Engine engine, IGameStorage storage)
    {
        _engine = engine;
        _storage = storage;
    }

    private static void SlowWriteLine(string text = "")
    {
        Thread.Sleep(50);
        Console.WriteLine(text);
    }
    
    public void Setup()
    {
        var gameState = _engine.State;
        _storage.SaveGame(gameState);
        var cardDeck = gameState.Deck;

        cardDeck.Shuffle();
        SlowWriteLine("Shuffling deck...");
        _engine.DealCards();
        var initialPlayer = _engine.GetCurrentPlayer();
        var firstCard = cardDeck.DrawCard();

        while (firstCard?.Value == ECardValue.WildDrawFour)
        {
            WriteAndSaveHistory($"First card is {firstCard}. Choosing a new card...");
            cardDeck.InsertRandomly(firstCard);
            firstCard = cardDeck.DrawCard();
        }

        WriteAndSaveHistory($"First card is {firstCard}");

        gameState.DiscardPile.Add(firstCard!);
        gameState.CurrentColor = firstCard!.Color;
        gameState.CurrentValue = firstCard.Value;

        switch (firstCard.Value)
        {
            case ECardValue.Reverse:
                gameState.IsReversed = true;
                _engine.NextPlayer();
                SlowWriteLine($"Game is reversed. {_engine.GetCurrentPlayer().Name} starts.");
                break;
            case ECardValue.DrawTwo:
                var cards = _engine.GiveCards(initialPlayer, 2);
                SlowWriteLine(CanRevealCards(initialPlayer)
                    ? $"{initialPlayer.Name} drew {cards[0]} and {cards[1]}"
                    : $"{initialPlayer.Name} drew 2 cards");
                gameState.History.Add($"{initialPlayer.Name} drew 2 cards");
                _engine.NextPlayer();
                break;
            case ECardValue.Skip:
                WriteAndSaveHistory($"{initialPlayer.Name} was skipped");
                _engine.NextPlayer();
                break;
            case ECardValue.Wild:
                SlowWriteLine(CanRevealCards(initialPlayer)
                    ? $"{initialPlayer.Name}'s hand: {initialPlayer.Hand}"
                    : $"{initialPlayer.Name} has {initialPlayer.Hand.Cards.Count} cards");
                gameState.CurrentColor = GetColorFromPlayer(initialPlayer);
                gameState.CurrentValue = null;
                break;
        }
        _storage.SaveGame(gameState);
    }

    public void Run()
    {
        while (true)
        {
            var currentPlayer = _engine.GetCurrentPlayer();
            if (currentPlayer.Type == EPlayerType.Human) GiveGameInfo(currentPlayer);
            else Console.WriteLine($"{currentPlayer} is playing...");

            var card = GetCardFromPlayer(currentPlayer);
            var playerSaidUno = currentPlayer.SaidUno;
            var gameState = _engine.State;

            if (card is not null)
            {
                WriteAndSaveHistory($"{currentPlayer.Name} played {card}");
                gameState.DiscardPile.Add(card);
                gameState.CurrentColor = card.Color;
                gameState.CurrentValue = card.Value;
                ApplyCardEffect(card);
            }
            else
            {
                WriteAndSaveHistory($"{currentPlayer.Name} passed");
            }

            switch (currentPlayer.Hand.Cards.Count)
            {
                case 1 when !playerSaidUno:
                    var cards = _engine.GiveCards(currentPlayer, 2);
                    SaidUno(currentPlayer, false, cards);
                    break;
                case 0:
                    _storage.DeleteGame(gameState.Id);
                    SlowWriteLine($"{currentPlayer.Name} won the game!");
                    return;
            }

            Thread.Sleep(3000);
            Console.Clear();
            _engine.NextPlayer();
            _storage.SaveGame(gameState);
            var newPlayer = _engine.GetCurrentPlayer();

            if (newPlayer != currentPlayer && newPlayer.Type != EPlayerType.Computer)
            {
                SlowWriteLine($"Press enter to play as {newPlayer.Name}");
                while (Console.ReadKey(true).Key != ConsoleKey.Enter)
                {
                }
            }

            Console.Clear();
        }
    }

    private void ApplyCardEffect(Card card)
    {
        var gameState = _engine.State;
        var causer = _engine.GetCurrentPlayer();

        switch (card.Value)
        {
            case ECardValue.Reverse:
                if (gameState.Players.Count > 2)
                {
                    gameState.IsReversed = !gameState.IsReversed;
                    SlowWriteLine($"{causer.Name} reversed the game");
                }
                else
                {
                    _engine.NextPlayer();
                    WriteAndSaveHistory($"{_engine.GetCurrentPlayer().Name} was skipped");
                }
                break;
            case ECardValue.Skip:
                _engine.NextPlayer();
                WriteAndSaveHistory($"{_engine.GetCurrentPlayer().Name} was skipped");
                break;
            case ECardValue.DrawTwo:
                _engine.NextPlayer();
                var victim = _engine.GetCurrentPlayer();
                var cards = _engine.GiveCards(victim, 2);
                SlowWriteLine(CanRevealCards(victim)
                    ? $"{victim.Name} drew {cards[0]} and {cards[1]}"
                    : $"{victim.Name} drew 2 cards");
                gameState.History.Add($"{victim.Name} drew 2 cards");
                break;
            case ECardValue.Wild:
                gameState.CurrentColor = GetColorFromPlayer(causer);
                gameState.CurrentValue = null;
                break;
            case ECardValue.WildDrawFour:
                gameState.CurrentColor = GetColorFromPlayer(causer);
                gameState.CurrentValue = null;
                _engine.NextPlayer();
                victim = _engine.GetCurrentPlayer();
                cards = _engine.GiveCards(victim, 4);
                SlowWriteLine(CanRevealCards(victim)
                    ? $"{victim.Name} drew {cards[0]}, {cards[1]}, {cards[2]} and {cards[3]}"
                    : $"{victim.Name} drew 4 cards");
                gameState.History.Add($"{victim.Name} drew 4 cards");
                break;
        }
    }

    private void GiveGameInfo(Player player)
    {
        var state = _engine.State;

        var separator = new string('=', Console.WindowWidth);
        // SlowWriteLine();
        SlowWriteLine(separator);
        SlowWriteLine($"{player.Name}'s turn");
        SlowWriteLine(separator);
        if (state.Players.Count > 2) SlowWriteLine($"Game is reversed: {state.IsReversed}");
        var deck = state.Deck;
        SlowWriteLine($"Cards left in deck: {deck.Cards.Count}");
        SlowWriteLine($"Current card: {state.DiscardPile[^1]}");
        SlowWriteLine($"Current color: {state.CurrentColor}");
        SlowWriteLine($"Current value: {state.CurrentValue}");
        SlowWriteLine(CanRevealCards(player)
            ? $"{player.Name}'s hand: {player.Hand}"
            : $"{player.Name} has {player.Hand.Cards.Count} cards");
    }

    /**
     * Don't contradict saidUno and drawnCards parameters. If saidUno is false, drawnCards must not be null.
     */
    private void SaidUno(Player player, bool saidUno, IReadOnlyList<Card>? drawnCards = null)
    {
        if (saidUno)
        {
            SlowWriteLine($"{player.Name} said UNO!");
        }
        else
        {
            SlowWriteLine(CanRevealCards(player)
                ? $"{player.Name} didn't say UNO and drew {drawnCards![0]} and {drawnCards[1]} as a penalty"
                : $"{player.Name} didn't say UNO and drew 2 cards as a penalty");

            _engine.State.History.Add($"{player.Name} didn't say UNO and drew 2 cards as a penalty");
        }
    }

    private Card? GetCardFromPlayer(Player player)
    {
        return player.Type == EPlayerType.Human
            ? GetCardFromUser()
            : _engine.GetCardFromBot(player, out _);

        Card? GetCardFromUser()
        {
            player.SaidUno = false;

            while (true)
            {
                var input = GetCardToPlay();
                var cardIndex = Engine.ValidateCardIndex(player, input, 1);

                Card? cardToPlay;
                if (!IsChoiceValid(player, input, cardIndex))
                {
                    if (input.ToLower().Contains("uno"))
                    {
                        RegisterSaidUno(player);
                    }
                    else if (cardIndex < 0)
                    {
                        SlowWriteLine($"Invalid input: {input}");
                    }
                    else if (!_engine.CanPlayCard(player, cardToPlay = player.Hand[cardIndex]))
                    {
                        SlowWriteLine($"Can't play {cardToPlay}");
                    }

                    continue;
                }

                if (input.ToLower() is "draw" or "pass")
                {
                    if (!DrawnCardWasPlayed(player, out cardToPlay)) return null;
                }
                else
                {
                    cardToPlay = player.Hand[cardIndex];
                }

                if (cardToPlay != null) player.Hand.Remove(cardToPlay);
                return cardToPlay;
            }
        }
    }

    public ECardColor GetColorFromPlayer(Player player)
    {
        return player.Type == EPlayerType.Human
            ? GetColorFromUser()
            : Engine.GetColorFromBot(player);

        // SlowWriteLine($"{player.Name} chose {chosenColor} as the new color");

        ECardColor GetColorFromUser()
        {
            var input = ChooseColor();

            ECardColor? chosenColor;
            while (!Engine.IsValidColor(input, out chosenColor))
            {
                SlowWriteLine($"Invalid color: {input}");
                input = ChooseColor();
            }
            SlowWriteLine($"{player.Name} chose {chosenColor} as the new color");

            return chosenColor!.Value;

            string ChooseColor()
            {
                SlowWriteLine("Choose a color:");
                var s = Console.ReadLine()?.ToLower() ?? "";
                return s;
            }
        }
    }

    private bool DrawnCardWasPlayed(Player player, out Card? cardToPlay)
    {
        player.SaidUno = false;
        cardToPlay = _engine.GiveCard(player);

        if (!PlayerDrewCard(player, cardToPlay)) return false;

        if (_engine.CanPlayCard(player, cardToPlay!))
        {
            if (!UserWantsToPlayDrawnCard(player, cardToPlay!)) return false;
        }
        else
        {
            SlowWriteLine($"{cardToPlay} can't be played");
            return false;
        }

        return true;
    }

    private bool PlayerDrewCard(Player player, Card? cardToPlay)
    {
        if (cardToPlay == null)
        {
            SlowWriteLine($"{player.Name} didn't draw any cards, because there are no more cards.");
            return false;
        }

        SlowWriteLine(CanRevealCards(player)
            ? $"{player.Name} drew {cardToPlay}"
            : $"{player.Name} drew a card");
        return true;
    }

    private bool UserWantsToPlayDrawnCard(Player player, Card cardToPlay)
    {
        var playDrawnCard = PlayDrawnCard(cardToPlay).ToLower();

        while (playDrawnCard is not ("yes" or "no"))
        {
            if (playDrawnCard.Contains("uno"))
                RegisterSaidUno(player);
            else
                Console.Write("Invalid input. ");

            playDrawnCard = PlayDrawnCard(cardToPlay).ToLower();
        }

        return playDrawnCard.Equals("yes");
    }

    private static string PlayDrawnCard(Card card)
    {
        SlowWriteLine($"Would you like to play {card}? (y/n)");
        var input = Console.ReadLine() ?? "";
        return input.ToLower() switch
        {
            "y" => "yes",
            "n" => "no",
            _ => input
        };
    }

    private static string GetCardToPlay()
    {
        SlowWriteLine("Which card do you want to play? (Enter index to choose a card, \"draw\"/\"pass\" to draw a card, or \"uno\" to say UNO!)");
        var input = Console.ReadLine();
        return input ?? "";
    }

    private bool CanRevealCards(Player player) =>
        // player.Type == EPlayerType.Human || _engine.State.Players.All(p => p.Type == EPlayerType.Computer);
        true;

    private bool IsChoiceValid(Player player, string input, int cardIndex) =>
        input.ToLower() is "draw" or "pass"
        || (cardIndex >= 0 && _engine.CanPlayCard(player, cardIndex));

    private void RegisterSaidUno(Player player)
    {
        player.SaidUno = true;
        SaidUno(player, true);
    }

    private void WriteAndSaveHistory(string message)
    {
        SlowWriteLine(message);
        _engine.State.History.Add(message);
    }
}