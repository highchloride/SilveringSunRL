using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SilveringSunRL.MapObjects
{
    class Floor : TileBase
    {
        public Floor(bool blocksMovement=false, bool blocksLOS = false) : base(Color.DarkGray, Color.Black, '.', blocksMovement, blocksLOS)
        {
            Name = "Floor";
        }
    }
}
