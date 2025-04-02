using Android.Views;
using AndroidX.ViewPager2.Widget;

namespace Nauti_Control_Wear.Views
{
    public class GaugePageTransformer : Java.Lang.Object, ViewPager2.IPageTransformer
    {
        private const float MIN_SCALE = 0.85f;
        private const float MIN_ALPHA = 0.5f;

        public void TransformPage(View page, float position)
        {
            var pageWidth = page.Width;

            if (position < -1)
            {
                // Page is off-screen to the left
                page.Alpha = 0;
                page.ScaleX = MIN_SCALE;
                page.ScaleY = MIN_SCALE;
            }
            else if (position <= 1)
            {
                // Page is visible
                float scaleFactor = System.Math.Max(MIN_SCALE, 1 - System.Math.Abs(position) * 0.15f);
                float alphaFactor = System.Math.Max(MIN_ALPHA, 1 - System.Math.Abs(position) * 0.5f);

                // Position the page
                page.TranslationX = pageWidth * -position;

                // Scale and fade the page
                page.ScaleX = scaleFactor;
                page.ScaleY = scaleFactor;
                page.Alpha = alphaFactor;

                // Add a slight rotation effect
                page.RotationY = position * 15;
            }
            else
            {
                // Page is off-screen to the right
                page.Alpha = 0;
                page.ScaleX = MIN_SCALE;
                page.ScaleY = MIN_SCALE;
            }
        }
    }
} 