using System.Collections.Generic;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Sprava : Action
        {
            public Sprava(string input, MainWindow window) : base(input, window)
            {
            }

            public override bool DoWork(Dictionary<string, string> parameters)
            {
                var temp = ToString(parameters, false);
                Window.Output.AppendText($"{temp.Substring(1, temp.Length-2)}\n");
                return false;
            }
        }
    }
}