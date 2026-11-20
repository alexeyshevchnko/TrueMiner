using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiLbl : MonoBehaviour, IBasePoolObject {
    private static Dictionary<string, GuiLbl> CurrentVisible = new Dictionary<string, GuiLbl>();

    [IoC.Inject]
    public IGuiPoolManager GuiPoolManager { set; protected get; }

    public TextMesh label;

    public int Count;
    public string Name;

    void Awake() {
        if (label == null) {
            label = GetComponent<TextMesh>();
            if (label == null) {
                label = gameObject.AddComponent<TextMesh>();
            }
            var mr = GetComponent<MeshRenderer>();
            if (mr == null) {
                mr = gameObject.AddComponent<MeshRenderer>();
            }
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.font = font;
            label.anchor = TextAnchor.MiddleCenter;
            label.characterSize = 0.13f;
            label.fontSize = 26;
            if (font != null) {
                mr.material = font.material;
            }
        }
    }

    public void Init(Color color, string text) {
        if (label == null) Awake();
        label.color = color;
        label.text = text;
        StartCoroutine(Life(1f));
    }

    public void Init(string name, int count) {
        if (label == null) Awake();
        Count = count;
        Name = name;
        if (CurrentVisible.ContainsKey(name)) {
            Count += CurrentVisible[name].Count;
            CurrentVisible[name].ForceUnSpawn();
        }

        foreach (var lbl in CurrentVisible) {
            if (lbl.Key != name && lbl.Value.transform.position.y - transform.position.y < 25f) {
                lbl.Value.transform.position += Vector3.up * 25f;
            }
        }

        CurrentVisible[name] = this;
        label.color = Color.white;
        label.text = string.Format("{0} :({1})", name, Count);
        StartCoroutine(Life(2f));
    }

    IEnumerator Life(float duration) {
        Vector3 start = transform.position;
        Color c = label.color;
        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            transform.position = start + Vector3.up * (25f * t / duration);
            c.a = Mathf.Clamp01(1f - t / duration);
            label.color = c;
            yield return null;
        }
        UnSpawn();
    }

    public void OnSpawn() {
        this.Inject();
    }

    public void ForceUnSpawn() {
        StopAllCoroutines();
        UnSpawn();
    }

    protected virtual void UnSpawn() {
        StopAllCoroutines();
        if (CurrentVisible.ContainsKey(Name)) {
            CurrentVisible.Remove(Name);
        }
        GuiPoolManager.Unspawn(gameObject);
    }
}