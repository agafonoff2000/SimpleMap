using System.Windows.Forms;

namespace ProgramMain.ExampleForms
{
    public static class Miscellaneous
    {
        public static System.Drawing.Icon DefaultIcon()
        {
            return System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        public static string GetAssemblyTitle()
        {
            var aTitle = "Simple Map";
            var thisAssembly = Program.MainForm.GetType().Assembly;
            var attributes = thisAssembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
            if (attributes.Length == 1)
            {
               aTitle = ((System.Reflection.AssemblyTitleAttribute) attributes[0]).Title;
            }
            return aTitle;
        }
    }
}
