using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Vymaz : Action
        {
            public Vymaz(string input, MainWindow window) : base(input, window)
            {
            }

            public override bool DoWork(IReadOnlyDictionary<string, string> parameters)
            {
                var regex = new Regex(ToString(parameters, true), RegexOptions.Multiline);
                var output = ToString(parameters, false);
                var match = regex.Match(Window.GetFakty);
                if (match.Success)
                {
                    Window.Dispatcher.Invoke(() => { 
                        Window.DebugOutput.AppendText($"Vymazavam: {output}{Environment.NewLine}");
                        Debug.WriteLine($@"Vymazavam: {output}");
                        var part = Window.Memory.Text.Remove(match.Index).TrimEnd();
                        if (match.Index + match.Length < Window.Memory.Text.Length)
                            part += Environment.NewLine + Window.Memory.Text.Substring(match.Index + match.Length).TrimStart();
                        Window.Memory.Text = part.Trim();
                    });
                    return false;
                }
                Window.Dispatcher.Invoke(() => { 
                    Window.DebugOutput.AppendText($"Nenasiel som fakt '{output}' na vymadzanie.{Environment.NewLine}");
                    Debug.WriteLine($@"Nenasiel som fakt '{output}' na vymadzanie");
                });
                return false;
            }
        }
    }
}