using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AudioUtils : MonoBehaviour
{

    public static AudioUtils audioUtils;
    private void Awake() {
        if (audioUtils != null) {
            GameObject.Destroy(audioUtils);
        } else {
            audioUtils = this;
        }
        DontDestroyOnLoad(this);
    }

    public enum SoundType { None, ParkOutdoor, CityOutdoor, EnzoFootsteps, AlFish, AlSplash, Dog, ScissorCut, PhoneDial, PhoneRing, UIClick, UIPocket, BusArrive, BusDepart, KeyRetrieval, PhoneHangup, Whistle}
    private static readonly Dictionary<SoundType, FMOD.Studio.EventInstance> soundFmodEvents = new Dictionary<SoundType, FMOD.Studio.EventInstance>();
    

    public enum DialogConversation
    {
        None, Sandy, Al, Lily, Jane, Paulo, OfficerLewis, Scott, Arrest, Enzo, Jim
    }

    private static readonly Dictionary<DialogConversation, FMOD.Studio.EventInstance> DialogFmodEvents = new Dictionary<DialogConversation, FMOD.Studio.EventInstance>();
    private static readonly Dictionary<DialogConversation, string> dialogEventNames = new Dictionary<DialogConversation, string>() { 
        { DialogConversation.Sandy, "event:/Dialogue/Coffee Shop Owner" },
        { DialogConversation.Al, "event:/Dialogue/Fisherman" },
        { DialogConversation.Lily, "event:/Dialogue/Flower Shop Owner" },
        { DialogConversation.Jane, "event:/Dialogue/Lady at the Park" },
        { DialogConversation.Paulo, "event:/Dialogue/Mens Warehouse Owner" },
        { DialogConversation.OfficerLewis, "event:/Dialogue/Officer Lewis" },
        { DialogConversation.Scott, "event:/Dialogue/Shady Guy" },
        { DialogConversation.Arrest, "event:/Dialogue/The Arrest" },
        { DialogConversation.Enzo, "event:/Dialogue/Enzo Solo" },
        { DialogConversation.Jim, "event:/Dialogue/Kid at the Park" },
    };

    private static readonly Dictionary<DialogConversation, string> dialogParameterNames = new Dictionary<DialogConversation, string>() {
        { DialogConversation.Sandy, "Dialog Line" },
        { DialogConversation.Al, "Fisherman Dialogue Line" },
        { DialogConversation.Lily, "Flower Shop Dialogue" },
        { DialogConversation.Jane, "Lady at the Park Dialogue" },
        { DialogConversation.Paulo, "Mens Warehouse Dialogue Line" },
        { DialogConversation.OfficerLewis, "Officer Lewis Dialogue" },
        { DialogConversation.Scott, "Scott Dialogue" },
        { DialogConversation.Arrest, "Arrest Dialogue" },
        { DialogConversation.Enzo, "Enzo Dialogue" },
        { DialogConversation.Jim, "Jim Dialogue" },
    };

    private static readonly Dictionary<SoundType, string> soundPaths = new Dictionary<SoundType, string>() {
        { SoundType.CityOutdoor, "event:/SFX/Ambiences/Interior" },
        { SoundType.ParkOutdoor, "event:/SFX/Ambiences/Outdoor" },
        { SoundType.EnzoFootsteps, "event:/SFX/Enzo/Footsteps" },
        { SoundType.AlFish, "event:/SFX/Fisherman/Fish" },
        { SoundType.AlSplash, "event:/SFX/Fisherman/Splash" },
        { SoundType.Dog, "event:/SFX/Jane/Jane Dog" },
        { SoundType.ScissorCut, "event:/SFX/Kite/ScissorCut" },
        { SoundType.PhoneDial, "event:/SFX/Phonecall Cutscene/Phone Dial" },
        { SoundType.PhoneRing, "event:/SFX/Phonecall Cutscene/Phone Ring" },
        { SoundType.UIClick, "event:/SFX/UI/Mouse Click" },
        { SoundType.UIPocket, "event:/SFX/UI/Pocket" },
        { SoundType.BusArrive, "event:/SFX/Bus/Bus Arrive" },
        { SoundType.BusDepart, "event:/SFX/Bus/Bus Depart" },
        { SoundType.KeyRetrieval, "event:/SFX/Key Retrieval/Key Retrieval" },
        { SoundType.PhoneHangup, "event:/SFX/Phonecall Cutscene/Phone Hangup" },
        { SoundType.Whistle, "event:/SFX/The Arrest/Police Whistle" },
    };

    public enum Music
    {
        MainTheme = 0,
        Dialog = 1,
        Wearhouse = 2,
        Puzzle = 3,
        IntroCredits = 4,
        BusGraveyard = 5,
    }
    public enum Surface { Asphalt = 0, Grass = 1}
    private static FMOD.Studio.EventInstance musicInstance = new FMOD.Studio.EventInstance();
    private static bool musicLoaded = false;
    private static bool musicPlaying = false;
    private static float musicVolume = 1f;
    private static Music currentMusicPlayed = Music.MainTheme;
    private static SoundType currentAtmosphericPlayed = SoundType.ParkOutdoor;
    private static bool walkingSoundPlaying = false;
    private static FMOD.Studio.EventInstance currentDialogInstance = new FMOD.Studio.EventInstance();
    private static Action currentDialogCallback;
    //private static int currentDialogConversationId = -1;
    //private static int conversationCaret = -1;
    private static GCHandle timelineHandle;
    private static TimelineInfo timelineInfo;
    private static bool isDone;
    private static FMOD.Studio.EVENT_CALLBACK fmodEventCallback;

    class TimelineInfo
    {
        public int currentMusicBar = 0;
        public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
    }

    private void Update() {
        FMOD.ATTRIBUTES_3D position = FMODUnity.RuntimeUtils.To3DAttributes(Camera.main.transform.position);
        if (currentAtmosphericPlayed != SoundType.None) {
            GetSoundInstance(currentAtmosphericPlayed).set3DAttributes(position);
        }
        if (walkingSoundPlaying) {
            GetSoundInstance(SoundType.EnzoFootsteps).set3DAttributes(position);
        }
        if (musicPlaying) {
            musicInstance.set3DAttributes(position);
            switch (currentMusicPlayed) {
                case Music.MainTheme:
                    musicInstance.setVolume(0.6f);
                    break;
                case Music.Wearhouse:
                    musicInstance.setVolume(0.8f);
                    break;
                default:
                    musicInstance.setVolume(0.4f);
                    break;
            }
        }

        if (isDone && currentDialogCallback != null) {
            isDone = false;
            currentDialogCallback();
            currentDialogCallback = null;
        }
    }

    private static FMOD.Studio.EventInstance GetSoundInstance(SoundType sound) {
        if (!soundFmodEvents.ContainsKey(sound)) {
            soundFmodEvents[sound] = FMODUnity.RuntimeManager.CreateInstance(soundPaths[sound]);
        }
        return soundFmodEvents[sound];
    }

    public static void PlayAtmospheric(SoundType soundPlayed) {
        if (soundPlayed == SoundType.None) return;
        if (currentAtmosphericPlayed != soundPlayed) {
            currentAtmosphericPlayed = soundPlayed;
            SoundType soundToStop = soundPlayed == SoundType.ParkOutdoor ? SoundType.CityOutdoor : SoundType.ParkOutdoor;
            GetSoundInstance(soundToStop).stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            GetSoundInstance(soundPlayed).start();
            GetSoundInstance(soundPlayed).release();
        }
    }

    public static void PlayWalkingSound(Surface surface) {
        GetSoundInstance(SoundType.EnzoFootsteps).setParameterByName("Surface", (int) surface);
        if (!walkingSoundPlaying) {
            walkingSoundPlaying = true;
            GetSoundInstance(SoundType.EnzoFootsteps).start();
        }

    }

    public static void StopWalkingSound() {
        if (soundFmodEvents.ContainsKey(SoundType.EnzoFootsteps)) {
            GetSoundInstance(SoundType.EnzoFootsteps).stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        walkingSoundPlaying = false;
    }

    public static void PlaySound(SoundType sound) {
        if (sound == SoundType.None) return;
        FMOD.ATTRIBUTES_3D position = FMODUnity.RuntimeUtils.To3DAttributes(Camera.main.transform.position);
        GetSoundInstance(sound).set3DAttributes(position);
        GetSoundInstance(sound).start();
    }

    public static void StopSound(SoundType sound) {
        if (sound == SoundType.None) return;
        GetSoundInstance(sound).stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public static void PlayMusic(Music music, float volume = 1f) {
        if (!musicLoaded) {
            musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/MX/Main Themes");
            musicLoaded = true;
        }
        musicVolume = volume;
        musicInstance.setParameterByName("Game Stage", (int) music);
        currentMusicPlayed = music;
        FMOD.ATTRIBUTES_3D position = FMODUnity.RuntimeUtils.To3DAttributes(Camera.main.transform.position);

        if (!musicPlaying) {
            musicPlaying = true;
            musicInstance.set3DAttributes(position);
            musicInstance.start();
            musicInstance.setVolume(musicVolume);
        }
    }

    public static void RestoreMainTrackOrVolume() {
        if (currentMusicPlayed == Music.Wearhouse) {
            musicInstance.setVolume(0.9f);
        } else {
            PlayMusic(Music.MainTheme, 0.9f);
        }
    }

    public static Music GetCurrentMusicPlaying() {
        return currentMusicPlayed;
    }

    public static void StopDialog(DialogConversation conversation, int conversationId) {
        if (conversation == DialogConversation.None || conversationId < 0)
            return;

        FMOD.Studio.EventInstance instance = LoadDialogInstance(conversation);
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        isDone = true;
    }

    public static void PlayDialog(DialogConversation conversation, int conversationId, Action onDialogEnd=null) {
        if (currentDialogCallback != null) {
            Debug.Log("still playing another dialog");
        }
        if (conversation == DialogConversation.None || conversationId < 0)
            return;
        isDone = false;
        currentDialogInstance = LoadDialogInstance(conversation);
        FMOD.ATTRIBUTES_3D position = FMODUnity.RuntimeUtils.To3DAttributes(Camera.main.transform.position);
        currentDialogInstance.setParameterByName(dialogParameterNames[conversation], (float)conversationId);
        currentDialogInstance.set3DAttributes(position);
        currentDialogInstance.start();
        currentDialogCallback = onDialogEnd;
    }
    
    private static FMOD.Studio.EventInstance LoadDialogInstance(DialogConversation conversation) {
        FMOD.Studio.EventInstance instance;
        if (DialogFmodEvents.ContainsKey(conversation)) {
            instance = DialogFmodEvents[conversation];
        } else {
            try {
                instance = FMODUnity.RuntimeManager.CreateInstance(dialogEventNames[conversation]);
                
                timelineHandle = GCHandle.Alloc(timelineInfo);
                instance.setUserData(GCHandle.ToIntPtr(timelineHandle));
                fmodEventCallback = new FMOD.Studio.EVENT_CALLBACK(DialogEventCallback);
                instance.setCallback(fmodEventCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.SOUND_STOPPED);

                DialogFmodEvents[conversation] = instance;
            } catch (EventNotFoundException e) {
                Debug.Log("fmod event not found: " + e.Message);
                throw e;
            }
        }
        return instance;
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT DialogEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr) {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);
        if (result != FMOD.RESULT.OK) {
            Debug.LogError("Timeline Callback error: " + result);
        } else if (timelineInfoPtr != IntPtr.Zero) {
            isDone = true;
        }
        return FMOD.RESULT.OK;
    }
}
