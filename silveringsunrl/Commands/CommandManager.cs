using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SilveringSunRL.Entities;
using RogueSharp.DiceNotation;
using RogueSharp;

namespace SilveringSunRL.Commands
{
    //Contains all generic action handlers
    public class CommandManager
    {
        public CommandManager()
        {
        }

        //Command to move the actor by the given Point
        public bool MoveActorBy(Actor actor, Microsoft.Xna.Framework.Point position)
        {
            return actor.MoveBy(position);
        }

        //Execute combat from Attacking actor to Defending actor
        public void Attack(Actor attacker, Actor defender)
        {
            //Create two messages for the outcomes of attack and defense
            StringBuilder attackMessage = new StringBuilder();
            StringBuilder defenseMessage = new StringBuilder();

            //Total out the successful hits and blocks
            int hits = ResolveAttack(attacker, defender, attackMessage);
            int blocks = ResolveDefense(defender, hits, attackMessage, defenseMessage);

            //Add the messages to the MessageLog queue
            GameLoop.UIManager.MessageLog.Add(attackMessage.ToString());
            if(!string.IsNullOrWhiteSpace(defenseMessage.ToString()))
            {
                GameLoop.UIManager.MessageLog.Add(defenseMessage.ToString());
            }

            //Calculate damage
            int damage = hits - blocks;
            ResolveDamage(defender, damage);
        }

        //Calculate the attack chance against d100
        private static int ResolveAttack(Actor attacker, Actor defender, StringBuilder attackMessage)
        {
            //Create the attack message
            int hits = 0;
            attackMessage.AppendFormat("{0} attacks {1}, ", attacker.Name, defender.Name);

            //Roll 1d100 per attacker's Attack value
            for(int dice = 0; dice < attacker.Attack; dice++)
            {
                //Roll a d100
                int diceOutcome = Dice.Roll("1d100");

                //If the outcome beats the AttackChance, register the hit
                if (diceOutcome >= 100 - attacker.AttackChance)
                    hits++;
            }

            return hits;
        }

        //Calculate the defense chance against d100
        private static int ResolveDefense(Actor defender, int hits, StringBuilder attackMessage, StringBuilder defenseMessage)
        {
            int blocks = 0;

            if (hits > 0)
            {
                //Create the attack and defense messages
                attackMessage.AppendFormat("scoring {0} hits.", hits);
                defenseMessage.AppendFormat("{0} defends, ", defender.Name);

                //Roll 1d100 per defender's Defense value
                for(int dice = 0; dice < defender.Defense; dice++)
                {
                    //Roll a d100
                    int diceOutcome = Dice.Roll("1d100");

                    //If the outcome beats the DefenseChance, register a block
                    if (diceOutcome >= 100 - defender.DefenseChance)
                        blocks++;
                }
                defenseMessage.AppendFormat("resulting in {0} blocks.", blocks);   
            }
            else
            {
                attackMessage.Append("and misses completely!");
            }

            return blocks;
        }

        //Calculate and subtract damage from the defender, and report it
        private static void ResolveDamage(Actor defender, int damage)
        {
            if(damage > 0)
            {
                defender.Health = defender.Health - damage;
                GameLoop.UIManager.MessageLog.Add($"{defender.Name} was hit for {damage} damage");
                if(defender.Health <= 0)
                {
                    ResolveDeath(defender);
                }
            }
            else
            {
                GameLoop.UIManager.MessageLog.Add($"{defender.Name} blocked all damage!");
            }
        }

        //Remove a dead actor and report it
        private static void ResolveDeath(Actor defender)
        {
            GameLoop.EntityManager.Entities.Remove(defender);

            if(defender is Player)
            {
                GameLoop.UIManager.MessageLog.Add($" {defender.Name} was killed.");
            }
            else if (defender is Monster)
            {
                GameLoop.UIManager.MessageLog.Add($"{defender.Name} died and dropped {defender.Gold} gold coins.");
            }
        }

        //Move a monster to the given location
        public void MoveMonster(Monster monster, Cell cell)
        {
            if (!monster.MoveTo(new Microsoft.Xna.Framework.Point(cell.X, cell.Y)))
            {
                if (GameLoop.World.Player.X == cell.X && GameLoop.World.Player.Y == cell.Y)
                {
                    Attack(monster, GameLoop.World.Player);
                }
            }
        }
    }
}
