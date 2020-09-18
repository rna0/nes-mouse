using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SlimDX.DirectInput;
using System.Drawing;

namespace nes_mouse
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : Form
	{
		private ContextMenu contextMenu1;
		private List<MenuItem> menuItemX;
		private NotifyIcon NES_notif;
		private MenuItem exitMenuItem;
		private Timer timer1;
		private System.ComponentModel.IContainer components;

		public Form1()
		{
			sticks = GetSticks();
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			InitializeContextmenu();
			//
			if (sticks.Length==0)
				System.Environment.Exit(1);
			stick = sticks[0];
			timer1.Enabled = true;
			timer1.Interval = 1;
		}

		// used from directX sdk - slimDX
		// setting up values needed for activation
		DirectInput input = new DirectInput();
		Joystick stick;
		Joystick[] sticks;
		//Thumstick variables.
		int yValue = 0;
		int xValue = 0;
		int velocity = 20;
		//right and left click
		bool mouseLC = false;
		bool mouseRC = false;
		bool[] buttons;

		[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		public static extern void mouse_event(uint flag, uint _X, uint _y, uint btn, uint exInfo);
		private const int MOUSE_EVENT_LEFTDOWN = 0x02;
		private const int MOUSE_EVENT_LEFTUP = 0x04;
		private const int MOUSE_EVENT_RIGHTDOWN = 0x08;
		private const int MOUSE_EVENT_RIGHTUP = 0x10;
		public const int WM_MOUSEWHEEL = 0x020A;
		private void timer1_Tick(object sender, EventArgs e)
		{
			StickHandlingLogic(stick);
		}
		public Joystick[] GetSticks()
		{

			List<Joystick> sticks = new List<Joystick>(); // Creates the list of joysticks connected to the computer via USB.

			foreach (DeviceInstance device in input.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
			{
				// Creates a joystick for each game device in USB Ports
				try
				{
					stick = new Joystick(input, device.InstanceGuid);
					stick.Acquire();

					// Gets the joysticks properties and sets the range for them.
					foreach (DeviceObjectInstance deviceObject in stick.GetObjects())
					{
						if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
							stick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-100, 100);
					}

					// Adds how ever many joysticks are connected to the computer into the sticks list.
					sticks.Add(stick);
				}
				catch (DirectInputException)
				{
				}
			}
			return sticks.ToArray();
		}
		void StickHandlingLogic(Joystick stick)
		{
			// Creates an object from the class JoystickState.
			JoystickState state = new JoystickState();

			state = stick.GetCurrentState(); //Gets the state of the joystick
											 //These are for the thumbstick readings
			yValue = state.Y;
			xValue = state.X;


			buttons = state.GetButtons(); // Stores the number of each button on the gamepad into the bool[] butons.
										  // Console.WriteLine("# of button = " + buttons.Length);
										  //Here is an example on how to use this for the joystick in the first index of the array list

			MouseMoved(xValue, yValue);
			// This is when button 0 of the gamepad is pressed, the label will change. Button 0 should be the square button.
			if (buttons[1])//botton A is on
			{
				if (!mouseRC)
				{
					mouse_event(MOUSE_EVENT_RIGHTDOWN, 0, 0, 0, 0);
					mouseRC = true;
				}
			}
			else if (mouseRC)
			{
				mouse_event(MOUSE_EVENT_RIGHTUP, 0, 0, 0, 0);
				mouseRC = false;
			}
			if (buttons[0])//botton B is on
			{
				if (!mouseLC)
				{
					mouse_event(MOUSE_EVENT_LEFTDOWN, 0, 0, 0, 0);
					mouseLC = true;
				}
			}
			else if (mouseLC)
			{
				mouse_event(MOUSE_EVENT_LEFTUP, 0, 0, 0, 0);
				mouseLC = false;
			}
			//for select and start uses
			
			if (buttons[8] && velocity < 100)//SELECT botton is on
			{
				velocity += 5;
			}
			if (buttons[9] && velocity > 5)//START botton is on
			{
				velocity -= 5;
			}
		}
		public void MouseMoved(int posx, int posy)
		{
			Cursor.Position = new Point(Cursor.Position.X + posx / velocity, Cursor.Position.Y + posy / velocity);
			//Cursor.Clip = new Rectangle(Location, Size);
		}
		 

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		private void InitializeContextmenu()
		{
			this.menuItemX = new List<MenuItem>();
			this.exitMenuItem = new System.Windows.Forms.MenuItem();
			for (int i = 0; i < sticks.Length; i++)
			{
				menuItemX.Add(new MenuItem());
				// 
				// menuItemX
				// 
				menuItemX[i].Text = "JoyStick number " + i.ToString();
				int itemNumber= new int();
				itemNumber = i;
				menuItemX[i].Click += delegate (object sender, EventArgs e) { menuItemX_Click(sender, e, itemNumber); };
				//new System.EventHandler(this.menuItemX_Click);

				this.contextMenu1.MenuItems.AddRange(new MenuItem[] { this.menuItemX[i] });
			}
			// 
			// exitMenuItem
			// 
			this.exitMenuItem.Text = "E&xit";
			this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
			this.contextMenu1.MenuItems.AddRange(new MenuItem[] { this.exitMenuItem });
		}
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			System.Configuration.AppSettingsReader configurationAppSettings = new System.Configuration.AppSettingsReader();
			this.NES_notif = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// NES_notif
			// 
			this.NES_notif.ContextMenu = this.contextMenu1;
			this.NES_notif.Icon = ((System.Drawing.Icon)(resources.GetObject("NES_notif.Icon")));
			this.NES_notif.Text = "NES Mouse";
			this.NES_notif.Visible = true;
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.Name = "Form1";
			this.Opacity = ((double)(configurationAppSettings.GetValue("Form1.Opacity", typeof(double))));
			this.ShowInTaskbar = ((bool)(configurationAppSettings.GetValue("Form1.ShowInTaskbar", typeof(bool))));
			this.Text = "Form1";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.Run(new Form1());
		}


		private void menuItemX_Click(object sender, System.EventArgs e, int id)
		{
			//MessageBox.Show(id.ToString());
			stick = sticks[id];
		}


		private void exitMenuItem_Click(object sender, System.EventArgs e)
		{
			Dispose(true);
			if (System.Windows.Forms.Application.MessageLoop)
			{
				// WinForms app
				System.Windows.Forms.Application.Exit();
			}
			else
			{
				// Console app
				System.Environment.Exit(1);
			}
		}

	}
}
