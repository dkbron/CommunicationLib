using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DdkTestTool.Attached
{
    public class TextBoxExtension: DependencyObject
    {
         
        public static int GetInputValue(DependencyObject obj)
        {
            return (int)obj.GetValue(InputValueProperty);
        }

        public static void SetInputValue(DependencyObject obj, int value)
        {
            obj.SetValue(InputValueProperty, value);
        }

        // Using a DependencyProperty as the backing store for InputValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputValueProperty =
            DependencyProperty.RegisterAttached("InputValue", typeof(int), typeof(TextBoxExtension), new PropertyMetadata((sender, e) =>
            {
                if(sender is TextBox textBox)
                {
                    textBox.PreviewTextInput += TextBox_PreviewTextInput;
                }
            }));

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        { 
            e.Handled = !Regex.IsMatch(e.Text,@"^[0-9.\-]+$");
        }




        public static string GetHintText(DependencyObject obj)
        {
            return (string)obj.GetValue(HintTextProperty);
        }

        public static void SetHintText(DependencyObject obj, string value)
        {
            obj.SetValue(HintTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.RegisterAttached("HintText", typeof(string), typeof(TextBoxExtension), new PropertyMetadata(null)); 
    }
     
}
