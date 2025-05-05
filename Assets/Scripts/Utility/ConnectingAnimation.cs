using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kanbarudesu.Utility
{
    [RequireComponent(typeof(Image))]
    public class ConnectingAnimation : MonoBehaviour
    {
        [SerializeField]
        private float rotationSpeed;

        [SerializeField]
        private CanvasGroup canvasGroup;

        void Update()
        {
            if (canvasGroup.alpha >= 0.9)
                transform.Rotate(new Vector3(0, 0, rotationSpeed * Mathf.PI * Time.deltaTime));
        }
    }
}
