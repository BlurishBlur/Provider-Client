﻿using Provider.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Provider.gui
{
    /// <summary>
    /// Interaction logic for CreateNewPost.xaml
    /// </summary>
    public partial class CreateNewPostPage : Page
    {
        private BulletinBoardPage bulletinBoardPage;
        public CreateNewPostPage(BulletinBoardPage Bulletinboard)
        {
            InitializeComponent();
            this.bulletinBoardPage = Bulletinboard;
            CreationDateTextBlock.Text = DateTime.Today.ToShortDateString();
            OwnerTextBlock.Text = Controller.instance.GetLoggedInUserName();
        }
        /// 0 is error
        /// "1" is warningPost
        /// "2" is requestPost
        /// "3" is offerPost
        /// skal måske laves om... det snakker vi lige om
        private void CreateNewPost(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PostDescriptionTextBox.Text) && !string.IsNullOrWhiteSpace(postTitleTextBox.Text))
            {
                if (WarningRB.IsChecked == false && OfferRB.IsChecked == false && requestRB.IsChecked == false)
                {
                    SomthingWentWrongLabel.Content = "Husk at vælge katagori";
                    SomthingWentWrongLabel.Visibility = Visibility.Visible;
                } else
                {
                    CreatePost();
                } 

            } else
            {
                SomthingWentWrongLabel.Content = "Der skal være noget indhold dit opslag";
                SomthingWentWrongLabel.Visibility = Visibility.Visible;
            }
              
        }
        private void CreatePost()
        {

            domain.bulletinboard.Post.Types typeOfPost;

            if (WarningRB.IsChecked.Value)
            {
                typeOfPost = domain.bulletinboard.Post.Types.Warning;
            }
            else if (requestRB.IsChecked.Value)
            {
                typeOfPost = domain.bulletinboard.Post.Types.Request;
            }
            else if (OfferRB.IsChecked.Value)
            {
                typeOfPost = domain.bulletinboard.Post.Types.Offer;
            } else
            {
                typeOfPost = domain.bulletinboard.Post.Types.NotAvailabe;
            }
                Controller.instance.CreatePost(Controller.instance.GetLoggedInUserName(), postTitleTextBox.Text, PostDescriptionTextBox.Text, typeOfPost);
                bulletinBoardPage.RefreshPage(true);
        }

        private void TitleLostFocus(object sender, RoutedEventArgs e)
        {
            if (postTitleTextBox.Text.Length == 0)
                titleText.Visibility = Visibility.Visible;
        }

        private void TitleGotFocus(object sender, RoutedEventArgs e)
        {
            titleText.Visibility = Visibility.Hidden;
        }

        private void PostDescriptionLostFocus(object sender, RoutedEventArgs e)
        {
            if (PostDescriptionTextBox.Text.Length == 0)
                desriptionText.Visibility = Visibility.Visible;
        }

        private void PostDescriptionGotFocus(object sender, RoutedEventArgs e)
        {
            desriptionText.Visibility = Visibility.Hidden;
        }
    }
}