using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace ThunderKit.Markdown.Helpers
{
    public static class VisualElementUtility
    {
        public static void SetupImage(Image imageElement, Texture texture)
        {
            imageElement.image = texture;
#if UNITY_2019_1_OR_NEWER
            imageElement.style.width = texture.width;
            imageElement.style.height = texture.height;
#else
            imageElement.style.width = new StyleValue<float>(texture.width);
            imageElement.style.height = new StyleValue<float>(texture.height);
#endif
        }
    }
}