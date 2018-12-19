using System;
using SadConsole;
using Console = SadConsole.Console;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SilveringSunRL.Entities;
using SilveringSunRL.MapObjects;
using SilveringSunRL.Systems;
using SilveringSunRL.Screens;
using SilveringSunRL.Commands;

namespace SilveringSunRL
{
    class GameLoop
    {
        //Screen params
        public const int ScreenWidth = 80;
        public const int ScreenHeight = 60;

        //Managers
        public static EntityManager EntityManager;
        public static UIManager UIManager;
        public static CommandManager CommandManager;
        public static SchedulingSystem SchedulingSystem;


        public static World World;

        //MAIN INSERT
        static void Main(string[] args)
        {
            // Setup the engine and creat the main window.
            SadConsole.Game.Create("Cheepicus12.font", ScreenWidth, ScreenHeight);

            // Hook the start event so we can add consoles to the system.
            SadConsole.Game.OnInitialize = Init;

            // Hook the update event that happens each frame so we can trap keys and respond.
            SadConsole.Game.OnUpdate = Update;
                        
            // Start the game.
            SadConsole.Game.Instance.Run();

            //
            // Code here will not run until the game window closes.
            //
            
            //Final cleanup on close
            SadConsole.Game.Instance.Dispose();
        }
        
        private static void Update(GameTime time)
        {

        }

        private static void Init()
        {
            //Inst the EntityManager
            EntityManager = new EntityManager();

            //Inst the CommandManager
            CommandManager = new CommandManager();

            //Inst the UI manager
            UIManager = new UIManager();

            //Inst the Scheduling System
            SchedulingSystem = new SchedulingSystem();

            //Inst the World
            World = new World();

            //Pop the splash screen
            //UIManager.Splash();

            //Init the UIManager
            UIManager.Init();
        }
    }
}
