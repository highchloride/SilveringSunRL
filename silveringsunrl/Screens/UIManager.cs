using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SadConsole;
using SadConsole.Controls;
using SilveringSunRL.Entities;

namespace SilveringSunRL.Screens
{
    //Manages/makes addressable all consoles in the game
    public class UIManager : ConsoleContainer
    {
        public SadConsole.Console MapConsole;

        public Window MapWindow;

        public MessageLogWindow MessageLog;

        public SplashScreen SplashScreen;

        //Construct the UI Manager
        public UIManager()
        {
            IsVisible = true;
            IsFocused = true;

            Parent = SadConsole.Global.CurrentScreen;
        }

        public void Splash()
        {
            SplashScreen = new SplashScreen();
            SplashScreen.IsVisible = true;

            //Add the screen to the UIManager 
            Children.Add(SplashScreen);
            
            //Connect the INIT method to the action invokd w/ the animation is completed
            SplashScreen.SplashCompleted = Init;
        }

        public void Init()
        {
            //Remove the splash screen
            if(Children.Contains(SplashScreen))
                Children.Remove(SplashScreen);

            //2/3s of the screen
            int twoThirdsWidth = GameLoop.ScreenWidth - GameLoop.ScreenWidth / 3;
            int twoThirdsHeight = GameLoop.ScreenHeight - GameLoop.ScreenHeight / 3;

            //Create the map console and window
            CreateConsoles();
            CreateMapWindow(twoThirdsWidth, twoThirdsHeight, "");

            //Create the Message log window and scrollbar
            MessageLog = new MessageLogWindow(GameLoop.ScreenWidth, GameLoop.ScreenHeight / 3, "Messages");
            Children.Add(MessageLog);
            MessageLog.Show();
            MessageLog.Position = new Point(0, twoThirdsHeight);
        }

        //One-stop shop to create all subconsoles
        public void CreateConsoles()
        {
            MapConsole = new SadConsole.Console(GameLoop.World.CurrentMap.Width, GameLoop.World.CurrentMap.Height, Global.FontDefault, new Rectangle(0, 0, GameLoop.ScreenWidth, GameLoop.ScreenHeight), GameLoop.World.CurrentMap.Tiles);

            //Parent.Children.Add(MapConsole);

            MapConsole.Children.Add(GameLoop.EntityManager);            
        }

        //Creates a window that encloses the map console
        public void CreateMapWindow(int width, int height, string title)
        {
            MapWindow = new Window(width, height);
            //MapWindow.Dragable = true;            

            //Pad the window from the screen edges
            int mapConsoleWidth = width - 2;
            int mapConsoleHeight = height - 2;
            MapConsole.Position = new Point(1, 1);

            //Resize the viewport to fit the window
            MapConsole.ViewPort = new Rectangle(0, 0, mapConsoleWidth, mapConsoleHeight);

            //Add a button to close the window
            //Button closeButton = new Button(3, 1);
            //closeButton.Position = new Point(0, 0);
            //closeButton.Text = "[X]";
            //MapWindow.Add(closeButton);

            //Center the title's text
            MapWindow.Title = title.Align(HorizontalAlignment.Center, mapConsoleWidth);

            //Add the map viewer to the console
            MapWindow.Children.Add(MapConsole);

            //Add that to the UIManager
            Children.Add(MapWindow);

            //Display the mapwindow
            MapWindow.Show();
        }

        //Center the viewport on the actor
        public void CenterOnActor(Actor actor)
        {
            MapConsole.CenterViewPortOnPoint(actor.Position);
        }

        //Override update to check keystrokes
        public override void Update(TimeSpan timeElapsed)
        {
            CheckKeyboard();
            base.Update(timeElapsed);
        }

        //Handle Keyboard input
        private void CheckKeyboard()
        {
            //Move up
            if (SadConsole.Global.KeyboardState.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, -1));
                CenterOnActor(GameLoop.World.Player);
            }
            //Move down
            if (SadConsole.Global.KeyboardState.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(0, 1));
                CenterOnActor(GameLoop.World.Player);
            }
            //Move left
            if (SadConsole.Global.KeyboardState.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Left))
            {
                GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(-1, 0));
                CenterOnActor(GameLoop.World.Player);
            }
            //Move right
            if (SadConsole.Global.KeyboardState.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Right))
            {
                GameLoop.CommandManager.MoveActorBy(GameLoop.World.Player, new Point(1, 0));
                CenterOnActor(GameLoop.World.Player);
            }
            //Exit game on Escape
            if (SadConsole.Global.KeyboardState.IsKeyReleased(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                SadConsole.Game.Instance.Exit();
            }
        }
    }
}
