using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace SilveringSunRL.Entities
{
    public class EntityManager : SadConsole.Entities.EntityManager
    {
        //constructor
        public EntityManager()
        {
        }

        //Return the entity at the specified location in the manager's list
        public T GetEntityAt<T>(Point location) where T : SadConsole.Entities.Entity
        {
            return (T)Entities.FirstOrDefault(entity => entity is T && entity.Position == location);
        }
    }
}
