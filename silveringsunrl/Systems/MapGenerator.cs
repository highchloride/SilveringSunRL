using RogueSharp;
using RLNET;
using SilveringSunRL.Core;
using System.Linq;
using System;
using RogueSharp.DiceNotation;
using SilveringSunRL.Monsters;
using System.Collections.Generic;

namespace SilveringSunRL.Systems
{
    public class MapGenerator
    {
        //Sizes
        private readonly int _width;
        private readonly int _height;
        private readonly int _maxRooms;
        private readonly int _roomMaxSize;
        private readonly int _roomMinSize;

        //Map var
        private readonly DungeonMap _map;

        //Constructor
        public MapGenerator(int width, int height, int maxRooms, int roomMaxSize, int roomMinSize, int mapLevel)
        {
            _width = width;
            _height = height;
            _maxRooms = maxRooms;
            _roomMaxSize = roomMaxSize;
            _roomMinSize = roomMinSize;
            _map = new DungeonMap();
        }

        //Generate a new random map
        public DungeonMap CreateMap()
        {
            //Initialize the map by setting all to false
            _map.Initialize(_width, _height);

            //Try to place as many rooms based on maxRooms
            for(int r = _maxRooms; r > 0; r--)
            {
                //Randomly roll the size/pos of the room
                int roomWidth = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomHeight = Game.Random.Next(_roomMinSize, _roomMaxSize);
                int roomXPosition = Game.Random.Next(0, _width - roomWidth - 1);
                int roomYPosition = Game.Random.Next(0, _height - roomHeight - 1);

                //Define the room as a rectangle to fit in the list
                var newRoom = new Rectangle(roomXPosition, roomYPosition, roomWidth, roomHeight);

                //Ensure room does not intersect w/ existing rooms
                bool newRoomIntersects = _map.Rooms.Any(room => newRoom.Intersects(room));

                //If the room does not intersect, add it
                if(!newRoomIntersects)
                {
                    _map.Rooms.Add(newRoom);
                }
            }

            //Iterate through each generated room and randomly add a tunnel
            for(int r = 1; r < _map.Rooms.Count; r++)
            {
                //Get the center of each room and the previous room
                int previousRoomCenterX = _map.Rooms[r - 1].Center.X;
                int previousRoomCenterY = _map.Rooms[r - 1].Center.Y;
                int currentRoomCenterX = _map.Rooms[r].Center.X;
                int currentRoomCenterY = _map.Rooms[r].Center.Y;

                //50/50 chance of adding a hallway
                if(Game.Random.Next(1,2) == 1)
                {
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, previousRoomCenterY);
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, currentRoomCenterX);
                }
                else
                {
                    CreateVerticalTunnel(previousRoomCenterY, currentRoomCenterY, previousRoomCenterX);
                    CreateHorizontalTunnel(previousRoomCenterX, currentRoomCenterX, currentRoomCenterY);
                }
            }

            //Iterate through each verified room and call CreateRoom to draw it
            //Then, draw the doors!
            foreach (Rectangle room in _map.Rooms)
            {
                CreateRoom(room);
                CreateDoors(room);
            }

            //Add the staircases
            CreateStairs();

            //Add the player to the first created room
            PlacePlayer();

            //Add monsters to the map
            PlaceMonsters();

            return _map;
        }

        //Set the cell properties true for the rectangular area of each room
        private void CreateRoom(Rectangle room)
        {
            for(int x = room.Left + 1; x < room.Right; x++)
            {
                for(int y = room.Top + 1; y < room.Bottom; y++)
                {
                    _map.SetCellProperties(x, y, true, true, true);
                }
            }
        }

        //Create some doors for the dungeon
        private void CreateDoors(Rectangle room)
        {
            //Define the room's boundaries
            int xMin = room.Left;
            int xMax = room.Right;
            int yMin = room.Top;
            int yMax = room.Bottom;

            //List out the room's border cells
            List<ICell> borderCells = _map.GetCellsAlongLine(xMin, yMin, xMax, yMin).ToList();
            borderCells.AddRange(_map.GetCellsAlongLine(xMin, yMin, xMin, yMax));
            borderCells.AddRange(_map.GetCellsAlongLine(xMin, yMax, xMax, yMax));
            borderCells.AddRange(_map.GetCellsAlongLine(xMax, yMin, xMax, yMax));

            //Iterate through the border cells to find a suitable door location
            foreach(Cell cell in borderCells)
            {
                if(IsPotentialDoor(cell))
                {
                    //Set the door to block FoV while closed
                    _map.SetCellProperties(cell.X, cell.Y, false, true);
                    _map.Doors.Add(new Door { X = cell.X, Y = cell.Y, IsOpen = false });
                }
            }
        }

        //Find the center of the first room and stick the Player in it
        private void PlacePlayer()
        {
            Player player = Game.Player;
            if(player == null)
            {
                player = new Player();
            }

            player.X = _map.Rooms[0].Center.X;
            player.Y = _map.Rooms[0].Center.Y;

            _map.AddPlayer(player);
        }

        //Place Monsters in the dungeon
        private void PlaceMonsters()
        {
            foreach(var room in _map.Rooms)
            {
                //Each room has a 60% chance to spawn a monster
                if(Dice.Roll("1D10") < 7)
                {
                    //Generate 1-4 monsters
                    var numberOfMonsters = Dice.Roll("1D4");
                    for(int i = 0; i < numberOfMonsters; i++)
                    {
                        //Find a random walkable location and place the monster
                        Point randomRoomLocation = _map.GetRandomWalkableLocationInRoom(room);

                        //If there's no space for the monster, skip it
                        if(randomRoomLocation != Point.Zero)
                        {
                            //Temporarily hardcoded at level 1
                            var monster = Kobold.Create(1);
                            monster.X = randomRoomLocation.X;
                            monster.Y = randomRoomLocation.Y;
                            _map.AddMonster(monster);
                        }
                    }
                }
            }
        }

        //HELPERS
        //Carve a tunnel parallel to the x-axis
        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for(int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                _map.SetCellProperties(x, yPosition, true, true);
            }
        }

        //Carve a tunnel parallel to the y-axis
        private void CreateVerticalTunnel(int yStart, int yEnd, int xPosition)
        {
            for(int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                _map.SetCellProperties(xPosition, y, true, true);
            }
        }

        //Create staircases
        private void CreateStairs()
        {
            _map.StairsUp = new Stairs
            {
                X = _map.Rooms.First().Center.X + 1,
                Y = _map.Rooms.First().Center.Y,
                IsUp = true
            };
            _map.StairsDown = new Stairs
            {
                X = _map.Rooms.Last().Center.X,
                Y = _map.Rooms.Last().Center.Y,
                IsUp = false
            };
        }

        //Check if this is a good place for a door
        private bool IsPotentialDoor(Cell cell)
        {
            //If the cell is not walkable, it's a wall, so don't put a door there
            if(!cell.IsWalkable)
            {
                return false;
            }

            //Store neighboring cell references
            Cell right = _map.GetCell(cell.X + 1, cell.Y) as Cell;
            Cell left = _map.GetCell(cell.X - 1, cell.Y) as Cell;
            Cell top = _map.GetCell(cell.X, cell.Y - 1) as Cell;
            Cell bottom = _map.GetCell(cell.X, cell.Y + 1) as Cell;

            //Ensure there isn't already a door here
            if (_map.GetDoor(cell.X, cell.Y) != null || _map.GetDoor(right.X, right.Y) != null || _map.GetDoor(left.X, left.Y) != null ||
                _map.GetDoor(top.X, top.Y) != null || _map.GetDoor(bottom.X, bottom.Y) != null)
            {
                return false;
            }

            //Mark this a suitable place for a door on either the left or right side of the room
            if(right.IsWalkable && left.IsWalkable && !top.IsWalkable && !bottom.IsWalkable)
            {
                return true;
            }

            //Mark this a suitable place fora  door on the top or bottom
            if(!right.IsWalkable && !left.IsWalkable && top.IsWalkable && bottom.IsWalkable)
            {
                return true;
            }

            return false;
        }
    }
}
