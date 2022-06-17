using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioUtils
{
    public enum SoundType { None, ParkOutdoor, CityOutdoor, EnzoFootsteps, AlFish, AlSplash, Dog, ScissorCut, PhoneDial, PhoneRing, UIClick, UIPocket}
    private static readonly Dictionary<SoundType, FMOD.Studio.EventInstance> soundFmodEvents = new Dictionary<SoundType, FMOD.Studio.EventInstance>();
    

    public enum DialogConversation
    {
        None, Sandy, Al, Lily, Jane, Paulo, OfficerLewis, Scott, Arrest, Enzo
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
    public enum Surface { Ground = 0, Grass = 1}
    private static FMOD.Studio.EventInstance musicInstance = new FMOD.Studio.EventInstance();
    private static bool musicLoaded = false;
    private static bool musicPlaying = false;
    private static Music currentMusicPlayed = Music.MainTheme;
    private static SoundType currentAtmosphericPlayed = SoundType.ParkOutdoor;
    private static bool walkingSoundPlaying = false;

    private static void LoadSounds() {
        if (soundFmodEvents.Count == 0) {
            soundFmodEvents.Add(SoundType.CityOutdoor, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ambiences/Interior"));
            soundFmodEvents.Add(SoundType.ParkOutdoor, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Ambiences/Outdoor"));
            soundFmodEvents.Add(SoundType.EnzoFootsteps, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Enzo/Footsteps"));
            soundFmodEvents.Add(SoundType.AlFish, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Fisherman/Fish"));
            soundFmodEvents.Add(SoundType.AlSplash, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Fisherman/Splash"));
            soundFmodEvents.Add(SoundType.Dog, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Jane/Jane Dog"));
            soundFmodEvents.Add(SoundType.ScissorCut, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Kite/ScissorCut"));
            soundFmodEvents.Add(SoundType.PhoneDial, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Phonecall Cutscene/Phone Dial"));
            soundFmodEvents.Add(SoundType.PhoneRing, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/Phonecall Cutscene/Phone Ring"));
            soundFmodEvents.Add(SoundType.UIClick, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/UI/Mouse Click"));
            soundFmodEvents.Add(SoundType.UIPocket, FMODUnity.RuntimeManager.CreateInstance("event:/SFX/UI/Pocket"));
        }
    }

    public static void PlayAtmospheric(SoundType soundPlayed, Vector3 position) {
        LoadSounds();
        if (soundPlayed == SoundType.None) return;
        if (currentAtmosphericPlayed != soundPlayed) {
            currentAtmosphericPlayed = soundPlayed;
            SoundType soundToStop = soundPlayed == SoundType.ParkOutdoor ? SoundType.CityOutdoor : SoundType.ParkOutdoor;
            soundFmodEvents[soundToStop].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            soundFmodEvents[soundPlayed].set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
            soundFmodEvents[soundPlayed].start();
            soundFmodEvents[soundPlayed].release();
        }
    }

    public static void PlayWalkingSound(Surface surface, Vector3 position) {
        LoadSounds();
        soundFmodEvents[SoundType.EnzoFootsteps].setParameterByName("Surface", (int) surface);
        if (!walkingSoundPlaying) {
            walkingSoundPlaying = true;
            soundFmodEvents[SoundType.EnzoFootsteps].set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
            soundFmodEvents[SoundType.EnzoFootsteps].start();
        }

    }

    public static void StopWalkingSound() {
        LoadSounds();
        soundFmodEvents[SoundType.EnzoFootsteps].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        walkingSoundPlaying = false;
    }

    public static void PlaySound(SoundType sound, Vector3 position) {
        LoadSounds();
        if (sound == SoundType.None) return;
        soundFmodEvents[sound].set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
        soundFmodEvents[sound].start();
    }

    public static void StopSound(SoundType sound) {
        LoadSounds();
        if (sound == SoundType.None) return;
        soundFmodEvents[sound].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }


    public static void PlayMusic(Music music, Vector3 position) {
        if (!musicLoaded) {
            musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/MX/Main Themes");
            musicLoaded = true;
        }

        musicInstance.setParameterByName("Game Stage", (int) music);
        currentMusicPlayed = music;

        if (!musicPlaying) {
            musicPlaying = true;
            musicInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
            musicInstance.start();
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
    }

    public static void PlayDialog(DialogConversation conversation, Vector3 position, int conversationId) {
        if (conversation == DialogConversation.None || conversationId < 0)
            return;

        FMOD.Studio.EventInstance instance = LoadDialogInstance(conversation);
        instance.setParameterByName(dialogParameterNames[conversation], (float) conversationId);
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
        instance.start();
        //instance.release();
    }

    private static FMOD.Studio.EventInstance LoadDialogInstance(DialogConversation conversation) {
        FMOD.Studio.EventInstance instance;
        if (DialogFmodEvents.ContainsKey(conversation)) {
            instance = DialogFmodEvents[conversation];
        } else {
            try {
                instance = FMODUnity.RuntimeManager.CreateInstance(dialogEventNames[conversation]);
                DialogFmodEvents[conversation] = instance;
            } catch (EventNotFoundException e) {
                Debug.Log("fmod event not found: " + e.Message);
                throw e;
            }
        }
        return instance;
    }
}
