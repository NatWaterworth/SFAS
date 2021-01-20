using System.Collections;
using UnityEngine;

public class Game : MonoBehaviour
{

    #region Level Variables
    [Header("Level Variables")]
    [SerializeField] StoryData _data;
    [SerializeField] StoryData _errorData;
    [SerializeField] TextDisplay _output;
    [SerializeField] Canvas _consoleCanvas;
    [SerializeField] Camera _defaultConsoleCamera;
    private BeatData _currentBeat;
    private WaitForSeconds _wait;
    bool acceptInput;
    #endregion
    private void Awake()
    {
        SetAcceptingInput(false);

        if (_output == null)
        {
            try
            {
                _output = GetComponentInChildren<TextDisplay>();
            }
            catch
            {
                Debug.LogError(this + " has not set " + _output);
            }
        }
        _currentBeat = null;
        _wait = new WaitForSeconds(0.75f);
    }

    public void SetAcceptingInput(bool _takeInput)
    {
        acceptInput = _takeInput;
    }

    private void Update()
    {
        if(_output.IsIdle)
        {
            if (_currentBeat == null)
            {
                DisplayBeat(1);
            }
            else
            {
                UpdateInput();
            }
        }
    }

    public void SetConsoleCamera(Camera _deviceCamera)
    {
        if(_deviceCamera != null)       
        {
            _consoleCanvas.worldCamera = _deviceCamera;
        }
    }

    public void SetDefaultConsoleCamera()
    {
        Debug.Log("Setting Default Camera");
        if (_defaultConsoleCamera != null)
            _consoleCanvas.worldCamera = _defaultConsoleCamera;
        else
            Debug.LogError("No default camera set for: " + this.gameObject);
    }

    public void SetDefaultConsoleCameraActivity(bool _active)
    {
        Debug.Log("Setting Default Camera to:"+ _active);
        if (_defaultConsoleCamera != null)
            _defaultConsoleCamera.enabled = _active;
        else
            Debug.LogError("No default camera set for: " + this.gameObject);
    }

    public void SetConsoleInformation(StoryData _information)
    {
        if (_information != null)
        {
            _data = _information;
            DisplayBeat(1);
        }
    }

    public void SetErrorInformation()
    {
        if (_errorData != null)
            _data = _errorData;
        else
            Debug.LogError("No default camera set for: " + this.gameObject);
    }

    private void UpdateInput()
    {
        if (!acceptInput)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(_currentBeat != null)
            {
                if (_currentBeat.ID == 1)
                {
                    Application.Quit();
                }
                else
                {
                    DisplayBeat(1);
                }
            }
        }
        else
        {
            KeyCode alpha = KeyCode.Alpha1;
            KeyCode keypad = KeyCode.Keypad1;

            for (int count = 0; count < _currentBeat.Decision.Count; ++count)
            {
                if (alpha <= KeyCode.Alpha9 && keypad <= KeyCode.Keypad9)
                {
                    if (Input.GetKeyDown(alpha) || Input.GetKeyDown(keypad))
                    {
                        ChoiceData choice = _currentBeat.Decision[count];
                        DisplayBeat(choice.NextID);
                        break;
                    }
                }

                ++alpha;
                ++keypad;
            }
        }
    }

    private void DisplayBeat(int id)
    {
        BeatData data = _data.GetBeatById(id);
        StartCoroutine(DoDisplay(data));
        _currentBeat = data;
    }

    public string GetCurrentBeatText()
    {
        if (_currentBeat != null)
            return _currentBeat.DisplayText;
        return "";
    }

    private IEnumerator DoDisplay(BeatData data)
    {
        _output.Clear();

        while (_output.IsBusy)
        {
            yield return null;
        }

        _output.Display(data.DisplayText);

        while(_output.IsBusy)
        {
            yield return null;
        }

        //display decisions if there is more than 1
        if (data.Decision.Count > 1)
        {
            for (int count = 0; count < data.Decision.Count; ++count)
            {
                ChoiceData choice = data.Decision[count];
                _output.Display(string.Format("{0}: {1}", (count + 1), choice.DisplayText));

                while (_output.IsBusy)
                {
                    yield return null;
                }

            }
            _output.ShowWaitingForInput();
        }
        else
        {
            yield return _wait;
            ChoiceData choice = _currentBeat.Decision[0];
            DisplayBeat(choice.NextID);
        }
        

    }
}
