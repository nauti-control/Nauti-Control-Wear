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
    public  class MenuItemVM
    {
        public string MenuText { get; set; }


        public MenuItemVM(string menuText)
        {
            MenuText = menuText;

        }
    }
}