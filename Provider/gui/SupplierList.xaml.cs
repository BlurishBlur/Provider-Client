﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;

namespace Provider.gui
{

    public partial class SupplierList : Page
    {
        private Frame mainWindow;
        private List<IO.Swagger.Model.Page> listToShow2;
        private ICollectionView dataView;
        private GridViewColumnHeader lastHeaderClicked;
        private ListSortDirection lastDirection = ListSortDirection.Descending;

        public SupplierList(Frame mainWindow, List<IO.Swagger.Model.Page> listToShow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.listToShow2 = listToShow;
            listView.ItemsSource = this.listToShow2;
            foreach(IO.Swagger.Model.Page p in this.listToShow2)
            {
                System.Diagnostics.Debug.WriteLine(p.Owner);
                //System.Diagnostics.Debug.WriteLine(p.Note.Text);
            }
            dataView = CollectionViewSource.GetDefaultView(listView.ItemsSource);
        }


        private void ViewSupplierInformation(object sender, MouseButtonEventArgs e)
        {
            //mainWindow.Content = new SupplierInformation((Provider.domain.page.Page) listView.SelectedItem);
            mainWindow.Content = new SupplierInformation((IO.Swagger.Model.Page) listView.SelectedItem);
        }

        private void SortSupplierInformation(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = (GridViewColumnHeader)e.OriginalSource;
            ListSortDirection direction;

            if(headerClicked != null)
            {
                if(headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if(headerClicked != lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if(lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }
                    string header = (string)headerClicked.Content;
                    Sort(header, direction);
                    if(direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate = (DataTemplate)Resources["HeaderTemplateArrowDown"];
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate = (DataTemplate)Resources["HeaderTemplateArrowUp"];
                    }
                    if(lastHeaderClicked != null && lastHeaderClicked != headerClicked)
                    {
                        lastHeaderClicked.Column.HeaderTemplate = null;
                    }
                    lastHeaderClicked = headerClicked;
                    lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            dataView.SortDescriptions.Clear();
            if(sortBy.Equals("Navn"))
            {
                sortBy = "Owner";
            }
            else if(sortBy.Equals("Note"))
            {
                sortBy = "Note.Text";
            }
            SortDescription sortDesc = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sortDesc);
            dataView.Refresh();
        }
    }
}
