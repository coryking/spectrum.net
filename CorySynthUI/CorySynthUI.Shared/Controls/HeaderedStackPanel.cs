using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CorySynthUI.Controls
{
    public class HeaderedStackPanel : ContentControl
    {
        public HeaderedStackPanel() : base()
        {
            
        }
        #region HeaderText

        /// <summary>
        /// Identifies the <see cref="HeaderText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(String), typeof(HeaderedStackPanel),
                new PropertyMetadata(null));


        /// <summary>
        /// Header text
        /// </summary>
        public String HeaderText
        {
            get { return (String)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        #endregion

        #region HeaderStyle

        /// <summary>
        /// Identifies the <see cref="HeaderStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderStyleProperty =
            DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(HeaderedStackPanel),
                new PropertyMetadata(null));

        /// <summary>
        /// Style of header
        /// </summary>
        public Style HeaderStyle
        {
            get { return (Style)GetValue(HeaderStyleProperty); }
            set { SetValue(HeaderStyleProperty, value); }
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(HeaderedStackPanel),
                new PropertyMetadata((Orientation)Orientation.Vertical));

        /// <summary>
        /// Gets or sets the Orientation property.  This dependency property 
        /// indicates ....
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion

        

    }
}
