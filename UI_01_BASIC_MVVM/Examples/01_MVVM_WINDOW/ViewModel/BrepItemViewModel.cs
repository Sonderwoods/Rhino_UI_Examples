using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UI_01_BASIC_MVVM.Examples._01_MVVM_WINDOW.ViewModel
{

    /// <summary>
    /// This viewmodel is used to represent a Brep item in a listbox.
    /// 
    /// It could be implemented with INotifyPropertyChanged, but we are using DependencyObject instead.
    /// Check <see cref="BrepItemViewModel"/> for the INotifyPropertyChanged implementation.
    /// 
    /// In the end it doesnt matter which direction you opt for.
    /// </summary>
    public class BrepItemViewModel : DependencyObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The brep guid is not a dependency object as it will never change.
        /// </summary>
        public Guid Guid { get; set; }




        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(BrepItemViewModel), new PropertyMetadata(false));





        /// <summary>
        /// This is a dependencyproperty and will update the UI when changed.
        /// Units are in m2 no matter what the document units are. (converted at creation time)
        /// </summary>
        public double Area
        {
            get { return (double)GetValue(AreaProperty); }
            set { SetValue(AreaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Area.  This enables animation, styling, binding, etc...
        // The new PropertyMetadata is the 'default' value, in this case 0
        public static readonly DependencyProperty AreaProperty =
            DependencyProperty.Register("Area", typeof(double), typeof(BrepItemViewModel), new PropertyMetadata(0.0));



        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(BrepItemViewModel), new PropertyMetadata("Unnamed"));

        // TODO: Async?
        public static BrepItemViewModel FromGuid(Guid guid, RhinoDoc document)
        {

            BrepItemViewModel it = new BrepItemViewModel(guid, document);
            if (document != null)
            {
                RhinoObject obj = document.Objects.Find(guid);
                if (obj != null)
                {
                    it.Guid = obj.Id;
                    it.Name = obj.Name ?? string.Empty;

                    switch (obj.Geometry)
                    {
                        case Brep brep:
                            it.Area = AreaMassProperties.Compute(brep, true, false, false, false).Area;
                            break;

                        case Mesh mesh:
                            it.Area = AreaMassProperties.Compute(mesh).Area;
                            break;

                        case Extrusion extrusion:
                            it.Area = AreaMassProperties.Compute(extrusion).Area;
                            break;

                        case Surface surface:
                            it.Area = AreaMassProperties.Compute(surface).Area;
                            break;

                    }

                    return it;
                }

                return null;

            }

            return null;
        }


        public BrepItemViewModel(Guid guid, RhinoDoc document)
        {
            if (document != null)
            {
                RhinoObject obj = document.Objects.Find(guid);
                if (obj != null && obj.Geometry is Brep brep)
                {
                    Guid = obj.Id;
                    Name = obj.Name ?? string.Empty;
                    AreaMassProperties amp = AreaMassProperties.Compute(brep, true, false, false, false);
                    Area = amp.Area;

                    // TODO: Area conversion as per rhino doc units. Need helper method.
                }
                else
                {
                    Name = "Invalid object";
                }


            }
            else
            {
                Name = "No Document";
            }
        }

    }
}
