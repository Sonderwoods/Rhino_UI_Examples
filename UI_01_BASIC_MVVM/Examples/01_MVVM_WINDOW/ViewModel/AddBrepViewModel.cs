using Eto.Forms;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UI_01_BASIC_MVVM.Examples._01_MVVM_WINDOW.ViewModel
{

    /// <summary>
    /// Standard implementation of a viewmodel
    /// - Without the Rhino.UI.ViewModel class
    /// Using INotifyPropertyChanged  (Alternatively we need to use DependencyObject, which can be seen in <see cref="BrepItemViewModel"/>
    /// 
    /// Rules:
    /// all interaction from xaml goes through this class.
    /// The xaml has minimal code behind and no access to rhino directly
    /// This class does all the translation between UI and Rhino.
    /// </summary>
    public class AddBrepViewModel : INotifyPropertyChanged, IDisposable
    {

        /// <summary>
        /// This is out reference to the rhino world
        /// </summary>
        public RhinoDoc RhinoDoc { get; set; }


        /// <summary>
        /// Below is the snippet 'propfull' in visual studio, although we have to add the INotifyPropertyChanged.
        /// So we are now using the SetProperty method. Remember that it has a built in check if the value is the same.
        /// </summary>
        private int brepCount;

        /// <summary>
        /// This is the property that we will bind to in the xaml
        /// </summary>
        public int BrepCount
        {
            get { return brepCount; }
            set { SetProperty(ref brepCount, value); }
        }


        public ObservableCollection<BrepItemViewModel> Breps { get; set; } = new ObservableCollection<BrepItemViewModel>();
        public ObservableCollection<BrepItemViewModel> SelectedBreps { get; set; } = new ObservableCollection<BrepItemViewModel>();


        public AddBrepViewModel()
        {
                
        }

        public AddBrepViewModel(RhinoDoc doc)
        {
            RhinoDoc = doc;

            foreach (var brep in doc.Objects.FindByObjectType(ObjectType.Brep))
            {
                Breps.Add(new BrepItemViewModel(brep.Id, doc));
            }

            AddHandlers();
            
            //Rhino.Commands.Command.UndoRedo += Command_UndoRedo;
        }

        void AddHandlers()
        {
            RemoveHandlers();
            RhinoDoc.SelectObjects += RhinoDoc_SelectObjects;
            RhinoDoc.DeselectObjects += RhinoDoc_SelectObjects;
            RhinoDoc.AddRhinoObject += RhinoDoc_AddObject;
            RhinoDoc.DeleteRhinoObject += RhinoDoc_DeleteObject;
        }

        void RemoveHandlers()
        {
            RhinoDoc.SelectObjects -= RhinoDoc_SelectObjects;
            RhinoDoc.DeselectObjects -= RhinoDoc_SelectObjects;
            RhinoDoc.AddRhinoObject -= RhinoDoc_AddObject;
            RhinoDoc.DeleteRhinoObject -= RhinoDoc_DeleteObject;
        }


        private void RhinoDoc_DeleteObject(object sender, RhinoObjectEventArgs e)
        {
            var item = Breps.Where(b => b.Guid == e.ObjectId).FirstOrDefault();
            if (item != null)
            {
                item.Area = -1;
                item.Name = "<Missing>";

            }

            var item2 = SelectedBreps.Where(b => b.Guid == e.ObjectId).FirstOrDefault();
            if (item2 != null)
            {
                SelectedBreps.Remove(item2);
            }

        }

        private void RhinoDoc_AddObject(object sender, RhinoObjectEventArgs e)
        {
            RhinoObject obj = RhinoDoc.Objects.FindId(e.ObjectId);
            if (obj.Geometry is Brep)
            {
                Breps.Add(new BrepItemViewModel(obj.Id, RhinoDoc));
            }
        }

        private void RhinoDoc_SelectObjects(object sender, RhinoObjectSelectionEventArgs e)
        {


            foreach (var item in e.RhinoObjects)
            {
                if (e.Selected)
                {

                    BrepItemViewModel ii = Breps.Where(b => b.Guid == item.Id).FirstOrDefault();
                    if (ii != null)
                    {

                        SelectedBreps.Add(ii);

                    }
                }
                else
                {

                    BrepItemViewModel iii = SelectedBreps.Where(b => b.Guid == item.Id).FirstOrDefault();
                    if (iii != null)
                    {
                        SelectedBreps.Remove(iii);
                    }
                }
            }


        }

        public void BrepListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            foreach (BrepItemViewModel itemVM in e.AddedItems)
            {
                RhinoDoc.Objects.Find(itemVM.Guid).Select(true);
            }
            foreach (BrepItemViewModel itemVM in e.RemovedItems)
            {
                RhinoDoc.Objects.Find(itemVM.Guid).Select(false);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// https://www.danrigby.com/2015/09/12/inotifypropertychanged-the-net-4-6-way/
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        ///     Checks if a property already matches a desired value.  Sets the property and
        ///     notifies listeners only when necessary.
        ///     Source: https://www.danrigby.com/2015/09/12/inotifypropertychanged-the-net-4-6-way/
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">
        ///     Name of the property used to notify listeners.  This
        ///     value is optional and can be provided automatically when invoked from compilers that
        ///     support CallerMemberName.
        /// </param>
        /// <returns>
        ///     True if the value was changed, false if the existing value matched the
        ///     desired value.
        /// </returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        public void Dispose()
        {
            RemoveHandlers();
        }
    }
}
