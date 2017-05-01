using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    // ReSharper disable once UseNameofExpression
    [DebuggerDisplay("{Name}")]
    internal class Rule
    {
        /// <summary>
        ///     Podmienky pravidla
        /// </summary>
        private readonly List<Condition> _conditions = new List<Condition>();

        /// <summary>
        ///     Akcie, ktore sa maju vykonat po splneni pravidla
        /// </summary>
        public readonly List<MainWindow.Action> Actions = new List<MainWindow.Action>();

        /// <summary>
        ///     Meno pravidla pre testovacie ucely
        /// </summary>
        public string Name;

        /// <summary>
        ///     Pridaj podmienku do pravidla
        /// </summary>
        /// <param name="input">Napisana podmienka</param>
        public void AddCondition(string input)
        {
            _conditions.Add(new Condition(input));
        }

        /// <summary>
        ///     Najdi vsetky fakty splnajuce danu podmienku
        /// </summary>
        /// <param name="index">Cislo podmienky na spracovanie</param>
        /// <param name="params">Doteraz Najdene hodnoty z faktov pouzitim predchadzajucich pravidiel</param>
        /// <param name="memory">Fakty na, ktorych kontroluj pravidla</param>
        /// <returns>Najdene premenne, kt. splnili podmienku</returns>
        private IEnumerable<Dictionary<string, string>> CheckCondition(int index, Dictionary<string, string> @params,
            string memory)
        {
            // Ak je to posledna podmienka vrat vsetky najdene hodnoty
            if (index >= _conditions.Count)
                return new List<Dictionary<string, string>> {new Dictionary<string, string>(@params)};

            // Spracuj podmienku
            var condition = _conditions[index];
            return condition.Compare
                ? Compare(index, @params, memory, condition)
                : FindFacts(index, @params, memory, condition);
        }

        /// <summary>
        ///     Spracuj podmienku pre rozdielne hodnoty
        /// </summary>
        /// <param name="index">Cislo spracovanej podmienky</param>
        /// <param name="params">Doteraz Najdene hodnoty z faktov pouzitim predchadzajucich pravidiel</param>
        /// <param name="memory">Fakty na, ktorych kontroluj pravidla</param>
        /// <param name="condition">Podmienka, kt. sa overuje</param>
        /// <returns>Najdene premenne, kt. splnili podmienku</returns>
        private IEnumerable<Dictionary<string, string>> Compare(int index, Dictionary<string, string> @params,
            string memory, Condition condition)
        {
            // Snazim sa porovnat premenne, kt. ani neexistuju (neboli naplnene) ?
            if (!@params.ContainsKey(condition.ParamsOrder[0]) || !@params.ContainsKey(condition.ParamsOrder[1]))
                return new List<Dictionary<string, string>>(0);
            // Ak su hodnoty rovnake skonci (musia byt rozdielne)
            if (@params[condition.ParamsOrder[0]].Equals(@params[condition.ParamsOrder[1]]))
                return new List<Dictionary<string, string>>(0);

            // Spracuj dalsiu podmienku pravidla
            var result = CheckCondition(index + 1, @params, memory);

            // Zmaz naplnene hodnoty danou podmienkou pre overenim dalsieho faktu
            foreach (var param in condition.ParamsOrder)
                @params.Remove(param);

            return result;
        }

        /// <summary>
        ///     Spracuj podmienku
        /// </summary>
        /// <param name="index">Cislo spracovanej podmienky</param>
        /// <param name="params">Doteraz Najdene hodnoty z faktov pouzitim predchadzajucich pravidiel</param>
        /// <param name="memory">Fakty na, ktorych kontroluj pravidla</param>
        /// <param name="condition">Podmienka, kt. sa overuje</param>
        /// <returns>Najdene premenne, kt. splnili podmienku</returns>
        private IEnumerable<Dictionary<string, string>> FindFacts(int index, Dictionary<string, string> @params,
            string memory, Condition condition)
        {
            var reg = new Regex(condition.ToString(@params), RegexOptions.Multiline);
            var matches = reg.Matches(memory);
            foreach (Match match in matches)
            foreach (var p in CheckFacts(index, @params, memory, match, condition))
                yield return p;
        }

        /// <summary>
        ///     Spracuj najdeny fakt
        /// </summary>
        /// <param name="index">Cislo spracovanej podmienky</param>
        /// <param name="params">Doteraz Najdene hodnoty z faktov pouzitim predchadzajucich pravidiel</param>
        /// <param name="memory">Fakty na, ktorych kontroluj pravidla</param>
        /// <param name="fakt">Najedny fakt</param>
        /// <param name="condition">Podmienka, kt. sa overuje</param>
        /// <returns>Najdene premenne, kt. splnili podmienku</returns>
        private IEnumerable<Dictionary<string, string>> CheckFacts(int index, Dictionary<string, string> @params,
            string memory, Match fakt, Condition condition)
        {
            // Vyber najdeneho hodnoty
            for (var j = 1; j < fakt.Groups.Count; j++)
            {
                var group = fakt.Groups[j].Value;
                var param = condition.ParamsOrder[j - 1];
                if (!@params.ContainsKey(param))
                    @params.Add(param, group);
                // Ak bola hodnota naplnena predchadzajucov podmienkov musi sa hodnota zhodovat
                else if (!group.Equals(@params[param]))
                    yield break;
            }

            // Spracuj dalsiu podmienku pravidla
            foreach (var temp in CheckCondition(index + 1, @params, memory))
                yield return temp;

            // Zmaz naplnene hodnoty danou podmienkou pre overenim dalsieho faktu
            foreach (var param in condition.ParamsOrder)
                @params.Remove(param);
        }

        /// <summary>
        ///     Spusti overovanie vsetkych podmienok pravidla
        /// </summary>
        /// <param name="memory">Fakty na, ktorych kontroluj pravidla</param>
        /// <returns>Najdene premenne, kt. splnili pravidlo</returns>
        public IEnumerable<Dictionary<string, string>> Check(string memory)
            => CheckCondition(0, new Dictionary<string, string>(3), memory);

        private class Condition
        {
            /// <summary>
            ///     Konstantne casti textu
            /// </summary>
            private readonly List<Tuple<bool, string>> _parts;

            /// <summary>
            ///     Jedna sa o pravidlo porovnavania?
            /// </summary>
            internal readonly bool Compare;

            /// <summary>
            ///     Premenne pravidla
            /// </summary>
            internal readonly List<string> ParamsOrder = new List<string>(2);

            /// <summary>
            ///     Analyzuje string a vytvori k nemu prislichajuce pravidlo
            /// </summary>
            /// <param name="input">Napisane pravidlo</param>
            public Condition(string input)
            {
                var regex = new Regex("^\\s*\\<\\>\\s*(\\?[^\\s]+)\\s+(\\?[^\\s]+)\\s*$");
                var match = regex.Match(input);
                if (match.Success)
                {
                    Compare = true;
                    ParamsOrder.Add(match.Groups[1].Value);
                    ParamsOrder.Add(match.Groups[2].Value);
                }
                else
                {
                    Compare = false;
                    _parts = new List<Tuple<bool, string>>(4);
                    regex = new Regex("(\\?[^\\s]+)|([^\\?]+|\\?[^\\s]{0})");
                    var matches = regex.Matches(input);
                    foreach (Match m in matches)
                    {
                        var part = m.Value;
                        if (part[0] != '?' || part.Length == 1)
                        {
                            _parts.Add(new Tuple<bool, string>(false, Regex.Escape(part)));
                        }
                        else
                        {
                            _parts.Add(new Tuple<bool, string>(true, part));
                            ParamsOrder.Add(part);
                        }
                    }
                }
            }

            /// <summary>
            ///     Vytvor regex vzor pre hladanie faktov splnajuce tot pravidlo
            /// </summary>
            /// <param name="Params">Najdene hodnoty z faktov pouzitim predchadzajucich pravidiel</param>
            /// <returns>Regex vzor pre hladanie faktov</returns>
            public string ToString(IReadOnlyDictionary<string, string> Params)
            {
                if (Compare) return null;
                var sb = new StringBuilder();
                sb.Append("^\\s*\\(");
                foreach (var part in _parts)
                    if (part.Item1)
                        sb.Append(Params.ContainsKey(part.Item2) ? $"({Regex.Escape(Params[part.Item2])})" : "(.*)");
                    else
                        sb.Append(part.Item2);
                sb.Append("\\)\\s*$");
                return sb.ToString();
            }
        }
    }
}