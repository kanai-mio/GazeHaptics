using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Haptics;
using System;

public class haptic : MonoBehaviour
{
    public HapticClip clip;

    HapticClipPlayer _playerLeft;
    HapticClipPlayer _playerRight;

    protected virtual void Start()
    {
        // We create two haptic clip players for each hand.
        _playerLeft = new HapticClipPlayer(clip);
        _playerRight = new HapticClipPlayer(clip);
    }

    String GetControllerName(OVRInput.Controller controller)
    {
        if (controller == OVRInput.Controller.LTouch)
        {
            return "left controller";
        }
        else if (controller == OVRInput.Controller.RTouch)
        {
            return "right controller";
        }

        return "unknown controller";
    }

    void HandleControllerInput(OVRInput.Controller controller, HapticClipPlayer clipPlayer, Controller hand)
    {
        string controllerName = GetControllerName(controller);

        try
        {
            // Play first clip with default priority using the index trigger
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                clipPlayer.Play(hand);
                Debug.Log("Should feel vibration on " + controllerName + ".");
            }

            // Stop first clip when releasing the index trigger
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller))
            {
                clipPlayer.Stop();
                Debug.Log("Vibration on " + controllerName + " should stop.");
            }

            // Loop first clip using the B/Y-button
            if (OVRInput.GetDown(OVRInput.Button.Two, controller))
            {
                clipPlayer.isLooping = !clipPlayer.isLooping;
                Debug.Log(String.Format("Looping should be {0} on " + controllerName + ".", clipPlayer.isLooping));
            }

            /*
            // Modulate the amplitude and frequency of the first clip using the thumbstick
            // - Moving left/right modulates the frequency shift
            // - Moving up/down modulates the amplitude
            if (controller == OVRInput.Controller.LTouch)
            {
                clipPlayer.amplitude = Mathf.Clamp(1.0f + OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y, 0.0f, 1.0f);
                clipPlayer.frequencyShift = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x;
            }
            else if (controller == OVRInput.Controller.RTouch)
            {
                clipPlayer.amplitude = Mathf.Clamp(1.0f + OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y, 0.0f, 1.0f);
                clipPlayer.frequencyShift = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
            }
            */

            //Debug.Log(_playerRight.amplitude);
        }

        // If any exceptions occur, we catch and log them here.
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    protected virtual void Update()
    {
        HandleControllerInput(OVRInput.Controller.LTouch, _playerLeft, Controller.Left);
        HandleControllerInput(OVRInput.Controller.RTouch, _playerRight, Controller.Right);
    }

    protected virtual void OnDestroy()
    {
        _playerLeft?.Dispose();
        _playerRight?.Dispose();
    }

    protected virtual void OnApplicationQuit()
    {
        Haptics.Instance.Dispose();
    }



    /*
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */
}
