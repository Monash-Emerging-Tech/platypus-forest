using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // Source - https://stackoverflow.com/a/27912788
    // Posted by Varaquilex, modified by community. See post 'Timeline' for change history

    private static MusicManager _instance;

    public static MusicManager instance { get; private set; }

    private void Awake()    { instance = this; }
    private void OnDestroy() { if (instance == this) instance = null; }

    public void Play() { }




}
