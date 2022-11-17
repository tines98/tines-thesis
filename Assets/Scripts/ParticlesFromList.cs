using System.Collections;
using System.Collections.Generic;
using PBDFluid;
using UnityEngine;

public class ParticlesFromList : ParticleSource{
    private readonly List<Vector3> positionList;
    
    public ParticlesFromList(float spacing, List<Vector3> positionList) : base(spacing){
        this.positionList = positionList;
        Positions = positionList;
    }

    public override void CreateParticles() => positionList.ForEach(pos => Positions.Add(pos));
}
