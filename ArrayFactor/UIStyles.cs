using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ArrayFactor
{
    partial class UIStyles
    {
        private void OnUpDown_ScrollBar_MouseWheel(object Sender, MouseWheelEventArgs E)
        {
            var scroll_bar = (ScrollBar) Sender;
            if(E.Delta == 0) return;
            scroll_bar.Value += E.Delta / 120d * scroll_bar.SmallChange;
            E.Handled = true;
        }

        private void OnField_TextBox_MouseWheel(object Sender, MouseWheelEventArgs E)
        {
            var text_box = (TextBox) Sender;
            if(!double.TryParse(text_box.Text, out var value)) return;
            text_box.Text = (value + Math.Sign(E.Delta) / 10d).ToString(CultureInfo.CurrentCulture);
            E.Handled = true;
        } 
    }
}
