﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Vymaz : Action
        {
            public Vymaz(string input, MainWindow window, bool important) : base(input, window, important)
            {
            }

            public override bool DoWork(IReadOnlyDictionary<string, string> parameters)
            {
                Presiel = false;
                var regex = new Regex(ToString(parameters, true), RegexOptions.Multiline);
                var output = ToString(parameters, false);
                var match = regex.Match(Window.Memory.Text);
                if (match.Success)
                {
                    Window.DebugOutput.AppendText($"Vymazavam: {output}{Environment.NewLine}");
                    var part = Window.Memory.Text.Remove(match.Index).TrimEnd();
                    if (match.Index + match.Length < Window.Memory.Text.Length)
                        part += Environment.NewLine + Window.Memory.Text.Substring(match.Index + match.Length).TrimStart();
                    Window.Memory.Text = part.Trim();
                    Presiel = true;
                    return false;
                }
                Window.DebugOutput.AppendText($"Nenasiel som fakt '{output}' na vymadzanie.{Environment.NewLine}");
                return false;
            }
        }
    }
}