using System.Collections.Generic;
using UnityEngine;

public class BoardNode : MonoBehaviour
{
    public enum SpaceType 
    { 
        MemorySpace, StaticSpace, LossSpace, FriendshipSpace, 
        TapeSpace, NostalgiaSpace, GlitchSpace, DreamTrap 
    }

    [Header("Pengaturan Petak")]
    public SpaceType tipePetak = SpaceType.StaticSpace;
    public int nodeIndex;

    [Header("Jalur Selanjutnya")]
    public List<BoardNode> nextNodes = new List<BoardNode>();

    // Fungsi untuk memvisualisasikan jalur di Editor (Sangat membantu debugging)
    private void OnDrawGizmos()
    {
        if (nextNodes == null) return;

        foreach (BoardNode node in nextNodes)
        {
            if (node != null)
            {
                Gizmos.color = Color.green;
                // Menggambar garis dari petak ini ke petak berikutnya
                Gizmos.DrawLine(transform.position, node.transform.position);
                
                // Menggambar bola kecil di ujung garis sebagai panah petunjuk arah
                Gizmos.DrawSphere(node.transform.position, 0.2f);
            }
        }
    }
}