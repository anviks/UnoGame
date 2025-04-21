using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Azure;
using DAL;
using DAL.Context;
using Domain;
using Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnoEngine;

namespace WebApp.Pages
{
    public class CreateModel : PageModel
    {
        private readonly UnoDbContext _context;
        private readonly IGameRepository _repository;

        public CreateModel(UnoDbContext context)
        {
            _context = context;
            _repository = new GameRepositoryDb(context);
            // _repository = GameStorageJson.Instance;
        }

        [BindProperty]
        [UniquePlayerNames]
        public GameState GameState { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PlayerCount { get; set; } = 2;

        [BindProperty]
        [EnoughCards(nameof(PlayerCount))]
        public bool[] CardsInGame { get; set; } = new bool[54];


        public IActionResult OnGet()
        {
            return PlayerCount switch
            {
                < 2 => RedirectToPage(new { PlayerCount = 2 }),
                > 10 => RedirectToPage(new { PlayerCount = 10 }),
                _ => Page()
            };
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public IActionResult OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var configuration = GetConfig();
            Engine.ApplyConfiguration(GameState, configuration);

            _repository.SaveGame(GameState);

            return RedirectToPage("/Index");
        }

        private GameConfiguration GetConfig()
        {
            return GetConfig(CardsInGame);
        }

        public static GameConfiguration GetConfig(bool[] cardsInGame)
        {
            var config = new GameConfiguration();

            for (int i = 0; i < CardDeck.DefaultCardsDistinct.Count; i++)
            {
                if (!cardsInGame[i])
                {
                    config.DisabledCards.Add(CardDeck.DefaultCardsDistinct.ElementAt(i));
                }
            }

            return config;
        }
    }
}