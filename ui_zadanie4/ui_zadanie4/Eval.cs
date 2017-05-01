﻿using System;
using System.Collections.Generic;
using System.Data;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Eval : Action
        {
            public string Vysledok;
            public string Premenna;

            public Eval(string input, MainWindow window, bool important) : base(input, window, important)
            {
            }

            public override bool DoWork(IReadOnlyDictionary<string, string> parameters)
            {
                Presiel = false;
                var output = ToString(parameters, false);
                var temp = output.Split(new[] {'='}, 2, StringSplitOptions.RemoveEmptyEntries);
                if (MissingValues.Count != 1)
                {
                    Window.DebugOutput.AppendText($"Chybný zápis pre eval: '{output}'{Environment.NewLine}");
                    return false;
                }
                Premenna = MissingValues[0];
                using (var dt = new DataTable())
                    Vysledok = dt.Compute(temp[1], "").ToString();
                Window.DebugOutput.AppendText($"[{Premenna}]{Vysledok} = {temp[1]} {Environment.NewLine}");
                Presiel = true;
                return false;
            }
        }
    }
}