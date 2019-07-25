/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IBM.Watson.SpeechToText.V1;
using IBM.Watson.TextToSpeech.V1;
using IBM.Cloud.SDK;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.DataTypes;
using IBM.Watson.Assistant.V2.Model;
using IBM.Watson.Assistant.V2;

using System.Xml;
using System;
using System.Globalization;

public class SpeechSandboxStreaming : MonoBehaviour
{

    public GameManager gameManager;
    public GetVoicePlayWindows browser;



    public AudioClip respondClip;
    public AudioClip introClip;
    public AudioClip playingSongClip;
    public AudioClip[] apologizeClips;
    public AudioClip websearchRespondClip;
    public AudioClip turnOffClip;
    public AudioClip cctvClip;
    public AudioClip emotion1RespondClip;
    public AudioClip emotion2RespondClip;
    public AudioClip emotion3RespondClip;

    public bool isWakeUp;

    [SerializeField]

    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Header("Speech To Text")]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/speech-to-text/api\"")]
    private string speechToTextServiceUrl = "";
    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string speechToTextIamApikey;
    [Tooltip("The IAM url used to authenticate the apikey (optional). This defaults to \"https://iam.bluemix.net/identity/token\".")]
    [SerializeField]
    private string speechToTextIamUrl;

    [Header("Watson Assistant")]
    [Tooltip("The service URL (optional). This defaults to \"https://gateway.watsonplatform.net/assistant/api\"")]
    [SerializeField]
    private string assistantServiceUrl;
    [Tooltip("The Assistant ID to run the example.")]
    [SerializeField]
    private string assistantId;
    [Tooltip("The version date with which you would like to use the service in the form YYYY-MM-DD. Current is 2018-07-10")]
    [SerializeField]
    private string assistantVersionDate;

    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string assistantIamApikey;
    [Tooltip("The IAM url used to authenticate the apikey (optional). This defaults to \"https://iam.bluemix.net/identity/token\".")]
    [SerializeField]
    private string assistantIamUrl;

    [Header("Text to Speech")]
    [SerializeField]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/text-to-speech/api\"")]
    private string textToSpeechURL;
    [Tooltip("The apikey.")]
    [SerializeField]
    private string textToSpeechIamApikey;

    #endregion


    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;
    private bool _stopListeningFlag = false;

    private bool createSessionTested = false;
    private bool deleteSessionTested;
    private string sessionId;

    private SpeechToTextService _speechToText;
    private AssistantService _assistant;
    private TextToSpeechService _textToSpeech;

    private bool messageTested;
    


    private readonly string API_KEY = "4EDE66PW8NZD";
    private readonly string LOCATION_CODE = "Asia/Seoul";


    private readonly string AssistantName = "Neptune";
    private readonly string COMMAND_MUSIC = "Music";
    private readonly string COMMAND_CALENDAR = "Calendar";
    private readonly string COMMAND_BEAT = "Beat";
    private readonly string COMMAND_DATE = "Date";
    private readonly string COMMAND_TIME = "Time";
    private readonly string COMMAND_WEB_SEARCH = "Web_search";
    private readonly string COMMAND_TURN_OFF = "Turn_off";
    private readonly string COMMAND_CCTV = "CCTV";
    private readonly string COMMAND_EMOTION1 = "Emotion_1";
    private readonly string COMMAND_EMOTION2 = "Emotion_2";
    private readonly string COMMAND_EMOTION3 = "Emotion_3";



    private IEnumerator createServices(){

        Credentials stt_credentials = null;
        //  Create credential and instantiate service
        if (!string.IsNullOrEmpty(speechToTextIamApikey))
        {
            //  Authenticate using iamApikey
            TokenOptions tokenOptions = new TokenOptions()
            {
                IamApiKey = speechToTextIamApikey,
                IamUrl = speechToTextIamUrl
            };

            stt_credentials = new Credentials(tokenOptions, speechToTextServiceUrl);

            while (!stt_credentials.HasIamTokenData())
                yield return null;
        }
        else
        {
            throw new IBMException("Please provide IAM ApiKey for the Speech To Text service.");
        }

        Credentials tts_credentials = null;
        //  Create credential and instantiate service
        if (!string.IsNullOrEmpty(textToSpeechIamApikey))
        {
            TokenOptions tokenOptions = new TokenOptions()
            {
                IamApiKey = textToSpeechIamApikey
            };

            tts_credentials = new Credentials(tokenOptions, textToSpeechURL);

            while (!tts_credentials.HasIamTokenData())
                yield return null;
        }
        else
        {
            throw new IBMException("Please provide IAM ApiKey for the Text To Speech service.");
        }


        Credentials asst_credentials = null;
        //  Create credential and instantiate service

        if (!string.IsNullOrEmpty(assistantIamApikey))
        {
            //  Authenticate using iamApikey
            TokenOptions tokenOptions = new TokenOptions()
            {
                IamApiKey = assistantIamApikey
            };

            asst_credentials = new Credentials(tokenOptions, assistantServiceUrl);

            while (!asst_credentials.HasIamTokenData())
                yield return null;
        }
        else
        {
            throw new IBMException("Please provide IAM ApiKey for the Watson Assistant service.");
        }


        _speechToText = new SpeechToTextService(stt_credentials);
        _textToSpeech = new TextToSpeechService(tts_credentials);
        _assistant = new AssistantService(assistantVersionDate, asst_credentials);

        _assistant.VersionDate = assistantVersionDate;
        Active = true;

        _assistant.CreateSession(OnCreateSession, assistantId);

        while (!createSessionTested)
        {
            yield return null;
        }

        StartRecording();
    }

    void Start()
    {
        LogSystem.InstallDefaultReactors();


        //  Create credential and instantiate service
        Runnable.Run(createServices());


        // initialize the watson(neptune) engine wake up state
        isWakeUp = false;

        // playing clip the introduce about assistant service.
        gameManager.PlayClip(introClip);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //gameManager.PlayClip(emotion1RespondClip);
            Runnable.Run(ShowSchedule());
        }
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.01f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("SpeechSandboxStreaming.OnError()", "Error! {0}", error);
    }

    /*
    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("SpeechSandboxStreaming.OnFail()", "Error received: {0}", error.ToString());
    }
    */

    private IEnumerator RecordingHandler()
    {
        Log.Debug("SpeechSandboxStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        foreach(string deviceId in Microphone.devices)
        {
            Debug.Log("micro device: " + deviceId);
        }

        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("SpeechSandboxStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
				record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio,
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    if (res.final && alt.confidence > 0)
                    {
                        string transcript_text = alt.transcript;
                        Debug.Log("Result: " + transcript_text + " Confidence: " + alt.confidence);

                        var input = new MessageInput()
                        {
                            Text = transcript_text,
                            Options = new MessageInputOptions()
                            {
                                ReturnContext = false
                            }
                        };
                        Debug.Log("Input to Assistant:" + input.Text);

                        _assistant.Message(OnMessage,  assistantId, sessionId, input);
                    }
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("SpeechSandboxStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("SpeechSandboxStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach(var alternative in wordAlternative.alternatives)
                            Log.Debug("SpeechSandboxStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    void OnMessage(DetailedResponse<MessageResponse> resp, IBMError error)
    {
        if (resp != null && resp.Result.Output.Intents.Count != 0)
        {
            string intent = resp.Result.Output.Intents[0].Intent;
            Debug.Log("Intent: " + intent);

            /*
            string currentMat = null;
            string currentScale = null;
            string direction = null;
            */

            if (intent == AssistantName)
            {
                if (isWakeUp == false)
                {
                    isWakeUp = true;
                    gameManager.PlayClip(respondClip);

                    return;
                    //ReserveToSleep();
                }
            }

            // 엔진이 깨어있을 때만 명령어 받기
            if (!isWakeUp)
            {
                return;
            }



            // 커맨드 처리

            if (string.Equals(intent, COMMAND_MUSIC, StringComparison.OrdinalIgnoreCase))
            {
                gameManager.PlayClip(playingSongClip);
                Runnable.Run(PlayingMusic());
            }
            else if (string.Equals(intent, COMMAND_BEAT, StringComparison.OrdinalIgnoreCase))
            {
                //gameManager.PlayClip(playingSongClip);
                Runnable.Run(DropTheBeat());
            }
            else if (string.Equals(intent, COMMAND_DATE, StringComparison.OrdinalIgnoreCase))
            {
                Runnable.Run(GetDateAndAlert());
            }
            else if(string.Equals(intent, COMMAND_TIME, StringComparison.OrdinalIgnoreCase))
            {
                Runnable.Run(GetTimeAndAlert());
            }
            else if(string.Equals(intent, COMMAND_WEB_SEARCH, StringComparison.OrdinalIgnoreCase))
            {
                gameManager.PlayClip(websearchRespondClip);
                Runnable.Run(WebSearching());  
            }
            else if (string.Equals(intent, COMMAND_TURN_OFF, StringComparison.OrdinalIgnoreCase))
            {
                gameManager.PlayClip(turnOffClip);
                Runnable.Run(TurnOff());
            }
            else if (string.Equals(intent, COMMAND_CCTV, StringComparison.OrdinalIgnoreCase))
            {
                gameManager.PlayClip(cctvClip);
                Runnable.Run(ConnectingDoor());
            }
            else if (string.Equals(intent, COMMAND_CALENDAR, StringComparison.OrdinalIgnoreCase))
            {
                //gameManager.PlayClip(cctvClip);
                Runnable.Run(ShowSchedule());
            }
            else if (string.Equals(intent, COMMAND_EMOTION1, StringComparison.OrdinalIgnoreCase))
            {
                gameManager.PlayClip(emotion1RespondClip);
            }
            else if (string.Equals(intent, COMMAND_EMOTION2, StringComparison.OrdinalIgnoreCase))
            {
                gameManager.PlayClip(emotion2RespondClip);
            }
            else if (string.Equals(intent, COMMAND_EMOTION3, StringComparison.OrdinalIgnoreCase))
            {
                gameManager.PlayClip(emotion3RespondClip);
            }
            else
            {
                AudioClip apologizeClip = apologizeClips[UnityEngine.Random.Range(0, apologizeClips.Length)];
                gameManager.PlayClip(apologizeClip);
            }


            /*
            if (intent == "move")
            {
                foreach (RuntimeEntity entity in resp.Result.Output.Entities)
                {
                    Debug.Log("entityType: " + entity.Entity + " , value: " + entity.Value);
                    direction = entity.Value;
                    gameManager.MoveObject(direction);
                }
            }

            if (intent == "create")
            {
                bool createdObject = false;
                foreach (RuntimeEntity entity in resp.Result.Output.Entities)
                {
                    Debug.Log("entityType: " + entity.Entity + " , value: " + entity.Value);
                    if (entity.Entity == "material")
                    {
                        currentMat = entity.Value;
                    }
                    if (entity.Entity == "scale")
                    {
                        currentScale = entity.Value;
                    }
                    else if (entity.Entity == "object")
                    {
                        gameManager.CreateObject(entity.Value, currentMat, currentScale);
                        createdObject = true;
                        currentMat = null;
                        currentScale = null;
                    }
                }

                if (!createdObject)
                {
                    gameManager.PlayError(sorryClip);
                }
            }
            else if (intent == "destroy")
            {
                gameManager.DestroyAtPointer();
            }
            else if (intent == "help")
            {
                if (helpClips.Count > 0)
                {
                    gameManager.PlayClip(helpClips[Random.Range(0, helpClips.Count)]);
                }
            }
            */
        }
        else
        {
            Debug.Log("Failed to invoke OnMessage();");
        }
    }

    private IEnumerator CallTextToSpeech(string outputText)
    {
        Debug.Log("Sent to Watson Text To Speech: " + outputText);

        byte[] synthesizeResponse = null;
        AudioClip clip = null;


        // URL이 잘못되었거나, myClip이 로컬 혹은 넷에 존재하지 않아서인가,,?
        _textToSpeech.Synthesize(
            callback: (DetailedResponse<byte[]> response, IBMError error) =>
            {
                synthesizeResponse = response.Result;
                clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                PlayClip(clip);
            },
            text: outputText,
            voice: "en-US_AllisonV3Voice",
            accept: "audio/wav"
        );

        while(synthesizeResponse == null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(clip.length);
    }

    private void PlayClip(AudioClip clip)
    {
        Debug.Log("Received audio file from Watson Text To Speech");

        if (Application.isPlaying && clip != null)
        {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.volume = 1.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();

            Invoke("RecordAgain", source.clip.length);
            Destroy(audioObject, clip.length);
        }
    }
    private void RecordAgain()
    {
        Debug.Log("Played Audio received from Watson Text To Speech");
        if (!_stopListeningFlag)
        {
            OnListen();
        }
    }
    private void OnListen()
    {
        Log.Debug("AvatarPattern.OnListen", "Start();");

        Active = true;

        StartRecording();
    }


    private void ReserveToSleep(float time = 3f)
    {
        Invoke("SetToSleep", time);
    }
    
    private void SetToSleep()
    {
        isWakeUp = false;
    }

    private void OnDeleteSession(DetailedResponse<object> response, IBMError error)
    {
        Log.Debug("ExampleAssistantV2.OnDeleteSession()", "Session deleted.");
        deleteSessionTested = true;
    }

    private void OnCreateSession(DetailedResponse<SessionResponse> response, IBMError error)
    {
        Log.Debug("ExampleAssistantV2.OnCreateSession()", "Session: {0}", response.Result.SessionId);
        sessionId = response.Result.SessionId;
        createSessionTested = true;
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("SpeechSandboxStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }






    private string GetTimezoneURL()
    {
        string url = string.Format("http://api.timezonedb.com/v2.1/get-time-zone?key={0}&format=xml&by=zone&zone={1}", API_KEY, LOCATION_CODE);

        return url;
    }

    private IEnumerator GetDateAndAlert()
    {
        string url = GetTimezoneURL();
        WWW web = new WWW(url);
        do
        {
            yield return null;
        }
        while (!web.isDone);

        if (web.error != null)
        {
            Debug.LogError("web.error=" + web.error);
            yield break;
        }
        else
        {
            // xml variable
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(web.text.Trim());

            // get date
            XmlNode xmlNode = xmlDocument.SelectSingleNode("result/formatted");
            string formattedStringDate = xmlNode.InnerText;
            Debug.Log("date: " + formattedStringDate);

            // formatting
            DateTime dateTime = DateTime.Parse(formattedStringDate);
            CultureInfo culture = new CultureInfo("en-US");
            string finalDate = string.Format("Today is {0}, {1} of {2}.", dateTime.DayOfWeek, dateTime.Day, dateTime.ToString("MMMM", culture));

            // calling the tts.
            Runnable.Run(CallTextToSpeech(finalDate));            
        }
    }

    private IEnumerator GetTimeAndAlert()
    {
        string url = GetTimezoneURL();
        WWW web = new WWW(url);
        do
        {
            yield return null;
        }
        while (!web.isDone);

        if (web.error != null)
        {
            Debug.LogError("web.error=" + web.error);
            yield break;
        }
        else
        {
            // xml variable
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(web.text.Trim());

            // get date
            XmlNode xmlNode = xmlDocument.SelectSingleNode("result/formatted");
            string formattedStringDate = xmlNode.InnerText;
            Debug.Log("time: " + formattedStringDate);

            // formatting
            DateTime dateTime = DateTime.Parse(formattedStringDate);
            CultureInfo culture = new CultureInfo("en-US");
            string finalTime = string.Format("Time is {0} {1}.", dateTime.Hour, dateTime.Minute);

            // calling the tts.
            Runnable.Run(CallTextToSpeech(finalTime));
        }
    }

    private IEnumerator PlayingMusic()
    {
        //const string PLAY_URL = "https://www.youtube.com/watch?v=tIOwZjinOqk";
        const string PLAY_URL = "https://www.youtube.com/watch?v=RzEr66WYcm8";
        browser.InstantiateBrowserWindow(PLAY_URL);

        yield return null;
    }

    private IEnumerator DropTheBeat()
    {
        const string PLAY_URL = "https://youtu.be/_CL6n0FJZpk?t=20";
        browser.InstantiateBrowserWindow(PLAY_URL);

        yield return null;
    }

    private IEnumerator WebSearching()
    {
        const string SEARCHING_URL = "https://www.google.com/search?q=dog";
        browser.InstantiateBrowserWindow(SEARCHING_URL);

        yield return null;
    }

    private IEnumerator ShowSchedule()
    {
        const string Calendar_URL = "https://calendar.google.com/calendar/embed?src=bonokong03%40gmail.com&ctz=Asia%2FSeoul";
        browser.InstantiateBrowserWindow(Calendar_URL);

        yield return null;
    }

    private IEnumerator TurnOff()
    {
        browser.InitializeBrowserWindow();
        yield return null;
    }

    private IEnumerator ConnectingDoor()
    {
        const string CCTV_URL = "http://172.16.106.150:4747/video";
        browser.InstantiateBrowserWindow(CCTV_URL);

        yield return null;
    }
}
