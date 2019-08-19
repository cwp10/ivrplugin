using UnityEditor;

public class IVRPluginPackager
{
    [MenuItem("Window/Packager/Create IVRCamera Plugin")]
    public static void Export()
    {
        AssetDatabase.ExportPackage(
            new string[] {"Assets/IVRCamera3D"},
            "IVRPlugin.unitypackage",
            ExportPackageOptions.Recurse
        );
    }
}
