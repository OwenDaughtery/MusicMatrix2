﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Simple class for holding the enums of notes and a dictionary of those enums and their respective frequencies. 
public class NoteManager : MonoBehaviour {

    public enum Notes { none, C2, Cs2, D2, Ds2, E2, F2, Fs2, G2, Gs2, A2, As2, B2, C3, Cs3, D3, Ds3, E3, F3, Fs3, G3, Gs3, A3, As3, B3, C4, Cs4, D4, Ds4, E4, F4, Fs4, G4, Gs4, A4, As4, B4, C5, Cs5, D5, Ds5, E5, F5, Fs5, G5, Gs5, A5, As5, B5, C6, Cs6, D6, Ds6, E6, F6, Fs6, G6, Gs6, A6, As6, B6, C7 };

    public static Dictionary<Notes, float> noteToFreq = new Dictionary<Notes, float>{
        {Notes.none, 0f},
        {Notes.C2, 65.41f},
        {Notes.Cs2, 69.30f},
        {Notes.D2, 73.42f},
        {Notes.Ds2, 77.78f},
        {Notes.E2, 82.41f},
        {Notes.F2, 87.31f},
        {Notes.Fs2, 92.50f},
        {Notes.G2, 98.00f},
        {Notes.Gs2, 103.83f},
        {Notes.A2, 110.00f},
        {Notes.As2, 116.54f},
        {Notes.B2, 123.47f},
        {Notes.C3, 130.81f},
        {Notes.Cs3, 138.59f},
        {Notes.D3, 146.83f},
        {Notes.Ds3, 155.56f},
        {Notes.E3, 164.81f},
        {Notes.F3, 174.61f},
        {Notes.Fs3, 185.00f},
        {Notes.G3, 196.00f},
        {Notes.Gs3, 207.65f},
        {Notes.A3, 220.00f},
        {Notes.As3, 233.08f},
        {Notes.B3, 246.94f},
        {Notes.C4, 261.626f},
        {Notes.Cs4, 277.183f},
        {Notes.D4, 293.665f},
        {Notes.Ds4, 311.127f},
        {Notes.E4, 329.628f},
        {Notes.F4, 349.228f},
        {Notes.Fs4, 369.994f},
        {Notes.G4, 391.995f},
        {Notes.Gs4, 415.305f},
        {Notes.A4, 440.000f},
        {Notes.As4, 466.164f},
        {Notes.B4, 493.883f},
        {Notes.C5, 523.251f},
        {Notes.Cs5, 554.365f},
        {Notes.D5, 587.330f},
        {Notes.Ds5, 622.254f},
        {Notes.E5, 659.255f},
        {Notes.F5, 698.456f},
        {Notes.Fs5, 739.989f},
        {Notes.G5, 783.991f},
        {Notes.Gs5, 830.609f},
        {Notes.A5, 880.000f},
        {Notes.As5, 932.328f},
        {Notes.B5, 987.767f},
        {Notes.C6, 1046.50f},
        {Notes.Cs6, 1108.73f},
        {Notes.D6, 1174.66f},
        {Notes.Ds6, 1244.51f},
        {Notes.E6, 1318.51f},
        {Notes.F6, 1396.91f},
        {Notes.Fs6, 1479.98f},
        {Notes.G6, 1567.98f},
        {Notes.Gs6, 1661.22f},
        {Notes.A6, 1760.00f},
        {Notes.As6, 1864.66f},
        {Notes.B6, 1975.53f},
        {Notes.C7, 2093.00f}
    };
}
