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
            MenuItems.Add(new MenuItemVM("-1",0));
            MenuItems.Add(new MenuItemVM("+1",1));
            MenuItems.Add(new MenuItemVM("-10",2));
            MenuItems.Add(new MenuItemVM("+10",3));
            MenuItems.Add(new MenuItemVM("Port Tack",4));
            MenuItems.Add(new MenuItemVM("Stb Tack",5));
            MenuItems.Add(new MenuItemVM("Auto", 6));
            MenuItems.Add(new MenuItemVM("Stand By",7));
          
            MenuItems.Add(new MenuItemVM("Track",8));
            MenuItems.Add(new MenuItemVM("Wind Mode",9));
            MenuItems.Add(new MenuItemVM("MOB Alert",10));
            MenuItems.Add(new MenuItemVM("Cancel MOB",11));
            MenuItems.Add(new MenuItemVM("Start Timer",12));



        }

        public void OnItemClick(object sender, int position)
        {

            // 
            string test = position.ToString();
        }
    }
}