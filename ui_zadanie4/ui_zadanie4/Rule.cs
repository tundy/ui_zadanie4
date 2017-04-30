using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ui_zadanie4
{
    [DebuggerDisplay("{Name}")]
    internal class Rule
    {
        private class Condition
        {
            internal readonly List<string> ParamsOrder = new List<string>(2);
            private readonly List<Tuple<bool, string>> _parts = new List<Tuple<bool, string>>(4);
            internal readonly bool Compare;

            public string ToString(Dictionary<string, string> Params)
            {
                var sb = new StringBuilder();
                sb.Append("^\\s*\\(");
                foreach (var part in _parts)
                {
                    if (part.Item1)
                        if (Params.ContainsKey(part.Item2))
                            sb.Append($"({Params[part.Item2]})");
                        else
                            sb.Append(NoValue);
                    else
                        sb.Append(part.Item2);
                }
                sb.Append("\\)\\s*$");
                return sb.ToString();
            }

            private const string NoValue = "(.*)";

            public Condition(string input)
            {
                var regex = new Regex("^\\s*\\<\\>\\s*(\\?[^\\s]+)\\s+(\\?[^\\s]+)\\s*$");
                var match = regex.Match(input);
                if (match.Success)
                {
                    Compare = true;
                    _parts.Add(new Tuple<bool, string>(false, "<> "));
                    var value = match.Groups[1].Value;
                    ParamsOrder.Add(value);
                    //Params.Add(value, NoValue);
                    _parts.Add(new Tuple<bool, string>(true, value));
                    _parts.Add(new Tuple<bool, string>(false, " "));
                    value = match.Groups[2].Value;
                    ParamsOrder.Add(value);
                    _parts.Add(new Tuple<bool, string>(true, value));
                }
                else
                {
                    Compare = false;
                    regex = new Regex("(\\?[^\\s]+)|([^\\?]+|\\?[^\\s]{0})");
                    var matches = regex.Matches(input);
                    foreach (Match m in matches)
                    {
                        var part = m.Value;
                        if (part[0] != '?' || part.Length == 1)
                        {
                            _parts.Add(new Tuple<bool, string>(false, part));
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

        public string Name;
        private readonly List<Condition> _conditions = new List<Condition>();
        public readonly List<MainWindow.Action> Actions = new List<MainWindow.Action>();

        public void AddCondition(string input)
        {
            _conditions.Add(new Condition(input));
        }

        private IEnumerable<Dictionary<string, string>> CheckCondition(int index, Dictionary<string, string> @params, string memory)
        {
            if (index >= _conditions.Count)
            {
                yield return @params;
                yield break;
            }

            if (@params == null)
                @params = new Dictionary<string, string>(3);

            var condition = _conditions[index];
            if (condition.Compare)
            {
                if (!@params.ContainsKey(condition.ParamsOrder[0]) || !@params.ContainsKey(condition.ParamsOrder[1]))
                    yield break;
                if (@params[condition.ParamsOrder[0]].Equals(@params[condition.ParamsOrder[1]]))
                    yield break;
                foreach (var temp in CheckCondition(index + 1, @params, memory))
                    yield return new Dictionary<string, string>(temp);

                foreach (var param in condition.ParamsOrder)
                    @params.Remove(param);
            }
            else
            {
                var wrong = false;
                var reg = new Regex(condition.ToString(@params), RegexOptions.Multiline);
                var matches = reg.Matches(memory);
                for (var i = 0; i < matches.Count; i++)
                {
                    var match = matches[i];
                    for (var j = 1; j < match.Groups.Count; j++)
                    {
                        var @group = match.Groups[j].Value;
                        var param = condition.ParamsOrder[j - 1];
                        if (!@params.ContainsKey(param))
                            @params.Add(param, @group);
                        else
                        {
                            if (!@group.Equals(@params[param]))
                            {
                                wrong = true;
                                break;
                            }
                        }

                    }
                    if (wrong) continue;

                    //Console.WriteLine($@"{Name}: {condition.ToString(@params)}");
                    foreach (var temp in CheckCondition(index + 1, @params, memory))
                        yield return new Dictionary<string, string>(temp);

                    foreach (var param in condition.ParamsOrder)
                        @params.Remove(param);
                }
            }
        }

        public IEnumerable<Dictionary<string, string>> Check(string memory) => CheckCondition(0, null, memory);
    }
}