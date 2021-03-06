﻿using Restaurant_Aid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Restaurant_Aid.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Restaurant_Aid;

namespace Restaurant_Aid.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuEdit : ContentPage
    {
        TodoItemManager manager;
        
        public event EventHandler<RMenuItem> MenuItemSaved;

        RMenuItem TheMenuItem { get; set; }
        bool IsNew { get; set; }

        public MenuEdit()
        {
            InitializeComponent();

            TheMenuItem = new RMenuItem();
            IsNew = true;
            manager = TodoItemManager.DefaultManager;
            InitializePage();
        }

        public MenuEdit(RMenuItem item)
        {
            InitializeComponent();

            TheMenuItem = item;
            IsNew = false;

            InitializePage();
        }

        void InitializePage()
        {
            Title = TheMenuItem.Name ?? "New Menu Item";

            itemNameCell.Text = TheMenuItem.Name;
            descriptionCell.Text = TheMenuItem.Description;
            priceCell.Text = TheMenuItem.Price;

            saveButton.Clicked += async (sender, args) =>
            {
                SaveMenuItem();
                // ADD MENU ITEMS TO DB
                await addToDoItem();
                await CloseWindow();
            };

            cancelButton.Clicked += async (sender, args) =>
            {
                await CloseWindow();
            };
        }

        async Task CloseWindow()
        {
            if (IsNew)
                await Navigation.PopModalAsync(true);
            else
                await Navigation.PopAsync(true);
        }

        void SaveMenuItem()
        {
            TheMenuItem.Name = itemNameCell.Text;
            TheMenuItem.Description = descriptionCell.Text;
            TheMenuItem.Price = priceCell.Text;

            if (IsNew)
            {
                TheMenuItem.Id = Guid.NewGuid();
                //CHANGED
                App.RMenuList.Add(TheMenuItem);
            }
            else
            {
                //CHANGED
                var savedItem = App.RMenuList.Find(r => r.Id == TheMenuItem.Id);
                savedItem = TheMenuItem;

                MenuItemSaved?.Invoke(this, savedItem);
            }
        }

        async Task addToDoItem()
        {
            TodoItem item = new TodoItem { Name = TheMenuItem.Name };
            await manager.SaveTaskAsync(item);
        }
    }
}