using Rhino;
using Rhino.UI;
using System.Windows;
using System.Windows.Controls;
using UI_01_BASIC_MVVM.BindingHelpers;
using UI_01_BASIC_MVVM.Examples._01_MVVM_WINDOW.ViewModel;


namespace UI_01_BASIC_MVVM.Examples._01_MVVM_WINDOW.View
{
    /// <summary>
    /// Interaction logic for AddBrepView.xaml
    /// </summary>
    public partial class BrepOverviewView : Window
    {

        /// <summary>
        /// Usually we set up a property to access the viewmodel,
        /// but since it already exists as a property of the DataContext we use this.
        /// </summary>
        public MainViewModel ViewModel => DataContext as MainViewModel;

        public BrepOverviewView(RhinoDoc doc)
        {


            DataContext = new MainViewModel(doc);

            Closing += (s, e) => ViewModel?.Dispose();



            InitializeComponent();

            //in codebehind:
            var binder = new SelectedItemsBinder(BrepListView, ViewModel.SelectedBreps);
            binder.Bind();

        }

   
    }
}
