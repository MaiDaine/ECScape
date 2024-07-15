using Unity.Mathematics;
using UnityEngine;

namespace ECScape
{
    [CreateAssetMenu(fileName = "HelperTextsSO", menuName = "Scriptable Objects/HelperTextsSO")]
    public class HelperTextsSO : ScriptableObject
    {
        public string[] HelperTexts;
    }
}
