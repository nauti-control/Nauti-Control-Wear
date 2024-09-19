using Android.Views;
using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear.Adapters
{
    public class CommandViewHolder : RecyclerView.ViewHolder
    {
        public TextView MenuItem { get; private set; }

        private CommandItemVM _menuItemVM;
        public CommandItemVM MenuItemVM
        {
            get
            {
                return _menuItemVM;
            }

            set
            {
                MenuItem.Text = value.MenuText;
                _menuItemVM = value;
            }
        }


        public CommandViewHolder(View itemView,Action<int> listener) : base(itemView)
        {


            MenuItem = itemView.FindViewById<TextView>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.menu_text);
            itemView.Click += (sender, e) => listener(base.LayoutPosition);

        }


    }
}

