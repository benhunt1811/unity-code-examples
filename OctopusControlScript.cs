using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Audio;


public class OctopusControlScript : MonoBehaviour
{
    [Header("Limb Keycodes")]
    [SerializeField] private KeyCode knifeInput = KeyCode.Z;
    [SerializeField] private KeyCode grabInput1 = KeyCode.X;
    [SerializeField] private KeyCode bellInput = KeyCode.V;
    [SerializeField] private KeyCode plateInput = KeyCode.C;
    [SerializeField] private KeyCode gloveInput = KeyCode.B;
    [SerializeField] private KeyCode potInput = KeyCode.N;
    [SerializeField] private KeyCode grabInput2 = KeyCode.M;
    [SerializeField] private KeyCode panInput = KeyCode.L;

    [Header("Input Keycodes")]
    [SerializeField] private KeyCode choppingBoardInput;
    [SerializeField] private KeyCode serviceStationInput;
    [SerializeField] private KeyCode binInput;
    [SerializeField] private KeyCode cupboardInput;
    [SerializeField] private KeyCode ovenInput;
    [SerializeField] private KeyCode stoveInput;
    [SerializeField] private KeyCode sinkInput;

    

    [Header("Transform Collections")]
    public Transform[] tentacleTransforms;
    [SerializeField] private Transform[] kitchenTransforms;

    [Header("General Inputs")]
    [SerializeField] private KeyCode turnRight;
    [SerializeField] private KeyCode turnLeft;

    public bool readyToServe = false;
    public bool canPlay = true;
    public bool canRotate = true;

    [Header("Spin Settings")]
    [SerializeField] private float rotationSpeed = 10;

    [Header("Animation References")]
    [SerializeField] private Animator knifeTentacleAnimator;

    [Header("Maximum Angle Calculation")]
    [Range(0, 360)]
    public float maxAngle;
    public float range;
    public float tentacleOffset = 10;

    [Header("Cinemachine References")]
    [SerializeField] private CinemachineVirtualCamera mainCamera;
    [SerializeField] private CinemachineVirtualCamera cupboardCamera;
    

    [Header("Tentacle Script references")]
    [SerializeField] private ContentsScript grabTentacle1Script;
    [SerializeField] private ContentsScript grabTentacle2Script;
    [SerializeField] private PotScript potFillScript;
    [SerializeField] private ContentsScript potContents;
    [SerializeField] private ContentsScript plateContents;
    [SerializeField] private ContentsScript panContents;
    [SerializeField] private ContentsScript gloveContents;

    [Header("Kitchen Scripts")]
    [SerializeField] private ContentsScript choppingBoardContents;
    [SerializeField] private ChoppingBoardScript choppingBoardScript;
    [SerializeField] private CupboardScript cupboardScript;
    [SerializeField] private CookingScript ovenCookingScript;
    [SerializeField] private CookingScript stoveScript;
    [SerializeField] private HobScript hobScript;
    [SerializeField] private ContentsScript ovenContents;
    [SerializeField] private OvenScri ovenScript;
    [SerializeField] private ServiceStationScript serviceStationScript;
    [SerializeField] private ContentsScript serviceStationContents;

    [Header("Audio")]
    [SerializeField] private AudioSource binAudio;

    [Header("Gravy")]
    [SerializeField] private Material gravyColour;

    [Header("Transfer Visuals")]
    [SerializeField] private ThrowTestScript throwingScript;



    // Update is called once per frame
    void Update()
    {
        if(canPlay == false)
        {
            return;
        }


        PlayerInput();

        if(canRotate == false)
        {
            return;
        }

        // ROTATE OCTOPUS
        if (Input.GetKey(turnLeft))
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (Input.GetKey(turnRight))
        {
            transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime, Space.Self);
        }

        
    }

    public void PlayerInput()
    {
        // KNIFE
        if (Input.GetKey(knifeInput))
        {
            // KNIFE TOUCHES CHOPPING BOARD
            if (Input.GetKeyDown(choppingBoardInput))
            {
                // If the octopuses tentacle is within range of the item it is trying to interact with
                if (CheckAngle(tentacleTransforms[7], kitchenTransforms[0]))
                {
                    knifeTentacleAnimator.SetTrigger("Chop");  // Perform Chopping motion
                    choppingBoardScript.OnChop();
                }  
            }
        }

        // GRAB 1
        if (Input.GetKey(grabInput1))
        {

            // If the tentacle touches the chopping board
            if (Input.GetKeyDown(choppingBoardInput))
            {
                TransferIngredient(choppingBoardContents, grabTentacle1Script, false, true);
            }

            if (Input.GetKeyDown(cupboardInput))
            {
                // If the cupboard is already open
                if(cupboardScript.isOpen == true)
                {
                    cupboardScript.NextIngredient();  // Cycle to the next ingredient
                }

                // If the cupboard is closed
                else
                {
                    ToggleCupboardCam();
                    cupboardScript.OpenCupboard(grabTentacle1Script);  // Open the cupboard
                }
            }

            if (Input.GetKeyDown(ovenInput))
            {
                // If the oven isn't already cooking something
                if(ovenScript.isCooking == false)
                {
                    print("TapOven");
                    if(TransferIngredient(ovenContents, grabTentacle1Script, false, true))
                    {
                        print("Place");
                        ovenScript.PlaceInOven(ovenContents);
                    }
                }

                // If the oven is already cooking something
                else
                {
                    // BURN HAND
                }
            }

            if (Input.GetKeyDown(stoveInput))
            {
                // BURN HAND
            }

            // IF BIN AND HANDS ARENT EMPTY
            if (Input.GetKeyDown(binInput) && grabTentacle1Script.ingredients.Count > 0)
            {
                binAudio.Play();
                grabTentacle1Script.BinIngredients();
            }

            // If the tentacle touches the service station
            if (Input.GetKeyDown(serviceStationInput))
            {
                TransferIngredient(serviceStationContents, grabTentacle1Script, false, true);
            }
        }

        // PLATE
        if (Input.GetKey(plateInput))
        {
            // Touching plate on chopping board
            if (Input.GetKeyDown(choppingBoardInput))
            {
                TransferIngredient(choppingBoardContents, plateContents, true, true);
            }

            if (Input.GetKeyDown(ovenInput))
            {
                TransferIngredient(ovenContents, plateContents, true, true);
            }

            // BIN
            if (Input.GetKeyDown(binInput))
            {
                plateContents.BinIngredients();
            }

            // SERVICE STATION
            if (Input.GetKeyDown(serviceStationInput))
            {
                readyToServe = true;  // Tells the code that the plate is ready to serve

                if(serviceStationContents.ingredients.Count == 1)
                {
                    // Transfer what was already there onto the plate
                    TransferIngredient(serviceStationContents, plateContents, true, true);
                }
            }

            
        }

        if (readyToServe == true && Input.GetKeyUp(serviceStationInput))
        {
            readyToServe = false;  // Tells the code that the plate is no longer ready to serve
        }

        // OVEN GLOVE
        if (Input.GetKey(gloveInput))
        {
            // OVEN
            if (Input.GetKeyDown(ovenInput))
            {
                if(TransferIngredient(ovenContents, gloveContents, true, true))
                {
                    if(ovenScript.isCooking == true)
                    {
                        ovenScript.TakeOutOven();
                    }

                    else
                    {
                        ovenScript.PlaceInOven(ovenContents);
                    }
                    
                }
                
            }

            // Touching plate on chopping board
            if (Input.GetKeyDown(choppingBoardInput))
            {
                TransferIngredient(choppingBoardContents, gloveContents, true, true);
            }

            // BIN
            if (Input.GetKeyDown(binInput) && gloveContents.ingredients.Count > 0)
            {
                binAudio.Play();
                gloveContents.BinIngredients();
            }

            // If the tentacle touches the service station
            if (Input.GetKeyDown(serviceStationInput))
            {
                TransferIngredient(serviceStationContents, gloveContents, false, true);
            }
        }

        // POT HOLD
        if (Input.GetKey(potInput))
        {
            // WHILE ON SINK
            if (Input.GetKey(sinkInput))
            {
                if(potFillScript.isFilling == false)
                {
                    canRotate = false;
                    potFillScript.BeginFill();
                }
            }

            // WHEN OFF SINK
            if (Input.GetKeyUp(sinkInput))
            {
                canRotate = true;
                potFillScript.StopFill();
            }

            // WHILE ON STOVE
            if (Input.GetKey(stoveInput))
            {
                // If the pot is within range of the stove
                if (CheckAngle(tentacleTransforms[2], kitchenTransforms[2]))
                {
                    // If the stove is off and the pot is full of water
                    if (hobScript.hobStates[0] == false && potFillScript.isFullWater == true)
                    {
                        hobScript.TurnHobOn(0, potContents);  // Turn it on
                    }
                } 
            }

            // WHEN TAKING POT OFF STOVE
            if (Input.GetKeyUp(stoveInput))
            {
                hobScript.TurnHobOff(0);
            }

            
        }

        // WHEN POT IS LIFTED UP
        if (Input.GetKeyUp(potInput))
        {
            canRotate = true;
            potFillScript.StopFill();
            if (hobScript.hobStates[0] == true)
            {
                hobScript.TurnHobOff(0);
            }
        }

        // POT TAP

        if (Input.GetKey(potInput))
        {
            if(potContents.ingredients.Count > 0)
            {
                if (potContents.ingredients[potContents.ingredients.Count - 1]._name == "Gravy")
                {
                    if (potFillScript.isFullWater == true && potFillScript.water.gameObject.GetComponent<MeshRenderer>().material != gravyColour)
                    {
                        potFillScript.water.gameObject.GetComponent<MeshRenderer>().material = gravyColour;
                        Gravy(potContents.ingredients[potContents.ingredients.Count - 1]);
                    }
                }
            }
            

            // BIN
            if (Input.GetKeyDown(binInput))
            {
                potFillScript.ResetPot();
                potContents.BinIngredients();
            }

            if (Input.GetKeyDown(choppingBoardInput))
            {
                TransferIngredient(choppingBoardContents, potContents, true, true);
            }

            // If the tentacle touches the service station
            if (Input.GetKeyDown(serviceStationInput))
            {
                TransferIngredient(serviceStationContents, potContents, false, true);
            }
        }
        

        // GRAB 2
        if (Input.GetKey(grabInput2))
        {
            // If the tentacle touches the chopping board
            if (Input.GetKeyDown(choppingBoardInput))
            {
                TransferIngredient(choppingBoardContents, grabTentacle2Script, false, true);
            }

            if (Input.GetKeyDown(cupboardInput))
            {
                // If the cupboard is already open
                if (cupboardScript.isOpen == true)
                {
                    cupboardScript.NextIngredient();  // Cycle to the next ingredient
                }

                // If the cupboard is closed
                else
                {
                    ToggleCupboardCam();
                    cupboardScript.OpenCupboard(grabTentacle2Script);  // Open the cupboard
                }
            }

            if (Input.GetKeyDown(ovenInput))
            {
                // If the oven isn't already cooking something
                if (ovenScript.isCooking == false)
                {
                    print("TapOven");
                    if (TransferIngredient(ovenContents, grabTentacle2Script, false, true))
                    {
                        print("Place");
                        ovenScript.PlaceInOven(ovenContents);
                    }
                }

                // If the oven is already cooking something
                else
                {
                    // BURN HAND
                }
            }

            if (Input.GetKeyDown(stoveInput))
            {
                // BURN HAND
            }

            // IF BIN AND HANDS ARENT EMPTY
            if (Input.GetKeyDown(binInput) && grabTentacle2Script.ingredients.Count > 0)
            {
                binAudio.Play();
                grabTentacle2Script.BinIngredients();

            }

            // If the tentacle touches the service station
            if (Input.GetKeyDown(serviceStationInput))
            {
                TransferIngredient(serviceStationContents, grabTentacle2Script, false, true);
            }
        }

        // Pan Holding
        if (Input.GetKey(panInput))
        {
            // WHILE ON STOVE
            if (Input.GetKey(stoveInput))
            {
                // If the pot is within range of the stove
                if (CheckAngle(tentacleTransforms[0], kitchenTransforms[2]))
                {
                    // If the stove is off
                    if (hobScript.hobStates[1] == false)
                    {
                        hobScript.TurnHobOn(1, panContents);  // Turn it on
                    }
                }
            }

            // WHEN TAKING POT OFF STOVE
            if (Input.GetKeyUp(stoveInput))
            {
                hobScript.TurnHobOff(1);
            }
        }

        if (Input.GetKeyUp(panInput))
        {
            if(hobScript.hobStates[1] == true)
            {
                hobScript.TurnHobOff(1);
            }  
        }

        // PAN Tapping
        if (Input.GetKey(panInput))
        {
            if (Input.GetKeyDown(choppingBoardInput))
            {
                TransferIngredient(choppingBoardContents, panContents, true, true);
            }

            // BIN
            if (Input.GetKeyDown(binInput))
            {
                binAudio.Play();
                panContents.BinIngredients();
            }

            // If the tentacle touches the service station
            if (Input.GetKeyDown(serviceStationInput))
            {
                TransferIngredient(serviceStationContents, panContents, false, true);
            }
        }

        // BELL
        if (Input.GetKeyDown(bellInput))
        {
            if(readyToServe == true)
            {
                canPlay = false;
                serviceStationScript.ServeMeal();
            }
        }

    }

    // GRAB PREFERENCE DETERMINES WHETHER THE PLAYER IS MORE LIKELY TO WANT TO PLACE OR GRAB AN ITEM FROM THE KITCHEN COUNTER IF BOTH THE TENTACLE AND THE COUNTER HAVE AN EQUAL AMOUNT OF INGREDIENTS
    public bool TransferIngredient(ContentsScript kitchenContents, ContentsScript tentacleContents, bool grabPreference, bool shouldThrow)
    {
        // If the kitchen area has at least 1 ingredient on it
        if (kitchenContents.ingredients.Count > 0)
        {
            if (kitchenContents.ingredients[kitchenContents.ingredients.Count - 1].grabbable == false)
            {
                Debug.Log("Cannot pick up half chopped ingredient");
                return false;
            }

            // And the tentacle isn't already holding any items (NO ITEMS)
            if (tentacleContents.ingredients.Count < 1)
            {
                // Transfer the last added ingredient off the board to the tentacle
                if(tentacleContents.RecieveIngredient(kitchenContents.ingredientSpots[kitchenContents.ingredients.Count - 1].GetChild(0).gameObject) == true)
                {
                    if (shouldThrow == true)
                    {
                        throwingScript.InitialiseItem(kitchenContents.ingredientSpots[kitchenContents.ingredients.Count - 1].GetChild(0), tentacleContents.ingredientSpots[tentacleContents.nextAvailableSpot]);
                        tentacleContents.nextAvailableSpot++;
                    }
                    
                    kitchenContents.TransferIngredient(kitchenContents.ingredients[kitchenContents.ingredients.Count - 1]);
                    return true;
                }
                          
                return false;
            }

            // If the tentacle already has an ingredient but there is still space on the kitchen area (NOT NO ITEMS)
            else if (kitchenContents.ingredients.Count < kitchenContents.maxHoldAmount)
            {
                if (grabPreference)
                {
                    // Transfer the last added ingredient off the board to the tentacle
                    if(tentacleContents.RecieveIngredient(kitchenContents.ingredientSpots[kitchenContents.ingredients.Count - 1].GetChild(0).gameObject) == true)
                    {
                        if (shouldThrow == true)
                        {
                            throwingScript.InitialiseItem(kitchenContents.ingredientSpots[kitchenContents.ingredients.Count - 1].GetChild(0), tentacleContents.ingredientSpots[tentacleContents.nextAvailableSpot]);
                            tentacleContents.nextAvailableSpot++;
                        }
                        
                        kitchenContents.TransferIngredient(kitchenContents.ingredients[kitchenContents.ingredients.Count - 1]);
                        return true;
                    }

                    return false;
                }

                else
                {
                    if(shouldThrow == true)
                    {
                        throwingScript.InitialiseItem(tentacleContents.ingredientSpots[tentacleContents.ingredients.Count - 1].GetChild(0), kitchenContents.ingredientSpots[kitchenContents.nextAvailableSpot]);
                        
                    }
                    

                    // Transfer the last added ingredient from the kitchen counter to the tentacle
                    kitchenContents.RecieveIngredient(tentacleContents.ingredientSpots[tentacleContents.ingredients.Count - 1].GetChild(0).gameObject);
                    kitchenContents.nextAvailableSpot++;
                    tentacleContents.TransferIngredient(tentacleContents.ingredients[tentacleContents.ingredients.Count - 1]);
                    return true;
                }
            }

            // If the kitchen area is full and the player is trying to put something on it.
            else
            {
                Debug.Log("Kitchen area is full");
                return false;
            }
        }

        // If the kitchen area has no ingredients on it
        else
        {
            // If the tentacle has ingredients to put on
            if (tentacleContents.ingredients.Count > 0)
            {
                if(shouldThrow == true)
                {
                    throwingScript.InitialiseItem(tentacleContents.ingredientSpots[tentacleContents.ingredients.Count - 1].GetChild(0), kitchenContents.ingredientSpots[kitchenContents.nextAvailableSpot]);
                }
               

                // Transfer the last added ingredient from the kitchen counter to the tentacle
                kitchenContents.RecieveIngredient(tentacleContents.ingredientSpots[tentacleContents.ingredients.Count - 1].GetChild(0).gameObject);
                kitchenContents.nextAvailableSpot++;
                tentacleContents.TransferIngredient(tentacleContents.ingredients[tentacleContents.ingredients.Count - 1]);
                return true;
            }

            // if the tentacle has no ingredients to put on the kitchen area and the kitchen area has nothing to pick up
            else
            {
                Debug.Log("NOTHING TO PICK UP OR PUT DOWN");
                return false;
            }
        }
    }



    public void Gravy(IngredientScript gravyInstance)
    {
        gravyInstance.choppable = true;
        gravyInstance.Chop();
        gravyInstance.choppable = false;
    }

    public void MoveTentacleToComponent(Transform tentacleTransform, Transform componentTransform)
    {
        tentacleTransform.position = componentTransform.position;
    }

    // Returns the angle uwsed in the AngleDebuggingScript so we can have a visual of the max angle of each tentacle
    public Vector3 DirectionFromAngle(float angle)
    {
        angle += tentacleTransforms[7].eulerAngles.y + tentacleOffset;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    // If the angle between the tentacle and the component it wants to reach is too far, return false, if not return true
    public bool CheckAngle(Transform tentacle, Transform component)
    {
        Vector3 directionToComponent = (tentacle.position - component.position).normalized;
        return (Vector3.Angle(-tentacle.right, directionToComponent) + tentacleOffset < (maxAngle / 2)) ? false : true;
    }

    

    // Function can be called to toggle the cupboard cam on/off
    public void ToggleCupboardCam()
    {
        int tempPriority = cupboardCamera.Priority;
        cupboardCamera.Priority = mainCamera.Priority;
        mainCamera.Priority = tempPriority;
    }
}
