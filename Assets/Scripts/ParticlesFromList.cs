using System.Collections;
using System.Collections.Generic;
using PBDFluid;
using UnityEngine;

public class ParticlesFromList : ParticleSource{
    private readonly List<Vector3> positionList;
    
    public ParticlesFromList(float spacing, List<Vector3> positionList) : base(spacing){
        this.positionList = positionList;
        Positions = new List<Vector3>(positionList.Count);
    }

    public override void CreateParticles() => positionList.ForEach(pos => Positions.Add(pos));
}
