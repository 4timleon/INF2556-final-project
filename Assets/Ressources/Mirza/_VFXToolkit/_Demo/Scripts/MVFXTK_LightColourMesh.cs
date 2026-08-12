using UnityEngine;

namespace Mirza.VFXToolKit {
    // This script sets the emissive color of the material on the first child object
    // to match the colour of the Light component on the current GameObject.

    [ExecuteAlways]
    public class MVFXTK_LightColourMesh : MonoBehaviour {
        Light lightColor;
        Material material;

        Renderer render;

        public float intensityScale = 1.0f;
        public bool executeInEditMode;

        public enum Target { Parent, Child, Self }

        [Space]

        public Target target = Target.Child;

        [Space]

        public string propertyName = "_EmissionColor";

        // ...

        void Start() {
            lightColor = GetComponent<Light>();

            switch (target) {
                case Target.Parent: {
                        render = transform.parent.GetComponent<Renderer>();
                        break;
                    }
                case Target.Child: {
                        render = transform.GetChild(0).GetComponent<Renderer>();
                        break;
                    }
                case Target.Self: {
                        render = GetComponent<Renderer>();
                        break;
                    }
                default: {
                        throw new System.Exception("Unknown type.");
                    }
            }
        }

        void LateUpdate() {
            if (Application.isPlaying) {
                material = render.material;
            } else {
                if (!executeInEditMode) {
                    return;
                }

                material = render.sharedMaterial;
            }

            if (!material.IsKeywordEnabled("_EMISSION")) {
                material.EnableKeyword("_EMISSION");
            }

            float intensity = lightColor.intensity * intensityScale;
            Color colour = lightColor.color * intensity;

            material.SetColor(propertyName, colour);
        }
    }
}