using System.Text.RegularExpressions; 
using System.Windows.Controls; 
using System.Windows.Input; 

namespace DdkTestTool.Views.SocketViews.Dialogs
{
    /// <summary>
    /// AddServerDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddServerDialog : UserControl
    {
        public AddServerDialog()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");
            e.Handled = re.IsMatch(e.Text);
        }
    }
}
