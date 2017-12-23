using Java.Util;

namespace dondestoy.Model
{
    public class GPS
    {
        // Our thing properties
        public string uid { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string timestamp { get; set; }
        public double heading { get; set; }
        public double speed { get; set; }
        public double accuracy { get; set; }
        public double altitude { get; set; }
        public double altaccuracy { get; set; }
        public int battery { get; set; }

        public GPS()
        {
            // General constructor
        }

        /*public GPS(DataSnapshot snapShot)
        {
            // Get our model from the Firebase snapshot

            if (snapShot.GetValue(true) == null) return; // key, but no value, recently deleted. Return null.            

            uid = snapShot.Key;
            latitude = snapShot.Child("latitude")?.GetValue(true)?.ToString();
            longitude = snapShot.Child("longitude")?.GetValue(true)?.ToString();
            timestamp = snapShot.Child("timestamp")?.GetValue(true)?.ToString();
            heading = snapShot.Child("heading")?.GetValue(true)?.ToString();
            speed = snapShot.Child("speed")?.GetValue(true)?.ToString();
            accuracy = snapShot.Child("accuracy")?.GetValue(true)?.ToString();
            altitude = snapShot.Child("altitude")?.GetValue(true)?.ToString();
            altaccuracy = snapShot.Child("altaccuracy")?.GetValue(true)?.ToString();
        }*/

        public HashMap ModelToMap()
        {
            // Convert our model to Firebase map
            HashMap map = new HashMap();
            //map.Put("uid", uid); commented because our id is the key of the child, which is given by Firebase
            map.Put("latitude", latitude);
            map.Put("longitude", longitude);
            map.Put("timestamp", timestamp);
            map.Put("heading", heading);
            map.Put("speed", speed);
            map.Put("accuracy", accuracy);
            map.Put("altitude", altitude);
            map.Put("altaccuracy", altaccuracy);
            map.Put("battery", battery);

            return map;
        }
    }
}
 