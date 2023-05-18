#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExtras.Naming.Editor
{
    [Serializable]
    public struct NamingRule : ISerializationCallbackReceiver
    {
        [SerializeField][Delayed] private string _assetType;
        public string pattern;

        public Type? assetType;

        public NamingRule(Type? assetType, string pattern)
        {
            _assetType = (this.assetType = assetType)?.FullName ?? string.Empty;
            this.pattern = pattern;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _assetType = assetType?.FullName ?? string.Empty;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(_assetType))
            {
                assetType = null;
                return;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if ((assetType = assembly.GetType(_assetType, false)) != null)
                {
                    return;
                }
            }
        }

        public static implicit operator KeyValuePair<Type?, string>(NamingRule namingRule) => new(namingRule.assetType, namingRule.pattern);
        public static implicit operator NamingRule(KeyValuePair<Type?, string> keyValuePair) => new(keyValuePair.Key, keyValuePair.Value);
    }
}
