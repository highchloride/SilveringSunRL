using RLNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilveringSunRL.Systems
{
    //manages the queue of messages
    //draws said queue to the console
    public class MessageLog
    {
        //Define max queu lines
        private static readonly int _maxLines = 9;

        //Set Queue for the lines
        //First added is first removed
        private readonly Queue<string> _lines;

        public MessageLog()
        {
            _lines = new Queue<string>();
        }

        //Add a line to the MessageLog Queue
        public void Add(string message)
        {
            _lines.Enqueue(message);

            //if maxLines exceeded, remove the oldest line
            if(_lines.Count > _maxLines)
            {
                _lines.Dequeue();
            }
        }

        //Draw each line of the MessageLog to the console
        public void Draw(RLConsole console)
        {
            console.Clear();
            string[] lines = _lines.ToArray();

            for(int i = 0; i < lines.Length; i++)
            {
                console.Print(1, i + 1, lines[i], RLColor.White);
            }
        }
    }
}
