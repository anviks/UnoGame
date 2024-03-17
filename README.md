# UnoGame

This solution contains the implementation of the Uno game for both the console and the web.
The solution is divided into 7 projects:
- ConsoleApp: The console
- DAL: Data Access Layer, contains the database context and the repository classes
- Domain: Contains the classes like GameState, Player, Card, etc.
- Helpers: Contains the helper classes like AnsiConstants for simple console color formatting and JsonHelper, 
that contains serialization options
- MenuSystem: Contains all the logic for a general navigable menu system for the console
- UnoEngine: Contains the game logic
