using System.Collections.Generic;
using UnityEngine;

namespace Items.Nodes
{
    public abstract class MaterialNode<TNode, TMaterial> : MonoBehaviour 
        where TNode : NodeMaterial<TMaterial> 
        where TMaterial : RawMaterial

    {
        public List<TNode> material;
    }
}