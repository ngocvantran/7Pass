using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KeePass.Generator;

namespace KeePass.Controls
{
    public class QualityProgressBar : ProgressBar
    {
        public static readonly DependencyProperty PasswordProperty;

        private readonly ICharacterSet[] _sets;

        /// <summary>
        /// Occurs when value of <see cref="Password"/> has changed.
        /// </summary>
        public event EventHandler PasswordChanged;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set
            {
                if (!string.Equals(value, Password))
                    SetValue(PasswordProperty, value);
            }
        }

        static QualityProgressBar()
        {
            PasswordProperty = DependencyProperty.Register(
                "Password", typeof(string), typeof(QualityProgressBar),
                new PropertyMetadata(string.Empty, OnPasswordChanged));
        }

        public QualityProgressBar()
        {
            _sets = CharacterSets.GetAll();
        }

        /// <summary>
        /// Estimates the quality of the specified password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>Value between 0 to 1, with 1 is strongest.</returns>
        protected double EstimateQuality(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            var charBits = GetBitsPerChar(password);
            var length = GetEffectiveLength(password);

            return Math.Ceiling(charBits * length) / 100;
        }

        /// <summary>
        /// Raises the <see cref="PasswordChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/>
        /// instance containing the event data.</param>
        protected virtual void OnPasswordChanged(EventArgs e)
        {
            if (PasswordChanged != null)
                PasswordChanged(this, e);

            UpdateQuality();
        }

        private double GetBitsPerChar(string password)
        {
            var charSpace = _sets
                .Where(x => password
                    .IndexOfAny(x.Characters) >= 0)
                .Sum(x => x.Strength);

            if (charSpace == 0)
                return 0;

            return Math.Log(charSpace) / Math.Log(2);
        }

        private static double GetEffectiveLength(string password)
        {
            var result = 0.0;
            var diffs = new Dictionary<int, uint>();
            var usages = new Dictionary<char, uint>();

            for (var i = 0; i < password.Length; ++i)
            {
                var ch = password[i];
                var diffFactor = 1.0;

                if (i >= 1)
                {
                    var iDiff = ch - password[i - 1];

                    if (!diffs.ContainsKey(iDiff))
                        diffs.Add(iDiff, 1);
                    else
                    {
                        diffs[iDiff] = diffs[iDiff] + 1;
                        diffFactor /= diffs[iDiff];
                    }
                }

                if (!usages.ContainsKey(ch))
                {
                    usages.Add(ch, 1);
                    result += diffFactor;
                }
                else
                {
                    usages[ch] = usages[ch] + 1;
                    result += diffFactor * (1.0 / usages[ch]);
                }
            }

            return result;
        }

        private static void OnPasswordChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (QualityProgressBar)obj;
            control.OnPasswordChanged(EventArgs.Empty);
        }

        private void UpdateQuality()
        {
            var quality = EstimateQuality(Password);

            if (quality > 1)
                quality = 1;

            Value = Minimum + (Maximum - Minimum) * quality;
        }
    }
}