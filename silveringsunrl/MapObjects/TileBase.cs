using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadConsole;
using Microsoft.Xna.Framework;

namespace SilveringSunRL.MapObjects
{
    public abstract class TileBase: Cell
    {
        //Props
        public bool IsBlockingMove;
        public bool IsBlockingLOS;

        //Tile name
        protected string Name;

        //Abstract base constructor
        public TileBase(Color foreground, Color background, int glyph, bool blockingMove = false, bool blockingLOS=false, String name="") : base(foreground, background, glyph)
        {
            IsBlockingMove = blockingMove;
            IsBlockingLOS = blockingLOS;
            Name = name;
        }
    }
}
