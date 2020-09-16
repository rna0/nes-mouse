using System;
using System.Windows.Forms;

namespace nes_mouse
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.Run(new NESMouse());
		}
	}
}
