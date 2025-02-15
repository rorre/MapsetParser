﻿using MapsetParser.objects;
using MapsetParser.objects.hitobjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapsetParser.starrating.standard
{
    public abstract class Skill
    {
        protected abstract double SkillMultiplier  { get; }    // how much this skill is weighed
        protected abstract double StrainDecay      { get; }    // how quickly strain decays for this skill

        protected abstract double StrainValueOf(HitObject anObject);    // how much an object increases strain

        public double currentStrain        = 0;
        public double currentStrainPeak    = 0;
        
        /// <summary> Returns how much to decay the strain over a given delta time in ms. </summary>
        public double GetStrainDecay(double aDeltaTime) => Math.Pow(StrainDecay, aDeltaTime / 1000);

        public List<HitObject>  previousObjects    = new List<HitObject>();
        public List<double>     strainPeaks        = new List<double>();

        /// <summary> Covers base mechanics for strain, like decaying over time and increasing for each given object. </summary>
        public void Process(HitObject anObject)
        {
            currentStrain *= GetStrainDecay(anObject.GetPrevDeltaStartTime());
            if (!(anObject is Spinner))
                currentStrain += StrainValueOf(anObject) * SkillMultiplier;

            currentStrainPeak = Math.Max(currentStrain, currentStrainPeak);

            previousObjects.Add(anObject);
        }

        /// <summary> Adds the current strain peak to a list, but only if at least one object was processed in total. </summary>
        public void SaveCurrentPeak()
        {
            if (previousObjects.Count > 0)
                strainPeaks.Add(currentStrainPeak);
        }

        /// <summary> Decays the current strain peak based on the given offset and time since last object. </summary>
        public void StartNewSectionFrom(double anOffset)
        {
            if (previousObjects.Count > 0)
                currentStrainPeak = currentStrain * GetStrainDecay(anOffset - previousObjects.Last().time);
        }

        /// <summary> Returns the weighted total of peaks, where each is weighed 90% of the previous, starting from the highest. </summary>
        public double DifficultyValue()
        {
            strainPeaks.Sort((aStrain, anOtherStrain) => anOtherStrain.CompareTo(aStrain));

            double difficulty = 0;
            double weight     = 1;
            
            foreach (double strainPeak in strainPeaks)
            {
                difficulty += strainPeak * weight;
                weight *= 0.9;
            }

            return difficulty;
        }
    }
}
