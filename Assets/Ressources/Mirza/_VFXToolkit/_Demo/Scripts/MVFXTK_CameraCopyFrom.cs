using UnityEngine;

using UnityEngine.Rendering.Universal;

namespace Mirza.VFXToolKit {
    // This script copies *almost* all settings from the target camera.
    // Target texture and culling mask are preserved for specific rendering purposes.

    // It's typically used for that -> same camera view, different render and texture.

    [ExecuteAlways]
    public class MVFXTK_CameraCopyFrom : MonoBehaviour {
        Camera main;
        public Camera target;

        public float priorityOffset = -1;

        [Space]

        public bool copyBackgroundType = true;

        void LateUpdate() {
            if (!main) {
                main = GetComponent<Camera>();
            }

            RenderTexture targetTexture = main.targetTexture;
            int cullingMask = main.cullingMask;

            CameraClearFlags backgroundType = main.clearFlags;
            Color backgroundColour = main.backgroundColor;

            // ...

            main.CopyFrom(target);

            main.depth += priorityOffset;

            main.targetTexture = targetTexture;
            main.cullingMask = cullingMask;

            if (!copyBackgroundType) {
                main.clearFlags = backgroundType;
                main.backgroundColor = backgroundColour;
            }
        }
    }
}