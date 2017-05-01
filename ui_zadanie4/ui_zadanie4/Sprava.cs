using System.Collections.Generic;
using System.Windows;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Sprava : Action
        {
            public Sprava(string input, MainWindow window, bool important) : base(input, window, important)
            {
            }

            public override bool DoWork(IReadOnlyDictionary<string, string> parameters)
            {
                Presiel = false;
                var output = ToString(parameters, false);
#if ZatvorkyPreFakty
                Window.Output.AppendText(temp.Substring(1, temp.Length-2));
                if(!Important)
                    Window.Output.AppendText("\n");
#else
                Window.Output.AppendText(output);
                if(!Important)
                    Window.Output.AppendText("\n");
#endif
                Presiel = true;
                return false;
            }
        }
    }
}