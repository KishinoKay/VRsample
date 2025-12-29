using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Text;

public class FileController : MonoBehaviour
{
    string fileName;
    StreamWriter sw;
    public string currentInfo;
    int counter = 0;
    [Tooltip("タイマーのテキストオブジェクト")]
    [SerializeField]
    private GameObject timerText;
    private Camera mainCamera;
    string fileName2;
    TextAsset csvFile;
    public List<Vector3> inputPosition = new List<Vector3>();
    string imageNamesFileName = "positions";
    public GameObject bigBullet;


    // Start is called before the first frame update
    void Start()
    {
        //出力したい情報の初期化
        mainCamera = Camera.main;
        System.DateTime presentTime = System.DateTime.Now;
        fileName = Path.Combine(Application.persistentDataPath, $"output_scale_{presentTime:MMdd_HHmm}.csv");//アセットフォルダにcsvファイルを生成
        sw = new StreamWriter(fileName, false, Encoding.UTF8);

        //ファイル入力関係の処理
        ReadPositionsfromFile(imageNamesFileName);
        InstantiateSpherss();
    }

    void ReadPositionsfromFile(string fileName)
    {
        csvFile = Resources.Load(fileName) as TextAsset;
        StringReader sr = new StringReader(csvFile.text);

        string line;
        while ((line = sr.ReadLine()) != null)
        {
            string[] fields = line.Split(',');
            float.TryParse(fields[0], out float x);
            float.TryParse(fields[1], out float y);
            float.TryParse(fields[2], out float z);

            inputPosition.Add(new Vector3(x, y, z));
        }
    }
    void InstantiateSpherss()
    {
        foreach (Vector3 position in inputPosition)
        {
            GameObject bullet = Instantiate(bigBullet);
            bullet.transform.position = position;
            bullet.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            bullet.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        counter++;

        if((counter % 50) == 0)//50フレームに1回出力
        {
            currentInfo = "";
            currentInfo = timerText.GetComponent<TextMeshProUGUI>().text + "," + mainCamera.transform.position;
            sw.WriteLine(currentInfo);
            sw.Flush();
        }
    }
    private void OnApplicationQuit()
    {
        if(sw != null)
        {
            sw.Close();
        }
    }
}
