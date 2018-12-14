using RogueSharp;
using SilveringSunRL.Core;
using SilveringSunRL.Interfaces;
using SilveringSunRL.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilveringSunRL.Behaviors
{
    public class StandardMoveAndAttack : IBehavior
    {
        public bool Act(Monster monster, CommandSystem commandSystem)
        {
            DungeonMap dungeonMap = Game.DungeonMap;
            Player player = Game.Player;
            FieldOfView monsterFov = new FieldOfView(dungeonMap);

            //If the monster is not alerted, compute its field of view using Monster's Awareness
            //If the player is in the monster's FoV, alert it and update MessageQueue
            if(!monster.TurnsAlerted.HasValue)
            {
                monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                if(monsterFov.IsInFov(player.X, player.Y))
                {
                    Game.MessageLog.Add($"{monster.Name} is eager to fight {player.Name}");
                    monster.TurnsAlerted = 1;
                }
            }

            if(monster.TurnsAlerted.HasValue)
            {
                //Before we find a path, make sure to make the monster and player Cells walkable
                dungeonMap.SetIsWalkable(monster.X, monster.Y, true);
                dungeonMap.SetIsWalkable(player.X, player.Y, true);

                //Setup pathfinding
                PathFinder pathFinder = new PathFinder(dungeonMap);
                Path path = null;

                //Try to find a path from the monster to the Player
                try
                {
                    path = pathFinder.ShortestPath(
                        dungeonMap.GetCell(monster.X, monster.Y),
                        dungeonMap.GetCell(player.X, player.Y));
                }
                catch(PathNotFoundException)
                {
                    //The monster can see the Player, but can't get to him
                    //Print a message about it
                    Game.MessageLog.Add($"{monster.Name} waits for a turn...");
                }

                //Set the monster and player Cells unwalkable again
                dungeonMap.SetIsWalkable(monster.X, monster.Y, false);
                dungeonMap.SetIsWalkable(player.X, player.Y, false);

                //Once a path is found, invoke CommandSystem to move the monster
                if(path != null)
                {
                    try
                    {
                        //TODO: try this as path.StepForward()
                        commandSystem.MoveMonster(monster, (Cell)path.StepForward());
                    }
                    catch(NoMoreStepsException)
                    {
                        Game.MessageLog.Add($"{monster.Name} growls in frustration.");
                    }
                }

                monster.TurnsAlerted++;

                //Lose alerted status every 15 turns
                //Monster will renew alert as long as Player is in FoV, otherwise will quit chasing
                if(monster.TurnsAlerted > 15)
                {
                    monster.TurnsAlerted = null;
                }
            }

            return true;
        }
    }
}
