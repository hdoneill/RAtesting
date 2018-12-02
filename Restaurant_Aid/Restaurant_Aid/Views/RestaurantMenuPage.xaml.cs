using Xamarin.Forms;
using Restaurant_Aid.ViewModels;
using System;
using System.Collections.Generic;
using Restaurant_Aid.Model;
using Restaurant_Aid;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace Restaurant_Aid.Views
{


    public partial class RestaurantMenuPage : ContentPage
    {
        Xamarin.Forms.ListView todoList;
        Xamarin.Forms.ListView syncIndicator;
        Xamarin.Forms.ListView newItemName;
        Xamarin.Forms.ListView buttonsPanel;


        TodoItemManager manager;

        public RestaurantMenuPage()
        {
            InitializeComponent();

            manager = TodoItemManager.DefaultManager;
            if (Device.RuntimePlatform == Device.UWP)
            {
                var refreshButton = new Button
                {
                    Text = "Refresh",
                    HeightRequest = 30
                };
                refreshButton.Clicked += OnRefreshItems;
               // buttonsPanel.ChildAdded(refreshButton);
                if (manager.IsOfflineEnabled)
                {
                    var syncButton = new Button
                    {
                        Text = "Sync items",
                        HeightRequest = 30
                    };
                    syncButton.Clicked += OnSyncItems;
                   // buttonsPanel.Children.Add(syncButton);
                }
            }

            menuList.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem != null)
                {
                    var detailPage = new MenuDetailPage(e.SelectedItem as RMenuItem);

                    await Navigation.PushAsync(detailPage);

                    menuList.SelectedItem = null;
                }
                TodoItem item = new TodoItem { Name = "the item test2" };
                await manager.SaveTaskAsync(item);
            };
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            menuList.ItemsSource = null;
            //CHANGED
            menuList.ItemsSource = App.RMenuList;

            // Set syncItems to true in order to synchronize the data on startup when running in offline mode
           // await RefreshItems(true, syncItems: true);
        }

        async void Add_Clicked(object sender, System.EventArgs e)
        {
            var editPage = new MenuEdit();

            var editNavPage = new NavigationPage(editPage)
            {
                BarBackgroundColor = Color.FromHex("#01487E"),
                BarTextColor = Color.White
            };

            await Navigation.PushModalAsync(editNavPage);
        }

        // Data methods
        async Task AddItem(TodoItem item)
        {
            await manager.SaveTaskAsync(item);
            todoList.ItemsSource = await manager.GetTodoItemsAsync();
        }
        async Task CompleteItem(TodoItem item)
        {
            item.Done = true;
            await manager.SaveTaskAsync(item);
            todoList.ItemsSource = await manager.GetTodoItemsAsync();
        }

        public async void OnAdd(object sender, EventArgs e)
        {
           // var todo = new TodoItem { Name = newItemName.RotateXTo };
            //await AddItem(todo);

           // newItemName.Text = string.Empty;
           // newItemName.Unfocus();
        }

        // Event handlers
        public async void OnSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var todo = e.SelectedItem as TodoItem;
            if (Device.RuntimePlatform != Device.iOS && todo != null)
            {
                // Not iOS - the swipe-to-delete is discoverable there
                if (Device.RuntimePlatform == Device.Android)
                {
                    await DisplayAlert(todo.Name, "Press-and-hold to complete task " + todo.Name, "Got it!");
                }
                else
                {
                    // Windows, not all platforms support the Context Actions yet
                    if (await DisplayAlert("Mark completed?", "Do you wish to complete " + todo.Name + "?", "Complete", "Cancel"))
                    {
                        await CompleteItem(todo);
                    }
                }
            }

            // prevents background getting highlighted
            todoList.SelectedItem = null;
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#context
        public async void OnComplete(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            var todo = mi.CommandParameter as TodoItem;
            await CompleteItem(todo);
        }

        // http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/listview/#pulltorefresh
        public async void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            Exception error = null;
            try
            {
                await RefreshItems(false, true);
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                list.EndRefresh();
            }

            if (error != null)
            {
                await DisplayAlert("Refresh Error", "Couldn't refresh data (" + error.Message + ")", "OK");
            }
        }

        public async void OnSyncItems(object sender, EventArgs e)
        {
            await RefreshItems(true, true);
        }

        public async void OnRefreshItems(object sender, EventArgs e)
        {
            await RefreshItems(true, false);
        }

        private async Task RefreshItems(bool showActivityIndicator, bool syncItems)
        {
            //using (var scope = new ActivityIndicatorScope(syncIndicator, showActivityIndicator))
            //{
            //    todoList.ItemsSource = await manager.GetTodoItemsAsync(syncItems);
            //}
        }

        private class ActivityIndicatorScope : IDisposable
        {
            private bool showIndicator;
            private ActivityIndicator indicator;
            private Task indicatorDelay;

            public ActivityIndicatorScope(ActivityIndicator indicator, bool showIndicator)
            {
                this.indicator = indicator;
                this.showIndicator = showIndicator;

                if (showIndicator)
                {
                    indicatorDelay = Task.Delay(2000);
                    SetIndicatorActivity(true);
                }
                else
                {
                    indicatorDelay = Task.FromResult(0);
                }
            }

            private void SetIndicatorActivity(bool isActive)
            {
                this.indicator.IsVisible = isActive;
                this.indicator.IsRunning = isActive;
            }

            public void Dispose()
            {
                if (showIndicator)
                {
                    indicatorDelay.ContinueWith(t => SetIndicatorActivity(false), TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

    }
}
