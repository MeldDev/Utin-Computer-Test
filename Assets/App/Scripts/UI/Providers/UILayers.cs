using System;
using UnityEngine;

namespace UtinComputerTest.UI.Providers
{
    [Serializable]
    public sealed class UILayers
    {
        [SerializeField] private Transform _fullscreenFade;
        [SerializeField] private Transform _normal;
        [SerializeField] private Transform _navbar;
        [SerializeField] private Transform _topPanel;
        [SerializeField] private Transform _topAndNavbar;
        [SerializeField] private Transform _tutorial;

        public Transform FullscreenFade => _fullscreenFade;
        public Transform Normal => _normal;
        public Transform Navbar => _navbar;
        public Transform TopPanel => _topPanel;
        public Transform TopAndNavbar => _topAndNavbar;
        public Transform Tutorial => _tutorial;
    }
}
