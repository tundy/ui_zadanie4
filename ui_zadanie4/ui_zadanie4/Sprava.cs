using System.Collections.Generic;

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
                var output = ToString(parameters, false);
#if ZatvorkyPreFakty
                Window.Output.AppendText($"{temp.Substring(1, temp.Length-2)}\n");
#else
                Window.Output.AppendText($"{output}\n");
#endif
                Presiel = true;
                return false;
            }
        }
    }
}