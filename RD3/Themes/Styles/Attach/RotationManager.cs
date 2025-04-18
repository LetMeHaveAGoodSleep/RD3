using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RD3
{
    public class RotationManager
    {
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.RegisterAttached("Angle", typeof(double), typeof(RotationManager), new PropertyMetadata(0.0, OnAngleChanged));

        public static double GetAngle(DependencyObject obj)
        {
            return (double)obj.GetValue(AngleProperty);
        }

        public static void SetAngle(DependencyObject obj, double value)
        {
            obj.SetValue(AngleProperty, value);
        }

        private static void OnAngleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if(obj is UIElement element)
            {
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                element.RenderTransform = new RotateTransform((double)e.NewValue);
            }
        }
    }
}
