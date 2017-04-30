using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace ui_zadanie4
{
    public partial class MainWindow : Window
    {
        private class Pridaj : Action
        {
            public Pridaj(string input, MainWindow window) : base(input, window)
            {
            }

            public override bool DoWork(Dictionary<string, string> parameters)
            {
                var regex = new Regex(ToString(parameters, true), RegexOptions.Multiline);
                if (regex.IsMatch(Window.Memory.Text))
                    return false;
                var output = ToString(parameters, false);
                Window.DebugOutput.AppendText($"Pridavam: {output}{Environment.NewLine}");
                Window.Memory.AppendText($"\n{output}");
                return true;
            }
        }
    }
}
