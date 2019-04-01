using System.Collections;
using System.Collections.Generic;
using trasharia.weapons;
using UnityEngine;

public class GuiLbl : MonoBehaviour, IBasePoolObject {
    private static Dictionary<string, GuiLbl> CurrentVisible = new Dictionary<string, GuiLbl>();
    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }
    [IoC.Inject]
    public IGuiPoolManager GuiPoolManager { set; protected get; }

    public UILabel label;

    public TweenAlpha tweenAlpha;
    public TweenScale tweenScale;

    private ICooldownItem cooldown;

    public int Count;
    public string Name;

    public void Init(Color color, string text) {
        label.color = color;
        label.effectColor = Color.black;
        label.text = text;

        tweenAlpha.duration = 1;
        tweenScale.duration = 1;

        tweenScale.ResetToBeginning();
        tweenAlpha.ResetToBeginning();
        tweenScale.Play();
        tweenAlpha.Play();

        cooldown = CooldownManager.AddCooldown(tweenAlpha.duration, null, UnSpawn, 0, Time.fixedDeltaTime);
    }

    public void Init(string name, int count) {
        Count = count;
        Name = name;
        if (CurrentVisible.ContainsKey(name)) {
            Count += CurrentVisible[name].Count;
            CurrentVisible[name].ForceUnSpawn();
        }

        foreach (var lbl in CurrentVisible) {
            if (lbl.Key != name) {
                if (lbl.Value.transform.position.y - transform.position.y < 25) {
                    lbl.Value.transform.position = lbl.Value.transform.position + Vector3.up*25;
                }
            }
        }

        CurrentVisible[name] = this;

        label.color = Color.white;
        label.effectColor = Color.black;
        label.text = string.Format("{0} :({1})", name, Count);

        tweenAlpha.duration = 2;
        tweenScale.duration = 2;

        tweenScale.ResetToBeginning();
        tweenAlpha.ResetToBeginning();
        tweenScale.Play();
        tweenAlpha.Play();

        cooldown = CooldownManager.AddCooldown(tweenAlpha.duration, null, UnSpawn, 0, Time.fixedDeltaTime);
    }

    public void OnSpawn() {
        this.Inject();
        //cooldown = CooldownManager.AddCooldown(tweenAlpha.duration, null, UnSpawn, 0, Time.fixedDeltaTime);
    }

    public void ForceUnSpawn() {
        CooldownManager.RemoveCooldown(cooldown);
        UnSpawn();
    }

    protected virtual void UnSpawn() {
        GuiPoolManager.Unspawn(gameObject);

        if (CurrentVisible.ContainsKey(Name)) {
            CurrentVisible.Remove(Name);
        }
    }

}
