using System;
using System.Windows.Forms;
using ProgramMain.ExampleForms;


namespace ProgramMain
{
	static class Program
	{
	    static Program()
	    {
            MainForm = null;
	    }

        public static FrmOpticMap MainForm { get; private set; }

	    [STAThread]
        static void Main()
		{
            Application.EnableVisualStyles();

			MainForm = new FrmOpticMap();
            Application.Run(MainForm);
        }
	}
}
