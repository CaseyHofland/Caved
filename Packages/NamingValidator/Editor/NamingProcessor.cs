#nullable enable
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityExtras.Naming.Editor
{
    internal class NamingProcessor : AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            NamingValidator.ValidateProject();
            return paths;
        }
    }
}
