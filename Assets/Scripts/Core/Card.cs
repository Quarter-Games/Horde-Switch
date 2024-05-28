using Fusion;
using System;
using UnityEngine;

[Serializable]
public struct Card : INetworkStruct
{
    public int ID;
    public int Value;
}
