using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nauti_Control_Wear
{
    public class MainMenuAdapter : RecyclerView.Adapter
    {

        List<MenuItemVM> _menuItems;

        public event EventHandler<int> ItemClick;

        public MainMenuAdapter(MainMenuVM mainMenuVM)
        {
            _menuItems = mainMenuVM.MenuItems;
        }
        public override int ItemCount
        {
            get
            {
                return _menuItems.Count;
            }
        }
        

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            MainMenuViewHolder vh = holder as MainMenuViewHolder;

       
            
            // Load the photo caption from the photo album:
            vh.MenuItem.Text = _menuItems[position].MenuText;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Inflate the CardView for the photo:
            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.main_menu_item, parent, false);

            // Create a ViewHolder to hold view references inside the CardView:
            MainMenuViewHolder vh = new MainMenuViewHolder(itemView);
            return vh;
        }
    }
}