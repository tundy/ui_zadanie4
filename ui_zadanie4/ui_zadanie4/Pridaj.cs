using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Pridaj : Action
        {
            public Pridaj(string input, MainWindow window, bool important) : base(input, window, important)
            {
            }

            public override bool DoWork(IReadOnlyDictionary<string, string> parameters)
            {
                var regex = new Regex(ToString(parameters, true), RegexOptions.Multiline);
                var output = ToString(parameters, false);
                if (regex.IsMatch(Window.Memory.Text))
                {
                    Window.DebugOutput.AppendText($"Fakt '{output}' uz existuje.{Environment.NewLine}");
                    Presiel = false;
                    return false;
                }
                Window.DebugOutput.AppendText($"Pridavam: {output}{Environment.NewLine}");
                Window.Memory.AppendText($"\n{output}");
                Presiel = true;
                return true;
            }
        }
    }
}