using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UI_01_BASIC_MVVM.BindingHelpers
{

    /// <summary>
    /// https://stackoverflow.com/questions/31176949/binding-selecteditems-of-listview-to-viewmodel
    /// </summary>
    public class SelectedItemsBinder
    {
        private readonly ListView _listView;
        private readonly IList _collection;


        public SelectedItemsBinder(ListView listView, IList collection)
        {
            _listView = listView;
            _collection = collection;

            _listView.SelectedItems.Clear();

            foreach (var item in _collection)
            {
                _listView.SelectedItems.Add(item);
            }
        }

        public void Bind()
        {
            _listView.SelectionChanged += ListView_SelectionChanged;

            if (_collection is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged += Collection_CollectionChanged;
            }
        }

        public void UnBind()
        {
            if (_listView != null)
                _listView.SelectionChanged -= ListView_SelectionChanged;

            if (_collection != null && _collection is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged -= Collection_CollectionChanged;
            }
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems ?? new object[0])
            {
                if (!_listView.SelectedItems.Contains(item))
                    _listView.SelectedItems.Add(item);
            }
            foreach (var item in e.OldItems ?? new object[0])
            {
                _listView.SelectedItems.Remove(item);
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems ?? new object[0])
            {
                if (!_collection.Contains(item))
                    _collection.Add(item);
            }

            foreach (var item in e.RemovedItems ?? new object[0])
            {
                _collection.Remove(item);
            }
        }
    }
}
