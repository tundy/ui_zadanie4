using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    internal partial class Rule
    {
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
                        sb.Append(Params.ContainsKey(part.Item2) ? $"({Regex.Escape(Params[part.Item2])})" : "(.+)");
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