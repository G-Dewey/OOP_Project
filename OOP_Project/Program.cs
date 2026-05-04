// See https://aka.ms/new-console-template for more information
using ErrorOr;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Terminal.Gui;

namespace OOP_Project
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Debug.Log("Application started.");

            var gui = new FrontendHandler();

            gui.RunApp();
        }
    }
}