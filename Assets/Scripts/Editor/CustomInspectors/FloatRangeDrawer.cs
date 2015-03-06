using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer( typeof( FloatRangeAttribute ) )]
public class FloatRangeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return base.GetPropertyHeight( property, label ) + 16;
    }

    // Draw the property inside the given rect
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        // Now draw the property as a Slider or an IntSlider based on whether it’s a float or integer.
        if (property.type != typeof(FloatRange).ToString())
            Debug.LogWarning("Use only with IntRange type");
        else
        {
            FloatRangeAttribute range = attribute as FloatRangeAttribute;
            SerializedProperty minValue = property.FindPropertyRelative("RangeStart");
            SerializedProperty maxValue = property.FindPropertyRelative("RangeEnd");
            float newMin = minValue.floatValue;
            float newMax = maxValue.floatValue;

            float xDivision = position.width * 0.4f;
            float xLabelDiv = xDivision * 0.125f;

            float yDivision = position.height * 0.5f;
            EditorGUI.LabelField(new Rect(position.x, position.y, xDivision, yDivision)
            , label);


            Rect mmRect = new Rect(position.x + xDivision + xLabelDiv, position.y, position.width - (xDivision + xLabelDiv * 2), yDivision);

            EditorGUI.MinMaxSlider(mmRect, ref newMin, ref newMax, range.MinLimit, range.MaxLimit);


            Rect minRangeRect = new Rect(position.x + xDivision, position.y, xLabelDiv, yDivision);
            minRangeRect.x += xLabelDiv * 0.5f - 12;
            minRangeRect.width = 24;
            EditorGUI.LabelField(minRangeRect, range.MinLimit.ToString());

            Rect maxRangeRect = new Rect(minRangeRect);
            maxRangeRect.x = mmRect.xMax + xLabelDiv * 0.5f - 12;
            maxRangeRect.width = 24;
            EditorGUI.LabelField(maxRangeRect, range.MaxLimit.ToString());

            Rect minLabelRect = new Rect(mmRect);
            minLabelRect.x = minLabelRect.x + minLabelRect.width * (newMin / range.MaxLimit);
            minLabelRect.x -= 12;
            minLabelRect.y += yDivision;
            minLabelRect.width = 24;
            newMin = Mathf.Clamp(EditorGUI.FloatField(minLabelRect, newMin), range.MinLimit, newMax);
            //EditorGUI.LabelField(minLabelRect, newMin.ToString());

            Rect maxLabelRect = new Rect(mmRect);
            maxLabelRect.x = maxLabelRect.x + maxLabelRect.width * (newMax / range.MaxLimit);
            maxLabelRect.x -= 12;
            maxLabelRect.x = Mathf.Max(maxLabelRect.x, minLabelRect.xMax);
            maxLabelRect.y += yDivision;
            maxLabelRect.width = 24;
            newMax = Mathf.Clamp(EditorGUI.FloatField(maxLabelRect, newMax), newMin, range.MaxLimit);
            //EditorGUI.LabelField(maxLabelRect, newMax.ToString());


            minValue.floatValue = newMin;
            maxValue.floatValue = newMax;
        }
    }
}