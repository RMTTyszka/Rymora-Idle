using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    [SerializeField] private SelectHeroService _selectHeroService;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _selectHeroService.HeroSelected(0);
        }    
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            _selectHeroService.HeroSelected(1);
        }       
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            _selectHeroService.HeroSelected(2);
        }

    }
}
