using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using Console = SadConsole.Console;
using SilveringSunRL.Core;

namespace SilveringSunRL.MapObjects
{
    public class Stairs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsUp { get; set; }

        public void Draw(Console console, IMap map)
        {
            if (!map.GetCell(X, Y).IsExplored)
            {
                return;
            }

            if (map.IsInFov(X, Y))
            {
                if (IsUp)
                {
                    //console.CellData.SetCharacter(X, Y, '<', Colors.Player);

                    int tilePos = Y * GameLoop.World.CurrentMap.Width + X;
                    GameLoop.World.CurrentMap.Tiles[tilePos].Glyph = '<';
                    GameLoop.World.CurrentMap.Tiles[tilePos].Foreground = Colors.Player;
                }
                else
                {
                    //console.CellData.SetCharacter(X, Y, '>', Colors.Player);

                    int tilePos = Y * GameLoop.World.CurrentMap.Width + X;
                    GameLoop.World.CurrentMap.Tiles[tilePos].Glyph = '>';
                    GameLoop.World.CurrentMap.Tiles[tilePos].Foreground = Colors.Player;
                }
            }
            else
            {
                if (IsUp)
                {
                    //console.CellData.SetCharacter(X, Y, '<', Colors.Floor);

                    int tilePos = Y * GameLoop.World.CurrentMap.Width + X;
                    GameLoop.World.CurrentMap.Tiles[tilePos].Glyph = '<';
                    GameLoop.World.CurrentMap.Tiles[tilePos].Foreground = Colors.Floor;
                }
                else
                {
                    //console.CellData.SetCharacter(X, Y, '>', Colors.Floor);

                    int tilePos = Y * GameLoop.World.CurrentMap.Width + X;
                    GameLoop.World.CurrentMap.Tiles[tilePos].Glyph = '>';
                    GameLoop.World.CurrentMap.Tiles[tilePos].Foreground = Colors.Floor;
                }
            }
        }
    }
}
