using System;
using UnityEngine;

namespace FishingGameTool2D.CustomAttribute
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string _infoText;
        public int _fontSize;

        public InfoBoxAttribute(string infoText, int fontSize = 12)
        {
            _infoText = infoText;
            _fontSize = fontSize;
        }
    }
}
