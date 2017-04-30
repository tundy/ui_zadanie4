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
                var output = ToString(parameters, false);
                if (regex.IsMatch(Window.Memory.Text))
                {
                    Window.DebugOutput.AppendText($"Fakt '{output}' uz existuje.{Environment.NewLine}");
                    return false;
                }
                Window.DebugOutput.AppendText($"Pridavam: {output}{Environment.NewLine}");
                Window.Memory.AppendText($"\n{output}");
                return true;
            }
        }
    }
}
