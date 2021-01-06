using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class StoryEditor : EditorWindow
{
    private enum View { BeatList, Beat, DataList }

    private Vector2 _scroll = new Vector2();
    private int _currentIndex = -1;
    private int _dataIndex = -1;
    private View _view;

    private StoryData data;

    public const string PathToDataAssets = "Assets/Resources/Data/";

    [MenuItem("SFAS/Show Story Editor")]
    public static void ShowStoryEditor()
    {
        GetWindow(typeof(StoryEditor));
    }

#if UNITY_EDITOR

    /// <summary>
    /// Adds newly found assets in folder to list
    /// Returns how many found (not how many added)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="assetsFound">Adds to this list if it is not already there</param>
    /// <returns></returns>
    public static void TryGetUnityObjectsOfTypeFromPath<T>(string path, List<T> assetsFound) where T : UnityEngine.Object
    {
        string[] filePaths = System.IO.Directory.GetFiles(path);

        //Debug.Log(filePaths.Length);

        if (filePaths != null && filePaths.Length > 0)
        {
            for (int i = 0; i < filePaths.Length; i++)
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(filePaths[i], typeof(T));
                if (obj is T asset)
                {
                    if (!assetsFound.Contains(asset))
                    {
                        assetsFound.Add(asset);
                    }
                }
            }
        }
    }

    public const string PathToAsset = "Assets/Resources/Data//Story.asset";

    public static StoryData SetDefaultData()
    {
        StoryData newData = AssetDatabase.LoadAssetAtPath<StoryData>(PathToAsset);
        if (newData == null)
        {
            Debug.Log("data set to default!");
            newData = CreateInstance<StoryData>();
            AssetDatabase.CreateAsset(newData, PathToAsset);
            Debug.Log(newData);
        }

        return newData;
    }
#endif

    void OnGUI()
    {
        //Try set data to default
        if(data == null)
            data = SetDefaultData();

        // if returned data is null, do not continue.
        if (data == null)
        {
            Debug.LogWarning(this + "'s data == " + data);
            return;
        }
        SerializedObject dataObj = new SerializedObject(data);

        SerializedProperty beatList = dataObj.FindProperty("_beats");

        EditorGUILayout.BeginVertical();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (_view == View.Beat && _currentIndex != -1)
        {
            OnGUI_BeatView(beatList, _currentIndex);
        }
        else if(_view == View.DataList)
        {
            //Declare a list to store all found story data
            List<StoryData> allData = new List<StoryData>();

            //All StoryData that can be found in PathToDataAssets
            TryGetUnityObjectsOfTypeFromPath(PathToDataAssets, allData);

            OnGUI_DataView(allData);
        }
        else
        {
            OnGUI_ListView(beatList);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        dataObj.ApplyModifiedProperties();
    }

#if UNITY_EDITOR

#endif

    private void OnGUI_ListView(SerializedProperty beatList)
    {
        EditorGUILayout.BeginVertical();

        if (beatList.arraySize == 0)
        {
            AddBeat(beatList, 1, "First Story Beat");           
        }
        EditorGUILayout.LabelField("Beat List: " + data.name, EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        for (int count = 0; count < beatList.arraySize; ++count)
        {
            SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(count);
            SerializedProperty choiceList = arrayElement.FindPropertyRelative("_choices");
            SerializedProperty text = arrayElement.FindPropertyRelative("_text");
            SerializedProperty id = arrayElement.FindPropertyRelative("_id");



            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(id.intValue.ToString());

            if (GUILayout.Button("Edit"))
            {
                _view = View.Beat;
                _currentIndex = count;
                break;
            }

            if (GUILayout.Button("Delete"))
            {
                beatList.DeleteArrayElementAtIndex(count);
                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(text);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Return to Data List", GUILayout.Height(50)))
        {
            _view = View.DataList;
            _dataIndex = -1;
        }
    }

    private void OnGUI_DataView(List<StoryData> dataList)
    {
        EditorGUILayout.BeginVertical();

        if (dataList.Count == 0)
        {
            AssetDatabase.CreateAsset(CreateInstance<StoryData>(), PathToDataAssets);
        }

        EditorGUILayout.LabelField("Data List", EditorStyles.boldLabel);
        EditorGUILayout.Separator();

        for (int count = 0; count < dataList.Count; ++count)
        {
            string text = dataList[count].name;
            StoryData nextData = dataList[count];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(text.ToString());

            if (GUILayout.Button("Edit Data Set"))
            {
                _dataIndex = count;
                nextData = dataList[count];
                data = nextData;
                _view = View.BeatList;
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void OnGUI_BeatView(SerializedProperty beatList, int index)
    {
        SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(index);
        SerializedProperty choiceList = arrayElement.FindPropertyRelative("_choices");
        SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty id = arrayElement.FindPropertyRelative("_id");

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField(data.name+" Beat ID: " + id.intValue.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        text.stringValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.Height(200));

        OnGUI_BeatViewDecision(choiceList, beatList);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Return to Beat List", GUILayout.Height(50)))
        {
            _view = View.BeatList;
            _currentIndex = -1;
        }
    }

    private void OnGUI_BeatViewDecision(SerializedProperty choiceList, SerializedProperty beatList)
    {
        EditorGUILayout.BeginHorizontal();

        for (int count = 0; count < choiceList.arraySize; ++count)
        {
            OnGUI_BeatViewChoice(choiceList, count, beatList);
        }

        if (GUILayout.Button((choiceList.arraySize == 0 ? "Add Choice" : "Add Another Choice"), GUILayout.Height(100)))
        {
            int newBeatId = FindUniqueId(beatList);
            AddBeat(beatList, newBeatId);
            AddChoice(choiceList, newBeatId);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void OnGUI_BeatViewChoice(SerializedProperty choiceList, int index, SerializedProperty beatList)
    {
        SerializedProperty arrayElement = choiceList.GetArrayElementAtIndex(index);
        SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty beatId = arrayElement.FindPropertyRelative("_beatId");

        EditorGUILayout.BeginVertical();

        text.stringValue = EditorGUILayout.TextArea(text.stringValue, GUILayout.Height(50));
        EditorGUILayout.LabelField("Leads to Beat ID: " + beatId.intValue.ToString());

        if (GUILayout.Button("Go to Beat"))
        {
            _currentIndex = FindIndexOfBeatId(beatList, beatId.intValue);
            GUI.FocusControl(null);
            Repaint();
        }

        EditorGUILayout.EndVertical();
    }

    private int FindUniqueId(SerializedProperty beatList)
    {
        int result = 1;

        while (IsIdInList(beatList, result))
        {
            ++result; 
        }

        return result;
    }

    private bool IsIdInList(SerializedProperty beatList, int beatId)
    {
        bool result = false;

        for (int count = 0; count < beatList.arraySize && !result; ++count)
        {
            SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(count);
            SerializedProperty id = arrayElement.FindPropertyRelative("_id");
            result = id.intValue == beatId;
        }

        return result;
    }

    private int FindIndexOfBeatId(SerializedProperty beatList, int beatId)
    {
        int result = -1;

        for (int count = 0; count < beatList.arraySize; ++count)
        {
            SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(count);
            SerializedProperty id = arrayElement.FindPropertyRelative("_id");
            if (id.intValue == beatId)
            {
                result = count;
                break;
            }
        }

        return result;
    }

    private void AddBeat(SerializedProperty beatList, int beatId, string initialText = "New Story Beat")
    {
        int index = beatList.arraySize;
        beatList.arraySize += 1;
        SerializedProperty arrayElement = beatList.GetArrayElementAtIndex(index);
        SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty id = arrayElement.FindPropertyRelative("_id");

        text.stringValue = initialText;
        id.intValue = beatId;
    }

    private void AddChoice(SerializedProperty choiceList, int beatId, string initialText = "New Beat Choice")
    {
        int index = choiceList.arraySize;
        choiceList.arraySize += 1;
        SerializedProperty arrayElement = choiceList.GetArrayElementAtIndex(index);
        SerializedProperty text = arrayElement.FindPropertyRelative("_text");
        SerializedProperty nextId = arrayElement.FindPropertyRelative("_beatId");

        text.stringValue = initialText;
        nextId.intValue = beatId;
    }
}
