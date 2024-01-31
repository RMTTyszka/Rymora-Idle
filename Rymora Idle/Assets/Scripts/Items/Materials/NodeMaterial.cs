using UnityEngine;

namespace Items
{
    public class NodeMaterial<T> : MonoBehaviour where T : RawMaterial 
    {
        [SerializeField]
        public T material;
        [SerializeField]
        public int proportion;
    }
}