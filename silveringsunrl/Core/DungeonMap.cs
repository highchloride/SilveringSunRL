using RogueSharp;
using RLNET;
using System.Collections.Generic;
using System.Linq;

namespace SilveringSunRL.Core
{
    //custom DungeonMap extends the base RogueSharp Map
    public class DungeonMap : Map
    {
        //Rooms for the dungeon
        public List<Rectangle> Rooms;

        //Doors for the dungeon
        public List<Door> Doors { get; set; }

        //Staircases
        public Stairs StairsUp { get; set; }
        public Stairs StairsDown { get; set; }

        //Monsters in the dungeon
        private readonly List<Monster> _monsters;

        //Constructor
        public DungeonMap()
        {
            Game.SchedulingSystem.Clear();

            //Initialize a new list of rooms on new Dungeon Map
            Rooms = new List<Rectangle>();

            //Initialize monsters lists
            _monsters = new List<Monster>();

            //Init door lists
            Doors = new List<Door>();
        }

        //Renders symbols/colors to the map subconsole
        public void Draw(RLConsole mapConsole, RLConsole statConsole)
        {
            //Draw map cells first
            foreach(Cell cell in GetAllCells())
            {
                SetConsoleSymbolForCell(mapConsole, cell);
            }

            //Draw doors next
            foreach (Door door in Doors)
            {
                door.Draw(mapConsole, this);
            }

            //Draw stairs
            StairsUp.Draw(mapConsole, this);
            StairsDown.Draw(mapConsole, this);

            //Draw monsters atop everything
            int i = 0;
            foreach(Monster monster in _monsters)
            {
                monster.Draw(mapConsole, this);
                //When the monster is in the FoV, draw the stats
                if(IsInFov(monster.X, monster.Y))
                {
                    monster.DrawStats(statConsole, i);
                    i++;
                }
            }            
        }

        //Sets the symbol per-cell
        private void SetConsoleSymbolForCell(RLConsole console, Cell cell)
        {
            //Unexplored cells are not drawn
            if (!cell.IsExplored)
            {
                return;
            }

            //if the cell is in the FoV, draw it brighter
            if (IsInFov(cell.X, cell.Y))
            {
                //Set symbol based on tile walkability
                if(cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.FloorFov, Colors.FloorBackgroundFov, '.');
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.WallFov, Colors.WallBackgroundFov, '#');
                }
            }
            //if the cell is NOT in the FoV, draw it darker
            else
            {
                //Set symbol based on tile walkability
                if (cell.IsWalkable)
                {
                    console.Set(cell.X, cell.Y, Colors.Floor, Colors.FloorBackground, '.');
                }
                else
                {
                    console.Set(cell.X, cell.Y, Colors.Wall, Colors.WallBackground, '#');
                }
            }
        }

        //Update the FoV on player movement
        public void UpdatePlayerFieldOfView()
        {
            Player player = Game.Player;
            //ComputeFoV based on player location/awareness
            ComputeFov(player.X, player.Y, player.Awareness, true);
            //Mark cells in FoV as Explored
            foreach(Cell cell in GetAllCells())
            {
                if(IsInFov(cell.X, cell.Y))
                {
                    SetCellProperties(cell.X, cell.Y, cell.IsTransparent, cell.IsWalkable, true);
                }
            }
        }

        //Returns true when the Actor can enter the cell
        public bool SetActorPosition( Actor actor, int x, int y)
        {
            //Only allow if the cell is walkable
            if(GetCell(x,y).IsWalkable)
            {
                //Set previous cell as walkable
                SetIsWalkable(actor.X, actor.Y, true);

                //Try to open a door
                OpenDoor(actor, x, y);

                //Update actor position
                actor.X = x;
                actor.Y = y;

                //New cell is now not walkable (occupied by actor)
                SetIsWalkable(actor.X, actor.Y, false);
                
                //Update FoV as player position is updated
                if(actor is Player)
                {
                    UpdatePlayerFieldOfView();
                }
                return true;
            }            

            return false;
        }

        //Set the provided cell to/not be walkable
        public void SetIsWalkable(int x, int y, bool isWalkable)
        {
            Cell cell = GetCell(x, y) as Cell;
            SetCellProperties(cell.X, cell.Y, cell.IsTransparent, isWalkable, cell.IsExplored);
        }

        //Add the player to a newly created map
        public void AddPlayer(Player player)
        {
            Game.Player = player;
            SetIsWalkable(player.X, player.Y, false);
            UpdatePlayerFieldOfView();
            Game.SchedulingSystem.Add(player);
        }

        //Add a monster to the list
        public void AddMonster(Monster monster)
        {
            _monsters.Add(monster);
            //Set the cell as unwalkable(occupied by monster)
            SetIsWalkable(monster.X, monster.Y, false);
            Game.SchedulingSystem.Add(monster);
        }

        //Remove a monster from the list
        public void RemoveMonster(Monster monster)
        {
            _monsters.Remove(monster);
            //Set the tile walkable once the monster is removed
            SetIsWalkable(monster.X, monster.Y, true);
            Game.SchedulingSystem.Remove(monster);
        }

        //Return the monster at the given cell
        public Monster GetMonsterAt(int x, int y)
        {
            return _monsters.FirstOrDefault(m => m.X == x && m.Y == y);
        }

        //HELPERS
        //Find a random walkable location in the room 
        public Point GetRandomWalkableLocationInRoom(Rectangle room)
        {
            if(DoesRoomHaveWalkableSpace(room))
            {
                for(int i=0; i < 100; i++)
                {
                    int x = Game.Random.Next(1, room.Width - 2) + room.X;
                    int y = Game.Random.Next(1, room.Height - 2) + room.Y;
                    if(IsWalkable(x, y))
                    {
                        return new Point(x, y);
                    }
                }
            }

            //if there's no walkable space, return null
            return Point.Zero;
        }

        //Iterate through each cell in a given room and return true if walkable
        public bool DoesRoomHaveWalkableSpace(Rectangle room)
        {
            for(int x = 1; x <= room.Width - 2; x++)
            {
                for(int y = 1; y <= room.Height -2; y++)
                {
                    if(IsWalkable(x + room.X, y + room.Y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //Return the door at position or null if no door
        public Door GetDoor(int x, int y)
        {
            return Doors.SingleOrDefault(d => d.X == x && d.Y == y);
        }

        //Actor opens door located at position
        private void OpenDoor(Actor actor, int x, int y)
        {
            Door door = GetDoor(x, y);
            if(door != null && !door.IsOpen)
            {
                door.IsOpen = true;
                var cell = GetCell(x, y);

                //Once the door is opened, mark it transparent to not block FoV
                SetCellProperties(x, y, true, cell.IsWalkable, cell.IsExplored);

                Game.MessageLog.Add($"{actor.Name} opened a door.");
            }
        }

        //Check if we can take the stairs down
        public bool CanMoveDownToNextLevel()
        {
            Player player = Game.Player;
            return StairsDown.X == player.X && StairsDown.Y == player.Y;
        }
    }
}
