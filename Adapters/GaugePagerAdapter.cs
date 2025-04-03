using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.Models;
using Nauti_Control_Wear.ViewModels;
using Nauti_Control_Wear.Views;

namespace Nauti_Control_Wear.Adapters;

public class GaugePagerAdapter : RecyclerView.Adapter
{
    private readonly Context _context;
    private readonly WindGaugeVM _windGaugeVM;
    private readonly DepthGaugeVM _depthGaugeVM;
    private readonly SpeedGaugeVM _speedGaugeVM;
    private readonly CompassGaugeVM _compassGaugeVM;

    public GaugePagerAdapter(Context context)
    {
        _context = context;
        _windGaugeVM = new WindGaugeVM();
        _depthGaugeVM = new DepthGaugeVM();
        _speedGaugeVM = new SpeedGaugeVM();
        _compassGaugeVM = new CompassGaugeVM();
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        View view;
        switch (viewType)
        {
            case 0: // Wind Gauge
                view = new WindGaugeView(_context, _windGaugeVM);
                break;
            case 1: // Depth Gauge
                view = new DepthGaugeView(_context, _depthGaugeVM);
                break;
            case 2: // Speed Gauge
                view = new SpeedGaugeView(_context, _speedGaugeVM);
                break;
            case 3: // Compass Gauge
                view = new CompassGaugeView(_context, _compassGaugeVM);
                break;
            default:
                throw new ArgumentException("Invalid view type");
        }

        return new ViewHolder(view);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        // No binding needed as the views are created with their view models
    }

    public override int ItemCount => 4;

    public override int GetItemViewType(int position)
    {
        return position;
    }

    public void UpdateGaugeData(BoatData data)
    {
        _windGaugeVM.UpdateWindData((float)data.AWA, (float)data.AWS);
        _depthGaugeVM.UpdateValue((float)data.DPT);
        _speedGaugeVM.UpdateSpeedValues((float)data.SOG, (float)data.STW);
        _compassGaugeVM.UpdateCompassData((float)data.HDG, (float)data.COG);
    }

    private class ViewHolder : RecyclerView.ViewHolder
    {
        public ViewHolder(View itemView) : base(itemView)
        {
        }
    }
} 