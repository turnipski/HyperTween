using System.Collections;
using UnityEngine;

public class Scale : MonoBehaviour
{
    public PlaySoundNode playSoundNodePrefab;
    public string[] scaleNotes;
    public int notes, baseOctave;
    public float minDistance, maxDistance;
    public float minDelay, maxDelay;

    IEnumerator Start()
    {
        //yield return new WaitForSeconds(1);
        
        var minX = -5f;
        var maxX = 5f;
        
        for (int i = 0; i < notes; i++)
        {
            var note = scaleNotes[i % scaleNotes.Length];
            var octave = baseOctave + i / scaleNotes.Length;

            var a = Instantiate(playSoundNodePrefab);
            var b = Instantiate(playSoundNodePrefab);

            var t = (float)i/notes;

            a.hue = b.hue = t;
            
            b.triggerAferDelay = a.triggerAferDelay = Mathf.Lerp(minDelay, maxDelay, t);
            b.triggerAferDelay = -1;
            
            a.subsequentNodes = new[] { b };
            b.subsequentNodes = new[] { a };
            
            b.note = a.note = $"{note}{octave}";

            var distance = Mathf.Lerp(minDistance, maxDistance, t);

            a.transform.position = new Vector3(Mathf.Lerp(minX, maxX, t), 0, 0.5f * distance);
            b.transform.position = new Vector3(Mathf.Lerp(minX, maxX, t), 0, -0.5f * distance);
        }
        
        yield break;

    }
}
