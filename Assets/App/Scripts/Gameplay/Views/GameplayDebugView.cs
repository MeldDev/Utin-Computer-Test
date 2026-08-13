using UnityEngine;
using UnityEngine.UI;

namespace UtinComputerTest.Gameplay.Views
{
    public sealed class GameplayDebugView : MonoBehaviour
    {
        [SerializeField] private Text _debugText;

        public void SetText(string value)
        {
            _debugText.text = value;
        }
    }
}
