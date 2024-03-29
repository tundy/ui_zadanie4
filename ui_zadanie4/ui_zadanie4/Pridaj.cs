﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Pridaj : Action
        {
            public Pridaj(string input, MainWindow window) : base(input, window)
            {
            }

            public override bool DoWork(IReadOnlyDictionary<string, string> parameters)
            {
                var regex = new Regex(ToString(parameters, true), RegexOptions.Multiline);
                var output = ToString(parameters, false);
                if (regex.IsMatch(Window.GetFakty))
                {
                    Window.Dispatcher.Invoke(() => {
                        Window.DebugOutput.AppendText($"Fakt '{output}' uz existuje.{Environment.NewLine}");
                        Debug.WriteLine($@"Fakt '{output}' uz existuje.");
                    });
                    return false;
                }
                Window.Dispatcher.Invoke(() => {
                    Window.DebugOutput.AppendText($"Pridavam: {output}{Environment.NewLine}");
                    Debug.WriteLine($@"Pridavam: {output}");
                    Window.Memory.AppendText($"\n{output}");
                });
                return true;
            }
        }
    }
}