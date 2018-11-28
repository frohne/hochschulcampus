/*==============================================================================
Copyright (c) 2017 PTC Inc. All Rights Reserved.
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/
//Save Audio https://gist.github.com/darktable/2317063 
using UnityEngine;
using Vuforia;

using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System.IO;
using System;
using UnityEditor;

/// <summary>
/// A custom handler that implements the ITrackableEventHandler interface.
///
/// Changes made to this file could be overwritten when upgrading the Vuforia version.
/// When implementing custom event handler behavior, consider inheriting from this class instead.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class DefaultTrackableEventHandler : MonoBehaviour, ITrackableEventHandler
{
    #region PROTECTED_MEMBER_VARIABLES

    protected TrackableBehaviour mTrackableBehaviour;
    protected TrackableBehaviour.Status m_PreviousStatus;
    protected TrackableBehaviour.Status m_NewStatus;

    public AudioSource soundTarget;
    public AudioClip clipTarget;
    //private AudioSource[] allAudioSources;

    public int MemoLength = 10;
    public string MemoName = "Memo";
    public int MemoNumber = 1;
    public bool saving = false;
    public bool useMicrophone = true;
    public AudioClip audioClip;
    public AudioSource newAudioSource;
    public string selectedDevice;
    const int HEADER_SIZE = 44;

    /*
    void StopAllAudio()
    {
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach (AudioSource audioS in allAudioSources)
        {
            audioS.Stop();
        }
    }
    */


    void playSound(string filename)
    {
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
        clipTarget = (AudioClip)Resources.Load(ss);
        */
        soundTarget.clip = clipTarget;
        soundTarget.loop = false;
        soundTarget.playOnAwake = false;
        soundTarget.Play();

    }

    void deleteAudio(string filename)
    {
        //Android
        var filepath = Application.persistentDataPath;

        //PC
        //var filepath = Path.Combine(Application.dataPath, "Resources");
        //filepath = Path.Combine(filepath, "Memos");
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

    void createAudio()
    {
        if (useMicrophone)
        {
            if (Microphone.devices.Length > 0) // Wenn min. 1 Mikrofon vorhanden ist
            {
                Debug.Log("Recording...");
                selectedDevice = Microphone.devices[0].ToString();  // Erstes Mikrofon in der Liste wird genutzt

                newAudioSource = GetComponent<AudioSource>();
                newAudioSource.clip = Microphone.Start(selectedDevice, false, MemoLength, AudioSettings.outputSampleRate);
                //Ausgewähltes Mikrofon, loop, Länge der Aufname in Sekunden, Frequenz
                newAudioSource.Play();

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
    }



    void saveAudio()
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
            //var path = Path.Combine(Application.dataPath, "Resources");
            //path = Path.Combine(path, "Memos"); //Speichern unter: Assets\Resources\Memos


            var filepath = "";

            do
            {
                filename = MemoName + MemoNumber + ".wav";
                filepath = Path.Combine(path, filename);

                MemoNumber++;

                Debug.Log(filepath + " bereits vorhanden?");
            } while (File.Exists(filepath));

            MemoNumber = 1;


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

    #endregion // PROTECTED_MEMBER_VARIABLES

    #region UNITY_MONOBEHAVIOUR_METHODS

    protected virtual void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
            mTrackableBehaviour.RegisterTrackableEventHandler(this);

        soundTarget = (AudioSource)gameObject.AddComponent<AudioSource>();
    }

    protected virtual void OnDestroy()
    {
        if (mTrackableBehaviour)
            mTrackableBehaviour.UnregisterTrackableEventHandler(this);
    }

    #endregion // UNITY_MONOBEHAVIOUR_METHODS

    #region PUBLIC_METHODS

    /// <summary>
    ///     Implementation of the ITrackableEventHandler function called when the
    ///     tracking state changes.
    /// </summary>
    public void OnTrackableStateChanged(
        TrackableBehaviour.Status previousStatus,
        TrackableBehaviour.Status newStatus)
    {
        m_PreviousStatus = previousStatus;
        m_NewStatus = newStatus;

        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED || newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            /*
            if (mTrackableBehaviour.TrackableName == "Target1")
            {
                saveAudio();
            }
            */
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
            OnTrackingFound();
        }
        else if (previousStatus == TrackableBehaviour.Status.TRACKED &&
                 newStatus == TrackableBehaviour.Status.NO_POSE)
        {
            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");


            if (mTrackableBehaviour.TrackableName == "Target1")
            {
                saveAudio();
            }

            OnTrackingLost();
        }
        else
        {
            // For combo of previousStatus=UNKNOWN + newStatus=UNKNOWN|NOT_FOUND
            // Vuforia is starting, but tracking has not been lost or found yet
            // Call OnTrackingLost() to hide the augmentations
            OnTrackingLost();
        }
    }

    #endregion // PUBLIC_METHODS

    #region PROTECTED_METHODS

    protected virtual void OnTrackingFound()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Enable rendering:
        foreach (var component in rendererComponents)
            component.enabled = true;

        // Enable colliders:
        foreach (var component in colliderComponents)
            component.enabled = true;

        // Enable canvas':
        foreach (var component in canvasComponents)
            component.enabled = true;

        if (mTrackableBehaviour.TrackableName == "Target1")
        {
            createAudio();
        }

        if (mTrackableBehaviour.TrackableName == "Target2")
        {
            Debug.Log("Play " + MemoName + MemoNumber);
            //playSound("Memos/" + MemoName + MemoNumber);//PC
            playSound(MemoName + MemoNumber);//Android
        }

        if (mTrackableBehaviour.TrackableName == "Target3")
        {
            string filename = MemoName + MemoNumber;
            deleteAudio(filename);
        }
    }


    protected virtual void OnTrackingLost()
    {
        var rendererComponents = GetComponentsInChildren<Renderer>(true);
        var colliderComponents = GetComponentsInChildren<Collider>(true);
        var canvasComponents = GetComponentsInChildren<Canvas>(true);

        // Disable rendering:
        foreach (var component in rendererComponents)
            component.enabled = false;

        // Disable colliders:
        foreach (var component in colliderComponents)
            component.enabled = false;

        // Disable canvas':
        foreach (var component in canvasComponents)
            component.enabled = false;

        //StopAllAudio();
    }

    #endregion // PROTECTED_METHODS
}