
using Android.Views;

using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.ViewModels;


namespace Nauti_Control_Wear.Adapters
{
    public class CommandAdapter : RecyclerView.Adapter
    {

        List<CommandItemVM> _commandItems;

        public event EventHandler<CommandItemVM> ItemClick;

        public CommandAdapter(CommandMenuVM commandMenuVM)
        {
            _commandItems = commandMenuVM.MenuItems;
        }
        public override int ItemCount
        {
            get
            {
                return _commandItems.Count;
            }
        }

        private void OnClick(int position)
        {
            if (ItemClick != null)
                ItemClick(this, _commandItems[position]);
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CommandViewHolder vh = holder as CommandViewHolder;

            vh.MenuItemVM = _commandItems[position];

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            // Inflate the CardView for the photo:
            View? itemView = LayoutInflater.From(parent.Context).Inflate(_Microsoft.Android.Resource.Designer.ResourceConstant.Layout.command_item, parent, false);

            // Create a ViewHolder to hold view references inside the CardView:
            CommandViewHolder vh = new CommandViewHolder(itemView, OnClick);
            return vh;
        }
    }
}