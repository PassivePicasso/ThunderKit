#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#elif UNITY_2018_1_OR_NEWER
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.UIElements
{
    public class StringPopupField : PopupField<string>
    {
#if UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
        public StringPopupField() : base(new List<string>(), string.Empty, null, null)
        {

        }
#endif

        public new class UxmlFactory : UxmlFactory<StringPopupField, UxmlTraits> { }

        public new class UxmlTraits : PopupField<string>.UxmlTraits
        {
        }
    }
}