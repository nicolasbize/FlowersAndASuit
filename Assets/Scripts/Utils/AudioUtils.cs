using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioUtils
{

    public enum DialogConversation
    {
        None, Sandy, Al, Lily, Jane, Paulo
    }

    private static readonly Dictionary<DialogConversation, FMOD.Studio.EventInstance> DialogFmodEvents = new Dictionary<DialogConversation, FMOD.Studio.EventInstance>();
    private static readonly Dictionary<DialogConversation, string> dialogEventNames = new Dictionary<DialogConversation, string>() { 
        { DialogConversation.Sandy, "event:/Dialogue/Coffee Shop Owner" },
        { DialogConversation.Al, "event:/Dialogue/Fisherman" },
        { DialogConversation.Lily, "event:/Dialogue/Flower Shop Owner" },
        { DialogConversation.Jane, "event:/Dialogue/Lady at the Park" },
        { DialogConversation.Paulo, "event:/Dialogue/Mens Warehouse Owner" },
    };
    private static readonly Dictionary<DialogConversation, string> dialogParameterNames = new Dictionary<DialogConversation, string>() {
        { DialogConversation.Sandy, "Dialog Line" },
        { DialogConversation.Al, "Fisherman Dialogue Line" },
        { DialogConversation.Lily, "Flower Shop Dialogue" },
        { DialogConversation.Jane, "Lady at the Park Dialogue" },
        { DialogConversation.Paulo, "Mens Warehouse Dialogue Line" },
    };

    public enum Music
    {
        MainTheme = 0,
        Dialog = 1,
        Wearhouse = 2,
        Puzzle = 3
    }
    private static FMOD.Studio.EventInstance musicInstance = new FMOD.Studio.EventInstance();
    private static bool musicLoaded = false;
    private static bool musicPlaying = false;
    private static Music currentMusicPlayed = Music.MainTheme;

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
            musicInstance.release();
        }
    }

    public static Music GetCurrentMusicPlaying() {
        return currentMusicPlayed;
    }

    public static void PlayDialog(DialogConversation conversation, Vector3 position, int conversationId) {
        if (conversation == DialogConversation.None || conversationId < 0)
            return;

        FMOD.Studio.EventInstance instance = LoadDialogInstance(conversation);
        instance.setParameterByName(dialogParameterNames[conversation], (float) conversationId);
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
        instance.start();
        instance.release();
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
