using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure;
using DAL;
using DAL.Context;
using Domain;
using Domain.Cards;
using Domain.Players;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnoEngine;

namespace WebApp.Pages;

public partial class Game : PageModel
{
    private readonly UnoDbContext _context;

    private IGameStorage _repository;

    public static Dictionary<Guid, Dictionary<string, WebSocket>> ConnectedUsers { get; set; } = new();
    public static Dictionary<string, DateTime> UsersInGracePeriod { get; set; } = new();

    public Engine Engine { get; set; } = default!;
    public GameState? GameState { get; set; }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)] public string Player { get; set; } = default!;

    public string Error { get; set; } = "";

    private const string GameNotFoundError = "Game not found";
    private const string PlayerNotFoundError = "Player {0} not found";
    private const string AlreadyInGameError = "Player {0} is already in the game!";
    private const string PlayerIsComputerError = "Player {0} is a Computer and can't be played as";

    [BindProperty] public Card ChosenCard { get; set; } = default!;
    [BindProperty] public ECardColor ChosenColor { get; set; }
    [BindProperty(SupportsGet = true)] public bool SaidUno { get; set; }


    public Game(UnoDbContext context)
    {
        _context = context;
        _repository = new GameStorageDb(context);
        // _repository = GameStorageJson.Instance;
    }

    public async Task<IActionResult> OnPostDraw()
    {
        LoadState();

        var currentPlayer = Engine.GetCurrentPlayer();
        var drawnCard = Engine.GiveCard(currentPlayer);
        GameState!.History.Add($"{currentPlayer} drew a card");

        if (drawnCard != null && Engine.CanPlayCard(currentPlayer, drawnCard))
        {
            var serializedCard = JsonSerializer.Serialize(drawnCard, JsonHelper.Options);
            HttpContext.Session.SetString("DrawnCard", serializedCard);
        }
        else
        {
            await NextPlayerWrapper($"{currentPlayer} passed");
        }

        if (!TempData.ContainsKey("GameOver")) _repository.SaveGame(GameState!);

        return RedirectToPage(new { Id, Player });
    }

    public async Task<IActionResult> OnPostCard()
    {
        LoadState();
        var currentPlayer = Engine.GetCurrentPlayer();
        Request.Form.TryGetValue("PlayDrawnCard", out var playDrawnCardString);

        bool? playDrawnCard = playDrawnCardString.ToString() switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };

        if (playDrawnCard == false)
        {
            HttpContext.Session.Remove("DrawnCard");
            HttpContext.Session.Remove("ChooseColor");
            await NextPlayerWrapper($"{currentPlayer} passed");
        }
        else if (Engine.CanPlayCard(currentPlayer, ChosenCard))
        {
            if (playDrawnCard == true)
            {
                if (ChosenCard.Color == ECardColor.Wild && HttpContext.Session.GetInt32("ChooseColor") != 1)
                {
                    HttpContext.Session.SetInt32("ChooseColor", 1);
                    return RedirectToPage(new { Id, Player, SaidUno });
                }

                HttpContext.Session.Remove("DrawnCard");
                HttpContext.Session.Remove("ChooseColor");
            }

            GameState!.DiscardPile.Add(ChosenCard);
            currentPlayer.Hand.Remove(ChosenCard);
            GameState.CurrentColor = ChosenCard.Color;
            GameState.CurrentValue = ChosenCard.Value;
            if (ChosenCard.Color == ECardColor.Wild) GameState.CurrentColor = ChosenColor;
            switch (currentPlayer.Hand.Cards.Count)
            {
                case 1 when !SaidUno:
                    TempData["Warning"] = "You didn't say UNO!";
                    GameState.History.Add($"{currentPlayer} didn't say UNO and drew 2 cards as a penalty");
                    Engine.GiveCards(currentPlayer, 2);
                    break;
                case 0:
                    _repository.DeleteGame(Id);
                    TempData["GameOver"] = Player;
                    return RedirectToPage(new { Id, Player });
            }

            await NextPlayerWrapper($"{currentPlayer} played {ChosenCard}", ChosenCard);
        }
        else
        {
            TempData["Warning"] = "Can't play that card!";
        }

        if (!TempData.ContainsKey("GameOver")) _repository.SaveGame(GameState!);

        return RedirectToPage(new { Id, Player });
    }

    public IActionResult OnGet()
    {
        LoadState();

        if (Error != "") return Page();

        if (GameState!.Players.TrueForAll(p => !p.Hand.Cards.Any()))
        {
            Setup();
            _repository.SaveGame(GameState);
        }

        return Page();
    }


    public async void Setup()
    {
        GameState!.Deck.Shuffle();
        Engine.DealCards();
        var initialPlayer = Engine.GetCurrentPlayer();
        var firstCard = GameState.Deck.DrawCard();

        while (firstCard?.Value is ECardValue.WildDrawFour)
        {
            GameState.History.Add($"First card is {firstCard}. Choosing a new card...");
            GameState.Deck.InsertRandomly(firstCard);
            firstCard = GameState.Deck.DrawCard();
        }

        GameState.History.Add($"First card is {firstCard}");
        GameState.DiscardPile.Add(firstCard!);
        GameState.CurrentColor = firstCard!.Color;
        GameState.CurrentValue = firstCard.Value;

        switch (firstCard.Value)
        {
            case ECardValue.Reverse:
                if (GameState.Players.Count > 2)
                {
                    GameState.IsReversed = true;
                }
                else
                {
                    await NextPlayerWrapper();
                }

                break;
            case ECardValue.DrawTwo:
                var cards = Engine.GiveCards(initialPlayer, 2);
                await NextPlayerWrapper($"{initialPlayer.Name} drew 2 cards");
                break;
            case ECardValue.Skip:
                await NextPlayerWrapper($"{initialPlayer.Name} was skipped");
                break;
            case ECardValue.Wild:
                if (initialPlayer.Type == EPlayerType.Human)
                {
                    var serializedCard = JsonSerializer.Serialize(firstCard, JsonHelper.Options);
                    HttpContext.Session.SetString("DrawnCard", serializedCard);
                    HttpContext.Session.SetInt32("ChooseColor", 1);
                }
                else
                {
                    GameState.CurrentColor = Engine.GetColorFromBot(initialPlayer);
                    await NextPlayerWrapper();
                }

                break;
        }
    }


    public void LoadState()
    {
        var state = _repository.LoadGame(Id);

        if (state == null)
        {
            Error = GameNotFoundError;
            return;
        }

        var playerObject = state.Players.FirstOrDefault(p => p.Name == Player);

        if (playerObject == null)
        {
            Error = string.Format(PlayerNotFoundError, Player);
            return;
        }

        if (playerObject.Type == EPlayerType.Computer)
        {
            Error = string.Format(PlayerIsComputerError, Player);
            return;
        }

        lock (ConnectedUsers)
        {
            if (!ConnectedUsers.TryGetValue(Id, out var activePlayers))
            {
                activePlayers = new Dictionary<string, WebSocket>();
                ConnectedUsers[Id] = activePlayers;
            }

            if (activePlayers.ContainsKey(Player) && !UsersInGracePeriod.ContainsKey(Player))
            {
                Console.WriteLine("alr in game");
                Error = string.Format(AlreadyInGameError, Player);
                return;
            }
        }

        Error = "";
        GameState = state;

        Engine = new Engine
        {
            State = GameState
        };
    }


    private void ApplyCardEffect(Card card)
    {
        var causer = Engine.GetCurrentPlayer();
        Player? victim;
        Card[]? cards;

        switch (card.Value)
        {
            case ECardValue.Reverse:
                if (GameState!.Players.Count > 2)
                {
                    GameState.IsReversed = !GameState.IsReversed;
                }
                else
                {
                    Engine.NextPlayer();
                    GameState.History.Add($"{Engine.GetCurrentPlayer()} was skipped");
                }

                break;
            case ECardValue.Skip:
                Engine.NextPlayer();
                GameState!.History.Add($"{Engine.GetCurrentPlayer()} was skipped");
                break;
            case ECardValue.DrawTwo:
                Engine.NextPlayer();
                victim = Engine.GetCurrentPlayer();
                Engine.GiveCards(victim, 2);
                GameState!.History.Add($"{victim} drew 2 cards");
                break;
            case ECardValue.Wild:
                if (causer.Type == EPlayerType.Computer)
                {
                    GameState!.CurrentColor = Engine.GetColorFromBot(causer);
                }

                break;
            case ECardValue.WildDrawFour:
                if (causer.Type == EPlayerType.Computer)
                {
                    GameState!.CurrentColor = Engine.GetColorFromBot(causer);
                }

                Engine.NextPlayer();
                victim = Engine.GetCurrentPlayer();
                Engine.GiveCards(victim, 4);
                GameState!.History.Add($"{victim} drew 4 cards");
                break;
        }
    }


    public async Task NextPlayerWrapper(string playerMove = "", Card? playedCard = null)
    {
        if (playerMove != "") GameState!.History.Add(playerMove);
        if (playedCard != null) ApplyCardEffect(playedCard);

        Engine.NextPlayer();
        var currentPlayer = Engine.GetCurrentPlayer();

        if (currentPlayer.Type == EPlayerType.Human)
        {
            BroadCast(new { type = "NextPlayer" });
        }
        else
        {
            BroadCast(new { type = "NextPlayer" }, true);

            var card = Engine.GetCardFromBot(currentPlayer, out var drewCard);
            if (drewCard)
            {
                GameState!.History.Add($"{currentPlayer} drew a card");
            }

            if (card != null)
            {
                GameState!.DiscardPile.Add(card);
                Engine.State.CurrentColor = card.Color;
                Engine.State.CurrentValue = card.Value;
                if (card.Color == ECardColor.Wild) Engine.State.CurrentColor = Engine.GetColorFromBot(currentPlayer);

                if (currentPlayer.Hand.Cards.Count == 0)
                {
                    _repository.DeleteGame(Id);
                    TempData["GameOver"] = currentPlayer.Name;
                    return;
                }

                await NextPlayerWrapper($"{currentPlayer} played {card}", card);
            }
            else
            {
                await NextPlayerWrapper($"{currentPlayer} passed");
            }
        }
    }

    public void BroadCast(dynamic message, bool includeSelf = false)
    {
        var webSockets = ConnectedUsers[Id];

        foreach (var (player, socket) in webSockets)
        {
            if (player == Player && !includeSelf) continue;
            var segment = MessageToSegment(message);
            socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public HashSet<string> GetHumanPlayerNames()
    {
        return GetPlayerNames(EPlayerType.Human);
    }

    public HashSet<string> GetComputerPlayerNames()
    {
        return GetPlayerNames(EPlayerType.Computer);
    }

    private HashSet<string> GetPlayerNames(EPlayerType type)
    {
        return GameState!.Players
            .Where(p => p.Type == type)
            .Select(p => p.Name)
            .ToHashSet();
    }

    public static ArraySegment<byte> MessageToSegment(dynamic msg)
    {
        string jsonMessage = JsonSerializer.Serialize(msg);
        Console.WriteLine(jsonMessage);
        var data = Encoding.UTF8.GetBytes(jsonMessage);
        var arraySegment = new ArraySegment<byte>(data);
        return arraySegment;
    }

    public static string CardToFilePath(Card card, bool greyVariant = false)
    {
        var color = card.Color.ToString().ToLower();
        string value;

        // Skip has a value of 10, before that are 0-9 cards and they correspond to their indexes
        if (card.Value < ECardValue.Skip)
        {
            value = ((int)card.Value).ToString();
        }
        else
        {
            const string replacement = "-$1";
            value = CapitalLetterRegex().Replace(card.Value.ToString(), replacement)
                .ToLower();
        }

        return greyVariant
            ? $"/images/grey-cards/card-{color}/{value}.png"
            : $"/images/cards/card-{color}/{value}.png";
    }

    [GeneratedRegex("(?<!^)([A-Z])", RegexOptions.Compiled)]
    private static partial Regex CapitalLetterRegex();
}