using Android.App;
using Android.Widget;
using Android.OS;
using System;

namespace dondestoy
{
    [Activity(Label = "Dondestoy", MainLauncher = true, Icon = "@drawable/ic_launcher",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)] // Without that, it crashes when orientation changed
    public class MainActivity : Activity
    {
        bool locationEnabled = false;
        TextView info;
        Button button;
        Button buttonForce;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            info = this.FindViewById<TextView>(Resource.Id.info);
            button = this.FindViewById<Button>(Resource.Id.locationButton);
            buttonForce = this.FindViewById<Button>(Resource.Id.locationForceButton);

            info.Text = "Enable GPS first and start service";

            button.Click += Button_Click;
            buttonForce.Click += ButtonForce_Click;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            startStop(false);
        }

        private void ButtonForce_Click(object sender, EventArgs e)
        {
            startStop(true);
        }

        Android.Content.Intent intent;
        private void startStop(bool force)
        {
            if (locationEnabled)
            {
                info.Text = "Services are stopped";
                //StopListening();

                StopService(intent);

                button.Text = Resources.GetString(Resource.String.locationButton);
                buttonForce.Visibility = Android.Views.ViewStates.Visible;
                locationEnabled = false;
            }
            else
            {
                if (force)
                    info.Text = "Background & Location service is running";
                else
                    info.Text = "Background service is running";

                //StartListening();

                if (intent == null)
                    intent = new Android.Content.Intent(this, typeof(dondestoyService));
                intent.PutExtra("forcestart", force);
                StartService(intent);

                button.Text = "Stop services";
                buttonForce.Visibility = Android.Views.ViewStates.Invisible;
                locationEnabled = true;
            }
        }
    }
}

