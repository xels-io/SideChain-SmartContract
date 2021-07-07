﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace XelsDesktopWalletApp.Views.layout
{
    /// <summary>
    /// Interaction logic for MainLayout.xaml
    /// </summary>
    public partial class MainLayout : Window
    {
        public MainLayout()
        {
            InitializeComponent();
        }

        private void ButtonFechar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void GridBarraTitulo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            this.PageContent.Content = new Testpage();
        }

        //private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        //{
        //    this.ButtonCloseMenu.Visibility = Visibility.Visible;
        //    this.ButtonOpenMenu.Visibility = Visibility.Collapsed;
        //    this.GridMain.Width = 880;
        //}

        //private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        //{
        //    this.ButtonCloseMenu.Visibility = Visibility.Collapsed;
        //    this.ButtonOpenMenu.Visibility = Visibility.Visible;
        //    this.GridMain.Width = 1010;

        //}

        //private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //UserControl usc = null;
        //    //GridMain.Children.Clear();

        //    //switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
        //    //{
        //    //    case "ItemHome":
        //    //        usc = new UserControlHome();
        //    //        GridMain.Children.Add(usc);
        //    //        break;
        //    //    case "ItemCreate":
        //    //        usc = new UserControlCreate();
        //    //        GridMain.Children.Add(usc);
        //    //        break;
        //    //    case "Page1":
        //    //        GridMain.Children.Add(new Page1());
        //    //        break;
        //    //    default:
        //    //        break;
        //    //}

        //    Page usc = null;
        //    this.GridMain.Content = null;

        //    switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
        //    {

        //        case "Page1":
        //            this.GridMain.Content = new Page1();
        //            break;
        //        case "Page2":
        //            this.GridMain.Content = new Page2();
        //            break;
        //        default:
        //            break;
        //    }
        //}

    }
}
