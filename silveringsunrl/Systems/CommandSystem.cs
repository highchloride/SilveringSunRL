using RogueSharp;
using RogueSharp.DiceNotation;
using SilveringSunRL.Core;
using SilveringSunRL.Interfaces;
using System.Text;

namespace SilveringSunRL.Systems
{
    public class CommandSystem
    {
        //Tracking the player turn
        public bool IsPlayerTurn { get; set; }

        //End the player's turn
        public void EndPlayerTurn()
        {
            IsPlayerTurn = false;
        }

        //Returns true if player moved, false if player could not move
        public bool MovePlayer(Direction direction)
        {
            int x = Game.Player.X;
            int y = Game.Player.Y;

            switch(direction)
            {
                case Direction.Up:
                    {
                        y = Game.Player.Y - 1;
                        break;
                    }
                case Direction.Down:
                    {
                        y = Game.Player.Y + 1;
                        break;
                    }
                case Direction.Left:
                    {
                        x = Game.Player.X - 1;
                        break;
                    }
                case Direction.Right:
                    {
                        x = Game.Player.X + 1;
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }

            if(Game.DungeonMap.SetActorPosition(Game.Player, x, y))
            {
                return true;
            }

            Monster monster = Game.DungeonMap.GetMonsterAt(x, y);

            if(monster != null)
            {
                Attack(Game.Player, monster);
                return true;
            }

            return false;
        }

        //Activate Monsters
        public void ActivateMonsters()
        {
            IScheduleable scheduleable = Game.SchedulingSystem.Get();
            if(scheduleable is Player)
            {
                IsPlayerTurn = true;
                Game.SchedulingSystem.Add(Game.Player);
            }
            else
            {
                Monster monster = scheduleable as Monster;

                if(monster != null)
                {
                    monster.PerformAction(this);
                    Game.SchedulingSystem.Add(monster);
                }

                ActivateMonsters();
            }
        }

        //Move the monster
        public void MoveMonster(Monster monster, Cell cell)
        {
            if(!Game.DungeonMap.SetActorPosition(monster, cell.X, cell.Y))
            {
                if(Game.Player.X == cell.X && Game.Player.Y == cell.Y)
                {
                    Attack(monster, Game.Player);
                }
            }
        }

        //COMBAT HANDLERS
        //Base attack handler
        public void Attack(Actor attacker, Actor defender)
        {
            StringBuilder attackMessage = new StringBuilder();
            StringBuilder defenseMessage = new StringBuilder();

            int hits = ResolveAttack(attacker, defender, attackMessage);

            int blocks = ResolveDefense(defender, hits, attackMessage, defenseMessage);

            Game.MessageLog.Add(attackMessage.ToString());

            if(!string.IsNullOrWhiteSpace(defenseMessage.ToString()))
            {
                Game.MessageLog.Add(defenseMessage.ToString());
            }

            int damage = hits - blocks;

            ResolveDamage(defender, damage);
        }

        //Attacker rolls stats to determine if hit
        private static int ResolveAttack(Actor attacker, Actor defender, StringBuilder attackMesage)
        {
            int hits = 0;

            attackMesage.AppendFormat("{0} attack {1} and rolls: ", attacker.Name, defender.Name);

            //Roll Attackd100
            DiceExpression attackDice = new DiceExpression().Dice(attacker.Attack, 100);
            DiceResult attackResult = attackDice.Roll();

            //Get face value of the rolled die
            foreach(TermResult termResult in attackResult.Results)
            {
                attackMesage.Append(termResult.Value + ", ");
                //Compare result to 100-AttackChance
                if(termResult.Value >= 100 - attacker.AttackChance)
                {
                    hits++;
                }
            }

            return hits;
        }

        //Defender rolls stats to try to block Attacker's hits
        private static int ResolveDefense(Actor defender, int hits, StringBuilder attackMessage, StringBuilder defenseMessage)
        {
            int blocks = 0;

            if(hits > 0)
            {
                attackMessage.AppendFormat("scoring {0} hits.", hits);
                defenseMessage.AppendFormat("   {0} defends and rolls: ", defender.Name);

                //Roll Defensed100
                DiceExpression defenseDice = new DiceExpression().Dice(defender.Defense, 100);
                DiceResult defenseRoll = defenseDice.Roll();

                //Get face value of the rolled dice
                foreach(TermResult termResult in defenseRoll.Results)
                {
                    defenseMessage.Append(termResult.Value + ", ");

                    //Compare result to 100-DefenseChance
                    if(termResult.Value >= 100 - defender.DefenseChance)
                    {
                        blocks++;
                    }
                }
                defenseMessage.AppendFormat("resulting in {0} blocks.", blocks);
            }
            else
            {
                attackMessage.Append("and misses completely.");
            }

            return blocks;
        }

        //Apply all damage that wasn't blocked
        private static void ResolveDamage(Actor defender, int damage)
        {
            if(damage > 0)
            {
                defender.Health = defender.Health - damage;

                Game.MessageLog.Add($"{defender.Name} was hit for {damage} damage.");

                if(defender.Health <= 0)
                {
                    ResolveDeath(defender);
                }
            }
            else
            {
                Game.MessageLog.Add($"{defender.Name} blocked all damage!");
            }
        }

        //On death, remove the actor and update the message queue
        private static void ResolveDeath(Actor defender)
        {
            if(defender is Player)
            {
                Game.MessageLog.Add($"{defender.Name} was killed. GAME OVER.");
            }
            else if (defender is Monster)
            {
                Game.DungeonMap.RemoveMonster((Monster)defender);

                Game.MessageLog.Add($"{defender.Name} died and dropped {defender.Gold} gold.");
            }
        }
    }
}
