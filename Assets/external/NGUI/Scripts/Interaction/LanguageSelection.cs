//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright � 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Turns the popup list it's attached to into a language selection list.
/// </summary>

[RequireComponent(typeof(UIPopupList))]
[AddComponentMenu("NGUI/Interaction/Language Selection")]
public class LanguageSelection : MonoBehaviour {
    UIPopupList mList;

    void Start() {
        mList = GetComponent<UIPopupList>();
        UpdateList();
        //mList.eventReceiver = gameObject;
        //smList.functionName = "OnLanguageSelection";
    }

    void UpdateList() {
        if (Localization.instance != null && Localization.instance.languages != null) {
            mList.items.Clear();

            for (int i = 0, imax = Localization.instance.languages.Length; i < imax; ++i) {
                string lenName = Localization.instance.languages[i];
                if (lenName != null) mList.items.Add(lenName);
            }
            mList.value = Localization.instance.currentLanguage;
        }
    }

    void OnLanguageSelection(string language) {
        if (Localization.instance != null) {
            Localization.instance.currentLanguage = language;
        }
    }
}