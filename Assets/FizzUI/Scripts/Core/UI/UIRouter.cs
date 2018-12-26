//
//  UIRouter.cs
//
//  Copyright (c) 2016 Fizz Inc
//
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fizz.UI.Core {
    public class UIRouter : IUIRouter {

        public override void Show (IUITransitable panel, RouterHistoryMode mode, UITransitionConfig config) {
            transitionQueue.Enqueue (new RouterItem { panel = panel, mode = mode, config = config, toShow = true });
            CheckQueue ();
        }

        public override void Hide (IUITransitable panel, UITransitionConfig config) {
            transitionQueue.Enqueue (new RouterItem { panel = panel, config = config, toShow = false });
            CheckQueue ();
        }

        private void Show (RouterItem routerItem) {
            if (routerItem.panel == null) {
                Debug.LogError ("Can not transition to null view.");
                return;
            }

            if (topPanel != null && topPanel.Equals (routerItem.panel) && routerItem.mode == RouterHistoryMode.pushState) {
                RefreshCurrentView ();
                CheckQueue ();
                return;
            }

            if (IsPanelInStack (routerItem.panel)) {
                Debug.LogError ("Component already in stack, can not spawn copies of component.");
                return;
            }

            if (routerItem.config == null) {
                Debug.LogError ("Transition config must be defined to display component.");
                return;
            }

            IUITransitable currentPanel = topPanel;
            UITransition transition = Registry.transitionRegistry[routerItem.config.type];
            if (transition == null) {
                Debug.LogError ("No transition with id: " + routerItem.config.type.ToString () + " found.");
                return;
            }

            transitionInProcess = true;
            UITransitionContext context = new UITransitionContext (routerItem.config, currentPanel, routerItem.panel);
            transition.Do (context, (string error, UITransitionContext inContext) => {
                transitionInProcess = false;
                if (error != null) {
                    Debug.LogError (error);
                    return;
                }

                if (inContext.from != null && routerItem.mode == RouterHistoryMode.replaceState) {
                    viewStack.RemoveFirst ();
                }

                viewStack.AddFirst (routerItem.panel);

                routerItem.panel.OnUITransitionComplete ();
                CheckQueue ();
            });
        }

        private void Hide (RouterItem routerItem) {
            if (routerItem.panel == null) {
                Debug.LogError ("Must specify component to be hidden.");
                return;
            }
            if (viewStack.Count <= 0) {
                Debug.LogError ("No components are visible.");
                return;
            }
            if (topPanel != routerItem.panel) {
                Debug.LogError ("Unable to hide component. Only top most compoenent can be hidden.");
                return;
            }
            if (!IsPanelInStack (routerItem.panel)) {
                Debug.LogError ("Can not hide component that is not visible.");
                return;
            }

            HideTopmost (routerItem.config);
        }

        public override void HideTopmost (UITransitionConfig config) {
            if (viewStack.Count <= 0) {
                Debug.LogError ("No Component");
                return;
            }

            if (config == null) {
                Debug.LogError ("config null");
                return;
            }

            IUITransitable currentPanel = topPanel;
            IUITransitable toPanel = viewStack.Count > 1 ? viewStack.Skip (1).First () : null;
            UITransition transition = Registry.transitionRegistry[config.type];
            if (transition == null) {
                Debug.LogError ("No transition found to hide view.");
                return;
            }

            transitionInProcess = true;
            UITransitionContext context = new UITransitionContext (config, currentPanel, toPanel);
            transition.Do (context, (string error, UITransitionContext inContext) => {
                transitionInProcess = false;
                if (error != null) {
                    Debug.LogError (error);
                    return;
                }

                viewStack.RemoveFirst ();
                CheckQueue ();
            });
        }

        public override void HideAll () {
            while (viewStack.Count > 0) {
                IUITransitable top = viewStack.First ();
                viewStack.RemoveFirst ();

                Utils.SetInteractable (top.rect.transform, false);
                top.OnUIDisable ();

                top.rect.gameObject.SetActive (false);
                top.OnUIHide ();
            }
        }

        private bool IsPanelInStack (IUITransitable inPanel) {
            if (viewStack.Contains (inPanel)) {
                if (viewStack.First ().Equals (inPanel)) {
                    return true;
                } else {
                    viewStack.Remove (inPanel);
                    return false;
                }
            } else {
                return false;
            }
        }

        private IUITransitable topPanel {
            get {
                return viewStack.Count > 0 ? viewStack.First () : null;
            }
        }

        private void RefreshCurrentView () {
            IUITransitable top = viewStack.First ();

            top.OnUIRefresh ();
        }

        private void CheckQueue () {
            if (!transitionInProcess) {
                if (transitionQueue.Count > 0) {
                    RouterItem rItem = transitionQueue.Dequeue ();
                    if (rItem.toShow)
                        Show (rItem);
                    else {
                        Hide (rItem);
                    }
                }
            }
        }

        struct RouterItem {
            public IUITransitable panel;
            public RouterHistoryMode mode;
            public UITransitionConfig config;
            public bool toShow;
        }

        private bool transitionInProcess = false;
        private Queue<RouterItem> transitionQueue = new Queue<RouterItem> ();
        private LinkedList<IUITransitable> viewStack = new LinkedList<IUITransitable> ();
    }
}