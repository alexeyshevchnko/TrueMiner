using UnityEngine;
using System.Collections;
using trasharia;

public class BasePoolEffect : MonoBehaviour, IBasePoolObject, IoC.IInitialize {
    [IoC.Inject]
    public IGOPoolManager IgoPoolManager { set; protected get; }

    [IoC.Inject]
    public ICooldownManager CooldownManager { set; protected get; }

    [IoC.Inject]
    public IRandom Random { set; protected get; }

    public ParticleSystem MainParticle;

    public virtual void Start() {
        this.Inject();
    }

    public void OnInject() {}

    public virtual void OnSpawn() {
        this.Inject();
        CooldownManager.AddCooldown(MainParticle.duration, null, UnSpawn, 0, Time.fixedDeltaTime);
    }

    public virtual void OnUnSpawn() {
        
    }

    protected virtual void UnSpawn() {
        IgoPoolManager.Unspawn(gameObject);
    }
}
