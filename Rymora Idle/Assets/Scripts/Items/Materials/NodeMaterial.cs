using UnityEngine;

namespace Items
{
    public class NodeMaterial<T> : MonoBehaviour where T : Material 
    {
        [SerializeField]
        public T material;
        [SerializeField]
        public int proportion;
    }
}