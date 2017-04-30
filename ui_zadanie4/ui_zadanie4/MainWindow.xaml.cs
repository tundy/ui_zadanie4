using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;

namespace ui_zadanie4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Rule> _rules = new List<Rule>();

        public MainWindow()
        {
            InitializeComponent();

            Memory.AppendText(@"(Peter je rodic Jano)
(Peter je rodic Vlado)
(manzelia Peter Eva)
(Vlado je rodic Maria)
(Vlado je rodic Viera)
(muz Peter)
(muz Jano)
(muz Vlado)
(zena Maria)
(zena Viera)
(zena Eva)");

            Rules.AppendText(@" DruhyRodic1:
AK ((?X je rodic ?Y)(manzelia ?X ?Z))
POTOM ((pridaj ?Z je rodic ?Y))

DruhyRodic2:
AK ((?X je rodic ?Y)(manzelia ?Z ?X))
POTOM ((pridaj ?Z je rodic ?Y))

Otec:
AK ((?X je rodic ?Y)(muz ?X))
POTOM ((pridaj ?X je otec ?Y))

Matka:
AK ((?X je rodic ?Y)(zena ?X))
POTOM ((pridaj ?X je matka ?Y))

Surodenci:
AK ((?X je rodic ?Y)(?X je rodic ?Z)(<> ?Y ?Z))
POTOM ((pridaj ?Y a ?Z su surodenci))

Brat:
AK ((?Y a ?Z su surodenci)(muz ?Y))
POTOM ((pridaj ?Y je brat ?Z))

Stryko:
AK ((?Y je brat ?Z)(?Z je rodic ?X))
POTOM ((pridaj ?Y je stryko ?X)(sprava ?X ma stryka))

Test mazania:
AK ((?Y je stryko ?X)(zena ?X))
POTOM ((vymaz zena ?X))");
        }

        private void ParseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Output.Clear();
            DebugOutput.Clear();
            CleanUpMemory();
            if (!CheckMemory()) return;
            if (!ParseRules()) return;

            var work = true;
            while (work)
            {
                work = false;
                foreach (var rule in _rules)
                {
                    foreach (var @params in rule.Check(Memory.Text))
                    {
                        DebugOutput.AppendText($@"{rule.Name}: ");
                        foreach (var param in @params)
                            DebugOutput.AppendText($@"[{param.Key}]{param.Value} ");
                        DebugOutput.AppendText(Environment.NewLine);
                        foreach (var action in rule.Actions)
                            work |= action.DoWork(@params);
                    }
                }
            }
        }

        private bool CheckMemory()
        {
            var regex = new Regex("^\\s*(([^\\(\\s].*)|(.*[^\\)\\s]))\\s*$", RegexOptions.Multiline);
            var match = regex.Match(Memory.Text);
            if (match.Success)
            {
                Output.AppendText($"ERROR: Chybny zapis faktu:{Environment.NewLine}{match.Value}{Environment.NewLine}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Remove duplicates and Empty lines from memory Textbox
        /// </summary>
        private void CleanUpMemory()
        {
            Memory.Text = string.Join("\n",
                Memory.Text.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).Distinct());
        }

        private bool ParseRules()
        {
            _rules.Clear();

            var temp = Rules.Text.Trim();

            while (temp.Length > 0)
            {
                var rule = new Rule();
                if (!GetRuleName(rule, ref temp)) return false;
                if (!GetRuleConditions(rule, ref temp)) return false;
                if (!GetRuleActions(rule, ref temp)) return false;
                _rules.Add(rule);
            }
            return true;
        }

        private bool GetRuleActions(Rule rule, ref string temp)
        {
            if (!temp.StartsWith("POTOM", StringComparison.OrdinalIgnoreCase))
            {
                Output.AppendText($"ERROR: Chyba Akcia pre pravidlo '{rule.Name}'.{Environment.NewLine}");
                return false;
            }
            temp = temp.Substring(5).TrimStart();
            if (temp[0] != '(')
            {
                Output.AppendText($"ERROR: Za slovom POTOM sa nenachadza '(' v pravidle '{rule.Name}'.{Environment.NewLine}");
                return false;
            }

            var level = 1;
            var index = 1;
            for (var start = 0; level > 0; index++)
            {
                if (temp[index] == '(')
                {
                    ++level;
                    if (level > 2)
                    {
                        Output.AppendText(
                            $"ERROR: V akcii pre pravidlo '{rule.Name}' je trojite vnorenie zatvoriek.{Environment.NewLine}");
                        return false;
                    }
                    start = index + 1;
                    continue;
                }

                if (temp[index] == ')')
                {
                    --level;
                    if (level == 1)
                    {
                        var actionText = temp.Substring(start, index - start);
                        var action = GetAction(actionText);
                        if (action == null)
                        {
                            Output.AppendText($"ERROR: Neznama akcia:{Environment.NewLine}{actionText}{Environment.NewLine}");
                            return false;
                        }
                        rule.Actions.Add(action);
                    }
                    continue;
                }

                if (level == 1)
                {
                    if (!char.IsWhiteSpace(temp[index]))
                    {
                        Output.AppendText(
                            $"ERROR: Chyba v pravidle '{rule.Name}'. Medzi akciami nemoze byt text.{Environment.NewLine}");
                        return false;
                    }
                }
            }
            temp = temp.Length <= index ? "" : temp.Substring(index).TrimStart();
            return true;
        }

        private bool GetRuleConditions(Rule rule, ref string temp)
        {
            if (!temp.StartsWith("AK", StringComparison.OrdinalIgnoreCase))
            {
                Output.AppendText($"ERROR: Chyba podmienka pre pravidlo '{rule.Name}'.{Environment.NewLine}");
                return false;
            }
            temp = temp.Substring(2).TrimStart();
            if (temp[0] != '(')
            {
                Output.AppendText($"ERROR: Za slovom AK sa nenachadza '(' v pravidle '{rule.Name}'.{Environment.NewLine}");
                return false;
            }

            var level = 1;
            var index = 1;
            for (var start = 0; level > 0; index++)
            {
                if (temp[index] == '(')
                {
                    ++level;
                    if (level > 2)
                    {
                        Output.AppendText(
                            $"ERROR: V podmienke pre pravidlo '{rule.Name}' je trojite vnorenie zatvoriek.{Environment.NewLine}");
                        return false;
                    }
                    start = index + 1;
                    continue;
                }

                if (temp[index] == ')')
                {
                    --level;
                    if (level == 1)
                        rule.AddCondition(temp.Substring(start, index - start));
                    continue;
                }

                if (level == 1)
                {
                    if (!char.IsWhiteSpace(temp[index]))
                    {
                        Output.AppendText(
                            $"ERROR: Chyba v pravidle '{rule.Name}'. Medzi podmienkami nemoze byt text.{Environment.NewLine}");
                        return false;
                    }
                }
            }

            if (temp.Length <= index)
            {
                Output.AppendText($"ERROR: V pravidle '{rule.Name}' sa nenachadza akcia.{Environment.NewLine}");
                return false;
            }
            temp = temp.Substring(index).TrimStart();
            return true;
        }

        private bool GetRuleName(Rule rule, ref string temp)
        {
            var nameEnd = temp.IndexOf(":", StringComparison.Ordinal);
            if (nameEnd < 0)
            {
                Output.AppendText(
                    $"ERROR: Chyba pocas parsovania pravidiel! Za poslednym pravidlom sa nachadza neznamy text.{Environment.NewLine}");
                return false;
            }

            rule.Name = temp.Remove(nameEnd++);
            if (rule.Name.Contains('\n') || rule.Name.Contains('\r'))
            {
                Output.AppendText($"ERROR: Nazov pravidlu musi byt na jednom riadku '{rule.Name}'.{Environment.NewLine}");
                return false;
            }

            if (temp.Length <= nameEnd)
            {
                Output.AppendText(
                    $"ERROR: V pravidle '{rule.Name}' sa nenachadza podmienka a akcia.{Environment.NewLine}");
                return false;
            }
            temp = temp.Substring(nameEnd).TrimStart();
            return true;
        }
    }
}
