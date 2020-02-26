using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using SlimDX.DirectInput;

namespace nes_mouse
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            sticks = GetSticks();
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
        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < sticks.Length; i++)
            {
                StickHandlingLogic(sticks[i], i);
            }
        }
        void StickHandlingLogic(Joystick stick, int id)
        {
            // Creates an object from the class JoystickState.
            JoystickState state = new JoystickState();

            state = stick.GetCurrentState(); //Gets the state of the joystick
            //These are for the thumbstick readings
            yValue = state.Y;
            xValue = state.X;
            MouseMoved(xValue, yValue);


            buttons = state.GetButtons(); // Stores the number of each button on the gamepad into the bool[] butons.
                                          // Console.WriteLine("# of button = " + buttons.Length);
                                          //Here is an example on how to use this for the joystick in the first index of the array list
            if (id == 0)
            {
                // This is when button 0 of the gamepad is pressed, the label will change. Button 0 should be the square button.
                if (buttons[0])//botton B is on
                {
                    if(!mouseRC)
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
                if (buttons[1])//botton A is on
                {
                    if(!mouseLC)
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
                if (buttons[8])//SELECT botton is on
                {
                }
                if (buttons[9])//START botton is on
                {
                }
            }
        }
        public void MouseMoved(int posx, int posy)
        {
            Cursor.Position = new Point(Cursor.Position.X + posx/20, Cursor.Position.Y + posy/20);
            Cursor.Clip = new Rectangle(Location, Size);
        }
    }
}
