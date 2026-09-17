using System;
using System.Collections;
using UnityEngine;

public abstract class ReloadBase : ScriptableObject
{
    public abstract IEnumerator StartReload(ReloadController controller, WeaponController weapon, Action<int> onReloadSound);

    public abstract void CancelReload(ReloadController controller, WeaponController weapon);
}
