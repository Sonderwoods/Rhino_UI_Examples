using Rhino;
using System.Windows;
using System.Windows.Controls;
using UI_01_BASIC_MVVM.Examples._01_MVVM_WINDOW.ViewModel;


namespace UI_01_BASIC_MVVM.Examples._01_MVVM_WINDOW.View
{
    /// <summary>
    /// Interaction logic for AddBrepView.xaml
    /// </summary>
    public partial class AddBrepView : Window
    {

        /// <summary>
        /// Usually we set up a property to access the viewmodel,
        /// but since it already exists as a property of the DataContext we use this.
        /// </summary>
        public AddBrepViewModel ViewModel => DataContext as AddBrepViewModel;

        public AddBrepView(RhinoDoc doc)
        {


            DataContext = new AddBrepViewModel(doc);

            //this.Closed += (s, e) => ViewModel?.Dispose();
            InitializeComponent();

        }

        

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    //ViewModel?.Dispose();
        //    base.OnClosing(e);
        //}

        private void BrepListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.BrepListView_SelectionChanged(sender, e);
        }

        //private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        //{
        //    ViewModel.BtnDeleteItem_Click(sender, e);
        //}

   
    }
}
