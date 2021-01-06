using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateAssetMenu(menuName = "SFAS Assets/StoryData")]
public class StoryData : ScriptableObject
{
    [SerializeField] private List<BeatData> _beats;
 
    public BeatData GetBeatById( int id )
    {
        return _beats.Find(b => b.ID == id);
    }

}

