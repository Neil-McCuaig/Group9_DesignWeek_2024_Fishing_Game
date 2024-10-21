using FishingGameTool2D.CustomAttribute;
using UnityEditor;
using UnityEngine;

namespace FishingGameTool2D.CustomDrawer
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDrawer : PropertyDrawer
    {
        private const float _closeButtonSize = 22f;
        private const float _closeButtonPadding = 2f;
        private Color _closeButtonColor = Color.grey;
        private const float _helpButtonSize = 20f;
        private const float _helpButtonPadding = 2f;

        private string _playerPrefsKey;
        private bool _infoBoxVisible;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InfoBoxAttribute infoAttribute = attribute as InfoBoxAttribute;
            _playerPrefsKey = property.propertyPath + "_InfoBoxVisible";

            // Load visibility state from PlayerPrefs
            _infoBoxVisible = PlayerPrefs.GetInt(_playerPrefsKey, 1) == 1;

            float infoHeight = _infoBoxVisible ? GetInfoHeight(infoAttribute) : 0f;

            Rect propertyPosition = new Rect(position.x, position.y + infoHeight, position.width - (!_infoBoxVisible ? _helpButtonSize - _helpButtonPadding : 0f), EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(propertyPosition, property, label);

            if (!string.IsNullOrEmpty(infoAttribute?._infoText))
            {
                Rect helpButtonRect = new Rect(propertyPosition.xMax + _helpButtonPadding, position.y + (EditorGUIUtility.singleLineHeight - _helpButtonSize) * 0.5f, _helpButtonSize, _helpButtonSize);

                if (!_infoBoxVisible && GUI.Button(helpButtonRect, EditorGUIUtility.IconContent("_Help"), GUIStyle.none))
                    _infoBoxVisible = true;

                if (_infoBoxVisible)
                {
                    float infoWidth = position.width - _closeButtonSize;
                    Rect infoRect = new Rect(position.x, position.y, infoWidth, infoHeight - EditorGUIUtility.standardVerticalSpacing);

                    GUIStyle infoStyle = new GUIStyle(EditorStyles.helpBox);
                    EditorStyles.helpBox.fontSize = infoAttribute._fontSize;
                    EditorGUI.HelpBox(infoRect, infoAttribute._infoText, MessageType.Info);

                    Rect closeButtonRect = new Rect(infoRect.xMax + _closeButtonPadding, infoRect.y + _closeButtonPadding, _closeButtonSize, _closeButtonSize);
                    GUIStyle closeButtonStyle = new GUIStyle(GUI.skin.button);
                    closeButtonStyle.normal.textColor = _closeButtonColor;
                    Color bgColor = GUI.backgroundColor;
                    GUI.backgroundColor = Color.clear;
                    closeButtonStyle.normal.background = EditorGUIUtility.whiteTexture;

                    if (GUI.Button(closeButtonRect, "X", closeButtonStyle))
                        _infoBoxVisible = false;

                    GUI.backgroundColor = bgColor;
                }
            }

            // Save visibility state to PlayerPrefs
            PlayerPrefs.SetInt(_playerPrefsKey, _infoBoxVisible ? 1 : 0);
            PlayerPrefs.Save();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InfoBoxAttribute infoAttribute = attribute as InfoBoxAttribute;

            float infoHeight = _infoBoxVisible ? GetInfoHeight(infoAttribute) : 0f;
            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);

            return propertyHeight + infoHeight;
        }

        private float GetInfoHeight(InfoBoxAttribute infoAttribute)
        {
            if (string.IsNullOrEmpty(infoAttribute?._infoText))
                return 0f;

            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.fontSize = infoAttribute._fontSize;
            GUIContent content = new GUIContent(infoAttribute._infoText);

            float infoHeight = style.CalcHeight(content, EditorGUIUtility.currentViewWidth);
            float padding = EditorGUIUtility.singleLineHeight * 0.5f;

            return infoHeight + padding;
        }
    }
}
