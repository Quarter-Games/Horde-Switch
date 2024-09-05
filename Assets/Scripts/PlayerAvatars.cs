using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAvatars", menuName = "ScriptableObjects/PlayerAvatars", order = 1)]
public class PlayerAvatars : ScriptableObject
{
    [SerializeField] List<Sprite> AvatarList;
    public Sprite this[int index] => AvatarList[index];
    public int Count => AvatarList.Count;
}
