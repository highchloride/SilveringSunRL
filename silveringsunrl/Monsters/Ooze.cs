using RogueSharp.DiceNotation;
using SilveringSunRL.Commands;
using SilveringSunRL.Core;
using SilveringSunRL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilveringSunRL.Monsters
{
    public class Ooze : Monster
    {
        public static Ooze Create(int level)
        {
            int health = Dice.Roll("4D5");
            return new Ooze
            {
                Attack = Dice.Roll("1D2") + level / 3,
                AttackChance = Dice.Roll("10D5"),
                Awareness = 10,
                Color = Colors.OozeColor,
                Defense = Dice.Roll("1D2") + level / 3,
                DefenseChance = Dice.Roll("10D4"),
                Gold = Dice.Roll("1D20"),
                Health = health,
                MaxHealth = health,
                Name = "Ooze",
                Speed = 14,
                Symbol = 'o'
            };
        }

        public override void PerformAction(CommandManager commandManager)
        {
            var splitOozeBehavior = new SplitOoze();
            if (!splitOozeBehavior.Act(this, commandManager))
            {
                base.PerformAction(commandManager);
            }
        }
    }
}
