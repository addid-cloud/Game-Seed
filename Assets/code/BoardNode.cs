using System.Collections.Generic;
using UnityEngine;

public class BoardNode : MonoBehaviour
{
    [Header("Masukkan kotak selanjutnya di sini")]
    // List ini bisa diisi 1 kotak (jalan lurus) atau lebih (jalur bercabang)
    public List<BoardNode> nextNodes = new List<BoardNode>();
}