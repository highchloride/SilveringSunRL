using RogueSharp;
using SilveringSunRL.Entities;
using SilveringSunRL.Interfaces;
using SilveringSunRL.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilveringSunRL.Commands
{
    public class StandardMoveAndAttack : IBehavior
    {
        public bool Act(Monster monster, CommandManager commandManager)
        {
            DungeonMap dungeonMap = GameLoop.World.CurrentMap;
            Player player = GameLoop.World.Player;
            FieldOfView monsterFov = new FieldOfView(dungeonMap);
            if (!monster.TurnsAlerted.HasValue)
            {
                monsterFov.ComputeFov(monster.X, monster.Y, monster.Awareness, true);
                if (monsterFov.IsInFov(player.X, player.Y))
                {
                    GameLoop.UIManager.MessageLog.Add($"{monster.Name} is eager to fight {player.Name}");
                    monster.TurnsAlerted = 1;
                }
            }
            if (monster.TurnsAlerted.HasValue)
            {
                dungeonMap.SetIsWalkable(monster.X, monster.Y, true);
                dungeonMap.SetIsWalkable(player.X, player.Y, true);

                PathFinder pathFinder = new PathFinder(dungeonMap);
                Path path = null;

                try
                {
                    path = pathFinder.ShortestPath(dungeonMap.GetCell(monster.X, monster.Y), dungeonMap.GetCell(player.X, player.Y));
                }
                catch (PathNotFoundException)
                {
                    GameLoop.UIManager.MessageLog.Add($"{monster.Name} waits for a turn");
                }

                dungeonMap.SetIsWalkable(monster.X, monster.Y, false);
                dungeonMap.SetIsWalkable(player.X, player.Y, false);

                if (path != null)
                {
                    try
                    {
                        commandManager.MoveMonster(monster, (Cell)path.StepForward());
                    }
                    catch (NoMoreStepsException)
                    {
                        GameLoop.UIManager.MessageLog.Add($"{monster.Name} waits for a turn");
                    }
                }

                monster.TurnsAlerted++;

                // Lose alerted status every 15 turns. As long as the player is still in FoV the monster will be realerted otherwise the monster will quit chasing the player.
                if (monster.TurnsAlerted > 15)
                {
                    monster.TurnsAlerted = null;
                }
            }
            return true;
        }
    }
}
