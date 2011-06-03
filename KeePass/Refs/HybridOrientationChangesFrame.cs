// Copyright (C) Microsoft Corporation. All Rights Reserved.
// This code released under the terms of the Microsoft Public License
// (Ms-PL, http://opensource.org/licenses/ms-pl.html).

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Delay
{
    /// <summary>
    /// PhoneApplicationFrame subclass that animates and fades between device orientation changes.
    /// </summary>
    public class HybridOrientationChangesFrame : PhoneApplicationFrame
    {
        /// <summary>
        /// Stores the previous orientation.
        /// </summary>
        private PageOrientation _previousOrientation = PageOrientation.PortraitUp;

        /// <summary>
        /// Stores the Popup for displaying the "before" content.
        /// </summary>
        private Popup _popup = new Popup();

        /// <summary>
        /// Stores the Storyboard used to animate the transition.
        /// </summary>
        private Storyboard _storyboard = new Storyboard();

        /// <summary>
        /// Stores the Timeline used to perform the "before" fade.
        /// </summary>
        private DoubleAnimation _beforeOpacityAnimation = new DoubleAnimation { From = 1, To = 0 };

        /// <summary>
        /// Stores the Timeline used to perform the "after" fade.
        /// </summary>
        private DoubleAnimation _afterOpacityAnimation = new DoubleAnimation { From = 0, To = 1 };

        /// <summary>
        /// Stores the Timeline used to perform the "before" rotation.
        /// </summary>
        private DoubleAnimation _beforeRotationAnimation = new DoubleAnimation();

        /// <summary>
        /// Stores the Timeline used to perform the "before" rotation.
        /// </summary>
        private DoubleAnimation _afterRotationAnimation = new DoubleAnimation();

        /// <summary>
        /// Stores the Transform used to create the "after" rotation.
        /// </summary>
        private RotateTransform _afterRotateTransform = new RotateTransform();

        /// <summary>
        /// Initializes a new instance of the HybridOrientationChangesFrame class.
        /// </summary>
        public HybridOrientationChangesFrame()
        {
            // Set up animations
            Storyboard.SetTargetProperty(_beforeOpacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            _storyboard.Children.Add(_beforeOpacityAnimation);
            Storyboard.SetTargetProperty(_afterOpacityAnimation, new PropertyPath(UIElement.OpacityProperty));
            _storyboard.Children.Add(_afterOpacityAnimation);
            Storyboard.SetTargetProperty(_beforeRotationAnimation, new PropertyPath(RotateTransform.AngleProperty));
            _storyboard.Children.Add(_beforeRotationAnimation);
            Storyboard.SetTargetProperty(_afterRotationAnimation, new PropertyPath(RotateTransform.AngleProperty));
            _storyboard.Children.Add(_afterRotationAnimation);
            _storyboard.Completed += new EventHandler(HandleStoryboardCompleted);

            // Initialize variables
            EasingFunction = new QuadraticEase(); // Initialized here to avoid a single shared instance

            // Add custom transform to end of existing group
            var transformGroup = RenderTransform as TransformGroup;
            if (null != transformGroup)
            {
                transformGroup.Children.Add(_afterRotateTransform);
            }

            // Hook events
            OrientationChanged += new EventHandler<OrientationChangedEventArgs>(HandleOrientationChanged);
        }

        /// <summary>
        /// Gets or sets a value indicating whether animation is enabled.
        /// </summary>
        public bool IsAnimationEnabled
        {
            get { return (bool)GetValue(IsAnimationEnabledProperty); }
            set { SetValue(IsAnimationEnabledProperty, value); }
        }
        /// <summary>
        /// Identifies the IsAnimationEnabled DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty IsAnimationEnabledProperty =
            DependencyProperty.Register("IsAnimationEnabled", typeof(bool), typeof(HybridOrientationChangesFrame), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating the duration of the orientation change animation.
        /// </summary>
        public TimeSpan Duration
        {
            get { return (TimeSpan)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }
        /// <summary>
        /// Identifies the Duration DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(TimeSpan), typeof(HybridOrientationChangesFrame), new PropertyMetadata(TimeSpan.FromSeconds(0.4)));

        /// <summary>
        /// Gets or sets a value indicating the IEasingFunction to use for the orientation change animation.
        /// </summary>
        public IEasingFunction EasingFunction
        {
            get { return (IEasingFunction)GetValue(EasingFunctionProperty); }
            set { SetValue(EasingFunctionProperty, value); }
        }
        /// <summary>
        /// Identifies the EasingFunction DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty EasingFunctionProperty =
            DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(HybridOrientationChangesFrame), new PropertyMetadata(null));

        /// <summary>
        /// Handles the OrientationChanged event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            // Stop/complete Storyboard in case it's active
            _storyboard.Stop();
            HandleStoryboardCompleted(null, null);

            if (IsAnimationEnabled)
            {
                // Capture device width/height
                var actualWidth = ActualWidth;
                var actualHeight = ActualHeight;

                // Get "before" width/height
                bool normal = PageOrientation.Portrait == (PageOrientation.Portrait & _previousOrientation);
                var width = normal ? actualWidth : actualHeight;
                var height = normal ? actualHeight : actualWidth;

                // Capture "before" visuals in a WriteableBitmap
                var writeableBitmap = new WriteableBitmap((int)width, (int)height);
                writeableBitmap.Render(this, null);
                writeableBitmap.Invalidate();

                // Create transforms for "before" content
                var beforeTranslateTransform = new TranslateTransform();
                var beforeRotateTransform = new RotateTransform { CenterX = actualWidth / 2, CenterY = actualHeight / 2 };
                var beforeTransforms = new TransformGroup();
                beforeTransforms.Children.Add(beforeTranslateTransform);
                beforeTransforms.Children.Add(beforeRotateTransform);

                // Configure transforms for "before" content
                var translateDelta = (actualHeight - actualWidth) / 2;
                var beforeAngle = 0.0;
                if (PageOrientation.LandscapeLeft == _previousOrientation)
                {
                    beforeAngle = -90;
                    beforeTranslateTransform.X = -translateDelta;
                    beforeTranslateTransform.Y = translateDelta;
                }
                else if (PageOrientation.LandscapeRight == _previousOrientation)
                {
                    beforeAngle = 90;
                    beforeTranslateTransform.X = -translateDelta;
                    beforeTranslateTransform.Y = translateDelta;
                }
                beforeRotateTransform.Angle = -beforeAngle;

                // Configure for "after" content
                var afterAngle = 0.0;
                if (PageOrientation.LandscapeLeft == e.Orientation)
                {
                    afterAngle = -90;
                }
                else if (PageOrientation.LandscapeRight == e.Orientation)
                {
                    afterAngle = 90;
                }
                _afterRotateTransform.CenterX = actualWidth / 2;
                _afterRotateTransform.CenterY = actualHeight / 2;

                // Create content with default background and WriteableBitmap overlay for "before"
                var container = new Grid
                {
                    Width = width,
                    Height = height,
                    Background = (Brush)Application.Current.Resources["PhoneBackgroundBrush"],
                    RenderTransform = beforeTransforms,
                    IsHitTestVisible = false,
                };
                var content = new Rectangle
                {
                    Fill = new ImageBrush
                    {
                        ImageSource = writeableBitmap,
                        Stretch = Stretch.None,
                    }
                };
                container.Children.Add(content);

                // Configure Popup for displaying "before" content
                _popup.Child = container;
                _popup.IsOpen = true;

                // Update animations to fade from "before" to "after"
                Storyboard.SetTarget(_beforeOpacityAnimation, container);
                _beforeOpacityAnimation.Duration = Duration;
                _beforeOpacityAnimation.EasingFunction = EasingFunction;
                Storyboard.SetTarget(_afterOpacityAnimation, this);
                _afterOpacityAnimation.Duration = Duration;
                _afterOpacityAnimation.EasingFunction = EasingFunction;

                // Update animations to rotate from "before" to "after"
                Storyboard.SetTarget(_beforeRotationAnimation, beforeRotateTransform);
                _beforeRotationAnimation.From = beforeRotateTransform.Angle;
                _beforeRotationAnimation.To = _beforeRotationAnimation.From + (beforeAngle - afterAngle);
                _beforeRotationAnimation.Duration = Duration;
                _beforeRotationAnimation.EasingFunction = EasingFunction;
                Storyboard.SetTarget(_afterRotationAnimation, _afterRotateTransform);
                _afterRotationAnimation.From = -(beforeAngle - afterAngle);
                _afterRotationAnimation.To = 0;
                _afterRotationAnimation.Duration = Duration;
                _afterRotationAnimation.EasingFunction = EasingFunction;

                // Play the animations
                _storyboard.Begin();
            }

            // Save current orientation for next time
            _previousOrientation = e.Orientation;
        }

        /// <summary>
        /// Handles the completion of the Storyboard.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleStoryboardCompleted(object sender, EventArgs e)
        {
            // Remove and clear Popup
            _popup.IsOpen = false;
            _popup.Child = null;
        }
    }
}
