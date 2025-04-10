using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear.Adapters
{
    public class CommandViewHolder : RecyclerView.ViewHolder
    {
        private readonly TextView _menuItem;
        private CommandItemVM _menuItemVM = null!;

        public CommandItemVM MenuItemVM
        {
            get => _menuItemVM;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _menuItemVM = value;
                _menuItem.Text = value.MenuText;
            }
        }

        public CommandViewHolder(View itemView, Action<int> listener) : base(itemView ?? throw new ArgumentNullException(nameof(itemView)))
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            var menuItem = itemView.FindViewById<TextView>(Resource.Id.menu_text);
            if (menuItem == null)
            {
                throw new InvalidOperationException("Could not find menu_text view");
            }
            _menuItem = menuItem;

            itemView.Click += (sender, e) => listener(LayoutPosition);
        }
    }
}

