using System.Collections.Generic;
using UnityEngine;

public class BoardNode : MonoBehaviour
{
    // 1. Membuat daftar pilihan tipe petak sesuai konsep desain
  public enum SpaceType 
    { 
        MemorySpace,      // Biru
        StaticSpace,      // Abu-abu
        LossSpace,        // Merah
        FriendshipSpace,  // Berbagi daun
        TapeSpace,        // Kuning
        NostalgiaSpace,   // Hijau
        GlitchSpace,      // Ungu
        DreamTrap         // Hitam
    }

    [Header("Pengaturan Petak")]
    // 2. Variabel untuk menentukan tipe petak ini di Inspector
    public SpaceType tipePetak = SpaceType.StaticSpace;

    [Header("Jalur Selanjutnya")]
    public List<BoardNode> nextNodes = new List<BoardNode>();
}