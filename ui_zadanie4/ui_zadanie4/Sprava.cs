﻿using System;
using System.Collections.Generic;
using System.Windows;

namespace ui_zadanie4
{
    public partial class MainWindow : Window
    {
        internal class Sprava : Action
        {
            public Sprava(string Input) : base(Input)
            {
            }

            public override bool DoWork(Dictionary<string, string> parameters)
            {
                throw new NotImplementedException();
            }
        }
    }
}