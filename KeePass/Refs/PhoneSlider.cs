using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KeePass.Refs
{
    public class PhoneSlider : Slider
    {
        public PhoneSlider()
        {
            SizeChanged += PhoneSlider_SizeChanged;
        }

        private void PhoneSlider_SizeChanged(
            object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0)
                return;

            var clipRect = new Rect(0, 0,
                e.NewSize.Width, e.NewSize.Height);

            if (Orientation == Orientation.Horizontal)
            {
                clipRect.X -= 12;
                clipRect.Width += 24;

                var margin = Resources["PhoneHorizontalMargin"];
                if (margin != null)
                    Margin = (Thickness)margin;
            }
            else
            {
                clipRect.Y -= 12;
                clipRect.Height += 24;

                var margin = Resources["PhoneVerticalMargin"];
                if (margin != null)
                    Margin = (Thickness)margin;
            }

            Clip = new RectangleGeometry
            {
                Rect = clipRect
            };
        }
    }
}