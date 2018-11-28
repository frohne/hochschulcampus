using UnityEngine;
using Vuforia;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System.IO;
using System;
using UnityEditor;
using UnityEngine.UI;

public class Sprachmemo : MonoBehaviour
{
    public AudioSource soundTarget;
    public AudioClip clipTarget;
    public GameObject scrollItemLayer;
    public Transform scrollContent;

    public int MemoLength = 60;
    public string MemoName = "Memo";
    public int MemoNumber = 1;
    public bool saving = false;
    public bool useMicrophone = true;
    public AudioClip audioClip;
    public AudioSource newAudioSource;
    public string selectedDevice;
    const int HEADER_SIZE = 44;

    // Use this for initialization
    void Start()
    {
        soundTarget = (AudioSource)gameObject.AddComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void playSound(string filename)
    {
        Debug.Log("Play Audio " + filename);
        //Android
        
        var filepath = Application.persistentDataPath;
        if (!System.IO.File.Exists(filepath + "/" + filename + ".wav"))
        {
            Debug.Log("Data Path existiert nicht!!!");
        }
        else
        {
            Debug.Log("Data Path existiert");
        }

        WWW www = new WWW("file://" + filepath + "/" + filename + ".wav");
        clipTarget = www.GetAudioClip(true, true);
        
        //PC
        /*
        AssetDatabase.Refresh();
        clipTarget = (AudioClip)Resources.Load("Memos/" + filename);*/

        soundTarget.clip = clipTarget;
        soundTarget.loop = false;
        soundTarget.playOnAwake = false;
        soundTarget.Play();

    }

    public void deleteAudio(string filename)
    {
        //Android
        var filepath = Application.persistentDataPath;

        //PC
        /*var filepath = Path.Combine(Application.dataPath, "Resources");
        filepath = Path.Combine(filepath, "Memos");*/



        filepath = Path.Combine(filepath, filename + ".wav");

        Debug.Log("Delete Path " + filepath);

        if (File.Exists(filepath))
        {
            File.Delete(filepath);
            Debug.Log("Delete " + filename);
        }
        else
        {
            Debug.Log("File konnte nicht gelöscht werden.");
        }
    }

    public void deleteAll()
    {
        for (int i = 1; i < MemoNumber; i++)
        {
            deleteAudio(MemoName + i);
        }
        MemoNumber = 1;

        foreach (Transform child in scrollContent)
        {
            if (child.name.StartsWith("Abspielen"))
            {
                GameObject.Destroy(child.gameObject);
            }
        }

    }

    public void createAudio()
    {
        //if (saving)
        //{
        //    saveAudio();
        //}
        //else
        //{
        if (useMicrophone)
        {
            if (Microphone.devices.Length > 0) // Wenn min. 1 Mikrofon vorhanden ist
            {
                Debug.Log("Recording...");
                selectedDevice = Microphone.devices[0].ToString();  // Erstes Mikrofon in der Liste wird genutzt

                newAudioSource = GetComponent<AudioSource>();
                newAudioSource.clip = Microphone.Start(selectedDevice, false, MemoLength, AudioSettings.outputSampleRate);
                //Ausgewähltes Mikrofon, loop, Länge der Aufname in Sekunden, Frequenz
                //newAudioSource.Play();

                saving = true;
            }
            else
            {
                useMicrophone = false;
            }
        }
        else
        {
            //playSound("sound/AudioTest1");
        }
        //}
    }

    public void saveAudio()
    {
        if (saving)
        {
            Debug.Log("Save Audio");
            selectedDevice = Microphone.devices[0].ToString();
            Microphone.End(selectedDevice);

            audioClip = newAudioSource.clip;

            string filename;

            //Android
            var path = Application.persistentDataPath; //Speichern unter: Android\data\com.Company.HSHL2030\files

            //PC
            /*var path = Path.Combine(Application.dataPath, "Resources");
            path = Path.Combine(path, "Memos"); //Speichern unter: Assets\Resources\Memos*/


            var filepath = "";

            do
            {
                filename = MemoName + MemoNumber + ".wav";
                filepath = Path.Combine(path, filename);

                MemoNumber++;

                Debug.Log(filepath + " bereits vorhanden?");
            } while (File.Exists(filepath));

            generateLayerItem(MemoNumber - 1);

            //MemoNumber = 1;


            Debug.Log("Datei wird gespeichert unter : " + filepath);

            // Make sure directory exists if user is saving to sub dir.
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            using (var fileStream = CreateEmpty(filepath))
            {
                ConvertAndWrite(fileStream, audioClip);
                WriteHeader(fileStream, audioClip);
            }

            saving = false;
            Debug.Log("Save end");
        }
    }


    #region Save Audio https://gist.github.com/darktable/2317063 
    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        var samples = new float[clip.samples];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {

        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 two = 2;
        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);

        //		fileStream.Close();
    }
    #endregion


    void generateLayerItem(int i)
    {
        GameObject scrollItemLayerObj = Instantiate(scrollItemLayer);
        scrollItemLayerObj.transform.position = new Vector3(50.0f, -25.0f + ((i - 1) * 188.38f), 0.0f);
        scrollItemLayerObj.transform.SetParent(scrollContent.transform, false);
        scrollItemLayerObj.GetComponent<Button>().onClick.AddListener(() => playSound(MemoName + i));
        Debug.Log(i);
        //scrollItemLayerObj.transform.position = new Vector3(-175.0f, -25.0f + (i * 150), 0.0f);
        //scrollItemLayerObj.GetComponentInChildren<TextMeshProUGUI>().text = layerObject.GetComponent<Transform>().GetChild(i).name;
        //GameObject go = layerObject.GetComponent<Transform>().GetChild(i).gameObject;

        //Toggle toggle = scrollItemLayerObj.GetComponent<Toggle>();
        //toggle.onValueChanged.AddListener(delegate { ToggleValueChanged(go); });
    }
}
