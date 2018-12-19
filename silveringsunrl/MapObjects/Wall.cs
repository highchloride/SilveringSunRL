using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SilveringSunRL.MapObjects
{
    class Wall : TileBase
    {
        //Construct wall and block everything
        public Wall(bool blocksMovement=true, bool blocksLOS=true) : base(Color.LightGray, Color.Black, '#', blocksMovement, blocksLOS)
        {
            Name = "Wall";
        }
    }
}
