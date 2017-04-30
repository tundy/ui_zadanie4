using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace ui_zadanie4
{
    public partial class MainWindow : Window
    {
        private class Vymaz : Action
        {
            public Vymaz(string input, MainWindow window) : base(input, window)
            {
            }

            public override bool DoWork(Dictionary<string, string> parameters)
            {
                var regex = new Regex(ToString(parameters, true), RegexOptions.Multiline);
                var match = regex.Match(Window.Memory.Text);
                if (match.Success)
                {
                    var part = Window.Memory.Text.Remove(match.Index);
                    if(match.Index + match.Length < Window.Memory.Text.Length)
                        part += Window.Memory.Text.Substring(match.Index + match.Length);
                    Window.Memory.Text = part;
                    return false;
                }
                var output = ToString(parameters, false);
                Window.DebugOutput.AppendText($"Vymazavam: {output}{Environment.NewLine}");
                return false;
            }
        }
    }
}
