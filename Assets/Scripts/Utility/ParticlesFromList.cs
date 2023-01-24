using System.Collections;
using System.Collections.Generic;
using PBDFluid;
using PBDFluid.Scripts;
using UnityEngine;

public class ParticlesFromList : ParticleSource{
    private readonly List<Vector3> positionList;
    private Matrix4x4 trs;
    
    public ParticlesFromList(float spacing, List<Vector3> positionList, Matrix4x4 trs) : base(spacing){
        this.positionList = positionList;
        Positions = positionList;
        this.trs = trs;
    }

    public override void CreateParticles(){
        Positions = new List<Vector3>(positionList.Count);
        positionList.ForEach(pos => Positions.Add(trs.MultiplyPoint(pos)));
    }
}
