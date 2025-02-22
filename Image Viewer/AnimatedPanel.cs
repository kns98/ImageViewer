﻿using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ImageViewer
{
    internal class AnimatedPanel
    {
        private static Action emptyAction = () => { };
        private readonly IAnimatable _fader;
        private readonly UIElement _focus;
        private readonly FrameworkElement _parent;
        private readonly DependencyProperty _parentProperty;
        private readonly Func<bool> _pred;
        private readonly DependencyProperty _property;
        private readonly IInputElement _returnFocus;
        private readonly FrameworkElement _slider;
        private Animation _animation = Animation.None;

        public AnimatedPanel(FrameworkElement slider, IAnimatable fader, UIElement focus, FrameworkElement parent,
            DependencyProperty property, DependencyProperty parentProperty, Func<bool> pred,
            IInputElement returnFocus = null)
        {
            _slider = slider;
            _fader = fader;
            _focus = focus;
            _parent = parent;
            _property = property;
            _pred = pred;
            _returnFocus = returnFocus;
            _parentProperty = parentProperty;
        }

        public bool Hide(Action end = null)
        {
            if (_animation == Animation.Closing) return false;
            if (_pred()) return false;

            _animation = Animation.Closing;
            Animate(0, () =>
            {
                end?.Invoke();
                if (_animation == Animation.Closing)
                    _animation = Animation.None;

                _returnFocus?.Focus();
            });
            return true;
        }

        public bool Show(Action end = null)
        {
            if (_animation == Animation.Opening) return false;
            if (!_pred()) return false;

            _animation = Animation.Opening;
            _slider.Width = double.NaN;

            Animate(1, () =>
            {
                end?.Invoke();
                if (_animation == Animation.Opening)
                    _animation = Animation.None;
            });
            return true;
        }

        private void Animate(double to, Action end)
        {
            _focus.Focus();
            var from = 1 - to;

            var se = new CubicEase { EasingMode = EasingMode.EaseInOut };

            var pp = (double)_parent.GetValue(_parentProperty);
            var dur1 = new Duration(TimeSpan.FromMilliseconds(125));
            var dur2 = new Duration(TimeSpan.FromMilliseconds(250));

            var da1 = new DoubleAnimation(pp * from, pp * to, dur2) { EasingFunction = se };
            var da2 = new DoubleAnimation(from, to, dur2) { EasingFunction = se };
            da2.Completed += (o, args) => end();
            _slider.BeginAnimation(_property, da1);
            _fader.BeginAnimation(UIElement.OpacityProperty, da2);
        }

        private enum Animation
        {
            None,
            Opening,
            Closing
        }
    }
}