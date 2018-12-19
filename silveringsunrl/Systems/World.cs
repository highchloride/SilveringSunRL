using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SilveringSunRL.Entities;
using SilveringSunRL.MapObjects;
using SilveringSunRL.Monsters;

namespace SilveringSunRL.Systems
{
    public class World
    {
        //Map params
        private static int _mapWidth = 100;
        private static int _mapHeight = 100;
        private static int _maxRooms = 500;
        private static int _minRoomSize = 6;
        private static int _maxRoomSize = 15;

        //Map objects
        private TileBase[] _mapTiles;

        public DungeonMap CurrentMap { get; set; }

        //Player
        public Player Player { get; set; }

        //Create a new World object
        public World()
        {
            //Build a map
            CreateMap();

            //Create a new Player
            CreatePlayer();

            //Create some monsters
            CreateMonsters();
        }

        //Create a new map and Gen it
        private void CreateMap()
        {
            _mapTiles = new TileBase[_mapWidth * _mapHeight];
            CurrentMap = new DungeonMap(_mapWidth, _mapHeight);
            MapGenerator mapGen = new MapGenerator();
            CurrentMap = mapGen.GenerateMap(_mapWidth, _mapHeight, _maxRooms, _minRoomSize, _maxRoomSize);
        }

        //Create a new player and place it
        private void CreatePlayer()
        {
            Player = new Player(Color.Yellow, Color.Transparent);
            Player.Position = new Point(5, 5);

            //If the player is in a wall, move it until it isn't
            //while(CurrentMap.Tiles[Player.Position.Y * _mapWidth + Player.Position.X].IsBlockingMove)
            //{
            //    Player.Position = new Point(Player.Position.X + 1, Player.Position.Y + 1);
            //}

            //Add to Entities
            GameLoop.EntityManager.Entities.Add(Player);
        }

        //Create some Monsters and place them
        private void CreateMonsters()
        {
            int numMonsters = 30;

            Random rndNum = new Random();

            //Spawn numMonsters at random points on the map
            for (int i = 0; i < numMonsters; i++)
            {
                int monsterPosition = 0;
                Monster newMonster = new Ooze();
                //If the monster is in a wall, randomly move it until it isn't
                while(CurrentMap.Tiles[monsterPosition].IsBlockingMove)
                {
                    monsterPosition = rndNum.Next(0, CurrentMap.Width * CurrentMap.Height);
                }

                //Set the position of the new monster and add it to the entities
                newMonster.Position = new Point(monsterPosition & CurrentMap.Width, monsterPosition / CurrentMap.Width);
                GameLoop.EntityManager.Entities.Add(newMonster);
            }
        }
    }
}
