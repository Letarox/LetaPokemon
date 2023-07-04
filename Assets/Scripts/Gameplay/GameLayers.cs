using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoSingleton<GameLayers>
{
    [SerializeField] LayerMask _solidObjectsLayer;
    [SerializeField] LayerMask _tallGrassLayer;
    [SerializeField] LayerMask _interactableLayer;
    public LayerMask SolidObjectsLayer { get => _solidObjectsLayer; }
    public LayerMask TallGrassLayer { get => _tallGrassLayer; }
    public LayerMask InteractableLayer { get => _interactableLayer; }
}
