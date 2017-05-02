using System.Collections.Generic;

namespace ui_zadanie4
{
    public partial class MainWindow
    {
        private class Sprava : Action
        {
            public string Value { get; private set; }

            public Sprava(string input, MainWindow window) : base(input, window)
            {
            }

            public override bool DoWork(IReadOnlyDictionary<string, string> parameters)
            {
                var output = ToString(parameters, false);
#if ZatvorkyPreFakty
                Value = output.Substring(1, output.Length-2) + '\n';
#else
                Value = output + '\n';
#endif
                return false;
            }
        }
    }
}