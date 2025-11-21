using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Units
{
    public class Party : MonoBehaviour
    {
        [SerializeField] private List<AbstractCommandable> party;
    }
}