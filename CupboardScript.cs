using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class CupboardScript : MonoBehaviour
{
    [Header("IngredientsList")]
    [SerializeField] private GameObject[] ingredients;
    public int currentIngredient = 0;
    [SerializeField] private GameObject ingredientDisplay;

    [Header("Cupboard State")]
    public bool isOpen = false;
    [SerializeField] private float exitTimer = 3.0f;
    private float resetTimer;

    [Header("Octopus Reference")]
    private ContentsScript grabTentacle;
    [SerializeField] private OctopusControlScript controlScript;

    [Header("Animation")]
    private Animator cupboardAnimator;

    [Header("Cupboard UI")]
    [SerializeField] private Canvas cupboardUI;
    [SerializeField] private TextMeshProUGUI IngredientText;
    [SerializeField] private TextMeshProUGUI CookTimeText;
    [SerializeField] private TextMeshProUGUI CutTimeText;

    [Header("Audio")]
    [SerializeField] private AudioSource cupboardDoorAudio;
    [SerializeField] private AudioSource cupboardSwitchAudio;


    private void Start()
    {
        cupboardAnimator = GetComponent<Animator>();  // Assign cupboardAnimators value (assumes that the animator is on the same object as this script)
        resetTimer = exitTimer;  // Assigns the value of reset timer to whatever the actual timer is in the editor
    }

    // Function can be called to cycle through the current ingredients
    public void NextIngredient()
    {
        print("NextIngredient");
        cupboardSwitchAudio.Play();
        ingredients[currentIngredient].SetActive(false);

        currentIngredient = (currentIngredient + 1) < ingredients.Length ? (currentIngredient + 1) : 0;  // If the next value of currentIngredient would exceed the length of the collection, reset back to 0, if not, just increment it
        
        ingredientDisplay = ingredients[currentIngredient];  // Change the display ingredient
        IngredientScript ingredientInfo = ingredients[currentIngredient].GetComponent<IngredientScript>();

        IngredientTextFunction(ingredientInfo.name, ingredientInfo.cookTime, (ingredientInfo.choppedVariants.Length));

        ingredients[currentIngredient].SetActive(true);

        exitTimer = resetTimer;  // Reset the timer
    }

    // Function can be called to open the cupboard
    public void OpenCupboard(ContentsScript whichTentacle)
    {
        cupboardDoorAudio.Play();
        grabTentacle = whichTentacle;
        cupboardUI.gameObject.SetActive(true);

        IngredientScript ingredientInfo = ingredients[currentIngredient].GetComponent<IngredientScript>();
        IngredientTextFunction(ingredientInfo.name, ingredientInfo.cookTime, (ingredientInfo.choppedVariants.Length));

        isOpen = true;  // Tells the code the cupboard is open
        ingredientDisplay.SetActive(true);  // Activates the display ingredient
        cupboardAnimator.SetTrigger("Open");  // Visually open the cupboard
    }

    private void Update()
    {
        // If the cupboard isn't open
        if (!isOpen)
        {
            return;  // Don't run this code
        }

        // If the timer has not finished yet
        if(exitTimer >= 0)
        {
            exitTimer -= Time.deltaTime;  // Decrement the timer every frame
            return;
        }

        CloseCupboard();

        
    }

    // Function can be called to close the cupboard once opened
    public void CloseCupboard()
    {
        cupboardDoorAudio.Play();
        cupboardUI.gameObject.SetActive(false);
        GameObject newIngredient = Instantiate(ingredients[currentIngredient]);
        newIngredient.transform.rotation = Quaternion.identity;

        if(grabTentacle.nextAvailableSpot < grabTentacle.ingredientSpots.Length)
        {
            newIngredient.transform.position = grabTentacle.ingredientSpots[grabTentacle.nextAvailableSpot].position;
            newIngredient.transform.SetParent(grabTentacle.ingredientSpots[grabTentacle.nextAvailableSpot]);

            grabTentacle.RecieveIngredient(newIngredient);  // Assigns the correct ingredient to the grab tentacle
            grabTentacle.nextAvailableSpot++;
        }

        else
        {
            Debug.Log("Already Holding");
        }
        

        isOpen = false; // Tells code the cupboard is closes
        exitTimer = resetTimer;  // Resets the exit timer for next run
        ingredientDisplay.SetActive(false);  // Activates the display prefab
        cupboardAnimator.SetTrigger("Close");  // Closes the cupboard visually

        controlScript.ToggleCupboardCam();
    }

    void IngredientTextFunction(string Ingredient, float CookTime, int CutTime)
    {
        IngredientText.text = Ingredient;
        CookTimeText.text = ("Cook time: " + CookTime.ToString());
        CutTimeText.text = ("Cut time: " + CutTime.ToString());
    }
}
