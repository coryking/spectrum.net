using CorySignalGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CorySynthUI.Controls
{
    public delegate void KeyboardEventCallback(object sender, CorySignalGenerator.Sequencer.Midi.IMidiMessage message);

    public class Keyboard : Canvas
    {

        Brush onBrushWhite;
        Brush offBrushWhite;
        Brush onBrushBlack;
        Brush offBrushBlack;
        
        MidiNotes notes = MidiNotes.GenerateNotes();

        public Keyboard() : base()
        {
            onBrushWhite = new SolidColorBrush(Windows.UI.Colors.PowderBlue);
            offBrushWhite = new SolidColorBrush(Windows.UI.Colors.White);

            onBrushBlack = new SolidColorBrush(Windows.UI.Colors.DarkGray);
            offBrushBlack = new SolidColorBrush(Windows.UI.Colors.Black);
            RenderControl();
        }

        #region CenterNoteNumber

        /// <summary>
        /// Identifies the <see cref="CenterNoteNumber"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CenterNoteNumberProperty =
            DependencyProperty.Register("CenterNoteNumber", typeof(int), typeof(Keyboard),
                new PropertyMetadata((int)60,
                    new PropertyChangedCallback(OnCenterNoteNumberChanged)));

        /// <summary>
        /// Gets or sets the CenterNoteNumber property.  This dependency property 
        /// indicates ....
        /// </summary>
        public int CenterNoteNumber
        {
            get { return (int)GetValue(CenterNoteNumberProperty); }
            set { SetValue(CenterNoteNumberProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CenterNoteNumber property.
        /// </summary>
        private static void OnCenterNoteNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Keyboard)d).OnCenterNoteNumberChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CenterNoteNumber property.
        /// </summary>
        protected virtual void OnCenterNoteNumberChanged(DependencyPropertyChangedEventArgs e)
        {
            RenderControl();
        }

        #endregion

        #region NumberOfNotes

        /// <summary>
        /// Identifies the <see cref="NumberOfNotes"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NumberOfNotesProperty =
            DependencyProperty.Register("NumberOfNotes", typeof(int), typeof(Keyboard),
                new PropertyMetadata((int)60,
                    new PropertyChangedCallback(OnNumberOfNotesChanged)));

        /// <summary>
        /// Gets or sets the NumberOfNotes property.  This dependency property 
        /// indicates ....
        /// </summary>
        public int NumberOfNotes
        {
            get { return (int)GetValue(NumberOfNotesProperty); }
            set { SetValue(NumberOfNotesProperty, value); }
        }

        /// <summary>
        /// Handles changes to the NumberOfNotes property.
        /// </summary>
        private static void OnNumberOfNotesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Keyboard)d).OnNumberOfNotesChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the NumberOfNotes property.
        /// </summary>
        protected virtual void OnNumberOfNotesChanged(DependencyPropertyChangedEventArgs e)
        {
            RenderControl();
        }

        #endregion


        #region WhiteKeyWidth

        /// <summary>
        /// Identifies the <see cref="WhiteKeyWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WhiteKeyWidthProperty =
            DependencyProperty.Register("WhiteKeyWidth", typeof(double), typeof(Keyboard),
                new PropertyMetadata((double)35));

        /// <summary>
        /// Gets or sets the WhiteKeyWidth property.  This dependency property 
        /// indicates ....
        /// </summary>
        public double WhiteKeyWidth
        {
            get { return (double)GetValue(WhiteKeyWidthProperty); }
            set { SetValue(WhiteKeyWidthProperty, value); }
        }

        #endregion

        #region WhiteKeyHeight

        /// <summary>
        /// Identifies the <see cref="WhiteKeyHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WhiteKeyHeightProperty =
            DependencyProperty.Register("WhiteKeyHeight", typeof(double), typeof(Keyboard),
                new PropertyMetadata((double)200));

        /// <summary>
        /// Gets or sets the WhiteKeyHeight property.  This dependency property 
        /// indicates ....
        /// </summary>
        public double WhiteKeyHeight
        {
            get { return (double)GetValue(WhiteKeyHeightProperty); }
            set { SetValue(WhiteKeyHeightProperty, value); }
        }

        #endregion


        public event KeyboardEventCallback KeyboardEvent;

        protected void OnKeyboardEvent(MidiNote note, CorySignalGenerator.Sequencer.Midi.MidiMessageType type)
        {
            CorySignalGenerator.Sequencer.Midi.IMidiMessage message = null;
            switch (type)
            {
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.None:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.NoteOff:
                    message = new CorySignalGenerator.Sequencer.Midi.MidiNoteOffMessage(0, (byte)note.Number, 0);
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.NoteOn:
                    message = new CorySignalGenerator.Sequencer.Midi.MidiNoteOnMessage(0, (byte)note.Number, 127);
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.PolyphonicKeyPressure:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.ControlChange:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.ProgramChange:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.ChannelPressure:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.PitchBendChange:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.SystemExclusive:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.MidiTimeCode:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.SongPositionPointer:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.SongSelect:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.TuneRequest:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.TimingClock:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.Start:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.Continue:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.Stop:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.ActiveSensing:
                    break;
                case CorySignalGenerator.Sequencer.Midi.MidiMessageType.SystemReset:
                    break;
                default:
                    break;
            }
            if (KeyboardEvent != null)
                KeyboardEvent(this, message);
        }

      
        private void RenderControl()
        {
            this.Children.Clear();
            var leftNumber = this.LeftNoteNumber;
            double leftPos = 0.0;
            for (int i = this.LeftNoteNumber; i <= this.RightNoteNumber; i++)
            {
                var key = notes[i];
                leftPos = SetKey(key, leftPos);
            }
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            var theSize = availableSize; // base.MeasureOverride(availableSize);
            int notes = (int)Math.Floor(theSize.Width / WhiteKeyWidth);
            theSize.Width = notes * WhiteKeyWidth;
            theSize.Height = WhiteKeyHeight;
            return theSize;
        }
      

        private double SetKey(MidiNote key, double leftPos)
        {
            // If the key is black, then we have to set it off by 1/2 of the black key's width
            var actualLeftPos = key.KeyColor == KeyColor.Black ? leftPos + WhiteKeyWidth / 2 : leftPos;
            var keyShape = new Windows.UI.Xaml.Shapes.Rectangle()
            {

                Width = WhiteKeyWidth,
                Height = key.KeyColor == KeyColor.White ? WhiteKeyHeight : WhiteKeyHeight / 2,
                Stroke = new SolidColorBrush(Windows.UI.Colors.Red),
                Fill = key.KeyColor == KeyColor.White ? offBrushWhite : offBrushBlack,

                Tag = key,
            };
            var zIndex = key.KeyColor == KeyColor.White ? 10 : 20;
            Canvas.SetLeft(keyShape, actualLeftPos);
            Canvas.SetZIndex(keyShape, zIndex);
            this.Children.Add(keyShape);
            keyShape.PointerPressed += key_PointerPressed;
            keyShape.PointerReleased += key_PointerReleased;

            return key.KeyColor == KeyColor.White ? leftPos + WhiteKeyWidth : leftPos;
        }

        void key_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var ele = sender as Windows.UI.Xaml.Shapes.Rectangle;
            var note = (MidiNote)ele.Tag;
            var off = note.KeyColor == KeyColor.White ? offBrushWhite : offBrushBlack;
            ele.Fill = off;
            OnKeyboardEvent(note, CorySignalGenerator.Sequencer.Midi.MidiMessageType.NoteOff);
        }

        void key_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var ele = sender as Windows.UI.Xaml.Shapes.Rectangle;
            var note = (MidiNote)ele.Tag;
            var on = note.KeyColor == KeyColor.White ? onBrushWhite : onBrushBlack;
            ele.Fill = on;
            OnKeyboardEvent(note, CorySignalGenerator.Sequencer.Midi.MidiMessageType.NoteOn);
        }

        public int NumberOfWhiteNotes
        {
            get
            {
                return (int)Math.Ceiling(this.NumberOfNotes / 12.0) * 7;
            }
        }

        public int OctavesToLeft
        {
            get
            {
                return ((NumberOfWhiteNotes - 1) / 2) / 7;
            }
        }
        public int OctavesToRight
        {
            get
            {
                return (NumberOfWhiteNotes / 2) / 7;
            }
        }

        public int LeftNoteNumber
        {
            get
            {
                return CenterNoteNumber - OctavesToLeft * 12;
            }
        }
        public int RightNoteNumber
        {
            get
            {
                return CenterNoteNumber + OctavesToRight * 12 - 1;
            }
        }
    }
}
