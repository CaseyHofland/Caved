using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityExtras.Naming.Editor
{
    [CreateAssetMenu(menuName = "Naming Ruleset")]
    public class NamingRuleset : ScriptableObject
    {
        public static NamingRuleset emptyRuleset => AssetDatabase.LoadAssetAtPath<NamingRuleset>("Packages/com.unityextras.namingvalidator/Editor/EmptyRuleset.asset");

        public List<NamingRule> namingRules;
    }
}
