using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using UnityEngine;

namespace FieldsToolkit
{
    public class FTKPole : MonoBehaviour
    {
        public static List<FTKPole> poles = new List<FTKPole>();
        private static Queue<LineRenderer> renderers = new Queue<LineRenderer>();

        public float strength = 1;
        public bool render = false;

        public bool simulate = false;

        public Rigidbody body;
        private bool set = false;

        private Vector3 prev;

        private class Line
        {
            public bool completed = false;
            public bool bufferEnd = false;
            public int count = 0;
            public int current = 1;
            public float step = FTK.settings.step;
            public Vector3 last;
        }

        private List<LineRenderer> lines = new List<LineRenderer>();
        private List<Line> lineData = new List<Line>();

        private LineRenderer GetLineRenderer()
        {
            LineRenderer r;
            if (renderers.Count > 0) r = renderers.Dequeue();
            else
            {
                GameObject l = new GameObject();
                l.name = "Field line";
                r = l.AddComponent<LineRenderer>();
            }

            r.transform.parent = transform;

            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0.003f);
            r.widthCurve = curve;
            
            return r;
        }

        public void OnEnable()
        {
            for (int i = 0; i < lines.Count; ++i)
            {
                lines[i].enabled = true;
            }
        }

        public void OnDisable()
        {
            for (int i = 0; i < lines.Count; ++i)
            {
                lines[i].enabled = false;
            }
        }

        public void Start()
        {
            poles.Add(this);

            prev = transform.position;

            ResetLines();
        }

        public void OnDestroy()
        {
            poles.Remove(this);
        }

        private void ResetLines()
        {
            prev = transform.position;

            while (lines.Count < FTK.settings.numLinesPerRing * FTK.settings.numRings)
            {
                lines.Add(GetLineRenderer());
                lineData.Add(new Line());
            }
            while (lines.Count > FTK.settings.numLinesPerRing * FTK.settings.numRings)
            {
                lines[0].gameObject.SetActive(false);
                renderers.Enqueue(lines[0]);
                lines.RemoveAt(0);
                lineData.RemoveAt(0);
            }

            int idx = 0;
            for (int i = 0; i < FTK.settings.numRings; ++i)
            {
                for (int j = 0; j < FTK.settings.numLinesPerRing; ++j, ++idx)
                {
                    lineData[idx] = new Line();
                    lineData[idx].last = transform.position + transform.rotation * Quaternion.Euler(j * 360f / FTK.settings.numLinesPerRing, i * 360f / FTK.settings.numRings, 0) * Vector3.forward * 0.025f;
                }
            }
        }

        private Vector3[] buffer = new Vector3[50];
        public void FixedUpdate()
        {
            for (int i = 0; i < lines.Count; ++i)
            {
                lines[i].enabled = render;
            }

            if (render)
            {
                if (body != null)
                {
                    set = true;
                    body.isKinematic = true;
                }

                if (poles.Any(l => l.prev != l.transform.position) && !lineData.Any(l => !l.completed && !l.bufferEnd)) ResetLines();

                int idx = 0;
                int count;
                for (int i = 0; i < FTK.settings.numRings; ++i)
                {
                    for (int j = 0; j < FTK.settings.numLinesPerRing; ++j, ++idx)
                    {
                        if (lineData[idx].completed || lineData[idx].bufferEnd)
                        {
                            lines[idx].positionCount = lineData[idx].count;
                            continue;
                        }

                        buffer[lineData[idx].current - 1] = lineData[idx].last;
                        lineData[idx].completed = FTK.Solve(buffer, Mathf.Sign(strength), lineData[idx].step, FTK.settings.tolerance, poles, out lineData[idx].step, out count, lineData[idx].current, FTK.settings.iterationsPerFrame);
                        lineData[idx].count = count;
                        lineData[idx].bufferEnd = lineData[idx].count == buffer.Length;
                        if (lines[idx].positionCount < count) lines[idx].positionCount = count;
                        for (--lineData[idx].current; lineData[idx].current < count; ++lineData[idx].current)
                            lines[idx].SetPosition(lineData[idx].current, buffer[lineData[idx].current]);
                        lineData[idx].last = buffer[lineData[idx].current - 1];
                    }
                }
            }
            else
            {
                if (body != null)
                {
                    if (set)
                    {
                        set = false;
                        body.isKinematic = false;
                    }

                    if (simulate) body.AddForceAtPosition(FTK.ForceOnPole(this, poles), transform.position);
                }
            }
        }
    }
}
