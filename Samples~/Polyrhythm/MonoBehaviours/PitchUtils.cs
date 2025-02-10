using System.Collections.Generic;
using UnityEngine;

public static class PitchUtils
{
    // Define the note to semitone mapping
    private static readonly Dictionary<string, int> NoteToSemitone = new Dictionary<string, int>
    {
        { "C", 0 },
        { "C#", 1 },
        { "D", 2 },
        { "D#", 3 },
        { "E", 4 },
        { "F", 5 },
        { "F#", 6 },
        { "G", 7 },
        { "G#", 8 },
        { "A", 9 },
        { "A#", 10 },
        { "B", 11 }
    };

    public static float ConvertNoteToPitchMultiplier(string note)
    {
        // Separate the note letter and octave number
        var noteLetter = note[..^1];
        var octave = int.Parse(note[^1..]);

        // Find semitone difference relative to C3
        var semitoneDifference = NoteToSemitone[noteLetter] + (octave - 3) * 12;

        // Calculate the pitch multiplier
        return Mathf.Pow(2f, semitoneDifference / 12f);
    }
}