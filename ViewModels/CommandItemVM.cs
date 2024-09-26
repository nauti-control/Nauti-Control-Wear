using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nauti_Control_Wear.ViewModels
{
    public  class CommandItemVM
    {
        /// <summary>
        /// Menu Text
        /// </summary>
        public string MenuText { get; set; }

        /// <summary>
        /// Command
        /// </summary>
        public int Command { get; set; }


        /// <summary>
        /// Command Item VM Constructor
        /// </summary>
        /// <param name="menuText">Menu Text</param>
        /// <param name="command">Command</param>
        public CommandItemVM(string menuText, int command)
        {
            MenuText = menuText;
            Command = command;  
        }
    }
}