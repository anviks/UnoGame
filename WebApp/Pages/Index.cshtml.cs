using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using DAL.Context;
using Domain;
using Domain.Players;
using Helpers;
using UnoEngine;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UnoDbContext _context;
        private IGameRepository _repository;

        public IndexModel(UnoDbContext context)
        {
            _context = context;
            _repository = new GameRepositoryDb(context);
            // _repository = GameStorageJson.Instance;
        }

        public IList<GameState> GameStates { get; set; } = default!;

        public IActionResult OnGet()
        {
            GameStates = _repository.FetchAllGames();

            return Page();
        }

        public static string GetPlayerLinkStyle(int gameId, Player player)
        {
            if (player.Type != EPlayerType.Human) return "player-link active-player";

            bool isPlayerInGracePeriod = false;

            lock (Game.UsersInGracePeriod)
            {
                if (Game.UsersInGracePeriod.ContainsKey(player.Name))
                {
                    isPlayerInGracePeriod = true;
                }
            }

            lock (Game.ConnectedUsers)
            {
                Game.ConnectedUsers.TryGetValue(gameId, out var activePlayers);

                if (activePlayers == null
                    || !activePlayers.ContainsKey(player.Name)
                    || isPlayerInGracePeriod)
                {
                    return "player-link";
                }
            }

            return "player-link active-player";
        }
    }
}
