//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

//#define SHOW_REPORT

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Localization manager is able to parse localization information from text assets.
/// Using it is simple: text = Localization.Localize(key), or just add a UILocalize script to your labels.
/// You can switch the language by using Localization.instance.currentLanguage = "French", for example.
/// This will attempt to load the file called "French.txt" in the Resources folder.
/// The localization file should contain key = value pairs, one pair per line.
/// </summary>

[AddComponentMenu("NGUI/Internal/Localization")]
public class Localization : MonoBehaviour {

    public static Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();

    static Localization mInstance;

    /// <summary>
    /// Whether there is an instance of the localization class present.
    /// </summary>

    static public bool isActive { get { return mInstance != null; } }

    /// <summary>
    /// The instance of the localization class. Will create it if one isn't already around.
    /// </summary>

    static public Localization instance {
        get {
            if (mInstance == null) {
                //PlayerPrefs.SetString("Language", "English");
                //string currentLanguage1 = PlayerPrefs.GetString("Language", "English1");
                //Debug.LogError(currentLanguage1);

                mInstance = Object.FindObjectOfType(typeof(Localization)) as Localization;

                if (mInstance == null) {
                    GameObject go = new GameObject("_Localization");
                    DontDestroyOnLoad(go);
                    mInstance = go.AddComponent<Localization>();

                }
            }
            return mInstance;
        }
    }

    /// <summary>
    /// Language the localization manager will start with.
    /// </summary>

    public string startingLanguage = "English";

    /// <summary>
    /// Available list of languages.
    /// </summary>

    public string[] languages = { "English", "Russian" };

    //Dictionary<string, string> mDictionary = new Dictionary<string, string>();
    string mLanguage;


    /// <summary>
    /// Name of the currently active language.
    /// </summary>

    public string currentLanguage {
        get {
            return mLanguage;
        }
        set {
            if (mLanguage != value) {
                //startingLanguage = value;

                if (!string.IsNullOrEmpty(value)) {
                    Load(value);
                    return;
                }

                // Either the language is null, or it wasn't found
                PlayerPrefs.DeleteKey("Language");
            }
        }
    }

    /// <summary>
    /// Determine the starting language.
    /// </summary>

    void Awake() {
        if (mInstance == null) {
            mInstance = this;
            DontDestroyOnLoad(gameObject);

            currentLanguage = PlayerPrefs.GetString("Language", startingLanguage);

            if (string.IsNullOrEmpty(mLanguage) && (languages != null && languages.Length > 0)) {
                currentLanguage = languages[0];
            }
        } else Destroy(gameObject);
    }

    /// <summary>
    /// Oddly enough... sometimes if there is no OnEnable function in Localization, it can get the Awake call after UILocalize's OnEnable.
    /// </summary>

    void OnEnable() { if (mInstance == null) mInstance = this; }


    /// <summary>
    /// Remove the instance reference.
    /// </summary>

    void OnDestroy() { if (mInstance == this) mInstance = null; }

    /// <summary>
    /// Load the specified asset and activate the localization.
    /// </summary>

    void Load(string lenName) {
        mLanguage = lenName;
        PlayerPrefs.SetString("Language", mLanguage);

        //Set up the fonts
        LoadFont("fontSimple", mLanguage);
        LoadFont("fontOutlined", mLanguage);

        UIRoot.Broadcast("OnLocalize", this);
    }

    private void LoadFont(string fontName, string lang) {
        GameObject refObj = Resources.Load("fonts/" + fontName) as GameObject;
        if (refObj == null) {
            Debug.LogWarning("Failed to find reference font prefab for " + fontName);
            return;
        }
        UIFont refFont = refObj.GetComponent<UIFont>();
        if (refFont == null) {
            Debug.LogWarning("Failed to find reference font for " + fontName);
            return;
        }
        GameObject fontObj = Resources.Load("fonts/" + lang + "/" + fontName + lang, typeof(GameObject)) as GameObject;
        if (fontObj == null) {
            Debug.LogWarning("Failed to find " + lang + " language font prefab for " + fontName);
            return;
        }
        UIFont font = fontObj.GetComponent<UIFont>();
        if (font == null) {
            Debug.LogWarning("Failed to find " + lang + " language font for " + fontName);
            return;
        }
        refFont.replacement = font;
    }

    /// <summary>
    /// Localize the specified value.
    /// </summary>

    public string Get(string key) {
#if UNITY_EDITOR
        if (!Application.isPlaying) return key;
#endif

        /*
        LocalizationData val;

        GameData.Instance.LocalizationData.TryGetValue(key, out val);

        if (val != null) {

            switch (currentLanguage) {
                case "English":
                    return val.Eng;
                case "Russian":
                    return val.Rus;
                default:
                    return key + " not currentLanguage = " + currentLanguage;
            }
        }
        */


        return key;
    }

    /// <summary>
    /// Localize the specified value.
    /// </summary>

    static public string Localize(string key) { return (instance != null) ? instance.Get(key) : key + "_ERROR"; }
}
