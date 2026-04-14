using System.Collections.Generic;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#elif UNITY_2018_1_OR_NEWER
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.UIElements
{
#if UNITY_6000_3_OR_NEWER
    [UxmlElement]
    public partial class StringPopupField : PopupField<string>
#else
    public class StringPopupField : PopupField<string>
#endif
    {
#if UNITY_6000_3_OR_NEWER
        public StringPopupField() : base(new List<string>(), string.Empty, null, null)
        {

        }
#elif UNITY_2019_1_OR_NEWER
#elif UNITY_2018_1_OR_NEWER
        public StringPopupField() : base(new List<string>(), string.Empty, null, null)
        {

        }
#endif

#if !UNITY_6000_3_OR_NEWER
        public new class UxmlFactory : UxmlFactory<StringPopupField, UxmlTraits> { }

        public new class UxmlTraits : PopupField<string>.UxmlTraits
        {
        }
#endif
    }
}