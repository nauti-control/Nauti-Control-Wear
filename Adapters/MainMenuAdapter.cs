
using Android.Views;

using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.ViewModels;


namespace Nauti_Control_Wear.Adapters
{
    public class MainMenuAdapter : RecyclerView.Adapter
    {

        List<MenuItemVM> _menuItems;

        public event EventHandler<MenuItemVM> ItemClick;

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

        private void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, _menuItems[position]);
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            MainMenuViewHolder vh = holder as MainMenuViewHolder;

            vh.MenuItemVM = _menuItems[position];

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            // Inflate the CardView for the photo:
            View? itemView = LayoutInflater.From(parent.Context).Inflate(_Microsoft.Android.Resource.Designer.ResourceConstant.Layout.main_menu_item, parent, false);

            // Create a ViewHolder to hold view references inside the CardView:
            MainMenuViewHolder vh = new MainMenuViewHolder(itemView, OnClick);
            return vh;
        }
    }
}