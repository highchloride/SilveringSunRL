using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using SilveringSunRL.MapObjects;
using Microsoft.Xna.Framework;


namespace SilveringSunRL.Systems
{
    public class Map : RogueSharp.Map
    {
        //Set params
        TileBase[] _tiles;
        private int _width;
        private int _height;

        public TileBase[] Tiles { get { return _tiles; } set { _tiles = value; } }
        public int Width { get { return _width; } set { _width = value; } }
        public int Height { get { return _height; } set { _height = value; } }

        //Construct a new map
        public Map(int width, int height)
        {
            _width = width;
            _height = height;

            //Create the tiles
            _tiles = new TileBase[width * height];
        }

        //Check for Walkable
        public bool IsTileWalkable(Point location)
        {
            if (location.X < 0 || location.Y < 0 || location.X >= Width || location.Y >= Height)
            {
                return false;
            }

            return !_tiles[location.Y * Width + location.X].IsBlockingMove;
        }

        //Set walkable to true/false value of walkable
        public void SetIsWalkable(int tileX, int tileY, bool walkable)
        {
            _tiles[tileY * Width + tileX].IsBlockingMove = walkable;
        }
    }
}
