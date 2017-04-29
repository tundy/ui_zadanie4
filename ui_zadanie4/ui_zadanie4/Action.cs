using System;
using System.Collections.Generic;
using System.Windows;

namespace ui_zadanie4
{
    public partial class MainWindow : Window
    {
        private static MainWindow.Action GetAction(string text)
        {
            var parts = text.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            switch (parts[0].ToUpper())
            {
                case "PRIDAJ":
                    return new Pridaj(parts[1]);
                case "VYMAZ":
                    return new Vymaz(parts[1]);
                case "SPRAVA":
                    return new Sprava(parts[1]);
                default:
                    return null;
            }
        }

        internal abstract class Action
        {
            public abstract bool DoWork(Dictionary<string, string> parameters);

            protected Action(string Input)
            {
                // (^\s*\<\>\s*(\?[^\s]+)\s+(\?[^\s]+)\s*$)|(\?[^\s]+)|([^\?]+|\?\s)
            }
        }
    }
}
