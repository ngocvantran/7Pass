// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A progress bar implementation for a smoother appearance of the 
    /// indeterminate states, with the added behavior that after the behavior
    /// is no longer needed, it waits until the animation completes.
    /// </summary>
    /// <remarks>Important - this control is not really designed or tested for
    /// regular progress bar use, but only indeterminate. As a result, 
    /// IsIndeterminate is in the default style. Use the determinate version at
    /// your own risk as there are no benefits to this over the standard 
    /// progress bar in that situation.</remarks>
    public class PerformanceProgressBar : ProgressBar
    {
        /// <summary>
        /// The common state group name.
        /// </summary>
        private const string GroupName = "CommonStates";

        /// <summary>
        /// The indeterminate state name.
        /// </summary>
        private const string IndeterminateStateName = "Indeterminate";

        /// <summary>
        /// The storyboard that is used for the indeterminate animations.
        /// </summary>
        private Storyboard _indeterminateStoryboard;

        #region public bool IsLoading
        /// <summary>
        /// Gets or sets a property indicating whether the control should show
        /// that it is currently working.
        /// </summary>
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        /// <summary>
        /// Identifies the IsLoading dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(
                "IsLoading",
                typeof(bool),
                typeof(PerformanceProgressBar),
                new PropertyMetadata(false, OnIsLoadingPropertyChanged));

        /// <summary>
        /// IsLoadingProperty property changed handler.
        /// </summary>
        /// <param name="d">PerformanceProgressBar that changed its IsLoading.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsLoadingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as PerformanceProgressBar;
            if (source != null)
            {
                source.OnIsLoadingChanged((bool) e.NewValue);
            }
    }
        #endregion public bool IsLoading


        /// <summary>
        /// Initializes a new instance of the PerformanceProgressBar type.
        /// </summary>
        public PerformanceProgressBar() : base()
        {
            DefaultStyleKey = typeof (PerformanceProgressBar);
        }

        /// <summary>
        /// Overrides the template application stage so that the storyboard can
        /// be retrieved from the implementation root.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_indeterminateStoryboard != null)
            {
                _indeterminateStoryboard.Completed -= OnIndeterminateStoryboardCompleted;
            }

            base.OnApplyTemplate();

            _indeterminateStoryboard = GetStoryboard(this, GroupName, IndeterminateStateName);
            if (_indeterminateStoryboard != null)
            {
                _indeterminateStoryboard.Completed += OnIndeterminateStoryboardCompleted;
            }
        }

        private void OnIsLoadingChanged(bool newValue)
        {
            EvaluateWhatToDo(newValue);
        }

        private void EvaluateWhatToDo(bool value)
        {
            if (IsLoading)
            {
                if (!IsIndeterminate)
                {
                    ReallyStartIt();
                }
            }
            else
            {
                if (_indeterminateStoryboard != null)
                {
                    if (_indeterminateStoryboard.GetCurrentState() != ClockState.Active)
                    {
                        ReallyStopIt();
                    }
                    // Else we'll allow the animation to finish on its own time.
                }
                else
                {
                    ReallyStopIt();
                }
            }
        }

        // NOTE: These do not address the timing issue where a change during the
        // old animation could affect the fun.

        private void ReallyStopIt()
        {
            IsIndeterminate = false;
        }

        private void ReallyStartIt()
        {
            IsIndeterminate = true;
        }

        /// <summary>
        /// Hooks up to the Completed event of the indeterminate storyboard. For
        /// technical reasons, when a storyboard has a repeat behavior of
        /// 'Forever', this means the Completed event is never called. So this
        /// method needs to either 1) restart the storyboard animation again or
        /// 2) if the indeterminate behavior is finished, not restart the
        /// animation and update the underlying bound state.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        protected virtual void OnIndeterminateStoryboardCompleted(object sender, EventArgs e)
        {
            if (IsLoading)
            {
                // Restart the animation loop to similar the forever behavior.
                _indeterminateStoryboard.Begin();
            }
            else
            {
                EvaluateWhatToDo(IsLoading);
            }
        }

        /// <summary>
        /// Attempts to find a storyboard that matches the newTransition name.
        /// </summary>
        /// <param name="storyboardOwner">The control with the storyboard.</param>
        /// <param name="groupName">The group name to search in.</param>
        /// <param name="newTransition">The new transition.</param>
        /// <returns>A storyboard or null, if no storyboard was found.</returns>
        private Storyboard GetStoryboard(Control storyboardOwner, string groupName, string newTransition)
        {
            VisualStateGroup presentationGroup = VisualStates.TryGetVisualStateGroup(storyboardOwner, groupName);
            Storyboard newStoryboard = null;
            if (presentationGroup != null)
            {
                newStoryboard = presentationGroup.States
                    .OfType<VisualState>()
                    .Where(state => state.Name == newTransition)
                    .Select(state => state.Storyboard)
                    .FirstOrDefault();
            }
            return newStoryboard;
        }

        /// <summary>
        /// Names and helpers for visual states in the controls.
        /// </summary>
        internal static class VisualStates
        {
            /// <summary>
            /// Use VisualStateManager to change the visual state of the control.
            /// </summary>
            /// <param name="control">
            /// Control whose visual state is being changed.
            /// </param>
            /// <param name="useTransitions">
            /// A value indicating whether to use transitions when updating the
            /// visual state, or to snap directly to the new visual state.
            /// </param>
            /// <param name="stateNames">
            /// Ordered list of state names and fallback states to transition into.
            /// Only the first state to be found will be used.
            /// </param>
            internal static void GoToState(Control control, bool useTransitions, params string[] stateNames)
            {
                Debug.Assert(control != null, "control should not be null!");
                Debug.Assert(stateNames != null, "stateNames should not be null!");
                Debug.Assert(stateNames.Length > 0, "stateNames should not be empty!");

                foreach (string name in stateNames)
                {
                    if (VisualStateManager.GoToState(control, name, useTransitions))
                    {
                        break;
                    }
                }
            }

            /// <summary>
            /// Gets the implementation root of the Control.
            /// </summary>
            /// <param name="dependencyObject">The DependencyObject.</param>
            /// <remarks>
            /// Implements Silverlight's corresponding internal property on Control.
            /// </remarks>
            /// <returns>Returns the implementation root or null.</returns>
            internal static FrameworkElement GetImplementationRoot(DependencyObject dependencyObject)
            {
                Debug.Assert(dependencyObject != null, "DependencyObject should not be null.");
                return (1 == VisualTreeHelper.GetChildrenCount(dependencyObject)) ?
                    VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement :
                    null;
            }

            /// <summary>
            /// This method tries to get the named VisualStateGroup for the 
            /// dependency object. The provided object's ImplementationRoot will be 
            /// looked up in this call.
            /// </summary>
            /// <param name="dependencyObject">The dependency object.</param>
            /// <param name="groupName">The visual state group's name.</param>
            /// <returns>Returns null or the VisualStateGroup object.</returns>
            internal static VisualStateGroup TryGetVisualStateGroup(DependencyObject dependencyObject, string groupName)
            {
                FrameworkElement root = GetImplementationRoot(dependencyObject);
                if (root == null)
                {
                    return null;
                }

                return VisualStateManager.GetVisualStateGroups(root)
                    .OfType<VisualStateGroup>()
                    .Where(group => string.CompareOrdinal(groupName, group.Name) == 0)
                    .FirstOrDefault();
            }
        }
    }
}
