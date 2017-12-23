using System;

using Android.App;
using Android.Content;
using Android.OS;
using Plugin.Geolocator;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using dondestoy.Model;
using Plugin.DeviceInfo;

namespace dondestoy
{
    [Service(Exported = true, Name = "dondestoy.dondestoyService")]
    public class dondestoyService : Service
    {
        // Firebase variables, you can set them up by code
        private string ApplicationId = "";
        private string ApiKey = "";
        private string DatabaseUrl = "";

        // Firebase Auth, very unsecure, only use for debug purposes
        private string Email = "";
        private const string Password = "";

        string android_id = CrossDeviceInfo.Current.Id;
        public static Device devicesettings;

        // Firebase variables
        public static FirebaseApp app;
        public static FirebaseAuth auth;
        public static DatabaseReference databaseReference;
        
        const int NOTIFICATION_ID = 9000;
        Notification.Builder notificationBuilder;
        NotificationManager notificationManager;

        //private PowerManager.WakeLock mWakeLock;
        DateTimeOffset lastPositionTime;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // Work has finished, now dispatch anotification to let the user know.
            notificationBuilder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetContentTitle(Resources.GetString(Resource.String.ApplicationName))
                .SetContentText("Service location started");

            notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());

            bool forcestart = intent.GetBooleanExtra("forcestart", false);
            InitFirebase(forcestart);

            NotificationText("Service is running", "Location recorder is disabled");


            return StartCommandResult.StickyCompatibility;
        }

        private void NotificationText(string text, string subtext)
        {
            notificationBuilder.SetContentText(text);
            if (!string.IsNullOrEmpty(subtext))
                notificationBuilder.SetSubText(subtext);
            notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
        }

        public override IBinder OnBind(Intent intent)
        {
            // This example isn't of a bound service, so we just return NULL.
            return null;
        }

        public override void OnCreate()
        {
            devicesettings = null;

            base.OnCreate();
            //PowerManager pm = (PowerManager)GetSystemService(Context.PowerService);
            //mWakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "PartialWakeLockTag");
            //mWakeLock.Acquire();
        }

        public override void OnDestroy()
        {
            StopListening();
            //mWakeLock.Release();
            Firebase.FirebaseApp.Instance.Dispose();
            if (app != null)
                app.UnregisterFromRuntime();

            notificationManager.Cancel(NOTIFICATION_ID);
            base.OnDestroy();
        }

        private bool initializeLocation()
        {
            bool ret = false;
            string info = string.Empty;

            if (IsLocationAvailable())
            {
                if (IsLocationEnabled())
                {
                    info = "Waiting for position...";
                    ret = true;
                }
                else
                {
                    info = "GPS disabled";
                }
            }
            else
            {
                info = "Location NOT available";
            }

            NotificationText(info, string.Empty);

            return ret;
        }

        public bool IsLocationAvailable()
        {
            if (!CrossGeolocator.IsSupported)
                return false;

            return CrossGeolocator.Current.IsGeolocationAvailable;
        }

        public bool IsLocationEnabled()
        {
            return CrossGeolocator.Current.IsGeolocationEnabled;
        }

        async Task StartListening()
        {
            if (!initializeLocation())
                return;

            if (CrossGeolocator.Current.IsListening)
                return;

            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(devicesettings.frecuency), devicesettings.distancemin, true);
            await Task.Delay(3000);

            if (databaseReference != null)
                databaseReference.Child("devices/" + android_id + "/working").SetValue(1);

            CrossGeolocator.Current.PositionChanged += PositionChanged;
            CrossGeolocator.Current.PositionError += PositionError;
        }

        private void PositionChanged(object sender, PositionEventArgs e)
        {

            //If updating the UI, ensure you invoke on main thread
            var position = e.Position;
            /*var output = "Full: Lat: " + position.Latitude + " Long: " + position.Longitude;
            output += "\n" + $"Time: {position.Timestamp}";
            output += "\n" + $"Heading: {position.Heading}";
            output += "\n" + $"Speed: {position.Speed}";
            output += "\n" + $"Accuracy: {position.Accuracy}";
            output += "\n" + $"Altitude: {position.Altitude}";
            output += "\n" + $"Altitude Accuracy: {position.AltitudeAccuracy}";*/
            //Console.WriteLine(output);

            if (lastPositionTime == position.Timestamp)
                return;

            lastPositionTime = position.Timestamp;

            string text = string.Format("{0}, {1} - ({2}m)", position.Latitude, position.Longitude, Math.Round(position.Accuracy, 2));
            string subtext = string.Format("{0}", position.Timestamp.ToLocalTime().ToString("dd/MM/yyyy H:mm:ss"));

            if (position.Accuracy <= devicesettings.accuracytosave)
            {
                Create(position);
                subtext += " - saved";
            }

            NotificationText(text, subtext);            
        }

        private void PositionError(object sender, PositionErrorEventArgs e)
        {
            //Console.WriteLine(e.Error);
            //Handle event here for errors
            NotificationText(e.Error.ToString(), "Error");
        }

        async Task StopListening()
        {
            if (databaseReference != null)
            {
                if (devicesettings.enabled > 0)
                    databaseReference.Child("devices/" + android_id + "/enabled").SetValue(0);
                if (devicesettings.working > 0)
                    databaseReference.Child("devices/" + android_id + "/working").SetValue(0);
            }

            if (!CrossGeolocator.Current.IsListening)
                return;

            NotificationText("Location disabled", string.Empty);

            await CrossGeolocator.Current.StopListeningAsync();

            CrossGeolocator.Current.PositionChanged -= PositionChanged;
            CrossGeolocator.Current.PositionError -= PositionError;
        }

        private void InitFirebase(bool forcestart)
        {
            try
            {
                // Initialize Firebase app
                if (app == null)
                {
                    if (string.IsNullOrWhiteSpace(ApplicationId) || string.IsNullOrWhiteSpace(ApiKey) || string.IsNullOrWhiteSpace(DatabaseUrl))
                    {
                        // Show auth dialog to set firebase variables and authentication
                        //ShowAuthDialog();
                        return;
                    }

                    var options = new FirebaseOptions.Builder()
                    .SetApplicationId(ApplicationId)
                    .SetApiKey(ApiKey)
                    .SetDatabaseUrl(DatabaseUrl)
                    .Build();

                    app = FirebaseApp.InitializeApp(this, options);
                }

                // Get authorization on our Firebase app
                if (auth == null)
                {
                    auth = FirebaseAuth.GetInstance(app);

                    auth.SignInWithEmailAndPassword(Email, Password); //.AddOnCompleteListener(OnCompleteAuth);
                }

                // Get a database reference
                if (databaseReference == null)
                {
                    var db = FirebaseDatabase.GetInstance(app);
                    db.SetPersistenceEnabled(true);

                    // All our operations will be done on device id child
                    databaseReference = db.GetReference("/"); // + DateTime.Now.ToString("/yyyyMMdd"));

                    Load();
                }

                if (forcestart)
                {
                    databaseReference.Child("devices/" + android_id + "/enabled").SetValue(1);
                    //databaseReference.Child("devices/" + android_id + "/working").SetValue(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                NotificationText(ex.Message, string.Empty);
            }
        }

        private void Load()
        {
            try
            {
                OnValueEventListener OnValueEvent = new OnValueEventListener();
                OnValueEvent.Raised += (s, e) =>
                {
                    if (e.snapshot == null)
                        return;

                    bool firstTime = devicesettings == null;

                    devicesettings = new Device(e.snapshot);

                    if (firstTime || devicesettings.working == 0)
                    {
                        if (devicesettings.enabled > 0)
                            StartListening();
                    }
                    else
                    {
                        if (devicesettings.enabled == 0)
                            StopListening();
                    }                    
                };
                
                // Get our device sttings
                databaseReference.Child("devices/" + android_id).AddValueEventListener(OnValueEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                NotificationText(ex.Message, string.Empty);
            }
        }

        private void Create(Position position)
        {
            // Set the thing
            GPS th = new GPS()
            {
                latitude = position.Latitude,
                longitude = position.Longitude,
                timestamp = position.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"), //YYYY-MM-DDTHH:MM:SSZ
                heading = position.Heading,
                speed = position.Speed,
                accuracy = position.Accuracy,
                altitude = position.Altitude,
                altaccuracy = position.AltitudeAccuracy,
                battery = RemainingChargePercent
            };

            try
            {
                // Create a new one
                databaseReference.Child(android_id + "/" + position.Timestamp.ToLocalTime().ToString("yyyyMMdd")).Push().SetValue(th.ModelToMap());//.AddOnCompleteListener(OnComplete).AddOnFailureListener(OnFailure);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                NotificationText(ex.Message, string.Empty);
            }
        }

        public int RemainingChargePercent
        {
            get
            {
                try
                {
                    using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                    {
                        using (var battery = Application.Context.RegisterReceiver(null, filter))
                        {
                            var level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                            var scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);

                            return (int)Math.Floor(level * 100D / scale);
                        }
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Ensure you have android.permission.BATTERY_STATS");
                    return -1;
                }

            }
        }

    }
}