﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class NumberSequenceScript : MonoBehaviour
{
    [SerializeField] private KMBombInfo bombRef;
    [SerializeField] private KMAudio audioRef;

    [SerializeField] private KMSelectable[] buttons;

    [SerializeField] private TextMesh screenTextRef;
    [SerializeField] private GameObject[] lights;
    [SerializeField] private GameObject screenLight;
    [SerializeField] private Material green;
    [SerializeField] private Material black;
    [SerializeField] private Material orange;
    [SerializeField] private Material yellow;
    [SerializeField] private Material red;
    [SerializeField] private Material white;

    private Coroutine lightCycleRef;
    private bool areLightsCycling = true;
    private Coroutine lightFlashingRef;
    private bool areLightsBlinking = false;
    
    private int[] numberSequence = new int[4];
    private int currentLight = 1;
    private int timesCycled = 0;
    private string portToUse;
    private int litLEDS = 0;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = false;

    void Awake()
    {
        //NEEDED, DONT TOUCH
        moduleId = moduleIdCounter++;

        //Gives each selectable object a function
        foreach (KMSelectable button in buttons)
        {
            button.OnInteract += delegate () { PressButton(button); return false; };
        }
    }

    void Start()
    {
        for (int i = 0; i < numberSequence.Length; i++)
        {
            numberSequence[i] = UnityEngine.Random.Range(10, 100);
        }

        Debug.LogFormat("[Number Sequence #{0}] The current number sequence is {1}, {2}, {3}, {4}", moduleId, numberSequence[0], numberSequence[1], numberSequence[2], numberSequence[3]);

        if (bombRef.GetPortCount() > 0)
        {
            if (bombRef.IsPortPresent(Port.StereoRCA))
            {
                portToUse = "StereoRCA";
            }
            else if (bombRef.IsPortPresent(Port.RJ45))
            {
                portToUse = "RJ45";
            }
            else if (bombRef.IsPortPresent(Port.PS2))
            {
                portToUse = "PS2";
            }
            else if (bombRef.IsPortPresent(Port.Parallel))
            {
                portToUse = "Parallel";
            }
            else if (bombRef.IsPortPresent(Port.Serial))
            {
                portToUse = "Serial";
            }
            else if (bombRef.IsPortPresent(Port.DVI))
            {
                portToUse = "DVI";
            }
            else
            {
                portToUse = string.Empty;
            }

            Debug.LogFormat("[Number Sequence #{0}] The correct port row to use is {1}.", moduleId, portToUse);
        }
        else
        {
            portToUse = string.Empty;

            Debug.LogFormat("[Number Sequence #{0}] The bomb does not have any ports.", moduleId);
        }

        lightCycleRef = StartCoroutine(LightCycle());
    }

    private IEnumerator LightCycle()
    {
        areLightsCycling = true;
        screenLight.SetActive(true);

        while (true)
        {
            for (int i = currentLight - 1; i < lights.Length; i++)
            {
                //Sets lights to black or green
                for (int j = 0; j < lights.Length; j++)
                {
                    if (i == j)
                    {
                        if (timesCycled == 0)
                        {
                            lights[i].GetComponent<MeshRenderer>().material = green;
                            lights[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.green;
                        }
                        else if (timesCycled == 1)
                        {
                            lights[i].GetComponent<MeshRenderer>().material = yellow;
                            lights[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.yellow;
                        }
                        else if (timesCycled == 2)
                        {
                            lights[i].GetComponent<MeshRenderer>().material = orange;
                            lights[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Light>().color = new Color(1f, 0.61f, 0f);
                        }
                        else if (timesCycled == 3)
                        {
                            lights[i].GetComponent<MeshRenderer>().material = red;
                            lights[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.red;
                        }
                        else
                        {
                            timesCycled = 0;
                            lights[i].GetComponent<MeshRenderer>().material = green;
                            lights[i].gameObject.transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.green;

                            for (int k = 0; k < numberSequence.Length; k++)
                            {
                                numberSequence[k] = UnityEngine.Random.Range(10, 100);
                            }

                            Debug.LogFormat("[Number Sequence #{0}] The number sequence has been cycled through 4 times, generating new sequence. The new number sequence is {1}, {2}, {3}, {4}", moduleId, numberSequence[0], numberSequence[1], numberSequence[2], numberSequence[3]);
                        }
                        
                        lights[i].gameObject.transform.GetChild(0).gameObject.SetActive(true);
                    }
                    else
                    {
                        lights[j].GetComponent<MeshRenderer>().material = black;
                        lights[j].gameObject.transform.GetChild(0).gameObject.SetActive(false);
                    }

                    screenTextRef.text = numberSequence[i].ToString();
                }

                //Sets currentLight to the correct value
                currentLight = i + 1;

                yield return new WaitForSecondsRealtime(0.5f + (timesCycled * 0.065f));

                if (currentLight == 4)
                {
                    timesCycled++;
                    currentLight = 1;
                }
            }
        }
    }

    IEnumerator FlashLight()
    {
        while (true)
        {
            lights[currentLight - 1].GetComponent<MeshRenderer>().material = white;
            lights[currentLight - 1].gameObject.transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.white;
            lights[currentLight - 1].transform.GetChild(0).gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.75f);
            lights[currentLight - 1].transform.GetChild(0).gameObject.SetActive(false);
            lights[currentLight - 1].GetComponent<MeshRenderer>().material = black;
            yield return new WaitForSecondsRealtime(0.75f);
        }
    }

    void NextLight()
    {
        Debug.LogFormat("[Number Sequence #{0}] {1} was submitted correctly.", moduleId, screenTextRef.text);
        screenTextRef.text = string.Empty;
        screenLight.SetActive(false);

        lights[currentLight - 1].GetComponent<MeshRenderer>().material = green;
        lights[currentLight - 1].transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.green;

        currentLight = currentLight + 1 > 4 ? 1 : currentLight + 1;

        litLEDS++;

        if (litLEDS == 4)
        {
            StopCoroutine(lightFlashingRef);
            lights[currentLight - 1].GetComponent<MeshRenderer>().material = green;
            lights[currentLight - 1].transform.GetChild(0).gameObject.SetActive(true);
            moduleSolved = true;
            audioRef.PlaySoundAtTransform("PassSound", this.transform);
            StartCoroutine(BlinkLights(false));
            this.gameObject.GetComponent<KMBombModule>().HandlePass();
            return;
        }
        else
        {
            audioRef.PlaySoundAtTransform("Blip", this.transform);
            StopCoroutine(lightFlashingRef);
            lightFlashingRef = StartCoroutine(FlashLight());
            LogHandler();
        }
    }

    IEnumerator BlinkLights(bool strike)
    {
        areLightsBlinking = true;
        for (int j = 0; j < 6; j++)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (strike)
                {
                    lights[i].GetComponent<MeshRenderer>().material = red;
                    lights[i].transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.red;
                    lights[i].transform.GetChild(0).gameObject.SetActive(true);
                }
                else
                {
                    lights[i].GetComponent<MeshRenderer>().material = green;
                    lights[i].transform.GetChild(0).gameObject.GetComponent<Light>().color = Color.green;
                    lights[i].transform.GetChild(0).gameObject.SetActive(true);
                }
            }

            yield return new WaitForSecondsRealtime(0.1f);

            for (int i = 0; i < lights.Length; i++)
            {
                if (strike)
                {
                    lights[i].GetComponent<MeshRenderer>().material = black;
                    lights[i].transform.GetChild(0).gameObject.SetActive(false);
                }
                else
                {
                    lights[i].GetComponent<MeshRenderer>().material = black;
                    lights[i].transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            yield return new WaitForSecondsRealtime(0.1f);
        }

        if (strike)
        {
            lightCycleRef = StartCoroutine(LightCycle());
        }

        areLightsBlinking = false;
    }

    //When a button is pushed
    void PressButton(KMSelectable pressedButton)
    {
        if (moduleSolved | areLightsBlinking)
        {
            return;
        }

        if (pressedButton.name == "KeySubmit")
        {
            pressedButton.AddInteractionPunch(0.75f);
            audioRef.PlaySoundAtTransform("KeyBoard_Click2", this.transform);

            if (areLightsCycling)
            {
                //Stops the lights from cycling if they are
                StopCoroutine(lightCycleRef);
                lightFlashingRef = StartCoroutine(FlashLight());
                areLightsCycling = false;
                screenTextRef.text = string.Empty;
                screenLight.SetActive(false);
                LogHandler();
            }
            else
            {
                switch (portToUse)
                {
                    case ("StereoRCA"):
                        switch (currentLight)
                        {
                            case (1):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.Pow(numberSequence[0], 2), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (2):
                                if (screenTextRef.text == Mathf.Clamp((numberSequence[1] - numberSequence[3]) * 2, 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (3):
                                if (screenTextRef.text == Mathf.Clamp(50 - numberSequence[2], 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (4):
                                if (screenTextRef.text == Mathf.Clamp(numberSequence[0] * numberSequence[3], 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                        }
                        break;
                    case ("RJ45"):
                        switch (currentLight)
                        {
                            case (1):
                                if (screenTextRef.text == Mathf.Clamp((Mathf.Pow(numberSequence[2], 3)) - (numberSequence[0] * 5), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (2):
                                if (screenTextRef.text == Mathf.Clamp(25 - (-numberSequence[0]), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (3):
                                if (screenTextRef.text == Mathf.Clamp(numberSequence[1], 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (4):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt(100f / (float)numberSequence[3]), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                        }
                        break;
                    case ("PS2"):
                        switch (currentLight)
                        {
                            case (1):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt((float)numberSequence[0] + ((float)numberSequence[1] / 2f)), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (2):
                                if (screenTextRef.text == Mathf.Clamp((numberSequence[3] * 5) - numberSequence[2], 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (3):
                                if (screenTextRef.text == Mathf.Clamp(500 - (numberSequence[2] * 10), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (4):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt(((float)numberSequence[3] / (float)numberSequence[0]) + ((float)numberSequence[1] * 25f)), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                        }
                        break;
                    case ("Parallel"):
                        switch (currentLight)
                        {
                            case (1):
                                if (screenTextRef.text == Mathf.Clamp(9999 - (numberSequence[3] * numberSequence[3]), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (2):
                                if (screenTextRef.text == Mathf.Clamp((numberSequence[1] * 75) + 1000, 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (3):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt(((float)numberSequence[2] / 99f) * 3500f), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (4):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt((1f / (float)numberSequence[3]) * (Mathf.Pow((float)numberSequence[0], 2f))), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                        }
                        break;
                    case ("Serial"):
                        switch (currentLight)
                        {
                            case (1):
                                if (screenTextRef.text == Mathf.Clamp(numberSequence[0] * numberSequence[2] * numberSequence[3], 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (2):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt((float)numberSequence[1] - ((-(float)numberSequence[0]) / 35f)), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (3):
                                if (screenTextRef.text == Mathf.Clamp(8500 - (numberSequence[2] * 100), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (4):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt((float)numberSequence[3] / (float)numberSequence[0]), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                        }
                        break;
                    case ("DVI"):
                        switch (currentLight)
                        {
                            case (1):
                                if (screenTextRef.text == Mathf.Clamp((numberSequence[0] * numberSequence[3]) + 6000, 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (2):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt(((float)numberSequence[1] / 80f) * (Mathf.Pow((float)numberSequence[2], 2))), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (3):
                                if (screenTextRef.text == Mathf.Clamp(numberSequence[0] + numberSequence[1] + numberSequence[2] + numberSequence[3], 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (4):
                                if (screenTextRef.text == Mathf.Clamp(Mathf.CeilToInt((350f * (float)numberSequence[3]) / ((float)numberSequence[0] + (float)numberSequence[1])), 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                        }
                        break;
                    case (""):
                        switch (currentLight)
                        {
                            case (1):
                                if (screenTextRef.text == Mathf.Clamp((numberSequence[0] * numberSequence[1]) + 1250, 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (2):
                                if (screenTextRef.text == Mathf.Clamp((numberSequence[1] * numberSequence[2]) + 1500, 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (3):
                                if (screenTextRef.text == Mathf.Clamp((numberSequence[2] * numberSequence[3]) + 1750, 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                            case (4):
                                if (screenTextRef.text == Mathf.Clamp((numberSequence[3] * numberSequence[0]) + 2000, 0, 9999).ToString())
                                {
                                    NextLight();
                                    return;
                                }
                                break;
                        }
                        break;
                }

                Debug.LogFormat("[Number Sequence #{0}] {1} was submitted incorrectly. Resuming cycling.", moduleId, screenTextRef.text);
                litLEDS = 0;
                StopCoroutine(lightFlashingRef);
                screenTextRef.text = "wrong";
                screenLight.SetActive(true);
                this.gameObject.GetComponent<KMBombModule>().HandleStrike();
                StartCoroutine(BlinkLights(true));
            }
        }
        else if (pressedButton.name == "KeyClear")
        {
            pressedButton.AddInteractionPunch(0.75f);
            audioRef.PlaySoundAtTransform("KeyBoard_Click2", this.transform);

            //Clears the screen if the lights are not cycling
            if (!areLightsCycling)
            {
                screenTextRef.text = string.Empty;
                screenLight.SetActive(false);
            }
        }

        if (pressedButton.name != "KeyClear" && pressedButton.name != "KeySubmit")
        {
            pressedButton.AddInteractionPunch(0.5f);
            audioRef.PlaySoundAtTransform("KeyBoard_Click2", this.transform);

            if (!areLightsCycling && screenTextRef.text.Length < 4)
            {
                screenTextRef.text += pressedButton.name.Remove(0, 3);
                screenLight.SetActive(true);
            }
        }
    }

    void LogHandler()
    {
        switch (portToUse)
        {
            case ("StereoRCA"):
                switch (currentLight)
                {
                    case (1):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} ^ 2 = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[0], Mathf.Pow(numberSequence[0], 2), Mathf.Clamp(Mathf.Pow(numberSequence[0], 2), 0, 9999));

                        break;
                    case (2):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. ({2} - {3}) * 2 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[1], numberSequence[3], numberSequence[1] - numberSequence[3], Mathf.Clamp((numberSequence[1] - numberSequence[3]) * 2, 0, 9999));

                        break;
                    case (3):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. 50 - {2} = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[2], 50 - numberSequence[2], Mathf.Clamp(50 - numberSequence[2], 0, 9999));

                        break;
                    case (4):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * {3} = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[0], numberSequence[3], numberSequence[0] * numberSequence[3], Mathf.Clamp(numberSequence[0] * numberSequence[3], 0, 9999));

                        break;
                }
                break;
            case ("RJ45"):
                switch (currentLight)
                {
                    case (1):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} ^ 3 - {3} * 5 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[2], numberSequence[0], (Mathf.Pow(numberSequence[2], 3)) - (numberSequence[0] * 5), Mathf.Clamp((Mathf.Pow(numberSequence[2], 3)) - (numberSequence[0] * 5), 0, 9999));

                        break;
                    case (2):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. 25 - (-{2}) = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[0], 25 - (-numberSequence[0]), Mathf.Clamp(25 - (-numberSequence[0]), 0, 9999));

                        break;
                    case (3):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. The correct number to submit is {2}", moduleId, currentLight, numberSequence[1]);

                        break;
                    case (4):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. 100 / {2} = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[3], 100f / (float)numberSequence[3], Mathf.Clamp(Mathf.CeilToInt(100f / (float)numberSequence[3]), 0, 9999));

                        break;
                }
                break;
            case ("PS2"):
                switch (currentLight)
                {
                    case (1):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} + {3} / 2 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[0], numberSequence[1], (float)numberSequence[0] + ((float)numberSequence[1] / 2f), Mathf.Clamp(Mathf.CeilToInt((float)numberSequence[0] + ((float)numberSequence[1] / 2f)), 0, 9999));

                        break;
                    case (2):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * 5 - {3} = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[3], numberSequence[2], (numberSequence[3] * 5) - numberSequence[2], Mathf.Clamp((numberSequence[3] * 5) - numberSequence[2], 0, 9999));

                        break;
                    case (3):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. 500 - {2} * 10 = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[2], 500 - (numberSequence[2] * 10), Mathf.Clamp(500 - (numberSequence[2] * 10), 0, 9999));

                        break;
                    case (4):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} / {3} + {4} * 25 = {5}, the correct number to submit is {6}", moduleId, currentLight, numberSequence[3], numberSequence[0], numberSequence[1], ((float)numberSequence[3] / (float)numberSequence[0]) + ((float)numberSequence[1] * 25f), Mathf.Clamp(Mathf.CeilToInt(((float)numberSequence[3] / (float)numberSequence[0]) + ((float)numberSequence[1] * 25f)), 0, 9999));

                        break;
                }
                break;
            case ("Parallel"):
                switch (currentLight)
                {
                    case (1):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. 9999 - {2} * {2} = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[3], 9999 - (numberSequence[3] * numberSequence[3]), Mathf.Clamp(9999 - (numberSequence[3] * numberSequence[3]), 0, 9999));

                        break;
                    case (2):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * 75 + 1000 = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[1], (numberSequence[1] * 75) + 1000, Mathf.Clamp((numberSequence[1] * 75) + 1000, 0, 9999));

                        break;
                    case (3):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} / 99 * 3500 = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[2], ((float)numberSequence[2] / 99f) * 3500f, Mathf.Clamp(Mathf.CeilToInt(((float)numberSequence[2] / 99f) * 3500f), 0, 9999));

                        break;
                    case (4):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. 1 / {2} * {3} ^ 2 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[3], numberSequence[0], (1f / (float)numberSequence[3]) * (Mathf.Pow((float)numberSequence[0], 2f)), Mathf.Clamp(Mathf.CeilToInt((1f / (float)numberSequence[3]) * (Mathf.Pow((float)numberSequence[0], 2f))), 0, 9999));

                        break;
                }
                break;
            case ("Serial"):
                switch (currentLight)
                {
                    case (1):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * {3} * {4} = {5}, the correct number to submit is {6}", moduleId, currentLight, numberSequence[0], numberSequence[2], numberSequence[3], numberSequence[0] * numberSequence[2] * numberSequence[3], Mathf.Clamp(numberSequence[0] * numberSequence[2] * numberSequence[3], 0, 9999));

                        break;
                    case (2):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} - (-{3}) / 35 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[1], numberSequence[0], (float)numberSequence[1] - ((-(float)numberSequence[0]) / 35f), Mathf.Clamp(Mathf.CeilToInt((float)numberSequence[1] - ((-(float)numberSequence[0]) / 35f)), 0, 9999));

                        break;
                    case (3):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. 8500 - {2} * 100 = {3}, the correct number to submit is {4}", moduleId, currentLight, numberSequence[2], 8500 - (numberSequence[2] * 100), Mathf.Clamp(8500 - (numberSequence[2] * 100), 0, 9999));

                        break;
                    case (4):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} / {3} = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[3], numberSequence[0], (float)numberSequence[3] / (float)numberSequence[0], Mathf.Clamp(Mathf.CeilToInt((float)numberSequence[3] / (float)numberSequence[0]), 0, 9999));

                        break;
                }
                break;
            case ("DVI"):
                switch (currentLight)
                {
                    case (1):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * {3} + 6000 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[0], numberSequence[3], (numberSequence[0] * numberSequence[3]) + 6000, Mathf.Clamp((numberSequence[0] * numberSequence[3]) + 6000, 0, 9999));

                        break;
                    case (2):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} / 80 * {3} ^ 2 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[1], numberSequence[2], ((float)numberSequence[1] / 80f) * (Mathf.Pow((float)numberSequence[2], 2)), Mathf.Clamp(Mathf.CeilToInt(((float)numberSequence[1] / 80f) * (Mathf.Pow((float)numberSequence[2], 2))), 0, 9999));

                        break;
                    case (3):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} + {3} + {4} + {5} = {6}, the correct number to submit is {7}", moduleId, currentLight, numberSequence[0], numberSequence[1], numberSequence[2], numberSequence[3], numberSequence[0] + numberSequence[1] + numberSequence[2] + numberSequence[3], Mathf.Clamp(numberSequence[0] + numberSequence[1] + numberSequence[2] + numberSequence[3], 0, 9999));

                        break;
                    case (4):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. 350 * {2} / ({3} + {4}) = {5}, the correct number to submit is {6}", moduleId, currentLight, numberSequence[3], numberSequence[0], numberSequence[1], (350f * (float)numberSequence[3]) / ((float)numberSequence[0] + (float)numberSequence[1]), Mathf.Clamp(Mathf.CeilToInt((350f * (float)numberSequence[3]) / ((float)numberSequence[0] + (float)numberSequence[1])), 0, 9999));

                        break;
                }
                break;
            case (""):
                switch (currentLight)
                {
                    case (1):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * {3} + 1250 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[0], numberSequence[1], (numberSequence[0] * numberSequence[1]) + 1250, Mathf.Clamp((numberSequence[0] * numberSequence[1]) + 1250, 0, 9999));

                        break;
                    case (2):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * {3} + 1500 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[1], numberSequence[2], (numberSequence[1] * numberSequence[2]) + 1500, Mathf.Clamp((numberSequence[1] * numberSequence[2]) + 1500, 0, 9999));

                        break;
                    case (3):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * {3} + 1750 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[2], numberSequence[3], (numberSequence[2] * numberSequence[3]) + 1750, Mathf.Clamp((numberSequence[2] * numberSequence[3]) + 1750, 0, 9999));

                        break;
                    case (4):

                        Debug.LogFormat("[Number Sequence #{0}] The currently lit light is number {1}. {2} * {3} + 2000 = {4}, the correct number to submit is {5}", moduleId, currentLight, numberSequence[3], numberSequence[0], (numberSequence[3] * numberSequence[0]) + 2000, Mathf.Clamp((numberSequence[3] * numberSequence[0]) + 2000, 0, 9999));

                        break;
                }
                break;
        }
    }
}