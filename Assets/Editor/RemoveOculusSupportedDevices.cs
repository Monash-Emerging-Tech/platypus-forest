using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor.Android;

// Fixes the Android build error "Manifest merger failed : Attribute meta-data#com.oculus.supportedDevices@value".
// The Oculus XR Plugin (pulled in by the VR feature set) and OpenXR's Meta Quest feature both write the list of
// supported headsets, with different values. This runs after both and removes the Oculus plugin's shorter list,
// so the build keeps OpenXR's full one (Quest, Quest 2, Quest Pro, Quest 3, Quest 3S).
public class RemoveOculusSupportedDevices : IPostGenerateGradleAndroidProject
{
    const string AndroidNamespace = "http://schemas.android.com/apk/res/android";

    // The Oculus XR Plugin's hook runs at 10000; go after it.
    public int callbackOrder => 20000;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
        if (!File.Exists(manifestPath)) return;

        var manifest = new XmlDocument();
        manifest.Load(manifestPath);

        XmlNode application = manifest.SelectSingleNode("/manifest/application");
        if (application == null) return;

        bool changed = false;
        // Copy the list first so removing items doesn't upset the loop.
        foreach (XmlElement metaData in application.SelectNodes("meta-data").Cast<XmlElement>().ToList())
        {
            if (metaData.GetAttribute("name", AndroidNamespace) != "com.oculus.supportedDevices") continue;

            application.RemoveChild(metaData);
            changed = true;
        }

        if (changed)
            manifest.Save(manifestPath);
    }
}
