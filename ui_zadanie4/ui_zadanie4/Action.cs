using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        /// <summary>
        /// Analyzuj text a vytvor k nemu prisluchajucu akciu
        /// </summary>
        /// <param name="text">Napisana akcia</param>
        /// <returns>Akcia pre dany vstup</returns>
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
            /// <summary>
            ///     Konstantne casti textu
            /// </summary>
            private readonly List<Tuple<bool, string>> _parts = new List<Tuple<bool, string>>(4);
            protected readonly MainWindow Window;

            /// <summary>
            /// Analyzuj text a rozloz na konstatne casti textu a premenne
            /// </summary>
            /// <param name="input">Hodnota akcie</param>
            /// <param name="window">Okno kde sa prejavia vysledky akcie</param>
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

            /// <summary>
            /// Urob akciu na zaklade najdenych hodnot a vrat hodnotu ci sa nieco pridalo medzi fakty
            /// </summary>
            /// <param name="parameters">Najdene hodnoty splnajuce pravidlo</param>
            /// <returns>Pridal sa novy fakt?</returns>
            public abstract bool DoWork(IReadOnlyDictionary<string, string> parameters);

            /// <summary>
            /// Premen hodnotu na text alebo regex vzor
            /// </summary>
            /// <param name="Params">Najdene hodnoty splnajuce pravidlo</param>
            /// <param name="regex">Ma sa vytvor regex vzor</param>
            /// <returns>Hodnota akcie</returns>
            protected string ToString(IReadOnlyDictionary<string, string> Params, bool regex)
            {
                var sb = new StringBuilder();
#if ZatvorkyPreFakty
                sb.Append(regex ? "^\\s*\\(" : "(");
#else
                if(regex)
                    sb.Append("^");
#endif
                foreach (var part in _parts)
                    if (part.Item1 && Params.ContainsKey(part.Item2))
                        sb.Append(regex ? $"({Regex.Escape(Params[part.Item2])})" : Params[part.Item2]);
                    else
                        sb.Append(regex ? Regex.Escape(part.Item2) : part.Item2);

#if ZatvorkyPreFakty
                sb.Append(regex ? "\\)\\s*$" : ")");
#else
                if(regex)
                    sb.Append("$");
#endif
                return sb.ToString();
            }
        }
    }
}