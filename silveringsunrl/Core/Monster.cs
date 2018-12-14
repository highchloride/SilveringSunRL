using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RLNET;
using SilveringSunRL.Behaviors;
using SilveringSunRL.Systems;

namespace SilveringSunRL.Core
{
    public class Monster : Actor
    {
        //How many turns is the Monster alerted?
        public int? TurnsAlerted { get; set; }

        public virtual void PerformAction(CommandSystem commandSystem)
        {
            var behavior = new StandardMoveAndAttack();
            behavior.Act(this, commandSystem);
        }

        public void DrawStats(RLConsole statConsole, int position)
        {
            //Start at Y=13, below Player stats
            //Multiply by 2 to put a space between each slot
            int yPosition = 13 + (position * 2);

            //Begin each line w/ the monster symbol in the monster's color
            statConsole.Print(1, yPosition, Symbol.ToString(), Color);

            //Determine curHealth from maxHealth
            int width = Convert.ToInt32(( (double)Health / (double)MaxHealth) * 16.0 );
            int remainingWidth = 16 - width;

            //Set health bar colors
            statConsole.SetBackColor(3, yPosition, width, 1, Swatch.Primary);
            statConsole.SetBackColor(3 + width, yPosition, remainingWidth, 1, Swatch.PrimaryDarkest);

            //Print the monster's name
            statConsole.Print(2, yPosition, $": {Name}", Swatch.DbLight);

            

            
        }
    }
}
