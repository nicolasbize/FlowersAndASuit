using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioUtils
{

    public enum DialogConversation
    {
        None, Sandy, Al
    }

    private static readonly Dictionary<DialogConversation, FMOD.Studio.EventInstance> FmodEvents = new Dictionary<DialogConversation, FMOD.Studio.EventInstance>();
    private static readonly Dictionary<DialogConversation, string> eventNames = new Dictionary<DialogConversation, string>() { 
        { DialogConversation.Sandy, "event:/Dialogue/Coffee Shop Owner" },
        { DialogConversation.Al, "event:/Dialogue/Fisherman" },
    };
    private static readonly Dictionary<DialogConversation, string> parameterNames = new Dictionary<DialogConversation, string>() {
        { DialogConversation.Sandy, "Dialog Line" },
        { DialogConversation.Al, "Fisherman Dialogue" },
    };

    public static void PlaySound(DialogConversation conversation, Vector3 position, int conversationId) {
        if (conversation == DialogConversation.None)
            return;

        FMOD.Studio.EventInstance instance = LoadInstance(conversation);
        instance.setParameterByName(parameterNames[conversation], (float) conversationId);
        instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
        instance.start();
        instance.release();
    }

    private static FMOD.Studio.EventInstance LoadInstance(DialogConversation conversation) {
        FMOD.Studio.EventInstance instance;
        if (FmodEvents.ContainsKey(conversation)) {
            instance = FmodEvents[conversation];
        } else {
            try {
                instance = FMODUnity.RuntimeManager.CreateInstance(eventNames[conversation]);
                FmodEvents[conversation] = instance;
            } catch (EventNotFoundException e) {
                Debug.Log("fmod event not found: " + e.Message);
                throw e;
            }
        }
        return instance;
    }
}
