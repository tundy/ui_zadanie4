using System.Collections.Generic;
using System.Diagnostics;

namespace ui_zadanie4
{
    [DebuggerDisplay("Name")]
    internal class Rule
    {
        public string Name;
        public List<string> Conditions = new List<string>();
        public List<MainWindow.Action> Actions = new List<MainWindow.Action>();

        public void Check(string memory)
        {
            /*foreach (var condition in Conditions)
            {
                var reg = new Regex(condition);
                var matchse = reg.Matches(memory);

            }*/
        }
    }
}