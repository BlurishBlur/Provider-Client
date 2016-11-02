﻿using Provider.domain.page;
using Provider.domain.users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.domain
{
    public class Controller : IController
    {
        private static Controller instance;
        private UserManager userManager;
        private PageManager pageManager;

        public static Controller Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Controller();
                }
                return instance;
            }
        }

        private Controller()
        {
            userManager = new UserManager();
            pageManager = new PageManager();
        }

        public List<Page> ViewSuppliers()
        {
            return pageManager.GetSuppliers();
        }

        public Page GetSupplierPage(Supplier supplier)
        {
            return pageManager.GetSupplierPage(supplier);
        }
        
        public Boolean Login(String name, String password)
        {
            return userManager.Validate(name, password);
        }
    }
}