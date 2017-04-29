using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        }

        private void ParseButton_OnClick(object sender, RoutedEventArgs e)
        {
            output.Clear();
            CleanUpMemory();
            if (!CheckMemory()) return;
            if (!ParseRules()) return;
        }

        private bool CheckMemory()
        {
            var regex = new Regex("^\\s*(([^\\(].*)|(.*[^\\)]))\\s*$", RegexOptions.Multiline);
            var match = regex.Match(memory.Text);
            if (match.Success)
            {
                output.AppendText($"ERROR: Chybny zapis faktu:{Environment.NewLine}{match.Value}{Environment.NewLine}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Remove duplicates and Empty lines from memory Textbox
        /// </summary>
        private void CleanUpMemory()
        {
            memory.Text = string.Join("\n",
                memory.Text.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).Distinct());
        }

        private bool ParseRules()
        {
            _rules.Clear();

            var temp = rules.Text.Trim();

            while (temp.Length > 0)
            {
                var rule = new Rule();
                if (!GetRuleName(rule, ref temp)) return false;
                if (!GetRuleConditions(rule, ref temp)) return false;
                if (!GetRuleActions(rule, ref temp)) return false;

            }
            return true;
        }

        private bool GetRuleActions(Rule rule, ref string temp)
        {
            if (!temp.StartsWith("POTOM", StringComparison.OrdinalIgnoreCase))
            {
                output.AppendText($"ERROR: Chyba Akcia pre pravidlo '{rule.Name}'.{Environment.NewLine}");
                return false;
            }
            temp = temp.Substring(5).TrimStart();
            if (temp[0] != '(')
            {
                output.AppendText($"ERROR: Za slovom POTOM sa nenachadza '(' v pravidle '{rule.Name}'.{Environment.NewLine}");
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
                        output.AppendText(
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
                            output.AppendText($"ERROR: Neznama akcia:{Environment.NewLine}{actionText}{Environment.NewLine}");
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
                        output.AppendText(
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
                output.AppendText($"ERROR: Chyba podmienka pre pravidlo '{rule.Name}'.{Environment.NewLine}");
                return false;
            }
            temp = temp.Substring(2).TrimStart();
            if (temp[0] != '(')
            {
                output.AppendText($"ERROR: Za slovom AK sa nenachadza '(' v pravidle '{rule.Name}'.{Environment.NewLine}");
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
                        output.AppendText(
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
                        rule.Conditions.Add(temp.Substring(start, index - start));
                    continue;
                }

                if (level == 1)
                {
                    if (!char.IsWhiteSpace(temp[index]))
                    {
                        output.AppendText(
                            $"ERROR: Chyba v pravidle '{rule.Name}'. Medzi podmienkami nemoze byt text.{Environment.NewLine}");
                        return false;
                    }
                }
            }

            if (temp.Length <= index)
            {
                output.AppendText($"ERROR: V pravidle '{rule.Name}' sa nenachadza akcia.{Environment.NewLine}");
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
                output.AppendText(
                    $"ERROR: Chyba pocas parsovania pravidiel! Za poslednym pravidlom sa nachadza neznamy text.{Environment.NewLine}");
                return false;
            }

            rule.Name = temp.Remove(nameEnd++);
            if (rule.Name.Contains('\n') || rule.Name.Contains('\r'))
            {
                output.AppendText($"ERROR: Nazov pravidlu musi byt na jednom riadku '{rule.Name}'.{Environment.NewLine}");
                return false;
            }

            if (temp.Length <= nameEnd)
            {
                output.AppendText(
                    $"ERROR: V pravidle '{rule.Name}' sa nenachadza podmienka a akcia.{Environment.NewLine}");
                return false;
            }
            temp = temp.Substring(nameEnd).TrimStart();
            return true;
        }
    }

    [DebuggerDisplay("Name")]
    internal class Rule
    {
        public string Name;
        public List<string> Conditions = new List<string>();
        public List<MainWindow.Action> Actions = new List<MainWindow.Action>();

        public void Check(string memory)
        {
            /*foreach (var condition in Conditions)
            {
                var reg = new Regex(condition);
                var matchse = reg.Matches(memory);

            }*/
        }
    }
}
