using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SilveringSunRL.Systems;

namespace SilveringSunRL.MapObjects
{
    // based on tunnelling room generation algorithm
    // from RogueSharp tutorial
    // https://roguesharp.wordpress.com/2016/03/26/roguesharp-v3-tutorial-simple-room-generation/
    public class MapGenerator
    {
        public MapGenerator()
        {
        }

        //Hold the map we're building
        DungeonMap _map; 

        public DungeonMap GenerateMap(int mapWidth, int mapHeight, int maxRooms, int minRoomSize, int maxRoomSize)
        {
            // create an empty map of size (mapWidth x mapHeight)
            _map = new DungeonMap(mapWidth, mapHeight);

            // Create a random number generator
            Random randNum = new Random();

            // store a list of the rooms created so far
            List<Rectangle> Rooms = new List<Rectangle>();

            // create up to (maxRooms) rooms on the map
            // and make sure the rooms do not overlap with each other
            for (int i = 0; i < maxRooms; i++)
            {
                // set the room's (width, height) as a random size between (minRoomSize, maxRoomSize)
                int newRoomWidth = randNum.Next(minRoomSize, maxRoomSize);
                int newRoomHeight = randNum.Next(minRoomSize, maxRoomSize);

                // sets the room's X/Y Position at a random point between the edges of the map
                int newRoomX = randNum.Next(0, mapWidth - newRoomWidth - 1);
                int newRoomY = randNum.Next(0, mapHeight - newRoomHeight - 1);

                // create a Rectangle representing the room's perimeter
                Rectangle newRoom = new Rectangle(newRoomX, newRoomY, newRoomWidth, newRoomHeight);

                // Does the new room intersect with other rooms already generated?
                bool newRoomIntersects = Rooms.Any(room => newRoom.Intersects(room));

                if (!newRoomIntersects)
                {
                    Rooms.Add(newRoom);
                }
            }

            // This is a dungeon, so begin by flooding the map with walls.
            FloodWalls();

            // carve out rooms for every room in the Rooms list
            foreach (Rectangle room in Rooms)
            {
                CreateRoom(room);
            }

            // carve out tunnels between all rooms
            // based on the Positions of their centers
            for (int r = 1; r < Rooms.Count; r++)
            {
                //for all remaining rooms get the center of the room and the previous room
                Point previousRoomCenter = Rooms[r - 1].Center;
                Point currentRoomCenter = Rooms[r].Center;

                // give a 50/50 chance of which 'L' shaped connecting hallway to tunnel out
                if (randNum.Next(1, 2) == 1)
                {
                    CreateHorizontalTunnel(previousRoomCenter.X, currentRoomCenter.X, previousRoomCenter.Y);
                    CreateVerticalTunnel(previousRoomCenter.Y, currentRoomCenter.Y, currentRoomCenter.X);
                }
                else
                {
                    CreateVerticalTunnel(previousRoomCenter.Y, currentRoomCenter.Y, previousRoomCenter.X);
                    CreateHorizontalTunnel(previousRoomCenter.X, currentRoomCenter.X, currentRoomCenter.Y);
                }
            }

            // spit out the final map
            return _map;
        }

        //Create a room
        private void CreateRoom(Rectangle room)
        {
            //Generate the floors
            for(int x = room.Left + 1; x < room.Right - 1; x++)
            {
                for(int y = room.Top + 1; y < room.Bottom - 1; y++)
                {
                    CreateFloor(new Point(x, y));
                }
            }

            //Place walls at defined perimeter
            List<Point> perimeter = GetBorderCellLocations(room);
            foreach(Point location in perimeter)
            {
                CreateWall(location);
            }
        }

        //Creates a single floor tile at the specified location
        private void CreateFloor(Point location)
        {
            _map.Tiles[location.ToIndex(_map.Width)] = new Floor();
        }

        //Creates a single wall tile at the specified location
        private void CreateWall(Point location)
        {
            location.X = ClampX(location.X);
            location.Y = ClampY(location.Y);
            _map.Tiles[location.ToIndex(_map.Width)] = new Wall();
        }

        //Fill the map with wall tiles
        private void FloodWalls()
        {
            for(int i = 0; i < _map.Tiles.Length; i++)
            {
                _map.Tiles[i] = new Wall();
            }
        }

        //List out the perimeter points of the room
        private List<Point> GetBorderCellLocations(Rectangle room)
        {
            int xMin = room.Left;
            int xMax = room.Right;
            int yMin = room.Top;
            int yMax = room.Bottom;

            //Construct border list by adding each perimeter wall
            List<Point> borderCells = GetTileLocationsAlongLine(xMin, yMin, xMax, yMin).ToList();
            borderCells.AddRange(GetTileLocationsAlongLine(xMin, yMin, xMin, yMax));
            borderCells.AddRange(GetTileLocationsAlongLine(xMin, yMax, xMax, yMax));
            borderCells.AddRange(GetTileLocationsAlongLine(xMax, yMin, xMax, yMax));

            return borderCells;
        }

        //Gets a list of Points which correspond to a line
        public IEnumerable<Point> GetTileLocationsAlongLine(int xOrigin, int yOrigin, int xDestination, int yDestination)
        {
            //Prevent line from reaching beyond the map
            xOrigin = ClampX(xOrigin);
            yOrigin = ClampY(yOrigin);
            xDestination = ClampX(xDestination);
            yDestination = ClampY(yDestination);

            int dx = Math.Abs(xDestination - xOrigin);
            int dy = Math.Abs(yDestination - yOrigin);

            int sx = xOrigin < xDestination ? 1 : -1;
            int sy = yOrigin < yDestination ? 1 : -1;
            int err = dx - dy;

            while(true)
            {
                yield return new Point(xOrigin, yOrigin);
                if(xOrigin == xDestination && yOrigin == yDestination)
                {
                    break;
                }

                int e2 = 2 * err;
                if(e2 > -dy)
                {
                    err = err - dy;
                    xOrigin = xOrigin + sx;
                }
                if(e2 < dx)
                {
                    err = err + dx;
                    yOrigin = yOrigin + sy;
                }
            }
        }

        // carve a tunnel out of the map parallel to the x-axis
        private void CreateHorizontalTunnel(int xStart, int xEnd, int yPosition)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
            {
                CreateFloor(new Point(x, yPosition));
            }
        }

        // carve a tunnel using the y-axis
        private void CreateVerticalTunnel(int yStart, int yEnd, int xPosition)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
            {
                CreateFloor(new Point(xPosition, y));
            }
        }

        //Clamps a point horizontally
        private int ClampX(int x)
        {
            return (x < 0) ? 0 : (x > _map.Width - 1) ? _map.Width - 1 : x;
        }

        //Clamps a point vertically
        private int ClampY(int y)
        {
            return (y < 0) ? 0 : (y > _map.Height - 1) ? _map.Height - 1 : y;
        }
    }
}
