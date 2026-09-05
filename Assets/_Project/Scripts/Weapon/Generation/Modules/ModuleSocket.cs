using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponNamespace;

[Serializable]
public class ModuleSocket
{
    public Transform Joint;
    public ModuleType ModuleType;
    public JointSizeFilter AcceptedJointSizes;
}
