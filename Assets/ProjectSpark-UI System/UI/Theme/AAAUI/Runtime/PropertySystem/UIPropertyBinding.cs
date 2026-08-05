using System;
using UnityEngine;

namespace AAAUI
{
    [Serializable]
    public sealed class UIPropertyBinding
    {
        [SerializeField]
        private UIPropertyReference reference;

        public UIPropertyReference Reference => reference;

        public bool IsValid =>
            reference != null &&
            reference.IsValid;

        public int PropertyId =>
            reference != null
                ? reference.PropertyId
                : -1;

        public UIPropertyType Type =>
            reference != null
                ? reference.Type
                : UIPropertyType.Float;

        public string Property =>
            reference != null
                ? reference.Property
                : string.Empty;

        public UIPropertyBinding()
        {
            reference = new UIPropertyReference();
        }

        public UIPropertyBinding(
            string property,
            UIPropertyType type)
        {
            reference =
                new UIPropertyReference(
                    property,
                    type
                );
        }

        public void Set(
            string property,
            UIPropertyType type)
        {
            if (reference == null)
                reference = new UIPropertyReference();

            reference.Set(property, type);
        }
    }
}