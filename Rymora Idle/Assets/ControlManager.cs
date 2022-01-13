using UnityEngine;
using UnityEngine.Serialization;

public class ControlManager : MonoBehaviour
{
    private PartyManager PartyManager { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            PartyManager.PublishPartySelected(0);
        }    
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            PartyManager.PublishPartySelected(1);
        }       
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            PartyManager.PublishPartySelected(2);
        }

    }
}
