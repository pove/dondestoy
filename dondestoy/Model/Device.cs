using Firebase.Database;
using Java.Util;

namespace dondestoy.Model
{
    public class Device
    {
        // Our thing properties
        public string uid { get; set; }
        public int enabled { get; set; }
        public int frecuency { get; set; }
        public int distancemin { get; set; }
        public int accuracytosave { get; set; }
        public int accuracytoshow { get; set; }
        public int working { get; set; }

        public Device()
        {
            // General constructor
        }

        public Device(DataSnapshot snapShot)
        {
            // Get our model from the Firebase snapshot

            if (snapShot.GetValue(true) == null) return; // key, but no value, recently deleted. Return null.            

            uid = snapShot.Key;
            enabled = (int)snapShot.Child("enabled")?.GetValue(true);
            frecuency = (int)snapShot.Child("frecuency")?.GetValue(true);
            distancemin = (int)snapShot.Child("distancemin")?.GetValue(true);
            accuracytosave = (int)snapShot.Child("accuracytosave")?.GetValue(true);
            accuracytoshow = (int)snapShot.Child("accuracytoshow")?.GetValue(true);
            working = (int)snapShot.Child("working")?.GetValue(true);
        }

        public HashMap ModelToMap()
        {
            // Convert our model to Firebase map
            HashMap map = new HashMap();
            map.Put("uid", uid);
            map.Put("enabled", enabled);
            map.Put("frecuency", frecuency);
            map.Put("distancemin", distancemin);
            map.Put("accuracytosave", accuracytosave);
            map.Put("accuracytoshow", accuracytoshow);
            map.Put("working", working);

            return map;
        }
    }
}
 