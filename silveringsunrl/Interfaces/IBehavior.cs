using SilveringSunRL.Commands;
using SilveringSunRL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilveringSunRL.Interfaces
{
    public interface IBehavior
    {
        bool Act(Monster monster, CommandManager commandManager);
    }
}
