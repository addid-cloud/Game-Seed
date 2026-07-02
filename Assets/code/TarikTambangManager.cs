using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TarikTambangManager : MonoBehaviour
{
    [Header("Objek")]
    public Transform tali;
    public Animator animPemainKiri;
    public Animator animPemainKanan;

    [Header("Gameplay")]
    public float kekuatanTarik = 0.5f;
    public float batasMenang = 5f;

    [Header("Scene")]
    public string namaSceneUtama = "MapUtama";
    public float jedaPindahScene = 2f;

    [Header("Arah Tarik")]
    public Vector3 arahKiri = Vector3.back;
    public Vector3 arahKanan = Vector3.forward;

    [Header("Mash Detection")]
    public float hardPullInterval = 0.25f;

    private Vector3 posisiAwalTali;
    private bool gameSelesai = false;

    private float lastLeftPress;
    private float lastRightPress;

    // Parameter Animator
    private const string START_PULL = "StartPull";
    private const string HARD_PULL = "HardPull";
    private const string LOSE = "Lose";

    void Start()
    {
        posisiAwalTali = tali.position;

        // Masuk ke animasi tarik (loop)
        animPemainKiri.SetTrigger(START_PULL);
        animPemainKanan.SetTrigger(START_PULL);
    }

    void Update()
    {
        if (gameSelesai) return;

        //------------------------
        // Pemain Kiri (A)
        //------------------------
        if (Input.GetKeyDown(KeyCode.A))
        {
            tali.position += arahKiri * kekuatanTarik;

            float interval = Time.time - lastLeftPress;

            if (interval < hardPullInterval)
            {
                animPemainKiri.SetTrigger(HARD_PULL);
            }

            lastLeftPress = Time.time;

            CekMenang();
        }

        //------------------------
        // Pemain Kanan (L)
        //------------------------
        if (Input.GetKeyDown(KeyCode.L))
        {
            tali.position += arahKanan * kekuatanTarik;

            float interval = Time.time - lastRightPress;

            if (interval < hardPullInterval)
            {
                animPemainKanan.SetTrigger(HARD_PULL);
            }

            lastRightPress = Time.time;

            CekMenang();
        }
    }

    void CekMenang()
    {
        Vector3 offset = tali.position - posisiAwalTali;

        float arah =
            Vector3.Dot(offset.normalized, arahKiri.normalized);

        if (offset.magnitude < batasMenang)
            return;

        gameSelesai = true;

        if (arah > 0)
        {
            Debug.Log("PEMAIN KIRI MENANG");

            animPemainKanan.SetTrigger(LOSE);
        }
        else
        {
            Debug.Log("PEMAIN KANAN MENANG");

            animPemainKiri.SetTrigger(LOSE);
        }

        StartCoroutine(KembaliKeSceneUtama());
    }

    IEnumerator KembaliKeSceneUtama()
    {
        yield return new WaitForSeconds(jedaPindahScene);

        SceneManager.LoadScene(namaSceneUtama);
    }
}