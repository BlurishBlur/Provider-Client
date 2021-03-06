﻿using Provider.domain.page;
using Provider.domain.users;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Provider.domain.bulletinboard;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;
using Provider.domain.security;
using System.Text;
using System.Windows;

namespace Provider.domain
{
    public class Controller : IController
    {
        private static IController _instance;
        private IUserManager userManager;
        private IPageManager pageManager;
        private IBulletinboard bulletinboard;
        private ControllerApi api;
        private RSA rsa;
        private readonly Object updateLock = new Object();

        public static IController instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Controller();
                }
                return _instance;
            }
        }

        private Controller()
        {
            userManager = new UserManager();
            pageManager = new PageManager();
            bulletinboard = new Bulletinboard();
            api = new ControllerApi("http://tek-sb3-glo0a.tek.sdu.dk:16832");
            rsa = new RSA(api.RequestPublicKey());
            Update();
        }

        private void Update()
        {
            new Thread(() =>
            {
                try
                {
                    while ((bool) api.RequestUpdate())
                    {
                        lock (updateLock)
                        {
                            GetSuppliers();
                            GetPosts();
                            Monitor.PulseAll(updateLock);
                        }
                    }
                }
                catch (ApiException) //fanger timeout
                {
                    Update();
                }
            }).Start();
        }

        public Object GetUpdateLock()
        {
            return updateLock;
        }

        public bool LogIn(string userName, string password)
        {
            bool validated = userManager.LogIn(userName, rsa.Encrypt(password));
            if (validated)
            {
                GetSuppliers();
                GetPosts();
            }
            return validated;
        }

        /// <summary>
        /// Logs out the currently logged in user.
        /// </summary>
        public void LogOut()
        {
            userManager.LogOut();
        }

        /// <summary>
        /// Gets the currently logged in user. 
        /// </summary>
        /// <returns> The user which is logged in</returns>
        public User GetLoggedInUser()
        {
            return userManager.loggedInUser;
        }

        /// <summary>
        /// Sets all the suppliers to the supplier list.
        /// </summary>
        public void GetSuppliers()
        {
            pageManager.GetSuppliers();
        }

        public List<Page> GetPages()
        {
            return pageManager.pages;
        }

        /// <summary>
        /// This method lists all the posts on the bulletinboard. 
        /// </summary>
        /// <returns> A list of all posts</returns>
        public void GetPosts()
        {
            bulletinboard.GetPosts();
        }

        public List<Post> ViewAllPosts()
        {
            return bulletinboard.posts;
        }

        /// <summary>
        /// This method lists all the warning posts. 
        /// </summary>
        /// <returns> A list of all warning posts</returns>
        public List<Post> ViewWarningPosts()
        {
            return bulletinboard.GetPosts(PostType.Warning);
        }

        /// <summary>
        /// This method lists all the request posts. 
        /// </summary>
        /// <returns> A list of all request posts</returns>
        public List<Post> ViewRequestPosts()
        {
            return bulletinboard.GetPosts(PostType.Request);
        }

        /// <summary>
        /// This method lists all the offer posts. 
        /// </summary>
        /// <returns> A list of all offer posts</returns>
        public List<Post> ViewOfferPosts()
        {
            return bulletinboard.GetPosts(PostType.Offer);
        }

        public Page FindPage(Page page)
        {
            return GetPages().Find(p => p.Equals(page));
        }

        public Page FindPageByName(string name)
        {
            return GetPages().Find(p => p.Owner.Equals(name));
        }

        public Product FindProduct(string name, int? id)
        {
            return GetPages().Find(page => page.Owner.Equals(name)).Products.Find(product => product.Id.Equals(id));
        }

        /// <summary>
        /// This method creates a post with a title, description and a post type. 
        /// </summary>
        /// <param name="owner">The owner of the post</param>
        /// <param name="title"> The title of the post</param>
        /// <param name="description"> The description of the post</param>
        /// <param name="type">The type of the post</param>
        public void CreatePost(string owner, string title, string description, PostType type)
        {
            bulletinboard.CreatePost(owner, title, description, type);
        }

        /// <summary>
        /// Deletes a post and removes it from the bulletinboard.
        /// </summary>
        /// <param name="post">The post which is being deleted.</param>
        public void DeletePost(Post post)
        {
            bulletinboard.DeletePost(post);
        }

        /// <summary>
        /// Edits an existing post. 
        /// </summary>
        /// <param name="post">The post which is being edited</param>
        /// <param name="newDescription"> The updated description of the post</param>
        /// <param name="newTitle"> The updated title of the post</param>
        public void EditPost(Post post, string newDescription, string newTitle)
        {
            bulletinboard.EditPost(post, newDescription, newTitle);
        }

        /// <summary>
        /// Edits the information on a supplierpage. 
        /// </summary>
        /// <param name="page">The page which is being edited</param>
        public void ManageSupplierPage(Page page)
        {
            pageManager.ManageSupplierPage(page);
        }

        /// <summary>
        /// Adds a note to a supplie.
        /// </summary>
        /// <param name="supplierName">Name of the supplier</param>
        /// <param name="editor">Name of the person which is creating the note.</param>
        /// <param name="text">The note which is being added to the supplier</param>
        public void AddNoteToSupplier(string supplierName, string editor, string text)
        {
            if (text.Length != 0) { 
                text = rsa.Encrypt(text);
            }
            pageManager.AddNoteToSupplier(supplierName, editor, text);
        }

        /// <summary>
        /// Searches through all suppliers and products with a given search term. 
        /// </summary>
        /// <param name="searchTerm">The term which is being searched on</param>
        /// <returns> A list with searchresults </returns>
        public List<Page> Search(string searchTerm)
        {
            return pageManager.Search(searchTerm);
        }

        /// <summary>
        /// Edits an existing product.  It checks whether the logged in user has the rights to edit
        /// the product by checking if the user is the producer of the product or an administrator.
        /// </summary>
        /// <param name="product">The product which is being edited </param>
        /// <param name="newProductName">The new name of the product</param>
        /// <param name="newChemicalName">The new chemical name of the product</param>
        /// <param name="newMolWeight">The new mol weight of the product</param>
        /// <param name="newDescription">The new description of the product</param>
        /// <param name="newPrice">The new price of the product</param>
        /// <param name="newPackaging">The new packaging of the product</param>
        /// <param name="newDeliveryTime">The new deliverytime of the product</param>
        public void EditProduct(Product product, string newProductName, string newChemicalName, Double newMolWeight,
            string newDescription, Double newPrice, string newPackaging, string newDeliveryTime)
        {
            if (GetLoggedInUser().Username.Equals(product.Producer) || GetLoggedInUser().Rights == User.RightsEnum.Admin)
            {
                pageManager.EditProduct(product, newProductName, newChemicalName, newMolWeight, newDescription, newPrice, newPackaging, newDeliveryTime);
            }
        }
        
        /// <summary>
        /// Creates a product with the given parameters. 
        /// </summary>
        /// <param name="productName">The name of the product</param>
        /// <param name="chemicalName">The chemical name of the product</param>
        /// <param name="molWeight">The mol weight of the product</param>
        /// <param name="description">The description of the product</param>
        /// <param name="price">The price of the product</param>
        /// <param name="packaging">The packaging of the product</param>
        /// <param name="deliveryTime">The deliverytime of the product</param>
        /// <param name="producer">The producer of the product</param>
        public void CreateProduct(string productName, string chemicalName, Double molWeight, string description, Double price, string packaging, string deliveryTime, string producer)
        {
            pageManager.CreateProduct(productName, chemicalName, molWeight, description, price, packaging, deliveryTime, producer);
        }

        /// <summary>
        /// This method deletes a product. It checks whether the logged in user has the rights to delete
        /// the product by checking if the user is the producer of the product or an administrator.
        /// If the logged in user has the rights the product gets deleted.
        /// </summary>
        /// <param name="product">The product which is being deleted</param>
        public void DeleteProduct(Product product)
        {
            if (GetLoggedInUser().Username.Equals(product.Producer) || GetLoggedInUser().Rights == User.RightsEnum.Admin)
            {
                pageManager.DeleteProduct(product);
            }
        }

        /// <summary>
        /// Opens the PDF datasheet of a product. 
        /// </summary>
        /// <param name="id">The ID of the product</param>
        public void GetPDF(int? id)
        {
            new Thread(() =>
            {
                string finalString = new String(GetRandomCharArray(10)) + ".pdf";
                string filePath = Path.GetTempPath() + "Provider/";
                FileInfo FileInfo = new FileInfo(filePath);
                FileInfo.Directory.Create();
                var file = File.Create(filePath + finalString);

                api.GetPDF(id).CopyTo(file);

                file.Close();
                System.Diagnostics.Process.Start(filePath + finalString);
            }).Start();
        }

        /// <summary>
        /// When the PDF files are downloaded from the server, they will be saved locally in a temporary folder.
        /// The method deleted the temporary files when the program is closed. 
        /// </summary>
        public void DeleteTempFiles()
        {
            try
            {
                Directory.Delete(Path.GetTempPath() + "Provider", true);
            }
            catch (DirectoryNotFoundException)
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Generates a filename to the downloaded PDF file with random characters. 
        /// </summary>
        /// <param name="size">Number of characters in the filename</param>
        /// <returns></returns>
        private char[] GetRandomCharArray(int size)
        {
            char[] chars = new char[size];
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                chars[i] = (char) (random.Next(26) + 97);
            }
            return chars;
        }
    }
}
