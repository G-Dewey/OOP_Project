using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terminal.Gui;

namespace OOP_Project
{
    public sealed class FrontendHandler : Window
    {
        // Home Page created in the constructor
        public FrontendHandler()
        {
            CreateHomePage();
        }

        private void CreateHomePage()
        {
            var titleLable = new Label()
            {
                Text = "Welcome to the WestEng Job Shop Scheduler"
                
            };
            
            Add(titleLable);
        }
    }
}
