using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace ui_zadanie4
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<Rule> _rules = new List<Rule>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ParseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Output.Clear();
            DebugOutput.Clear();
            CleanUpMemory();
            if (!CheckMemory()) return;
            if (!ParseRules()) return;

            // Nastala zmena tuto iteraciu?
            var change = true;
            // Posledne pouzite pravidlo v predchadzajucej iteracii
            var lastUsed = _rules.Count;
            while (change)
            {
                change = false;
                for (var i = 0; i < _rules.Count; i++)
                {
                    if (!change && i >= lastUsed) continue;
                    var rule = _rules[i];
                    foreach (var @params in rule.Check(Memory.Text))
                    {
                        DebugOutput.AppendText($@"Pravidlo '{rule.Name}': ");
                        foreach (var param in @params)
                            DebugOutput.AppendText($@"[{param.Key}]{param.Value} ");
                        DebugOutput.AppendText(Environment.NewLine);
                        foreach (var action in rule.Actions)
                            if (action.DoWork(@params))
                            {
                                change = true;
                                lastUsed = i;
                            }
                    }
                }
            }
        }

        private bool CheckMemory()
        {
            var regex = new Regex("^\\s*(([^\\(\\s].*)|(.*[^\\)\\s])|(.*\\).*\\(.*)|(.*\\).*\\).*)|(.*\\(.*\\(.*))\\s*$", RegexOptions.Multiline);
            //var regex = new Regex("([^)\\s]+\\s*$)|(^\\s*[^(\\s]+)|(\\)[^\\(]*[^\\(\\s]+[^\\(]*\\()|(\\)[^\\(]*\\))|(\\([^\\)]*\\()", RegexOptions.Multiline);
            var match = regex.Match(Memory.Text);
            if (match.Success)
            {
                Output.AppendText($"ERROR: Chybny zapis faktu:{Environment.NewLine}{match.Value}{Environment.NewLine}");
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Remove duplicates and Empty lines from memory Textbox
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
                Output.AppendText(
                    $"ERROR: Za slovom POTOM sa nenachadza '(' v pravidle '{rule.Name}'.{Environment.NewLine}");
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
                        /*Output.AppendText(
                            $"ERROR: V akcii pre pravidlo '{rule.Name}' je trojite vnorenie zatvoriek.{Environment.NewLine}");
                        return false;*/
                    }
                    else
                    {
                        start = index + 1;
                    }
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
                            Output.AppendText(
                                $"ERROR: Neznama akcia:{Environment.NewLine}{actionText}{Environment.NewLine}");
                            return false;
                        }
                        rule.Actions.Add(action);
                    }
                    continue;
                }

                if (level == 1)
                    if (!char.IsWhiteSpace(temp[index]))
                    {
                        Output.AppendText(
                            $"ERROR: Chyba v pravidle '{rule.Name}'. Medzi akciami nemoze byt text.{Environment.NewLine}");
                        return false;
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
                Output.AppendText(
                    $"ERROR: Za slovom AK sa nenachadza '(' v pravidle '{rule.Name}'.{Environment.NewLine}");
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
                        /*Output.AppendText(
                            $"ERROR: V podmienke pre pravidlo '{rule.Name}' je trojite vnorenie zatvoriek.{Environment.NewLine}");
                        return false;*/
                    }
                    else
                    {
                        start = index + 1;
                    }
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
                    if (!char.IsWhiteSpace(temp[index]))
                    {
                        Output.AppendText(
                            $"ERROR: Chyba v pravidle '{rule.Name}'. Medzi podmienkami nemoze byt text.{Environment.NewLine}");
                        return false;
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

        private void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = false,
                ReadOnlyChecked = true,
                ShowReadOnly = false,
                DefaultExt = "xml"
            };
            if (dialog.ShowDialog(this) == true)
                using (var stream = File.OpenRead(dialog.FileName))
                {
                    var serializer = new XmlSerializer(typeof(Save));
                    try
                    {
                        var save = serializer.Deserialize(stream) as Save;
                        if (save == null) return;
                        Memory.Text = save.Fakty;
                        Rules.Text = save.Pravidla;
                        Output.Text = save.Vystup;
                        DebugOutput.Text = "Uspesne Nacitanie suboru: " + dialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        DebugOutput.AppendText(ex + Environment.NewLine);
                    }
                }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var save = new Save
            {
                Fakty = Memory.Text,
                Pravidla = Rules.Text,
                Vystup = Output.Text
            };

            var dialog = new SaveFileDialog
            {
                OverwritePrompt = true,
                CreatePrompt = false,
                AddExtension = true,
                DefaultExt = "xml"
            };
            if (dialog.ShowDialog(this) == true)
                using (var stream = File.Create(dialog.FileName))
                {
                    try
                    {
                        var serializer = new XmlSerializer(typeof(Save));
                        serializer.Serialize(stream, save);
                    }
                    catch (Exception ex)
                    {
                        DebugOutput.AppendText(ex + Environment.NewLine);
                    }
                }
        }
    }
}