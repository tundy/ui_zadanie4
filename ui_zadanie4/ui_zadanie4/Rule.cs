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
            return condition.Compare.HasValue
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
            string a, b;
            // Ak prvy parameter je konstanta
            if (!@params.ContainsKey(condition.ParamsOrder[0]))
            {
                // Snazim sa porovnat premenne, kt. ani neexistuju (neboli naplnene) ?
                if (!@params.ContainsKey(condition.ParamsOrder[1]))
                    return new List<Dictionary<string, string>>(0);

                a = condition.ParamsOrder[0];
                b = @params[condition.ParamsOrder[1]];
            }
            else
            {
                a = @params[condition.ParamsOrder[0]];
                // Ak druhy parameter je konstanta
                b = !@params.ContainsKey(condition.ParamsOrder[1])
                    ? condition.ParamsOrder[1]
                    : @params[condition.ParamsOrder[1]];
            }

            // ReSharper disable once PossibleInvalidOperationException
            return condition.Compare.Value == a.Equals(b)
                ? CheckCondition(index + 1, @params, memory)
                : new List<Dictionary<string, string>>(0);
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
            var @new = new List<string>(3);

            // Vyber najdeneho hodnoty
            for (var j = 1; j < fakt.Groups.Count; j++)
            {
                var group = fakt.Groups[j].Value;
                var param = condition.ParamsOrder[j - 1];
                if (!@params.ContainsKey(param))
                {
                    @params.Add(param, group);
                    @new.Add(param);
                }
                // Ak bola hodnota naplnena predchadzajucov podmienkov musi sa hodnota zhodovat
                else if (!group.Equals(@params[param]))
                {
                    yield break;
                }
            }

            // Spracuj dalsiu podmienku pravidla
            foreach (var temp in CheckCondition(index + 1, @params, memory))
                yield return temp;

            // Zmaz naplnene hodnoty danou podmienkou pre overenim dalsieho faktu
            foreach (var param in @new)
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
            internal readonly bool? Compare;

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
                //var regex = new Regex("^\\s*\\<\\>\\s*(\\?[^\\s]+)\\s+(\\?[^\\s]+)\\s*$");
                var regex =
                    new Regex("^\\s*\\<\\>\\s*(((\\?[^\\s]+)\\s+([^\\s]+.*))|(([^\\s]+.*)\\s+(\\?[^\\s]+)))\\s*$");
                var match = regex.Match(input);
                if (match.Success)
                {
                    Compare = false;
                    var i = match.Groups.Count - 1;
                    for (; i >= 0; i--)
                        if (match.Groups[i].Value.Length > 0)
                            break;
                    ParamsOrder.Add(match.Groups[--i].Value);
                    ParamsOrder.Add(match.Groups[i + 1].Value);
                }
                else
                {
                    regex =
                        new Regex("^\\s*\\=\\=\\s*(((\\?[^\\s]+)\\s+([^\\s]+.*))|(([^\\s]+.*)\\s+(\\?[^\\s]+)))\\s*$");
                    match = regex.Match(input);
                    if (match.Success)
                    {
                        Compare = true;
                        var i = match.Groups.Count - 1;
                        for (; i >= 0; i--)
                            if (match.Groups[i].Value.Length > 0)
                                break;
                        ParamsOrder.Add(match.Groups[--i].Value);
                        ParamsOrder.Add(match.Groups[i + 1].Value);
                    }
                    else
                    {
                        Compare = null;
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
            }

            /// <summary>
            ///     Vytvor regex vzor pre hladanie faktov splnajuce tot pravidlo
            /// </summary>
            /// <param name="Params">Najdene hodnoty z faktov pouzitim predchadzajucich pravidiel</param>
            /// <returns>Regex vzor pre hladanie faktov</returns>
            public string ToString(IReadOnlyDictionary<string, string> Params)
            {
                if (Compare.HasValue) return null;
                var sb = new StringBuilder();
#if ZatvorkyPreFakty
                sb.Append("^\\s*\\(");
#else
                sb.Append("^");
#endif
                foreach (var part in _parts)
                    if (part.Item1)
                        sb.Append(Params.ContainsKey(part.Item2) ? $"({Regex.Escape(Params[part.Item2])})" : "(.*)");
                    else
                        sb.Append(part.Item2);
#if ZatvorkyPreFakty
                sb.Append("\\)\\s*$");
#else
                sb.Append("$");
#endif
                return sb.ToString();
            }
        }
    }
}