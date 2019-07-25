using UnityEngine;
using HTC.UnityPlugin.Vive;
using System.Collections.Generic;

namespace ZenFulcrum.EmbeddedBrowser
{
    public class OdysseyControllerInput : MonoBehaviour
    {
        public bool Tracked { get; private set; }

        public MouseButton DepressedButtons { get; private set; }

        public Vector2 ScrollDelta { get; private set; }

        public string Name { get; private set; }

        public GameObject browserWindow;

        private LineRenderer visual;
        private void Start()
        {
            visual = GetComponent<LineRenderer>();
        }

        public virtual void Update()
        {
            ReadInput();
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject == browserWindow)
                {
                    visual.enabled = true;
                }
            }
        }

        protected virtual void ReadInput()
        {
            var leftClick = ViveInput.GetAxis(HandRole.RightHand, ControllerAxis.Trigger);
            if (leftClick > .9f) DepressedButtons |= MouseButton.Left;
        }
    }
}


