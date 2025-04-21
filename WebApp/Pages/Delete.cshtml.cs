using DAL;
using DAL.Context;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly UnoDbContext _context;
        private readonly IGameRepository _repository;

        [BindProperty]
        public GameState? GameState { get; set; }

        public DeleteModel(UnoDbContext context)
        {
            _context = context;
            _repository = new GameRepositoryDb(context);
            // _storage = GameStorageJson.Instance;
        }

        public IActionResult OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _repository.FetchAllGames().ForEach(g => Console.WriteLine(g.Id));
            var gameState = _repository.LoadGame(id.Value);

            if (gameState == null)
            {
                return NotFound();
            }

            GameState = gameState;

            return Page();
        }

        public IActionResult OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameState = _repository.LoadGame(id.Value);

            if (gameState != null)
            {
                GameState = gameState;
                _repository.DeleteGame(id.Value);
            }

            return RedirectToPage("./Index");
        }
    }
}