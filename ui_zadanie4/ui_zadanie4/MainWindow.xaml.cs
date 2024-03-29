﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using Microsoft.Win32;

#if ZatvorkyPreFakty
using System.Text.RegularExpressions;
#endif

namespace ui_zadanie4
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly List<Rule> _rules = new List<Rule>();

        private readonly BackgroundWorker _worker = new BackgroundWorker
        {
            WorkerReportsProgress = false,
            WorkerSupportsCancellation = false
        };

        public MainWindow()
        {
            InitializeComponent();
            _worker.DoWork += WorkerOnDoWork;
            _worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
        }


        private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            Dispatcher.Invoke(() =>
            {
                Buttons.IsEnabled = true;
                Memory.IsReadOnly = false;
                Rules.IsReadOnly = false;
            });
        }

        private string GetFakty
        {
            get
            {
                var result = "";
                Memory.Dispatcher.Invoke(() => { result = Memory.Text; });
                return result;
            }
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            Dispatcher.Invoke(() =>
            {
                Buttons.IsEnabled = false;
                Memory.IsReadOnly = true;
                Rules.IsReadOnly = true;
            });
            // Nastala zmena tuto iteraciu?
            var change = true;
            // Posledne pouzite pravidlo v predchadzajucej iteracii
            var lastUsed = _rules.Count;
            var pocitadlo = 1447;
            var spravy = new List<string>(2);
            while (change)
            {
                if (pocitadlo-- == 0)
                {
                    DebugOutput.Dispatcher.Invoke(
                        () =>
                        {
                            DebugOutput.AppendText(
                                $"Spracovanie bolo prerušené (Priveľa iterácií).{Environment.NewLine}");
                        });
                    return;
                }
                change = false;
                Debug.WriteLine(@"========================================");
                for (var i = 0; i < _rules.Count; i++)
                {
                    if (!change && i > lastUsed) break;
                    var rule = _rules[i];
                    Debug.WriteLine(@"----------------------------------------");
                    Debug.WriteLine($@"# Spracovavam pravidlo {rule.Name}");
                    foreach (var @params in rule.Check(GetFakty))
                    {
                        spravy.Clear();
                        DebugOutput.Dispatcher.Invoke(() =>
                        {
                            DebugOutput.AppendText($@"Pravidlo '{rule.Name}': ");
                            foreach (var param in @params)
                                DebugOutput.AppendText($@"[{param.Key}]{param.Value} ");
                            DebugOutput.AppendText(Environment.NewLine);
                        });
                        var zmena = false;
                        foreach (var action in rule.Actions)
                            try
                            {
                                if (action.DoWork(@params))
                                {
                                    zmena = true;
                                    lastUsed = i;
                                    Debug.WriteLine(@"Upravili sa fakty, nastav nutnost dalsej iteracie po aktualne pravidlo (vratane)");
                                }
                                if (action is Eval eval)
                                    @params.Add(eval.Premenna, eval.Vysledok);
                                if (action is Sprava sprava)
                                    spravy.Add(sprava.Value);
                            }
                            catch (Exception ex)
                            {
                                DebugOutput.Dispatcher.Invoke(
                                    () => { DebugOutput.AppendText($"Exception: '{ex}'{Environment.NewLine}"); });
                                return;
                            }
                        change |= zmena;
                        if (zmena)
                            foreach (var sprava in spravy)
                                Output.Dispatcher.Invoke(() => { Output.AppendText(sprava); });
                    }
                }
            }
        }


        private void ParseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Output.Clear();
            DebugOutput.Clear();
            CleanUpMemory();
#if ZatvorkyPreFakty
            if (!CheckMemory()) return;
#endif
            if (!ParseRules()) return;
            _worker.RunWorkerAsync();
        }

#if ZatvorkyPreFakty
        private bool CheckMemory()
        {
            var regex = new Regex("^\\s*(([^\\(\\s].*)|(.*[^\\)\\s]))\\s*$", RegexOptions.Multiline);
            //var regex = new Regex("^\\s*(([^\\(\\s].*)|(.*[^\\)\\s])|(.*\\).*\\(.*)|(.*\\).*\\).*)|(.*\\(.*\\(.*))\\s*$", RegexOptions.Multiline);
            //var regex = new Regex("([^)\\s]+\\s*$)|(^\\s*[^(\\s]+)|(\\)[^\\(]*[^\\(\\s]+[^\\(]*\\()|(\\)[^\\(]*\\))|(\\([^\\)]*\\()", RegexOptions.Multiline);
            var match = regex.Match(Memory.Text);
            if (match.Success)
            {
                Output.AppendText($"ERROR: Chybny zapis faktu:{Environment.NewLine}{match.Value}{Environment.NewLine}");
                return false;
            }
            return true;
        }
#endif

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
            {
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

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Output.Clear();
        }

        private void DebugOutput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
                textBox.ScrollToEnd();
        }
    }
}