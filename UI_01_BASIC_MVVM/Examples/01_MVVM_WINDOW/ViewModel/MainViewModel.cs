using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UI_01_BASIC_MVVM.Commands;

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
    /// 
    /// Slightly unsure whether we need to implement IDisposable for this class, but im using it as we are subscribing to events and we should unsubscribe.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {

        /// <summary>
        /// This is out reference to the rhino world
        /// </summary>
        public RhinoDoc RhinoDoc { get; set; }



        // START PROPFULL: Below is the snippet 'propfull' in visual studio, although we have to add the INotifyPropertyChanged.
        /// <summary>
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
        // END PROPFULL

        private int selectedBrepCount;

        public int SelectedBrepCount
        {
            get { return selectedBrepCount; }
            set { SetProperty(ref selectedBrepCount, value); }
        }




        /// <summary>
        /// This is the collection of breps that we will bind to in the xaml.
        /// It will contain all the breps in the RhinoDoc
        /// Handling happens here <see cref="Breps_CollectionChanged(object, NotifyCollectionChangedEventArgs)"/>
        /// </summary>
        public ObservableCollection<BrepItemViewModel> Breps { get; set; } = new ObservableCollection<BrepItemViewModel>();

        /// <summary>
        /// This is the collection of selected breps that we will bind to in the xaml.
        /// As a point of departure it should never contain any brep that is not part of the Breps collection.
        /// Handling happens here <see cref="SelectedBreps_CollectionChanged"/>
        /// </summary>
        public ObservableCollection<BrepItemViewModel> SelectedBreps { get; set; } = new ObservableCollection<BrepItemViewModel>();


        // Commands
        public RelayCommand DeleteBrepCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel(RhinoDoc doc)
        {
            RhinoDoc = doc;

            // Add event handlers. Must be done before populating the Breps list, otherwise BrepCount wont be updated.
            RegistrerEventHandlers();


            foreach (var brep in doc.Objects.FindByObjectType(ObjectType.Brep | ObjectType.Surface | ObjectType.Mesh | ObjectType.Extrusion))
            {
                var item = BrepItemViewModel.FromGuid(brep.Id, doc);
                if (item != null)
                    Breps.Add(item);
            }

            InitializeCommands();



            // TODO:
            //Rhino.Commands.Command.UndoRedo += Command_UndoRedo;
        }


        private void InitializeCommands()
        {
            DeleteBrepCommand = new RelayCommand((sender) => DeleteGuid((BrepItemViewModel)sender));
        }

        /// <summary>
        /// Adds the event handlers to the RhinoDoc
        /// </summary>
        private void RegistrerEventHandlers()
        {
            // First we remove all handlers to avoid duplicates
            UnregistrerEventHandlers();

            // Subscribing to changes in RhinoDoc
            RhinoDoc.SelectObjects += RhinoDoc_SelectObjects;
            RhinoDoc.DeselectObjects += RhinoDoc_SelectObjects; // triggered if unselected while still have some selection
            RhinoDoc.DeselectAllObjects += RhinoDoc_DeselectAll;
            RhinoDoc.AddRhinoObject += RhinoDoc_AddObject;
            RhinoDoc.DeleteRhinoObject += RhinoDoc_DeleteObject;
            RhinoDoc.ModifyObjectAttributes += RhinoDoc_ModifyObjectAttributes;
            RhinoDoc.UndeleteRhinoObject += RhinoDoc_UndeleteRhinoObject;

            // Subscribing to changes in the collections
            Breps.CollectionChanged += Breps_CollectionChanged;
            SelectedBreps.CollectionChanged += SelectedBreps_CollectionChanged;
        }

        private void RhinoDoc_DeselectAll(object sender, RhinoDeselectAllObjectsEventArgs e)
        {

            // Clear for some reason is not triggering WPF..
            //SelectedBreps.Clear();
            //OnPropertyChanged(nameof(SelectedBreps));

            for (int i = SelectedBreps.Count - 1; i >= 0; i--)
            {
                SelectedBreps.RemoveAt(i);
            }
        }


        /// <summary>
        /// Removes the event handlers from the RhinoDoc
        /// </summary>
        void UnregistrerEventHandlers()
        {
            RhinoDoc.SelectObjects -= RhinoDoc_SelectObjects;
            RhinoDoc.DeselectAllObjects += RhinoDoc_DeselectAll;
            RhinoDoc.DeselectObjects -= RhinoDoc_SelectObjects; //triggered if unselected while still have some selection
            RhinoDoc.AddRhinoObject -= RhinoDoc_AddObject;
            RhinoDoc.DeleteRhinoObject -= RhinoDoc_DeleteObject;
            RhinoDoc.ModifyObjectAttributes -= RhinoDoc_ModifyObjectAttributes;
            RhinoDoc.UndeleteRhinoObject -= RhinoDoc_UndeleteRhinoObject;

            Breps.CollectionChanged -= Breps_CollectionChanged;
            SelectedBreps.CollectionChanged -= SelectedBreps_CollectionChanged;
        }


        /// <summary>
        /// Will fire when the selected breps collection changes.
        /// Since this collection is bound to the WPF listview, this will fire when the user selects or deselects items in the listview.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedBreps_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            SelectedBrepCount = SelectedBreps.Count;

            // What is changed?
            switch (e.Action)
            {

                // We selected in the UI -> Added to collection -> Select in Rhino
                case NotifyCollectionChangedAction.Add:
                    //RhinoApp.WriteLine("SelectedBreps_CollectionChanged Add");
                    foreach (BrepItemViewModel item in e.NewItems)
                    {
                        RhinoObject obj = RhinoDoc.Objects.Find(item.Guid);
                        obj?.Select(true);
                    }

                    // Remember to redraw the view for more smooth interaction
                    RhinoDoc.Views.Redraw();
                    break;

                // We deselected in the UI -> Removed from collection -> Deselect in Rhino
                case NotifyCollectionChangedAction.Remove:
                    //RhinoApp.WriteLine("SelectedBreps_CollectionChanged Remove");


                    foreach (BrepItemViewModel item in e.OldItems)
                    {
                        RhinoObject obj = RhinoDoc.Objects.Find(item.Guid);
                        obj?.Select(false);
                    }
                    RhinoDoc.Views.Redraw();

                    break;

                // Just showcasing other options you can play with
                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;




            }
        }

        /// <summary>
        /// Will fire when the breps collection changes.
        /// This happens every time a brep is added or removed from the RhinoDoc
        /// This method will just update the selected breps collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Breps_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            // What is changed?
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;


                // We removed a brep from the collection -> Remove from selected collection
                case NotifyCollectionChangedAction.Remove:
                    foreach (BrepItemViewModel item in e.OldItems)
                    {
                        if (SelectedBreps.Contains(item))
                        {
                            SelectedBreps.Remove(item);
                        }


                        // Cleanup and remove from RhinoDoc
                        RhinoObject obj = RhinoDoc.Objects.Find(item.Guid);

                        if (obj != null)
                        {
                            // Start undo record
                            uint undo = RhinoDoc.BeginUndoRecord("Delete Brep");

                            // Delete object
                            RhinoDoc.Objects.Delete(obj.Id, false);

                            // Finish the undo record
                            RhinoDoc.EndUndoRecord(undo);


                        }
                    }


                    // Remember to redraw the view for more smooth interaction
                    RhinoDoc.Views.Redraw();
                    break;


                // This is called when we replace with Breps[index] = xxx
                case NotifyCollectionChangedAction.Replace:
                    break;

            }

            BrepCount = Breps.Count;
            SelectedBrepCount = SelectedBreps.Count;

        }

        /// <summary>
        /// Updates the brep in <see cref="Breps"/> to have the new name from Rhino.
        /// If other attributes are changed, this method can be expanded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RhinoDoc_ModifyObjectAttributes(object sender, RhinoModifyObjectAttributesEventArgs e)
        {
            BrepItemViewModel item = Breps.Where(b => b.Guid == e.RhinoObject.Id).FirstOrDefault();

            if (item != null)
            {
                item.Name = e.NewAttributes.Name;
            }
        }

        /// <summary>
        /// Fires when an object is deleted from Rhino.
        /// My experience is that it also happens if an object is moved, scaled etc.
        /// In that case its deleted and then added (with same GUID).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RhinoDoc_DeleteObject(object sender, RhinoObjectEventArgs e)
        {
            // Option 1: Keep the item in the list, but mark it as missing
            //var item = Breps.Where(b => b.Guid == e.ObjectId).FirstOrDefault();
            //if (item != null)
            //{
            //    item.Area = -1;
            //    item.Name = "<Missing>";

            //}

            // Option 2: Or just remove??
            var item3 = Breps.Where(b => b.Guid == e.ObjectId).FirstOrDefault();
            if (item3 != null)
            {
                Breps.Remove(item3);
            }

            // Make sure to remove from selected collection
            var item2 = SelectedBreps.Where(b => b.Guid == e.ObjectId).FirstOrDefault();
            if (item2 != null)
            {
                SelectedBreps.Remove(item2);
            }

            // Update BrepCount
            BrepCount = Breps.Count;

        }


        /// <summary>
        /// Fires when an object is added to Rhino.
        /// This also happens if an object is moved, scaled etc (Delete + Add with same GUID)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RhinoDoc_AddObject(object sender, RhinoObjectEventArgs e)
        {
            RhinoObject obj = RhinoDoc.Objects.FindId(e.ObjectId);

            // If object is of correct type
            if (obj.Geometry.ObjectType == (obj.Geometry.ObjectType & (ObjectType.Extrusion | ObjectType.Surface | ObjectType.Brep | ObjectType.Mesh)))
            {

                // Check if the object is already in the list
                var matchingBrep = Breps.Where(Breps => Breps.Guid == obj.Id).FirstOrDefault();

                // If it is
                if (matchingBrep != null)
                {
                    var index = Breps.IndexOf(matchingBrep);

                    // Update the area. Pay attention to using the index, since the object is already in the list
                    // This fires Breps.CollectionChanged with e.Action = Replace
                    Breps[index].Area = AreaMassProperties.Compute((Brep)obj.Geometry).Area;
                }
                else
                {
                    // If it is not in the list, add it
                    var item = BrepItemViewModel.FromGuid(obj.Id, RhinoDoc);
                    if (item != null)
                        Breps.Add(item);
                }

            }

            // Update BrepCount
            BrepCount = Breps.Count;
        }


        /// <summary>
        /// This method is called when the user selects or deselects objects in the Rhino viewport.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RhinoDoc_SelectObjects(object sender, RhinoObjectSelectionEventArgs e)
        {
            //// This SHOULDNT be needed as it is instanciated in the property declaration.
            //// I had a null bug though at some point so I'm keeping it here for now.
            //if (SelectedBreps == null)
            //    SelectedBreps = new ObservableCollection<BrepItemViewModel>();

            // Loop through all the objects that are selected or deselected in the event

            RhinoApp.WriteLine(e.Selected ? "Selected" : "Deselected");

            foreach (var item in e.RhinoObjects)
            {
                if (e.Selected)
                {
                    // It exists in Breps so we add it to SelectedBreps
                    BrepItemViewModel ii = Breps.Where(b => b.Guid == item.Id).FirstOrDefault();
                    if (ii != null && !SelectedBreps.Contains(ii))
                    {

                        // Add to the selected collection which should update UI with the listview binding
                        SelectedBreps.Add(ii);

                    }
                }
                else
                {
                    // It exists in selectedBreps so we remove it from selectedBreps
                    BrepItemViewModel iii = SelectedBreps.Where(b => b.Guid == item.Id).FirstOrDefault();
                    if (iii != null && SelectedBreps.Contains(iii))
                    {

                        // Remove from the selected collection which should update UI with the listview binding
                        SelectedBreps.Remove(iii);
                    }
                }
            }

            // Update the viewport
            RhinoDoc.Views.Redraw();


        }

        void DeleteGuid(BrepItemViewModel item)
        {
            if (Breps.Contains(item))
            {
                Breps.Remove(item);

            }
        }

        private void RhinoDoc_UndeleteRhinoObject(object sender, RhinoObjectEventArgs e)
        {
            RhinoDoc_AddObject(sender, e);
        }





        /// <summary>
        /// This is the most modern way of doing this.
        /// This way you dont have to specify the property name.
        /// https://www.danrigby.com/2015/09/12/inotifypropertychanged-the-net-4-6-way/
        /// 
        /// Some people also like to call this RaisePropertyChanged(?)
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        ///     Checks if a property already matches a desired value.
        ///     Sets the property and notifies listeners only when necessary.
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
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Dispose()
        {
            UnregistrerEventHandlers();
        }
    }
}
