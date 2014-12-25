﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using KinectFittingRoom.View.Buttons.Events;

namespace KinectFittingRoom.View.Buttons
{
    /// <summary>
    /// Button class that responds to Kincect events
    /// </summary>
    public class KinectButton : Button
    {
        #region Constants
        /// <summary>
        /// Number of seconds that Click event occures
        /// </summary>
        private const int ClickTimeout = 20;
        #endregion Constants
        #region Private Fields
        /// <summary>
        /// Number of elapsed ticks for _clickTimer
        /// </summary>
        private int _clickTicks;
        /// <summary>
        /// Number of elapsed ticks for _unclickTimer
        /// </summary>
        private int _unclickTicks;
        /// <summary>
        /// Determines how much time elapsed since HandCursorEnterEvent occured
        /// </summary>
        private readonly DispatcherTimer _clickTimer;
        /// <summary>
        /// Determines how much time elapsed since ClickEvent occured
        /// </summary>
        private readonly DispatcherTimer _unclickTimer;
        /// <summary>
        /// The last hand position
        /// </summary>
        private Point _lastHandPosition;
        #endregion Private Fields
        #region Events
        /// <summary>
        /// Hand cursor enter event
        /// </summary>
        public static readonly RoutedEvent HandCursorEnterEvent
            = KinectInput.HandCursorEnterEvent.AddOwner(typeof(KinectButton));
        /// <summary>
        /// Hand cursor move event
        /// </summary>
        public static readonly RoutedEvent HandCursorMoveEvent
            = KinectInput.HandCursorMoveEvent.AddOwner(typeof(KinectButton));
        /// <summary>
        /// Hand cursor leave event
        /// </summary>
        public static readonly RoutedEvent HandCursorLeaveEvent
            = KinectInput.HandCursorLeaveEvent.AddOwner(typeof(KinectButton));
        /// <summary>
        /// Hand cursor click event
        /// </summary>
        public static readonly RoutedEvent HandCursorClickEvent
            = KinectInput.HandCursorClickEvent.AddOwner(typeof(KinectButton));
        #endregion Events
        #region Event handlers
        /// <summary>
        /// Hand cursor enter event handler
        /// </summary>
        public event HandCursorEventHandler HandCursorEnter
        {
            add { AddHandler(HandCursorEnterEvent, value); }
            remove { RemoveHandler(HandCursorEnterEvent, value); }
        }
        /// <summary>
        /// Hand cursor move event handler
        /// </summary>
        public event HandCursorEventHandler HandCursorMove
        {
            add { AddHandler(HandCursorMoveEvent, value); }
            remove { RemoveHandler(HandCursorMoveEvent, value); }
        }
        /// <summary>
        /// Hand cursor leave event handler
        /// </summary>
        public event HandCursorEventHandler HandCursorLeave
        {
            add { AddHandler(HandCursorLeaveEvent, value); }
            remove { RemoveHandler(HandCursorLeaveEvent, value); }
        }
        /// <summary>
        /// Hand cursor click event handler
        /// </summary>
        public event HandCursorEventHandler HandCursorClick
        {
            add { AddHandler(HandCursorClickEvent, value); }
            remove { RemoveHandler(HandCursorClickEvent, value); }
        }
        #endregion Event handlers
        #region Properties
        /// <summary>
        /// Has Click event occured
        /// </summary>
        public bool IsClicked
        {
            get { return (bool)GetValue(IsClickedProperty); }
            set { SetValue(IsClickedProperty, value); }
        }
        /// <summary>
        /// Occures after Click event to undo scaling the button
        /// </summary>
        public bool IsUnclicked
        {
            get { return (bool)GetValue(IsUnclickedProperty); }
            set { SetValue(IsUnclickedProperty, value); }
        }
        /// <summary>
        /// Gets or sets the command to invoke when this button is pressed.
        /// </summary>
        public new ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        #endregion Properties
        #region Dependency Properties
        /// <summary>
        /// IsClicked dependency property
        /// </summary>
        public static readonly DependencyProperty IsClickedProperty = DependencyProperty.Register(
            "IsClicked", typeof(bool), typeof(KinectButton), new PropertyMetadata(default(bool)));
        /// <summary>
        /// IsUnclickedProperty dependency property
        /// </summary>
        public static readonly DependencyProperty IsUnclickedProperty = DependencyProperty.Register(
            "IsUnclicked", typeof(bool), typeof(KinectButton), new PropertyMetadata(default(bool)));
        #endregion Dependency Properties
        #region .ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="KinectButton"/> class.
        /// </summary>
        public KinectButton()
        {
            SetValue(IsClickedProperty, false);
            SetValue(IsUnclickedProperty, false);

            _clickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1) };
            _clickTicks = 0;
            _clickTimer.Tick += _clickTimer_Tick;

            _unclickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1) };
            _unclickTicks = 0;
            _unclickTimer.Tick += _unclickTimer_Tick;

            HandCursorEnter += KinectButton_HandCursorEnter;
            HandCursorMove += KinectButton_HandCursorMove;
            HandCursorLeave += KinectButton_HandCursorLeave;
            HandCursorClick += KinectButton_HandCursorClick;
        }
        #endregion .ctor
        #region Methods
        /// <summary>
        /// Handles HandCursorEnter event
        /// </summary>
        protected void KinectButton_HandCursorEnter(object sender, HandCursorEventArgs args)
        {
            _clickTimer.Start();
        }
        /// <summary>
        /// Handles HandCursorMove event
        /// </summary>
        protected void KinectButton_HandCursorMove(object sender, HandCursorEventArgs args)
        {
            _lastHandPosition = new Point(args.X, args.Y);
        }
        /// <summary>
        /// Handles HandCursorLeave event
        /// </summary>
        protected virtual void KinectButton_HandCursorLeave(object sender, HandCursorEventArgs args)
        {
            ResetTimer(_clickTimer, _clickTicks, IsClickedProperty, false);
        }
        /// <summary>
        /// Counts the number of timer ticks of _clickTimer
        /// </summary>
        private void _clickTimer_Tick(object sender, EventArgs e)
        {
            _clickTicks++;

            if (_clickTicks <= ClickTimeout)
                return;

            ResetTimer(_clickTimer, _clickTicks, IsClickedProperty, false);
            RaiseEvent(new HandCursorEventArgs(HandCursorClickEvent, _lastHandPosition));
            _unclickTimer.Start();
        }
        /// <summary>
        /// Counts the number of timer ticks of _unclickTimer
        /// </summary>
        private void _unclickTimer_Tick(object sender, EventArgs e)
        {
            _unclickTicks++;

            if (_unclickTicks <= ClickTimeout)
                return;
            ResetTimer(_unclickTimer, _unclickTicks, IsUnclickedProperty, true);
            SetValue(IsUnclickedProperty, false);
        }
        /// <summary>
        /// Imitates the click event
        /// </summary>
        protected virtual void KinectButton_HandCursorClick(object sender, HandCursorEventArgs args)
        {
            SetValue(IsClickedProperty, true);
            SetValue(IsUnclickedProperty, false);
        }
        /// <summary>
        /// Resets the timer
        /// </summary>
        private void ResetTimer(DispatcherTimer timer, int timerCounter, DependencyProperty propertyToSet, bool propertyValue)
        {
            SetValue(propertyToSet, propertyValue);
            timer.Stop();
            timerCounter = 0;
        }
        #endregion Methods
    }
}
