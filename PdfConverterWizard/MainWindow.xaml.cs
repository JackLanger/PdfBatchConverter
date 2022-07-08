using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace PdfConverterWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)lbPaths.Items).CollectionChanged += Handler;
        }

        private void lbPaths_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;

            if (lb.SelectedItems.Count > 1)
            {
                if (btnOpen.IsEnabled)
                {
                    btnOpen.IsEnabled = false;
                }
                btnRemove.IsEnabled = true;
            }
            else if (lb.SelectedItems.Count > 0)
            {
                if (btnOpen.IsEnabled && btnRemove.IsEnabled)
                    return;
                btnRemove.IsEnabled = true;
                btnOpen.IsEnabled = true;
            }
            else
            {
                btnRemove.IsEnabled = false;
                btnOpen.IsEnabled = false;
            }
        }

        void Handler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (lbPaths.Items.Count > 0)
            {
                btnConvert.IsEnabled = true;
            }
            else
            {
                btnConvert.IsEnabled = false;
                btnRemove.IsEnabled = false;
                btnOpen.IsEnabled = false;
            }
        }

        private void lbPaths_Drop(object sender, DragEventArgs e)
        {
            ListBox lb = sender as ListBox;
            var data = e.Data.GetData(DataFormats.FileDrop);

        }
    }
}