using System;
using UnityEngine;

[Serializable]
public class Card
{
    [property: SerializeField] int Value { get; set; }
    [property: SerializeField] string Name { get; set; }
    [property: SerializeField] Sprite Image { get; set; }

}
