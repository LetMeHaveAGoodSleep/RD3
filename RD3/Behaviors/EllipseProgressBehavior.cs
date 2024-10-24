﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ImTools;
using Microsoft.Xaml.Behaviors;

namespace RD3.Behaviors
{
    public class EllipseProgressBehavior : Behavior<Ellipse>
    {
        /// <summary>
        ///     标识 EndAngle 依赖属性。
        /// </summary>
        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register(nameof(EndAngle), typeof(double), typeof(EllipseProgressBehavior),
                new PropertyMetadata(default(double), OnEndAngleChanged));

        /// <summary>
        ///     标识 Progress 依赖属性。
        /// </summary>
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(EllipseProgressBehavior),
                new PropertyMetadata(0d, OnProgressChanged));

        /// <summary>
        ///     标识 StartAngle 依赖属性。
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(EllipseProgressBehavior),
                new PropertyMetadata(default(double), OnStartAngleChanged));

        private double _normalizedMaxAngle;
        private double _normalizedMinAngle;

        /// <summary>
        ///     获取或设置EndAngle的值
        /// </summary>
        public double EndAngle
        {
            get => (double)GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        /// <summary>
        ///     获取或设置Progress的值
        /// </summary>
        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        /// <summary>
        ///     获取或设置StartAngle的值
        /// </summary>
        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        protected virtual double GetTotalLength()
        {
            if (AssociatedObject == null || AssociatedObject.ActualHeight == 0)
            {
                return 0;
            }

            return (AssociatedObject.ActualHeight - AssociatedObject.StrokeThickness) * Math.PI *
                (_normalizedMaxAngle - _normalizedMinAngle) / 360;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            UpdateAngle();
            UpdateStrokeDashArray();
            AssociatedObject.SizeChanged += (s, e) => { UpdateStrokeDashArray(); };
        }

        /// <summary>
        ///     EndAngle 属性更改时调用此方法。
        /// </summary>
        /// <param name="oldValue">EndAngle 属性的旧值。</param>
        /// <param name="newValue">EndAngle 属性的新值。</param>
        protected virtual void OnEndAngleChanged(double oldValue, double newValue)
        {
            UpdateAngle();
            UpdateStrokeDashArray();
        }

        protected virtual void OnProgressChanged(double oldValue, double newValue)
        {
            UpdateStrokeDashArrayByAnimation(oldValue, newValue);
        }

        /// <summary>
        ///     StartAngle 属性更改时调用此方法。
        /// </summary>
        /// <param name="oldValue">StartAngle 属性的旧值。</param>
        /// <param name="newValue">StartAngle 属性的新值。</param>
        protected virtual void OnStartAngleChanged(double oldValue, double newValue)
        {
            UpdateAngle();
            UpdateStrokeDashArray();
        }

        private static double Mod(double number, double divider)
        {
            var result = number % divider;
            result = result < 0 ? result + divider : result;
            return result;
        }

        private static void OnEndAngleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var oldValue = (double)args.OldValue;
            var newValue = (double)args.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            var target = obj as EllipseProgressBehavior;
            target?.OnEndAngleChanged(oldValue, newValue);
        }

        private static void OnProgressChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var target = obj as EllipseProgressBehavior;
            var oldValue = (double)args.OldValue;
            var newValue = (double)args.NewValue;
            if (oldValue != newValue)
            {
                target.OnProgressChanged(oldValue, newValue);
            }
        }

        private static void OnStartAngleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var oldValue = (double)args.OldValue;
            var newValue = (double)args.NewValue;
            if (oldValue == newValue)
            {
                return;
            }

            var target = obj as EllipseProgressBehavior;
            target?.OnStartAngleChanged(oldValue, newValue);
        }

        private void UpdateAngle()
        {
            UpdateNormalizedAngles();
            if (AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.RenderTransformOrigin = new Point(0.5, 0.5);
            if (AssociatedObject.RenderTransform is RotateTransform transform)
            {
                transform.Angle = _normalizedMinAngle - 90;
            }
            else
            {
                AssociatedObject.RenderTransform = new RotateTransform { Angle = _normalizedMinAngle - 90 };
            }
        }

        private void UpdateNormalizedAngles()
        {
            var result = Mod(StartAngle, 360);

            if (result >= 180)
            {
                result -= 360;
            }

            _normalizedMinAngle = result;

            result = Mod(EndAngle, 360);

            if (result < 180)
            {
                result += 360;
            }

            if (result > _normalizedMinAngle + 360)
            {
                result -= 360;
            }

            _normalizedMaxAngle = result;
        }

        private void UpdateStrokeDashArray()
        {
            if (AssociatedObject == null || AssociatedObject.StrokeThickness == 0)
            {
                return;
            }

            var totalLength = GetTotalLength();
            if (totalLength == 0)
            {
                return;
            }

            totalLength /= AssociatedObject.StrokeThickness;
            var progressLenth = Progress * totalLength / 100;

            var result = new DoubleCollection { progressLenth, double.MaxValue };

            AssociatedObject.StrokeDashArray = result;
        }

        private void UpdateStrokeDashArrayByAnimation(double oldValue, double newValue)
        {
            double difference = Math.Abs(newValue - oldValue);
            if (difference < 10)
            {
                UpdateStrokeDashArray();
            }

            if (AssociatedObject == null || AssociatedObject.StrokeThickness == 0)
            {
                return;
            }

            var totalLength = GetTotalLength();
            if (totalLength == 0)
            {
                return;
            }
            double average = difference / 3;
            // 创建一个 Storyboard
            Storyboard storyboard = new Storyboard();

            // 创建一个 ObjectAnimationUsingKeyFrames
            ObjectAnimationUsingKeyFrames dashArrayAnimation = new ObjectAnimationUsingKeyFrames
            {
                Duration = new Duration(System.TimeSpan.FromSeconds(2))
            };
            if (newValue > oldValue)
            {
                for (var i = 1; i <= 3; i++)
                {
                    // 创建关键帧
                    DiscreteObjectKeyFrame keyFrame = new DiscreteObjectKeyFrame
                    {
                        KeyTime = KeyTime.FromTimeSpan(System.TimeSpan.FromSeconds(0)),
                        Value = new DoubleCollection { oldValue + i * average, double.MaxValue }
                    };
                    dashArrayAnimation.KeyFrames.Add(keyFrame);
                }
            }
            else
            {
                for (var i = 1; i <= 3; i++)
                {
                    // 创建关键帧
                    DiscreteObjectKeyFrame keyFrame = new DiscreteObjectKeyFrame
                    {
                        KeyTime = KeyTime.FromTimeSpan(System.TimeSpan.FromSeconds(0)),
                        Value = new DoubleCollection { newValue - i * average, double.MaxValue }
                    };
                    dashArrayAnimation.KeyFrames.Add(keyFrame);
                }
            }
            // 创建关键帧
            DiscreteObjectKeyFrame keyFrame1 = new DiscreteObjectKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(System.TimeSpan.FromSeconds(0)),
                Value = new DoubleCollection { newValue, double.MaxValue }
            };
            dashArrayAnimation.KeyFrames.Add(keyFrame1);

            // 将动画添加到 Storyboard
            Storyboard.SetTarget(dashArrayAnimation, AssociatedObject);
            Storyboard.SetTargetProperty(dashArrayAnimation, new PropertyPath("(Ellipse.StrokeDashArray)"));

            // 将动画添加到 Storyboard
            storyboard.Children.Add(dashArrayAnimation);

            // 开始动画
            storyboard.Begin();
        }
    }
}