using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace KeePass.Utils
{
    public class EaseEffect : DependencyObject
    {
        public static DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),
                typeof(EaseEffect), new PropertyMetadata(OnIsEnabledChanged));

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static bool GetIsEnabled(DependencyObject source)
        {
            return (bool)source.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject source, bool value)
        {
            source.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject target,
            DependencyPropertyChangedEventArgs e)
        {
            var element = target as FrameworkElement;
            if (element == null)
                return;

            if (!(bool)e.NewValue)
                return;

            var animation = new DoubleAnimation
            {
                To = 1,
                From = 0,
                EasingFunction = new PowerEase
                {
                    EasingMode = EasingMode.EaseOut
                },
                Duration = TimeSpan.FromMilliseconds(700),
            };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation,
                new PropertyPath(UIElement.OpacityProperty));

            var story = new Storyboard();
            story.Children.Add(animation);

            var trigger = new EventTrigger();
            trigger.Actions.Add(new BeginStoryboard
            {
                Storyboard = story,
            });

            element.Triggers.Add(trigger);
        }
    }
}