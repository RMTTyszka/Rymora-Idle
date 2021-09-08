using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ControlManager : MonoBehaviour
{
    [FormerlySerializedAs("_selectHeroService")] [SerializeField] private CurrentHeroService currentHeroService;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            currentHeroService.HeroSelected(0);
        }    
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            currentHeroService.HeroSelected(1);
        }       
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            currentHeroService.HeroSelected(2);
        }

    }
}
