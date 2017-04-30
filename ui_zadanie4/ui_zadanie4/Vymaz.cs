using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Vymaz : Action
        {
            public Vymaz(string input, MainWindow window) : base(input, window)
            {
            }

            public override bool DoWork(Dictionary<string, string> parameters)
            {
                var regex = new Regex(ToString(parameters, true), RegexOptions.Multiline);
                var output = ToString(parameters, false);
                var match = regex.Match(Window.Memory.Text);
                if (match.Success)
                {
                    Window.DebugOutput.AppendText($"Vymazavam: {output}{Environment.NewLine}");
                    var part = Window.Memory.Text.Remove(match.Index);
                    if (match.Index + match.Length < Window.Memory.Text.Length)
                        part += Window.Memory.Text.Substring(match.Index + match.Length);
                    Window.Memory.Text = part;
                    return false;
                }
                Window.DebugOutput.AppendText($"Nenasiel som fakt '{output}' na vymadzanie.{Environment.NewLine}");
                return false;
            }
        }
    }
}