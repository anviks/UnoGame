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
        private readonly IGameStorage _storage;

        [BindProperty]
        public GameState? GameState { get; set; }

        public DeleteModel(UnoDbContext context)
        {
            _context = context;
            _storage = new GameStorageDb(context);
            // _storage = GameStorageJson.Instance;
        }

        public IActionResult OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _storage.FetchAllGames().ForEach(g => Console.WriteLine(g.Id));
            var gameState = _storage.LoadGame(id.Value);

            if (gameState == null)
            {
                return NotFound();
            }

            GameState = gameState;

            return Page();
        }

        public IActionResult OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameState = _storage.LoadGame(id.Value);

            if (gameState != null)
            {
                GameState = gameState;
                _storage.DeleteGame(id.Value);
            }

            return RedirectToPage("./Index");
        }
    }
}