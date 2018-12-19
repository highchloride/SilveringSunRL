using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SadConsole;

namespace SilveringSunRL.Screens
{
    public class MessageLogWindow : Window
    {
        //Maximum lines for the queue
        private static readonly int _maxLines = 100;

        //FIFO Queue object
        private readonly Queue<string> _lines;

        //MessageConsole object
        private SadConsole.Console _messageConsole;

        //Scrollbar for MessageConsole
        private SadConsole.Controls.ScrollBar _messageScrollBar;

        private int _scrollBarCurrentPosition;

        //Track window thickness to prevent UI elements spilling over
        private int _windowBorderThickness = 2;

        //Create the message window
        public MessageLogWindow(int width, int height, string title) : base(width, height)
        {
            Theme.FillStyle.Background = Color.Black;
            _lines = new Queue<string>();
            Dragable = true;
            Title = title.Align(HorizontalAlignment.Left, width);

            //add the console and viewport
            _messageConsole = new SadConsole.Console(width - _windowBorderThickness, _maxLines);
            _messageConsole.Position = new Point(1, 1);
            _messageConsole.ViewPort = new Rectangle(0, 0, width - 1, height - _windowBorderThickness);

            //create the scrollbar and add it to the window
            _messageScrollBar = SadConsole.Controls.ScrollBar.Create(SadConsole.Orientation.Vertical, height - _windowBorderThickness);
            _messageScrollBar.Position = new Point(_messageConsole.Width + 1, _messageConsole.Position.X);
            _messageScrollBar.IsEnabled = false;
            _messageScrollBar.ValueChanged += MessageScrollBar_ValueChanged;
            Add(_messageScrollBar);

            //Enable mouse support
            UseMouse = true;
        }

        //Add a new line to the queue
        public void Add(string message)
        {
            _lines.Enqueue(message);
            //If we're at maxlines, remove the oldest line
            if(_lines.Count > _maxLines)
            {
                _lines.Dequeue();
            }
            Cursor.DisableWordBreak = true;
            Cursor.Position = new Point(1, _lines.Count);            
            Cursor.Print(message + '\n');
        }

        //Handle moving the scrollbar and the message window
        void MessageScrollBar_ValueChanged(object sender, EventArgs e)
        {
            _messageConsole.ViewPort = new Rectangle(0, _messageScrollBar.Value + _windowBorderThickness, _messageConsole.Width, _messageConsole.ViewPort.Height);
        }

        // copied directly from http://sadconsole.com/docs/make-a-scrolling-console.html
        // and modified to suit this class variable names
        public override void Update(TimeSpan time)
        {
            base.Update(time);

            // Ensure that the scrollbar tracks the current position of the _messageConsole.
            if (_messageConsole.TimesShiftedUp != 0 | _messageConsole.Cursor.Position.Y >= _messageConsole.ViewPort.Height + _scrollBarCurrentPosition)
            {
                //enable the scrollbar once the messagelog has filled up with enough text to warrant scrolling
                _messageScrollBar.IsEnabled = true;

                // Make sure we've never scrolled the entire size of the buffer
                if (_scrollBarCurrentPosition < _messageConsole.Height - _messageConsole.ViewPort.Height)
                    // Record how much we've scrolled to enable how far back the bar can see
                    _scrollBarCurrentPosition += _messageConsole.TimesShiftedUp != 0 ? _messageConsole.TimesShiftedUp : 1;

                _messageScrollBar.Maximum = (_messageConsole.Height + _scrollBarCurrentPosition) - _messageConsole.Height - _windowBorderThickness;

                // This will follow the cursor since we move the render area in the event.
                _messageScrollBar.Value = _scrollBarCurrentPosition;

                // Reset the shift amount.
                _messageConsole.TimesShiftedUp = 0;
            }
        }

        //Draw this window
        public override void Draw(TimeSpan drawTime)
        {
            base.Draw(drawTime);
        }
    }
}
