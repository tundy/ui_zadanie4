using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private Action GetAction(string text)
        {
            var parts = text.Trim().Split(new[] {' '}, 2);
            switch (parts[0].ToUpper())
            {
                case "PRIDAJ":
                    return new Pridaj(parts[1], this);
                case "VYMAZ":
                    return new Vymaz(parts[1], this);
                case "SPRAVA":
                    return new Sprava(parts[1], this);
                default:
                    return null;
            }
        }

        internal abstract class Action
        {
            private readonly List<Tuple<bool, string>> _parts = new List<Tuple<bool, string>>(4);
            protected readonly MainWindow Window;

            protected Action(string input, MainWindow window)
            {
                Window = window;
                var regex = new Regex("(\\?[^\\s]+)|([^\\?]+|\\?[^\\s]{0})");
                var matches = regex.Matches(input);
                foreach (Match m in matches)
                {
                    var part = m.Value;
                    if (part[0] != '?' || part.Length == 1)
                        _parts.Add(new Tuple<bool, string>(false, part));
                    else
                        _parts.Add(new Tuple<bool, string>(true, part));
                }
            }

            public abstract bool DoWork(IReadOnlyDictionary<string, string> parameters);

            protected string ToString(IReadOnlyDictionary<string, string> Params, bool regex)
            {
                var sb = new StringBuilder();
                sb.Append(regex ? "^\\s*\\(" : "(");
                foreach (var part in _parts)
                    if (part.Item1 && Params.ContainsKey(part.Item2))
                        sb.Append(regex ? $"({Regex.Escape(Params[part.Item2])})" : Params[part.Item2]);
                    else
                        sb.Append(regex ? Regex.Escape(part.Item2) : part.Item2);
                sb.Append(regex ? "\\)\\s*$" : ")");
                return sb.ToString();
            }
        }
    }
}