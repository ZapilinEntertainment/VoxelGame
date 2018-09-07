using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class StructureColliderLinker : MonoBehaviour {
    
    public Structure linkedStructure { get; private set; }

    public void SetLinkedStructure(Structure s)
    {
        linkedStructure = s;
    }
}
