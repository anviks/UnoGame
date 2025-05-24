using Domain;

namespace UnoEngine;

public class Engine
{
    public Game State { get; set; } = default!;
    public const int InitialHandSize = 7;

    public void ApplyConfiguration(GameConfiguration configuration)
    {
        ApplyConfiguration(State, configuration);
    }

    public static void ApplyConfiguration(Game state, GameConfiguration configuration)
    {
        state.Deck.Cards.RemoveAll(card => configuration.DisabledCards.Contains(card));
        if (!IsValidConfiguration(configuration, state.Players.Count))
            state.Deck.Cards = CardDeck.DefaultCards.ToList();
    }

    public static bool IsValidConfiguration(GameConfiguration configuration, int playerCount)
    {
        // If there are not enough cards in the deck to start the game
        // (InitialHandSize cards for each player + 1 card to the discard pile),
        // the configuration is invalid
        return CardDeck.DefaultCards.Count(card => !configuration.DisabledCards.Contains(card)) >=
               playerCount * InitialHandSize + 1;
    }
}