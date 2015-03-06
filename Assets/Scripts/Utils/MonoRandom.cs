
//#define LOG_RNG

//
// System.Random.cs
//
// Authors:
// Bob Smith (bob@thestuff.net)
// Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2001 Bob Smith. http://www.thestuff.net
// (C) 2003 Ben Maurer
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[ComVisible(true)]
public class MonoRandom //: Singleton<MonoRandom>
{
    const int MBIG = int.MaxValue;
    const int MSEED = 161803398;

    int inext, inextp;
    int[] SeedArray = new int[56];


    public MonoRandom()
        : this(Environment.TickCount)
    {
    }

    public MonoRandom(int Seed)
    {
        Reseed(Seed);
    }

    // for diagnostics only
    public int originalSeed;
    //int curLogIdx = 0;
    const int maxLogs = 80000;
    public string[] log = new string[maxLogs];
    public bool LoggingOn = false;

    protected virtual double Sample()
    {
        int retVal;

        if (++inext >= 56) inext = 1;
        if (++inextp >= 56) inextp = 1;

        retVal = SeedArray[inext] - SeedArray[inextp];

        if (retVal < 0)
            retVal += MBIG;

        SeedArray[inext] = retVal;


#if LOG_RNG
		if (LoggingOn && curLogIdx < maxLogs - 1)
		{
			StackTrace stackTrace = new StackTrace();           // get call stack
			StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

			string str = "";
			for (int i = 2; i < 3; ++i)
			{
				StackFrame stackFrame = stackFrames[i];
				str += stackFrame.GetMethod().DeclaringType.Name + "." + stackFrame.GetMethod().Name;
			}

			str += "  " + retVal * (1.0 / MBIG);

			log[curLogIdx] = str;

			curLogIdx++;
		}
#endif

        return retVal * (1.0 / MBIG);
    }

    public virtual int Next()
    {
        return (int)(Sample() * int.MaxValue);
    }

    public virtual int Next(int maxValue)
    {
        if (maxValue < 0)
            throw new ArgumentOutOfRangeException("Max value is less than min value.");

        return (int)(Sample() * maxValue);
    }

    public virtual int Next(int minValue, int maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException("Min value is greater than max value.");

        // special case: a difference of one (or less) will always return the minimum
        // e.g. -1,-1 or -1,0 will always return -1
        uint diff = (uint)(maxValue - minValue);
        if (diff <= 1)
            return minValue;

        return (int)((uint)(Sample() * diff) + minValue);
    }

    public virtual void NextBytes(byte[] buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException("buffer");

        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(Sample() * (byte.MaxValue + 1));
        }
    }

    public virtual double NextDouble()
    {
        return this.Sample();
    }

    public virtual float NextFloat()
    {
        return (float)this.Sample();
    }

    internal void Reseed(int Seed)
    {
        originalSeed = Seed;

        int ii;
        int mj, mk;

        // Numerical Recipes in C online @ http://www.library.cornell.edu/nr/bookcpdf/c7-1.pdf

        // Math.Abs throws on Int32.MinValue, so we need to work around that case.
        // Fixes: 605797
        if (Seed == Int32.MinValue)
            mj = MSEED - Math.Abs(Int32.MinValue + 1);
        else
            mj = MSEED - Math.Abs(Seed);

        SeedArray[55] = mj;
        mk = 1;
        for (int i = 1; i < 55; i++)
        { // [1, 55] is special (Knuth)
            ii = (21 * i) % 55;
            SeedArray[ii] = mk;
            mk = mj - mk;
            if (mk < 0)
                mk += MBIG;
            mj = SeedArray[ii];
        }
        for (int k = 1; k < 5; k++)
        {
            for (int i = 1; i < 56; i++)
            {
                SeedArray[i] -= SeedArray[1 + (i + 30) % 55];
                if (SeedArray[i] < 0)
                    SeedArray[i] += MBIG;
            }
        }
        inext = 0;
        inextp = 31;
    }

    internal bool NextBool()
    {
        return Next(0, 2) == 0;
    }

    internal bool NextBool(float chanceTrue)
    {
        return NextDouble() < chanceTrue;
    }

    internal UnityEngine.Color NextColor()
    {
        return new Color(NextFloat(), NextFloat(), NextFloat());
    }

    public float NextFloat(float min, float max)
    {
        return (max - min) * NextFloat() + min;
    }

	public List<T> ShuffleList<T>(List<T> list)
	{
		var shuffled = new List<T>();

		while (list.Count > 0)
		{
			shuffled.Add (PickFromList(list, true));
		}

		return shuffled;
	}
		
    public T PickFromList<T>(List<T> list, Predicate<T> match, bool remove)
	{
		if (list.Count == 0)
		{
			return default(T);
		}
		else if (list.Count == 1)
		{
			T result = list[0];

			if (match(result))
			{
				if (remove)
				{
					list.Clear();
				}
				return result;
			}
			else
			{
				return default(T);
			}
		}
		else
		{
			int randStartingIndex = Next(list.Count);

			// this assumes that the list has been shuffled first in order not to bias the results

			for (int i = 0; i < list.Count; ++i)
			{
				int idx = (randStartingIndex + i) % list.Count;

				T possResult = list[idx];
				if (match(possResult))
				{

					if (remove)
					{
						list.RemoveAt(idx);
					}

					return possResult;
				}
			}

			// failed to match - return null
			return default(T);
		}
	}

    public T PickFromList<T>(List<T> list, bool remove)
    {
        if (list.Count == 0)
        {
            return default(T);
        }
        else if (list.Count == 1)
        {
            T result = list[0];

            if (remove)
            {
                list.Clear();
            }
            return result;
        }
        else
        {
            int randIndex = Next(list.Count);
            T result = list[randIndex];

            if (remove)
            {
                list.RemoveAt(randIndex);
            }

            return result;
        }
    }
    /*
     * temp removal while bringing into clean slate.
    public T RoulettePickFromList<T>(List<T> list, bool remove) where T : IWeightedItem
    {
        // make a list of indices of items in list with non-zero weight
        //List<int> nonZeroWeightIndices = new List<int>();
        // build a RW
        RouletteWheel<int> rouletteWheel = new RouletteWheel<int>(this);
        for (int i = 0; i < list.Count; ++i)
        {
            float weight = list[i].GetWeight();
            if (weight > 0)
            {
                rouletteWheel.AddSlice(i, weight);
            }
        }

        // now, we may have an empty RW
        if (rouletteWheel.NumSlices() == 0)
        {
            return default(T);
        }
        else
        {
            // or a proper RW (code is same for an RW with only 1 slice)

            int resultIndex = rouletteWheel.Spin();

            T result = list[resultIndex];

            if (remove)
            {
                list.RemoveAt(resultIndex);
            }
            return result;
        }
    }
    */
    public void InsertInList<T>(T item, List<T> list)
    {
        int randIndex = (list.Count > 1) ? Next(list.Count) : 0;
        list.Insert(randIndex, item);
    }

    public int NextSign()
    {
        return NextBool() ? 1 : -1;
    }

    internal Vector3 NextVector3UnitSphere()
    {
        return Quaternion.Euler(NextFloat(-180f, 180f), NextFloat(-180f, 180f), NextFloat(-180f, 180f)) * Vector3.forward * NextFloat(0f, 1f);
    }

    internal Vector3 NextVector3Cube()
    {
        return new Vector3(NextFloat(-1f, 1f),NextFloat(-1f, 1f),NextFloat(-1f, 1f));
    }
}
