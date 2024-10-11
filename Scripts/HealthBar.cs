using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This could be a current over max bar or something. (Fill bar, it's a fill bar.)
/// </summary>
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillbarContainingImage = null;

    [SerializeField] private bool useColorChanges = false;

    private void Awake()
    {
        if (fillbarContainingImage == null)
            Debug.LogWarning($"There's no fill bar image on {this}, please rectify the problem", this); //the second 'this' will show which object is throwing the warning when clicked in the console.
    }

    internal void UpdateFill(float fill)
    {
        fillbarContainingImage.fillAmount = fill;

        if (useColorChanges)
            fillbarContainingImage.color = new Color(fill >.6f? 0f : 1f, fill, 0); //Should be green when full and red when not
    }
}
