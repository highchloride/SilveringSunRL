using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RogueSharp;
using Console = SadConsole.Console;

namespace SilveringSunRL.Interfaces
{
    public interface IDrawable
    {
        Color Color { get; set; }
        char Symbol { get; set; }
        int X { get; set; }
        int Y { get; set; }

        void Draw(Console console, IMap map);
    }
}
