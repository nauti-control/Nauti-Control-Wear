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
    public class MainMenuVM
    {
        public List<MenuItemVM> MenuItems { get; set; }

        public MainMenuVM ()
        {
            MenuItems = new List<MenuItemVM>();
            MenuItems.Add(new MenuItemVM("-1"));
            MenuItems.Add(new MenuItemVM("+1"));
            MenuItems.Add(new MenuItemVM("-10"));
            MenuItems.Add(new MenuItemVM("+10"));
            MenuItems.Add(new MenuItemVM("Port Tack"));
            MenuItems.Add(new MenuItemVM("Stb Tack"));
            MenuItems.Add(new MenuItemVM("Stand By"));
            MenuItems.Add(new MenuItemVM("Auto"));
            MenuItems.Add(new MenuItemVM("Track"));
            MenuItems.Add(new MenuItemVM("Wind Mode"));
            MenuItems.Add(new MenuItemVM("MOB Alert"));
            MenuItems.Add(new MenuItemVM("Cancel MOB"));
            MenuItems.Add(new MenuItemVM("Start Timer"));



        }

        public void OnItemClick(object sender, int position)
        {
            int photoNum = position + 1;
            //Toast.MakeText(this, "This is photo number " + photoNum, ToastLength.Short).Show();
        }
    }
}