using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[Serializable] [CreateAssetMenu(menuName = "Crystals/Config")]
public class CrystalConfig : ScriptableObject
{
    public float Lifetime = 5f;

    public int Amount = 3;
    public int ClusterRadius = 8;
    [Range(0f, 1f)] public float Decrease = .8f;
    [Range(1f, 90f)] public float MaxRotationAngle = 60f;

    public int MinLength = 4;
    public int MaxLength = 12;

    public int MinSegments = 4;
    public int MaxSegments = 8;

    public float MinPointiness = 2f;
    public float MaxPointiness = 5f;

    public float MinRadius = .5f;
    public float MaxRadius = 5f;

    [Range(0f, 100f)] public float MinOffset = 10f;
    [Range(0f, 100f)] public float MaxOffset = 30f;

    public CrystalConfig AdditionalConfig()
    {
        CrystalConfig returnConfig = ScriptableObject.CreateInstance<CrystalConfig>();

        returnConfig.Amount = (int)UnityEngine.Random.Range(Amount * 2, Amount * 3f);
        returnConfig.Decrease = Decrease;
        returnConfig.MinLength = (int)(MinLength * Decrease);
        returnConfig.MaxLength = (int)(MaxLength * Decrease);

        returnConfig.MinSegments = MinSegments;
        returnConfig.MaxSegments = MaxSegments;

        returnConfig.MinPointiness = MinPointiness * Decrease;
        returnConfig.MaxPointiness = MaxPointiness * Decrease;

        returnConfig.MinRadius = MinRadius * Decrease;
        returnConfig.MaxRadius = MaxRadius * Decrease;

        returnConfig.MinOffset = MinOffset * 2;
        returnConfig.MaxOffset = MaxOffset * 2;

        return returnConfig;
    }
}
