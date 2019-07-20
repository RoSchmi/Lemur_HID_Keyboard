// Copyright RoSchmi 2019, License Apache 2.0
// This is an USB HID Client for the GHI Lemur Mainboard for NETMF C#
// Normally the Mainboard uses the USB Connector for deploying and debugging code
// To use the USB connector for the USB HID Keyboard the Mode Pin must be pulled to GND when the board is powered up
// (see picture)
// You can load the program to the board via USB but then the App crashes since the USB connector can not be used as HID client
// --> Disconnect the board, pull the Mode Pin to GND and repower, now the USB HID Client should work
//
// The App can be used to send long passwords, e.g. to open your password save (e.g. KeyPass)
// Replace the content of the string variable 'PrintString' with your password and deploy the program.
// When the board is connected, click into the wanted textfield, then first press the button LDR1 and within the next second button LDR0
// if you wait to long, nothing is printed
//
// Not all possible characters are supported, please change the code that the app fits your demands (e.g. language specific keyboards)
//
// Caution: This is not really a save way how you should store your password but it is better than a note under your mousepad
// 
// Be sure that you delete the password in the program code after deploying the code to the board
// Be aware that everone who knows how the app works (Button Presses) can steal your password
// Be aware that the code with the password can be retrieved by skilled persons
// Protect the board from other persons
// There may be other pitfalls 

using System;
using System.Threading;
using GHI.Usb;
using GHI.Usb.Client;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.UsbClient;
using RoSchmi.ButtonNETMF;
using System.Collections;
//using System.Text.RegularExpressions;

namespace Lemur_HID_Keyboard
{
    public class Program
    {
        public static string PrintString = "VeryLongPassword";

        public static ButtonNETMF LDR1Button;
        public static ButtonNETMF LDR0Button;
        public static OutputPort LED1;
        public static OutputPort LED2;      
        public static int intervalButtonPressMs = 1000;    // 1000 ms        
        public static Timer intervalTimer = new Timer(new TimerCallback(IntervalTimer_Tick), null, new TimeSpan(10, 0, 0, 0, 0), new TimeSpan(10, 0, 0, 0, 0));  // very long time      
        private static Keyboard kb;
        public static bool intervalIsOpen = false;

        public static System.Collections.Hashtable KeyTable = new Hashtable();

        // Regex couldn't be used. Not enough memory
        //public static Regex _Regex = new Regex(@"^[a-z]+!§$%&/()=?");

        public static void Main()
        {
            // Fill Hashtable with character-Key-pairs
            KeyTable.Add('a', Key.A);
            KeyTable.Add('b', Key.B);
            KeyTable.Add('c', Key.C);
            KeyTable.Add('d', Key.D);
            KeyTable.Add('e', Key.E);
            KeyTable.Add('f', Key.F);
            KeyTable.Add('g', Key.G);
            KeyTable.Add('h', Key.H);
            KeyTable.Add('i', Key.I);
            KeyTable.Add('j', Key.J);
            KeyTable.Add('k', Key.K);
            KeyTable.Add('l', Key.L);
            KeyTable.Add('m', Key.M);
            KeyTable.Add('n', Key.N);
            KeyTable.Add('o', Key.O);
            KeyTable.Add('p', Key.P);
            KeyTable.Add('q', Key.Q);
            KeyTable.Add('r', Key.R);
            KeyTable.Add('s', Key.S);
            KeyTable.Add('t', Key.T);
            KeyTable.Add('u', Key.U);
            KeyTable.Add('v', Key.V);
            KeyTable.Add('w', Key.W);
            KeyTable.Add('x', Key.X);
            KeyTable.Add('y', Key.Y);
            KeyTable.Add('z', Key.Z);          
            KeyTable.Add('0', Key.D0);
            KeyTable.Add('1', Key.D1);
            KeyTable.Add('2', Key.D2);
            KeyTable.Add('3', Key.D3);
            KeyTable.Add('4', Key.D4);
            KeyTable.Add('5', Key.D5);
            KeyTable.Add('6', Key.D6);
            KeyTable.Add('7', Key.D7);
            KeyTable.Add('8', Key.D8);
            KeyTable.Add('9', Key.D9);
            KeyTable.Add('!', Key.D1);
            KeyTable.Add('§', Key.D3);
            KeyTable.Add('$', Key.D4);
            KeyTable.Add('%', Key.D5);
            KeyTable.Add('&', Key.D6);
            KeyTable.Add('/', Key.D7);
            KeyTable.Add('(', Key.D8);
            KeyTable.Add(')', Key.D9);
            KeyTable.Add('=', Key.D0);           
            KeyTable.Add(' ', Key.Space);
                       
            kb = new Keyboard();
            Controller.ActiveDevice = kb;
       
            LED1 = new OutputPort(GHI.Pins.FEZLemur.Gpio.Led1, false);
            LED2 = new OutputPort(GHI.Pins.FEZLemur.Gpio.Led2, false);

            LDR1Button = new ButtonNETMF(GHI.Pins.FEZLemur.Gpio.Ldr1, GHI.Pins.FEZLemur.Gpio.A0);
            LDR0Button = new ButtonNETMF(GHI.Pins.FEZLemur.Gpio.Ldr0, GHI.Pins.FEZLemur.Gpio.A1);
            
            LDR1Button.ButtonPressed += LDR1Button_ButtonPressed;
            LDR0Button.ButtonPressed += LDR0Button_ButtonPressed;
           
            Thread.Sleep(Timeout.Infinite);            
        }

        static void LDR1Button_ButtonPressed(ButtonNETMF sender, ButtonNETMF.ButtonState state)
        {
            intervalIsOpen = true;
            LED1.Write(true);
            intervalTimer.Change(intervalButtonPressMs, 10 * 1440 * 60 * 1000);   // period is a very long time
            
        }
        static void LDR0Button_ButtonPressed(ButtonNETMF sender, ButtonNETMF.ButtonState state)
        {
            if (!intervalIsOpen)
            {
               
                return;
            }
            else
            {
                LED2.Write(true);
                
                if (Controller.State == UsbController.PortState.Running)
                {
                    for (int i = 0; i < PrintString.Length; i++)
                    {
                        TypeChar(PrintString[i]);
                    }
                }
                Thread.Sleep(1000);   // for debouncing, send only one time
            }          
        }

        private static void TypeChar(char _char)
        {
            char[] specialCharacters = new char[] { '!', '§', '$', '%', '&', '/', '(', ')', '='};
            bool typeReleaseLeftShift = false;          
            if (((_char > 'A') && (_char <= 'Z')) || (Array.IndexOf(specialCharacters, _char) > -1))
            {
                kb.Press(Key. LeftShift);
                typeReleaseLeftShift = true;
            }

            Key KeyToPrint = Key.Space;
            try
            {
                KeyToPrint = (Key)KeyTable[_char.ToLower()];
            }
            catch { }

            kb.Stroke(KeyToPrint);

            if (typeReleaseLeftShift)
            {
                kb.Release(Key.LeftShift);
            }
        }

        private static void IntervalTimer_Tick(object state)
        {
            intervalIsOpen = false;
            LED1.Write(false);
            LED2.Write(false);
        }    
    }
}








