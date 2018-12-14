using RLNET;
using RogueSharp.Random;
using SilveringSunRL.Core;
using SilveringSunRL.Systems;
using System;

namespace SilveringSunRL
{
    public class Game
    {
        //Screen height and width in tiles
        private static readonly int _screenWidth = 100;
        private static readonly int _screenHeight = 70;
        private static RLRootConsole _rootConsole;

        //Map console 
        private static readonly int _mapWidth = 80;
        private static readonly int _mapHeight = 48;
        private static RLConsole _mapConsole;

        //Message console
        private static readonly int _messageWidth = 80;
        private static readonly int _messageHeight = 11;
        private static RLConsole _messageConsole;

        //Stat console
        private static readonly int _statWidth = 20;
        private static readonly int _statHeight = 70;
        private static RLConsole _statConsole;

        //Inventory console
        private static readonly int _inventoryWidth = 80;
        private static readonly int _inventoryHeight = 11;
        private static RLConsole _inventoryConsole;

        //Player
        public static Player Player { get; set; }

        //Maps
        public static DungeonMap DungeonMap { get; private set; }

        //Map Level
        private static int _mapLevel = 1;

        //Command System
        public static CommandSystem CommandSystem { get; private set; }

        //Message Log
        public static MessageLog MessageLog { get; private set; }

        //Scheduling system
        public static SchedulingSystem SchedulingSystem { get; private set; }

        //Prevent console update except when needed with this property
        private static bool _renderRequired = true;

        //Singleton of IRandom to generate random numbers
        public static IRandom Random { get; private set; }

        //MAIN INSERT
        public static void Main()
        {
            //Must be the exact name of the bitmap font file
            string fontFileName = "terminal8x8.png";

            //Generate a new seed on startup 
            int seed = (int)DateTime.UtcNow.Ticks;
            Random = new DotNetRandom(seed);

            //Console window title
            string consoleTitle = $"The Silvering Sun v0.1 - Level {_mapLevel} - Seed {seed}";

            //Assign font and tile sizes and spawn root console
            _rootConsole = new RLRootConsole(fontFileName, _screenWidth, _screenHeight, 8, 8, 1f, consoleTitle);

            //Spawn subconsoles
            _mapConsole = new RLConsole(_mapWidth, _mapHeight);
            _messageConsole = new RLConsole(_messageWidth, _messageHeight);
            _statConsole = new RLConsole(_statWidth, _statHeight);
            _inventoryConsole = new RLConsole(_inventoryWidth, _inventoryHeight);

            //OnUpdate Handler
            _rootConsole.Update += OnRootConsoleUpdate;

            //OnRender Handler
            _rootConsole.Render += OnRootConsoleRender;

            //Instantiate the Command System
            CommandSystem = new CommandSystem();

            //Instantiate the scheduling system
            SchedulingSystem = new SchedulingSystem();

            //Generate the map
            MapGenerator mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 20, 13, 7, _mapLevel);
            DungeonMap = mapGenerator.CreateMap();

            //Setup the player FoV
            DungeonMap.UpdatePlayerFieldOfView();

            //Set background colors for each subconsole
            _messageConsole.SetBackColor(0, 0, _messageWidth, _messageHeight, Swatch.DbDeepWater);

            _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Swatch.DbOldStone);

            _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Swatch.DbWood);

            //Create new Messagelog and add info
            MessageLog = new MessageLog();
            MessageLog.Add(Player.Name + " has arrived on level 1.");
            MessageLog.Add($"Level created with seed '{seed}'");

            //Begin Loop
            _rootConsole.Run();
        }

        //Event handler for RLNET's Update event
        private static void OnRootConsoleUpdate (object sener, UpdateEventArgs e)
        {
            //handle Keypressess
            bool didPlayerAct = false;
            RLKeyPress keyPress = _rootConsole.Keyboard.GetKeyPress();

            if (CommandSystem.IsPlayerTurn)
            {
                if (keyPress != null)
                {
                    if (keyPress.Key == RLKey.Up)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Up);
                    }
                    else if (keyPress.Key == RLKey.Down)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Down);
                    }
                    else if (keyPress.Key == RLKey.Left)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Left);
                    }
                    else if (keyPress.Key == RLKey.Right)
                    {
                        didPlayerAct = CommandSystem.MovePlayer(Direction.Right);
                    }
                    else if (keyPress.Key == RLKey.Period)
                    {
                        if(DungeonMap.CanMoveDownToNextLevel())
                        {
                            MapGenerator mapGenerator = new MapGenerator(_mapWidth, _mapHeight, 20, 13, 7, ++_mapLevel);
                            DungeonMap = mapGenerator.CreateMap();
                            MessageLog = new MessageLog();
                            CommandSystem = new CommandSystem();
                            _rootConsole.Title = $"The Silvering Sun RL v0.1 - Level {_mapLevel}";
                            didPlayerAct = true;
                        }
                    }
                    else if (keyPress.Key == RLKey.Escape)
                    {
                        _rootConsole.Close();
                    }
                }

                if (didPlayerAct)
                {
                    _renderRequired = true;
                    CommandSystem.EndPlayerTurn();
                }
            }            
            else
            {
                CommandSystem.ActivateMonsters();
                _renderRequired = true;
            }
        }

        //Event handler for RLNET's Render event
        private static void OnRootConsoleRender(object sender, UpdateEventArgs e)
        {
            if (_renderRequired)
            {
                _mapConsole.Clear();
                _statConsole.Clear();
                _messageConsole.Clear();

                //Draw the dungeon map in the console
                DungeonMap.Draw(_mapConsole, _statConsole);

                //Draw the Message Log
                MessageLog.Draw(_messageConsole);

                //Render Player FoV
                Player.Draw(_mapConsole, DungeonMap);

                //Draw the Player's Stats
                Player.DrawStats(_statConsole);

                //Set background colors for each subconsole
                _messageConsole.SetBackColor(0, 0, _messageWidth, _messageHeight, Swatch.DbDeepWater);

                _statConsole.SetBackColor(0, 0, _statWidth, _statHeight, Swatch.DbOldStone);

                _inventoryConsole.SetBackColor(0, 0, _inventoryWidth, _inventoryHeight, Swatch.DbWood);

                //Blit Subconsoles
                RLConsole.Blit(_mapConsole, 0, 0, _mapWidth, _mapHeight, _rootConsole, 0, _inventoryHeight);
                RLConsole.Blit(_statConsole, 0, 0, _statWidth, _statHeight, _rootConsole, _mapWidth, 0);
                RLConsole.Blit(_messageConsole, 0, 0, _messageWidth, _messageHeight, _rootConsole, 0, _screenHeight - _messageHeight);
                RLConsole.Blit(_inventoryConsole, 0, 0, _inventoryWidth, _inventoryHeight, _rootConsole, 0, 0);

                //Tell RLNET to draw the root console
                _rootConsole.Draw();

                _renderRequired = false;
            }
        }
    }
}
