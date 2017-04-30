using System;
using System.Collections.Generic;
using System.Windows;

namespace ui_zadanie4
{
    public partial class MainWindow : Window
    {
        private class Sprava : Action
        {
            public Sprava(string input, MainWindow window) : base(input, window)
            {
            }

            public override bool DoWork(Dictionary<string, string> parameters)
            {
                Window.Output.AppendText($"{ToString(parameters, false)}\n");
                return false;
            }
        }
    }
}
