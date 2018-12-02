using Restaurant_Aid.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Restaurant_Aid;



namespace Restaurant_Aid.Views
{

    public partial class MainPage : ContentPage
    {
        Microsoft.WindowsAzure.MobileServices.MobileServiceClient client;
        TodoItemManager manager;
        
        bool authenticated = false;
        public MainPage()
        {
           
            TodoItemManager manager;
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(MainPage)}:  ctor");
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Refresh items only when authenticated.
            if (authenticated == true)
            {
                // Set syncItems to true in order to synchronize the data
                // on startup when running in offline mode.
                await RefreshItems(true, syncItems: false);

                // Hide the Sign-in button.
                //this.loginButton.IsVisible = false;
            }
        }

        private MobileServiceUser user;
        public async Task<bool> Authenticate()
        {
            var success = false;
            var message = string.Empty;
            try
            {
                // Sign in with Facebook login using a server-managed flow.
                //user = await TodoItemManager.DefaultManager.CurrentClient.LoginAsync(
                //    MobileServiceAuthenticationProvider.Google,  "https://restaurantaid.azurewebsites.net");
                if (user != null)
                {
                    message = string.Format("you are now signed-in as {0}.",
                        user.UserId);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            // Display the success or failure message.
           // AlertDialog.Builder builder = new AlertDialog.Builder(this);
            //builder.SetMessage(message);
            //builder.SetTitle("Sign-in result");
            //builder.Create().Show();

            return success;
        }

        [Java.Interop.Export()]
        public async void LoginUser(View view)
        {
            // Load data only after authentication succeeds.
            if (await Authenticate())
            {
                //Hide the button after authentication succeeds.
                //FindViewById<Button>(Resource.Id.buttonLoginUser).Visibility = ViewStates.Gone;

                // Load the data.
                //OnRefreshItemsSelected();
            }
        }

        async void loginButton_Clicked(object sender, EventArgs e)
        {
            if (App.Authenticator != null)
                authenticated = await App.Authenticator.Authenticate();

            // Set syncItems to true to synchronize the data on startup when offline is enabled.
            if (authenticated == true)
            await RefreshItems(true, syncItems: false);
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